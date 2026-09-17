// Run with:  dotnet run parse_load_data.cs

#:property JsonSerializerIsReflectionEnabledByDefault=true
using System.Diagnostics;
using System.Text;
using System.Text.Json;

const int OLLAMA_TIMEOUT_SECONDS = 400;
// ── config ──────────────────────────────────────────────────────────
// Command-line arguments:
//   1. loadingString (required): the firearm load data string to parse
//   2. ollamaUrl (optional): Ollama API URL (default: http://localhost:11434/api/chat)
//   3. model (optional): Ollama model name (default: llama3.2)

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: dotnet run parse_load_data.cs <loadingString> [ollamaUrl] [model]");
    Console.Error.WriteLine("");
    Console.Error.WriteLine("Arguments:");
    Console.Error.WriteLine("  pathToCsv (required) - The path to the CSV file containing shot view data");
    Console.Error.WriteLine("  ollamaUrl (optional)     - Ollama API URL (default: http://localhost:11434/api/chat)");
    Console.Error.WriteLine("  model (optional)         - Ollama model name (default: llama3.2)");
    return 1;
}

string pathToCsv = args[0];
string ollamaUrl     = args.Length > 1 ? args[1] : "http://localhost:11434/api/chat";
string model         = args.Length > 2 ? args[2] : "llama3.2";

// ── load file ──────────────────────────────────────────────────────────

string loadingString;
try
{
    loadingString = await File.ReadAllTextAsync(pathToCsv);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error reading file '{pathToCsv}': {ex.Message}");
    return 1;
}


// ── prompt ──────────────────────────────────────────────────────────
string prompt =
    """
You are a data-extraction assistant. Parse the Garmin ShotView CSV file that is attached ONLY a JSON object — no prose, no markdown fences — in exactly
this shape:

{
    "id": "<guid>",
    "correlationId": "<guid>",
    "causationId": "<guid>",
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
    "created": "<string>",
    "modified":"<string>",
}

## Parsing Rules
- Each row in the CSV can have different meaning depending on the row number:
    - Row 1 should have a description of the the simple range event. It is not comma separated.
    - Row 2 is a header row for a table that
    - GUIDs must be a Version 7 GUID.
- Never guess at a value.  If it is unclear, then use "unknown" or 0 for missing values.
- Format all date and time values for ISO-8601 in UTC.

## Error Handling
- If input is malformed/incomplete: extract what you can, use "unknown"/"0" for missing values
- Always return valid JSON matching the schema above


### `id`, `correlationId`, and `causationId`
- These values will not be present in the attached CSV file, you must add the to the JSON structure.
- These values should all be the GUIDs.

### `velocity`
- This section is the average velocity of all the shots that were in fired in this session.
- The units of measure will be either feet per second (fps) or metres per second (m/s).
- The second value in the header will hold the units of measure for the velocity.  For example, "Speed (FPS)" means that the velocity is in feet per second (fps).

### `created` and `modified`
- These values doe not exist in the attached CSV file.  You must add them to the JSON structure.
- These values are must be the current date and time in UTC then UTC 8601 format.

### `event_date`
- The event date is the date that CSV file was created.
- The event date is on a line that starts with "Date" that is immediately after a line that contains only a '-'.
- THe event date does not have the time zone specified.  It is always in the local time.
- Convert the event date a DateTimeOffset, use the local timezone when making the conversion.

#### `shot_velocities`
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

### `notes`
- The `notes` value is the first line of this file concatenated with the "Session Note".
- Find the line that starts with "Session Note", and append the second field in the CSV to the `notes` element of the JSON file.

Loading string:
""" + loadingString;

// ── build the Ollama request body ───────────────────────────────────
var request = new
              {
                  model,
                  stream = false,
                  messages = new[]
                             {
                                 new { role = "system", content = "You output only raw JSON. No code fences." },
                                 new { role = "user", content   = prompt },
                             },
              };

#pragma warning disable IL2026, IL3050
string json = JsonSerializer.Serialize(request);
#pragma warning restore IL2026, IL3050

// ── call Ollama ─────────────────────────────────────────────────────
Console.Write($"Calling Ollama with the file {pathToCsv}...");
using var http    = new HttpClient { Timeout = TimeSpan.FromSeconds(OLLAMA_TIMEOUT_SECONDS) };
var       content = new StringContent(json, Encoding.UTF8, "application/json");

var                 stopwatch = Stopwatch.StartNew();
HttpResponseMessage resp      = await http.PostAsync(ollamaUrl, content);
stopwatch.Stop();
double requestTimeSeconds = stopwatch.Elapsed.TotalSeconds;

if (!resp.IsSuccessStatusCode)
{
    Console.Error.WriteLine($"Ollama error {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
    return 1;
}

Console.Write($"Ollama replied in {requestTimeSeconds} seconds.");
// ── extract the assistant's reply ───────────────────────────────────
string x = await resp.Content.ReadAsStringAsync();
using var doc = JsonDocument.Parse(x.Trim());
string reply = doc.RootElement.GetProperty("message")
                  .GetProperty("content")
                  .GetString()!;

Console.WriteLine(reply);
return 0;
