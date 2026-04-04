using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Gui.Services;
using SharedControls.Controls;
using SharedControls.Services;

namespace MyLittleRangeBook.Gui.ViewModels
{
    public partial class EditSimpleRangeEventViewModel : ViewModelBase, IDialogParticipant
    {
        readonly IDialogService _dialogService;
        readonly ISimpleRangeEventService _rangeEventService;

        public EditSimpleRangeEventViewModel(SimpleRangeEventViewModel simpleRangeEvent) : this(simpleRangeEvent,
            App.Services.GetRequiredService<ISimpleRangeEventService>(), null)
        {
        }

        public EditSimpleRangeEventViewModel(SimpleRangeEventViewModel simpleRangeEvent,
            ISimpleRangeEventService eventService,
            IDialogService? dialogService)
        {
            _dialogService = dialogService ?? new DialogService(this);
            _rangeEventService = eventService;
            Item = simpleRangeEvent;
        }

        public SimpleRangeEventViewModel Item { get; }


        [RelayCommand]
        async Task SaveAsync()
        {
            Item.Validate();
            if (Item.HasErrors)
            {
                await _dialogService.ShowOverlayDialogAsync<DialogResult>("Validation Error",
                    "Please correct the errors in the form before saving.", DialogCommands.Ok);

                return;
            }

            var simpleRangeEvent = Item.ToSimpleRangeEvent();
            var success = await _rangeEventService.SaveRangeEventAsync(simpleRangeEvent);
            if (success)
            {
                _dialogService.ReturnResultFromOverlayDialog(new SimpleRangeEventViewModel(simpleRangeEvent));
            }
            else
            {
                await _dialogService.ShowOverlayDialogAsync<bool>("Error",
                    "An error occured while trying to save the event.",
                    DialogCommands.Ok);
            }
        }

        [RelayCommand]
        async Task CancelAsync()
        {
            DialogCommand[] commands = [DialogCommands.No, DialogCommands.Yes];
            var userResponse = await _dialogService.ShowOverlayDialogAsync<DialogResult>(
                "Cancel editing?",
                "Do you want to discard your changes?",
                commands);

            switch (userResponse)
            {
                case DialogResult.Yes:
                    _dialogService.ReturnResultFromOverlayDialog(null);

                    break;
                case DialogResult.No:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
