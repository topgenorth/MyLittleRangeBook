using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.GUI.Services;
using MyLittleRangeBook.Persistence.Sqlite;
using SharedControls.Controls;
using SharedControls.Services;

namespace MyLittleRangeBook.GUI.ViewModels
{
    public partial  class EditFirearmViewModel : ViewModelBase, IDialogParticipant
    {
        private readonly IDialogService _dialogService;
        private readonly IFirearmsService _firearmsDbService;
        private readonly ILogger _logger;
        private readonly ISqliteHelper _sqliteHelper;

        public EditFirearmViewModel(FirearmViewModel firearm,
            IFirearmsService firearmsDbService,
            Func<IDialogParticipant, IDialogService> dialogServiceFactory,
            ISqliteHelper sqliteHelper,
            ILogger logger)
        {
            Item = firearm;
            _firearmsDbService = firearmsDbService;
            _dialogService = dialogServiceFactory(this);
            _logger = logger;
            _sqliteHelper = sqliteHelper;
        }


        public FirearmViewModel Item { get; }

        [RelayCommand]
        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            Item.Validate();
            if (Item.HasErrors)
            {
                await _dialogService.ShowOverlayDialogAsync<DialogResult>("Validation Error",
                    "Please correct the errors in the form before saving.", DialogCommands.Ok);

                return;
            }

            throw new NotImplementedException();
            /*
             var f = Item.ToFirearm();
            try
            {
                SqliteConnection connection = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
                var ctx = new DapperCommandContext(connection, null, cancellationToken);
                Result<EntityId> result = await _firearmsDbService.UpsertAsync(ctx, f);
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
            }*/
        }

        [RelayCommand]
        private async Task CancelAsync()
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