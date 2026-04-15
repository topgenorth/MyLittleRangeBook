using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database.Sqlite;
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
        readonly ISimpleRangeLogService _simpleRangeLogService;
        readonly ISqliteHelper _sqliteHelper;


        public EditSimpleRangeEventViewModel(SimpleRangeEventViewModel simpleRangeEvent,
            ISimpleRangeLogService simpleRangeLogService,
            IDialogService dialogService,
            ISqliteHelper sqliteHelper,
            ILogger logger)
        {
            Item = simpleRangeEvent;
            _simpleRangeLogService = simpleRangeLogService;
            _dialogService = dialogService;
            _sqliteHelper = sqliteHelper;
            _logger = logger;
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
                await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
                Result<long?> result =
                    await _simpleRangeLogService.UpsertAsync(conn, simpleRangeEvent, cancellationToken);
                if (result.IsSuccess)
                {
                    _logger.Debug("SimpleRangeEvent {Id} saved RowId: {RowId}", simpleRangeEvent.Id,
                        result.Value);
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
