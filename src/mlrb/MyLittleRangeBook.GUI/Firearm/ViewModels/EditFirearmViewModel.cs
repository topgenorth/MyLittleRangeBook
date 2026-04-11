using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.GUI.Services;
using SharedControls.Controls;
using SharedControls.Services;

namespace MyLittleRangeBook.GUI.ViewModels
{
    public partial class EditFirearmViewModel : ViewModelBase, IDialogParticipant
    {
        readonly IDialogService _dialogService;
        readonly IFirearmsService _firearmsService;

        public EditFirearmViewModel(FirearmViewModel firearmViewModel) : this(firearmViewModel,
            App.Services.GetRequiredService<IFirearmsService>(), null)
        {
        }

        public EditFirearmViewModel(FirearmViewModel firearmViewModel,
            IFirearmsService firearmsService,
            IDialogService? dialogService)
        {
            _dialogService = dialogService ?? new DialogService(this);
            _firearmsService = firearmsService;
            Item = firearmViewModel;
        }

        public FirearmViewModel Item { get; }

        [RelayCommand]
        async Task SaveAsync(CancellationToken cancellationToken)
        {
            Item.Validate();
            if (Item.HasErrors)
            {
                await _dialogService.ShowOverlayDialogAsync<DialogResult>("Validation Error",
                    "Please correct the errors in the form before saving.", DialogCommands.Ok);

                return;
            }

            var f = Item.ToFirearm();
            bool success = await _firearmsService.SaveFirearmAsync(f, cancellationToken);
            if (success)
            {
                _dialogService.ReturnResultFromOverlayDialog(new FirearmViewModel(f));
            }
            else
            {
                await _dialogService.ShowOverlayDialogAsync<bool>("Error",
                    "An error occured while trying to save the firearm.",
                    DialogCommands.Ok);
            }
        }

        [RelayCommand]
        async Task CancelAsync()
        {
            DialogCommand[] commands = [DialogCommands.No, DialogCommands.Yes];
            DialogResult userResponse = await _dialogService.ShowOverlayDialogAsync<DialogResult>(
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
