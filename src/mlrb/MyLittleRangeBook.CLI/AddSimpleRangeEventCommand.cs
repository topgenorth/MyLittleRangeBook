using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using static MyLittleRangeBook.CLI.ReturnCodes;
using static MyLittleRangeBook.Database.Sqlite.SqliteHelperExtensions;

namespace MyLittleRangeBook.CLI
{
    [RegisterCommands("rangetrip")]
    [UsedImplicitly]
    public class AddSimpleRangeEventCommand
    {
        readonly ICliDisplay _cliDisplay;
        readonly ILogger _logger;
        readonly ISimpleRangeEventRepository _repo;

        public AddSimpleRangeEventCommand(ICliDisplay cliDisplay,
            ILogger logger,
            [FromKeyedServices(SQLITE_KEY)] ISimpleRangeEventRepository repo)
        {
            _cliDisplay = cliDisplay;
            _logger = logger;
            _repo = repo;
        }


        /// <summary>
        ///     Add a new range trip.
        /// </summary>
        /// <param name="firearm"></param>
        /// <param name="rounds"></param>
        /// <param name="range"></param>
        /// <param name="ammo"></param>
        /// <param name="notes"></param>
        /// <param name="date">The date of the range trip in YYYY-MM-DD format. Default to today if omitted</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("add")]
        public async Task<int> AddSimpleRangeEventAsync(string firearm,
            int rounds,
            string range = "",
            string ammo = "",
            string notes = "",
            [RangeTripDateParser] DateOnly date = default,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var sre = new SimpleRangeEvent
                {
                    FirearmName = firearm,
                    RoundsFired = rounds,
                    RangeName = range,
                    AmmoDescription = ammo,
                    Notes = notes,
                    EventDate = date == default ? DateTime.Now.Date : date.ToDateTime(TimeOnly.MinValue).Date
                };

                Result<long?> result = await _repo.UpsertAsync(sre, cancellationToken);

                return SUCCESS;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to add SimpleRangeEvent");
                return FAILED_TO_CREATE_RANGE_EVENT;
            }
        }
    }
}
