using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using MyLittleRangeBook.GUI.Messages;
using MyLittleRangeBook.GUI.Services;
using MyLittleRangeBook.RangeEvents;
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
            Func<IDialogParticipant, IDialogService> dialogServiceFactory,
            ISimpleRangeEventRepository repo)
        {
            Item = simpleRangeEvent;
            _dialogService = dialogServiceFactory(this);
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
                    var updatedViewModel = new SimpleRangeEventViewModel(simpleRangeEvent);
                    _dialogService.ReturnResultFromOverlayDialog(updatedViewModel);

                    // Notify the rest of the app about the change
                    WeakReferenceMessenger.Default.Send(new UpdateDataMessage<SimpleRangeEvent>(

                        Item.RowId == null ? UpdateAction.Added : UpdateAction.Updated,[simpleRangeEvent]));
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
