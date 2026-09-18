using System.Diagnostics;
using System.Dynamic;
using System.Text;
using System.Text.Json;
using ConsoleAppFramework;
using Fisher;
using FluentResults;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;

namespace MyLittleRangeBook
{
    [RegisterCommands("garmin")]
    public class ImportGarminShotViewFileCommand
    {
        const string OLLAMA_URL             = "http://localhost:11434/api/chat";
        const string OLLAMA_MODEL           = "llama3.2";
        const int    OLLAMA_TIMEOUT_SECONDS = 420;

        const string OLLAMA_PROMPT =
            """
            You are a data-extraction assistant. Parse the Garmin ShotView CSV file that is attached ONLY a JSON object — no prose, no markdown fences — in exactly
            this shape:

            {
                "event_date": "<string>",
                "rounds_fired": <integer>,
                "velocity": {
                    "units": "<string>",
                    "average": <integer>,
                    "extreme_spread": <decimal>,
                    "standard_deviation" : <decimal>,
                },
                "shot_velocities": [
                    { "shot_number": <integer>, "velocity": <integer>, "shot_time" : "<string>", "clean_bore": <boolean>, "cold_bore": <boolean>, "shot_notes": <boolean> }
                ],
                "notes": "<string>",
            }

            ## Parsing Rules
            - Each row in the CSV can have different meaning depending on the row number:
                - Row 1 should have a description of the the simple range event. It is not comma separated.
                - Row 2 is a header row for the CSV values that hold the shot velocities.
            - Never guess at a value.  If it is unclear, then use "unknown" or 0 for missing values.
            - Format all date and time values for ISO-8601 in UTC.

            ## Error Handling
            - If input is malformed/incomplete: extract what you can, use "unknown"/"0" for missing values

            ## `velocity`
            - This section is the average velocity of all the shots that were in fired in this session.
            - The units of measure will be either feet per second (fps) or metres per second (m/s).
            - The second value in the header will hold the units of measure for the velocity.  For example, "Speed (FPS)" means that the velocity is in feet per second (fps).

            ## `event_date`
            - The event date is the date that CSV file was created.
            - The event date is on a line that starts with "Date" that is immediately after a line that contains only a '-'.
            - THe event date does not have the time zone specified.  It is always in the local time.
            - Convert the event date a DateTimeOffset, use the local timezone when making the conversion.
            - Format the event date according to ISO-8601

            ## `shot_velocities`
            - There must be at least one shot in each session. This is an array of the velocity for each shot in the session.
            - The header for the velocity data is the line where the first field in the CSV is "﻿#". Each line after this contains data for a shot.
            - Each line of shot data is  list of comma separated values (CSV).
            - The first CSV value is the `shot_number`. It is an integer.
            - The second CSV value is the `velocity`. It is an decimal value. It will be in double quotes. For example "2669.4".
            - Ignore the third CSV value.
            - Ignore the fourth CSV value.
            - Ignore the fifth CSV value.
            - The sixth CSV value is the `shot_time` of date that the shot was fired.
            - The seventh CSV value is the `clean_bore` value. It is a boolean. Assume that it is FALSE unless there is a true value.
            - The eighth CSV value is the `cold_bore` value. It is a boolean. Assume that it is FALSE unless there is a true value.
            - The ninth CSV value is the `shot_notes` value.  It is a string.
            - The collection of velocity data ends with first CSV line that has a "-" in the first field.

            ## `notes`
            - Find the line that starts with "Session Note".
            - The `notes` value is the first line of this file concatenated with the "Session Note".

            Loading string:

            """;

        readonly ICliDisplay      _cliDisplay;
        readonly IFirearmsService _firearmsService;
        readonly ILogger          _logger;
        readonly IDocumentSession _session;

        public ImportGarminShotViewFileCommand(ICliDisplay      cliDisplay,      IDocumentSession session,
                                               IFirearmsService firearmsService, ILogger          logger)
        {
            _cliDisplay      = cliDisplay;
            _session         = session;
            _firearmsService = firearmsService;
            _logger          = logger;
        }

