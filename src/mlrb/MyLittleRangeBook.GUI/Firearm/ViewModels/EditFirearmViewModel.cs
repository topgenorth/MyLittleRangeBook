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
    public partial class EditFirearmViewModel : ViewModelBase, IDialogParticipant
    {
        readonly IDialogService _dialogService;
        readonly IFirearmsService _firearmsService;
        readonly ILogger _logger;
        readonly ISqliteHelper _sqliteHelper;

        public EditFirearmViewModel(FirearmViewModel firearm,
            IFirearmsService firearmsService,
            IDialogService dialogService,
            ISqliteHelper sqliteHelper,
            ILogger logger)
        {
            Item = firearm;
            _firearmsService = firearmsService;
            _dialogService = dialogService;
            _logger = logger;
            _sqliteHelper = sqliteHelper;
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
            try
            {
                SqliteConnection connection = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
                Result<long?> result = await _firearmsService.UpsertAsync(connection, f, cancellationToken);
                if (result.IsSuccess)
                {
                    _logger.Debug("Firearm {Id} saved RowId: {RowId}", f.Id, result.Value);
                    _dialogService.ReturnResultFromOverlayDialog(new FirearmViewModel(f));
                }
                else
                {
                    await _dialogService.ShowOverlayDialogAsync<bool>("Error",
                        "An error occured while trying to save the firearm.",
                        DialogCommands.Ok);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save firearm {Id}.", f.Id);
                await _dialogService.ShowOverlayDialogAsync<bool>("Error",
                    $"An unexpected error occured while trying to save the firearm: {ex.Message}",
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
