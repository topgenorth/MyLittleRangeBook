using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentResults;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.GUI.Messages;
using MyLittleRangeBook.GUI.Services;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using MyLittleRangeBook.RangeEvents;
using SharedControls.Controls;
using SharedControls.Services;

namespace MyLittleRangeBook.GUI.ViewModels
{
    public partial class EditSimpleRangeEventViewModel : ViewModelBase, IDialogParticipant
    {
        readonly IDialogService                 _dialogService;
        readonly ILogger                        _logger;
        readonly ISimpleRangeEventDataProcessor _simpleRangeEventDataProcessor;
        readonly ISimpleRangeEventService       _simpleRangeEventService;
        readonly ISqliteHelper                  _sqliteHelper;
        readonly IFirearmsService               _firearmsService;

        [ObservableProperty]
        private IEnumerable<string> _firearmNames = [];

        /// <summary>
        ///     ViewModel responsible for editing a simple range event.
        ///     This class provides functionality for handling user inputs and interactions
        ///     related to editing range events, as well as integrating with services for logging and persistence.
        /// </summary>
        public EditSimpleRangeEventViewModel(SimpleRangeEventViewModel                simpleRangeEvent,
                                             ILogger                                  logger,
                                             Func<IDialogParticipant, IDialogService> dialogServiceFactory,
                                             ISqliteHelper                            sqliteHelper,
                                             ISimpleRangeEventDataProcessor           simpleRangeEventDataProcessor,
                                             ISimpleRangeEventService                 simpleRangeEventService,
                                             IFirearmsService                         firearmsService)
        {
            Item                           = simpleRangeEvent;
            _dialogService                 = dialogServiceFactory(this);
            _logger                        = logger;
            _sqliteHelper                  = sqliteHelper;
            _simpleRangeEventDataProcessor = simpleRangeEventDataProcessor;
            _simpleRangeEventService       = simpleRangeEventService;
            _firearmsService               = firearmsService;

            _ = LoadFirearmNamesAsync();
        }

        private async Task LoadFirearmNamesAsync()
        {
            await using DapperCommandContext ctx =
                await DapperCommandContext.NewAsync(_sqliteHelper).ConfigureAwait(false);

            Result<IEnumerable<Firearm>> firearmsResult =
                await _firearmsService.GetFirearmsAsync(ctx).ConfigureAwait(false);

            if (firearmsResult.IsSuccess)
            {
                FirearmNames = firearmsResult.Value.Select(f => f.Name).ToList();
            }
        }

        public SimpleRangeEventViewModel Item { get; }


        [RelayCommand]
        async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            Item.Validate();
            if (Item.HasErrors)
            {
                await _dialogService.ShowOverlayDialogAsync<DialogResult>("Validation Error",
                                                                          "Please correct the errors in the form before saving.",
                                                                          DialogCommands.Ok);
                return;
            }

            await using DapperCommandContext ctx =
                await DapperCommandContext.NewAsync(_sqliteHelper, cancellationToken, true).ConfigureAwait(false);

            try
            {
                Result<MlrbId> r1 = await _simpleRangeEventDataProcessor
                                         .ProcessSimpleRangeEventData(ctx,
                                                                      Item.FirearmName,
                                                                      Item.RoundsFired,
                                                                      Item.RangeName,
                                                                      Item.AmmoDescription,
                                                                      Item.Notes,
                                                                      DateOnly.FromDateTime(Item.EventDate))
                                         .ConfigureAwait(false);

                if (r1.IsSuccess)
                {
                    await ctx.CommitAsync().ConfigureAwait(false);
                    Result<SimpleRangeEvent> r3 = await _simpleRangeEventService
                                                       .GetAsync(ctx, r1.Value)
                                                       .ConfigureAwait(false);
                    SimpleRangeEvent          sre              = r3.Value;
                    SimpleRangeEventViewModel updatedViewModel = new(sre);
                    _dialogService.ReturnResultFromOverlayDialog(updatedViewModel);
                    WeakReferenceMessenger.Default.Send(new UpdateDataMessage<SimpleRangeEvent>(
                                                         Item.RowId == null
                                                             ? UpdateAction.Added
                                                             : UpdateAction.Updated,
                                                         sre));
                }
                else
                {
                    await ctx.RollbackAsync().ConfigureAwait(false);
                    await _dialogService.ShowOverlayDialogAsync<bool>("Error",
                                                                      "An error occured while trying to save the event.",
                                                                      DialogCommands.Ok);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to save simple range event {Id}.", Item.Id);
                await ctx.RollbackAsync().ConfigureAwait(false);
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