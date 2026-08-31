using ConsoleAppFramework;
using Fisher;
using Fisher.Exceptions;
using JasperFx.Events;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;

namespace MyLittleRangeBook
{
    [RegisterCommands("firearms")]
    [UsedImplicitly]
    public class AddGarminShotViewFileToFirearm
    {
        readonly ICliDisplay      _cliDisplay;
        readonly IFirearmsService _firearmsService;
        readonly ILogger          _logger;
        readonly IDocumentSession _session;


        public AddGarminShotViewFileToFirearm(ILogger logger, ICliDisplay cliDisplay, IFirearmsService firearmsService,
                                              IDocumentSession session)
        {
            _logger          = logger;
            _cliDisplay      = cliDisplay;
            _firearmsService = firearmsService;
            _session         = session;
        }

        /// <summary>
        ///     Append a Garmin ShotView CSV file to the named firearm.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("add-shotview-file"), UsedImplicitly]
        public async Task<int> AddGarminShotViewCSV(string            name,
                                                    string            file,
                                                    CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.Warning("Firearm name is null or empty.");
                _cliDisplay.PrintFailure("Firearm name must be provided.");
                return ReturnCodes.FAILURE;
            }
            if (string.IsNullOrWhiteSpace(file))
            {
                _logger.Warning("File path is null or empty.");
                _cliDisplay.PrintFailure("File path must be provided.");
                return ReturnCodes.SHOTVIEW_FILE_NOT_FOUND;
            }

            if (!File.Exists(file))
            {
                _logger.Warning("File does not exist: {File}", file);
                _cliDisplay.PrintFailure($"File does not exist: {file}");
                return ReturnCodes.SHOTVIEW_FILE_NOT_FOUND;
            }

            Guid firearmId;
            bool create    = false;
            try
            {
                IEventStream<Firearm> stream =
                    await _session.Events.FetchForWritingByNaturalKey<Firearm, string>(name,
                        cancellationToken);
                firearmId = stream.Id;
            }
            catch (UnknownNaturalKeyException)
            {
                _logger.Verbose("{0} is not a known natural key.", name);
                create = true;
                firearmId = Guid.CreateVersion7();
            }

            if (create)
            {
                try
                {
                    _session.Events.StartStream<Firearm>(firearmId,
                                                                 new FirearmCreated(name, DateTimeOffset.UtcNow));
                    _logger.Verbose("Created the stream for natural key {0}/{1}.", name, firearmId);

                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "An error occurred while creating the firearm stream for natural key {0}.",
                                  name);

                    _cliDisplay.PrintFailure(ex.Message);
                    return ReturnCodes.FAILURE;
                }
            }

            try
            {
                string fileContent = await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false);
                var e = new GarminShotViewFileAddedToFirearm()
                {
                    FirearmName = name,
                    FileContents = fileContent,
                    OccurredUtc = DateTimeOffset.UtcNow,
                };
                _session.Events.Append(firearmId, e);
                await _session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.Information("Add the Shotview file {File} to the {Firearm}", file, name);
                _cliDisplay.PrintSuccess($"Updated the firearm {name}   with the ShotView file {file}.");
                return ReturnCodes.SUCCESS;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to read file: {File}", file);
                _cliDisplay.PrintFailure($"Failed to read file: {ex.Message}");
                return ReturnCodes.SHOTVIEW_FILE_READ_FAILURE;
            }

            return ReturnCodes.SUCCESS;
        }
    }
}