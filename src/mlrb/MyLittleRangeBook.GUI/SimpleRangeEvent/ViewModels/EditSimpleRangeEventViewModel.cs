using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.GUI.Services;
using MyLittleRangeBook.Services;
using SharedControls.Controls;
using SharedControls.Services;

namespace MyLittleRangeBook.GUI.ViewModels
{
    public partial class EditSimpleRangeEventViewModel : ViewModelBase, IDialogParticipant
    {
        readonly IDialogService _dialogService;
        readonly ILogger _logger;
        readonly ISimpleRangeEventRepository _repo;

        public EditSimpleRangeEventViewModel(SimpleRangeEventViewModel simpleRangeEvent,
            ILogger logger,
            IDialogService dialogService,
            ISimpleRangeEventRepository repo)
        {
            Item = simpleRangeEvent;
            _dialogService = dialogService;
            _logger = logger;
            _repo = repo;
        }

        public SimpleRangeEventViewModel Item { get; }


        [RelayCommand]
        async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            Item.Validate();
            if (Item.HasErrors)
            {
                await _dialogService.ShowOverlayDialogAsync<DialogResult>("Validation Error",
                    "Please correct the errors in the form before saving.", DialogCommands.Ok);

                return;
            }

            var simpleRangeEvent = Item.ToSimpleRangeEvent();

            try
            {
                Result<long?> result = await _repo.UpsertAsync(simpleRangeEvent, cancellationToken);
                if (result.IsSuccess)
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
            catch (Exception e)
            {
                _logger.Error(e, "Failed to save simple range event {Id}.", simpleRangeEvent.Id);
                await _dialogService.ShowOverlayDialogAsync<bool>("Error",
                    "An error occured while trying to save the event.",
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
                case DialogResult.None:
                case DialogResult.Ok:
                case DialogResult.Cancel:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