        /// <summary>
        ///     Adds a Garmin ShotView CSV file. Capture the CSV file, and then covert it to a document.
        /// </summary>
        /// <param name="firearm"></param>
        /// <param name="file"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("add-shotview")]
        public async Task<int> AddShotViewFile(string            firearm, string file,
                                               CancellationToken cancellationToken = default)
        {
            _cliDisplay.PrintCommandHeader("Import Garmin ShotView CSV file.");
            int returnCode = -1;
            if (!File.Exists(file))
            {
                _cliDisplay.PrintFailure($"File {file} not found.");
                returnCode = ReturnCodes.SHOTVIEW_FILE_NOT_FOUND;
                goto Exit;
            }

            Result<Guid> rFirearmId = await _firearmsService.FetchStreamIdForFirearm(firearm, cancellationToken);
            if (rFirearmId.IsFailed)
            {
                _cliDisplay.PrintFailure($"Firearm {firearm} not found.");
                returnCode = ReturnCodes.SHOTVIEW_FILE_READ_FAILURE;
                goto Exit;
            }

            string fileContents = string.Empty;
            try
            {
                fileContents = await File.ReadAllTextAsync(file, cancellationToken);
            }
            catch (Exception ex)
            {
                _cliDisplay.PrintFailure($"Error reading file {file}: {ex.Message}");
                returnCode = ReturnCodes.SHOTVIEW_FILE_READ_FAILURE;
                goto Exit;
            }

            Guid causationId   = Guid.CreateVersion7();
            Guid correlationId = rFirearmId.Value;

            List<object> events = [];
            GarminShotViewFileAddedToFirearm e1 = new(firearm, correlationId, correlationId,
                                                      file, fileContents,
                                                      DateTimeOffset.UtcNow);
            events.Add(e1);


            Result<string> rTransmorgify = await OllamaConvertsToJson(fileContents, cancellationToken);
            if (rTransmorgify.IsSuccess)
            {
                GarminShotViewCsvTransmorgifiedToJson e2 = new(firearm, correlationId, causationId,
                                                               rTransmorgify.Value,
                                                               DateTimeOffset.UtcNow);
                events.Add(e2);

                /*JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

// reply is a string containing JSON
                dynamic data = JsonSerializer.Deserialize<ExpandoObject>(rTransmorgify.Value, options) ??
                               new ExpandoObject();

                */
                _logger.Debug("Converted the CSV to JSON");
            }
            else
            {
                _logger.Warning("Failed to convert CSV to JSON");
            }

            _session.Events.Append(rFirearmId.Value, events);
            await _session.SaveChangesAsync(cancellationToken);
            returnCode = ReturnCodes.SUCCESS;

            Exit:
            if (returnCode == ReturnCodes.SUCCESS)
            {
                _cliDisplay.PrintSuccess("Parsed the file.");
            }
            else
            {
                _cliDisplay.PrintFailure("Something went wrong.");
            }

            return returnCode;
        }

        async Task<Result<string>> OllamaConvertsToJson(string fileContents, CancellationToken cancellationToken)
        {
            #region Build the Ollama request.
            string prompt = OLLAMA_PROMPT + fileContents;

            var ollamaRequest = new
                                {
                                    model  = OLLAMA_MODEL,
                                    stream = false,
                                    messages = new[]
                                               {
                                                   new
                                                   {
                                                       role    = "system",
                                                       content = "You output only raw JSON. No code fences.",
                                                   },
                                                   new { role = "user", content = prompt },
                                               },
                                };

#pragma warning disable IL2026, IL3050
            string ollamaRequestJson = JsonSerializer.Serialize(ollamaRequest);
#pragma warning restore IL2026, IL3050
            #endregion

            #region Send the Ollama request to the local Ollama service via REST
            using HttpClient http                = new() { Timeout = TimeSpan.FromSeconds(OLLAMA_TIMEOUT_SECONDS) };
            StringContent    ollamaStringPayload = new(ollamaRequestJson, Encoding.UTF8, "application/json");

            Stopwatch stopwatch = Stopwatch.StartNew();
            _cliDisplay.PrintInfo("Putting Ollama to work...");
            _logger.Information("Sending request to Ollama");
            HttpResponseMessage ollamaHttpResponse =
                await http.PostAsync(OLLAMA_URL, ollamaStringPayload, cancellationToken);
            stopwatch.Stop();

            double requestTimeSeconds = stopwatch.Elapsed.TotalSeconds;
            _cliDisplay.PrintInfo($"Ollama finished in {requestTimeSeconds} seconds");
            _logger.Information("Ollama request time: {RequestTimeSeconds} seconds", requestTimeSeconds);
            #endregion

            #region Handle the response; get the JSON.
            if (!ollamaHttpResponse.IsSuccessStatusCode)
            {
                _logger.Error("Ollama error {StatusCode}: {Content}", ollamaHttpResponse.StatusCode,
                              await ollamaHttpResponse.Content.ReadAsStringAsync(cancellationToken));
                return Result.Fail("Ollama could not transmorgify the CSV file.");
            }

            try
            {
                string ollamaResponseJson = await ollamaHttpResponse.Content.ReadAsStringAsync(cancellationToken);

                using JsonDocument doc = JsonDocument.Parse(ollamaResponseJson.Trim());
                string jsonResult = doc.RootElement.GetProperty("message")
                                       .GetProperty("content")
                                       .GetString()!;
                return Result.Ok(jsonResult);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Something is wrong with the Ollama response - {ErrorMessage}", e.Message);
                _cliDisplay.PrintFailure($"Failed to parse Ollama response - {e.Message}");
                return new Result<string>().WithError(e.ToError());
            }
            #endregion
        }
    }
}