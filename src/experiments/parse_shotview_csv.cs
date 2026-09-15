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
        "id": "01a0844a-45a3-7c2e-99cc-6357918c2097",
        "correlationId": "01a0844a-45a3-7c2e-99cc-6357918c2097",
        "causationId": "01a0844a-45a3-7c2e-99cc-6357918c2097",
        "headers": "",
        "event_date": "<string>",
        "firearmName": "<string>",
        "rangeName": "<string>",
        "roundsFired": <integer>,
        "ammo": {
            "description": "<string>", 
            "cartridge": { "name": "<string>"},
            "projectile": { "manufacturer": "<string>", 
                        "type": "<string>", 
                        "weight": <number>, 
                        "name": "<string>", 
                        "units": "<string>" },
            "powder": { "manufacturer": "<string>", 
                "type": "<string>",
                "weight": <number>, 
                "units": "<string>",
                "uniquetek_value": <number>},            
            "coal": <decimal>,
            "cbto": <decimal>,
            "case": { "trim_length": <number>, 
                        "trim_length_units": "inches",
                        "manufacturer": "<string>",
                        "primer": "<string>" },
            "velocity": {
                "units": "<string>",
                "average": <integer>,
                "extreme_spread": <decimal>,
                "standard_deviation" : <decimal>,
            },
            "shot_velocities": [
                { "shot_number": <integer>, "velocity": <decimal>, "shot_time" : "<string>", "clean_bore": <boolean>, "cold_bore": <boolean>, "shot_notes": <boolean> }
            ]
        },
        "notes": "<string>",
        "created": "<string>",
        "modified":"<string>",
    }
    
    ## Parsing Rules
    
    Each row in the CSV can have different meaning depending on the row number:
    - Row 1 should have a description of the the simple range event. It is not comma separated.
    - Row 2 is a header row for a table that
    - GUIDs must be a Version 7 GUID.
    
    ### `id`, `correlationId`, and `causationId`
    - These values will not be present in the attached CSV file, you must add the to the JSON structure.
    - These values should all be the same version 7 GUID.
    
    ### `created` and `modified` 
    - These values doe not exist in the attached CSV file.  You must add them to the JSON structure.
    - These values are must be the current date and time in UTC then UTC 8601 format.
    
    ### `event_date`
    - The event date is the date that CSV file was created.
    - The event date is on a line that starts with "Date" that is immediately after a line that contains only a '-'.
    - THe event date does not have the time zone specified.  It is always in the local time.
    - Convert the event date to UTC, use the local timezone when making the conversion.
    
    ### `ammo`
    - This JSON elements holds data about the ammunition used in the ShotView session.
    - Extract the cartridge name as a single string (e.g., "6.5 Creedmoor", ".308 Winchester")
    - The average velocity is found on the CSV line where the first field is "AVERAGE SPEED". It is a decimal number but convert it to integer.
    - The standard deviation of the velocity is found on the CSV line where the first field is "STD DEV". It is a decimal number with a precision of one decimal place.
    - The extreme spread is found on the CSV line where the first field is "EXTREME SPREAD". It is a decimal number but convert it to integer.
    - The ammo element is the parent of the shots in this session.
    
    #### COAL & CBTO (Cartridge Overall Length / Cartridge Base to Ogive)
    - Default unit: inches
    - If source explicitly states "mm" or "millimeters": keep as-is, unit = "mm"
    - Identifier "COAL" or "CBTO" may appear before or after value (e.g., "COAL 2.263"" or "2.263" COAL")
    - This value is in inches with a precision of three decimal places. It will never be longer than 4 inches.

    #### `powder`
    - Powder type = product name (e.g., "H4895", "IMR 4064")
    - If the powder manufacturer is "VV", then replace it with "Vihtavouri"
    - If there is no powder manufacturer and the powder type starts with an "N", then the powder manufacturer is "Vihtavouri".
    - Default unit: grains (gr)
    - If the powder type starts with "IMR" or "H", then the powder manufacturer is "Hodgdon".
    - If source states "g" or "grams": convert to grains using 1g = 15.4324 gr, round to 1 decimals
    - Units field must be "gr" or "g" (use "gr" after conversion)
    - the manufacturer and type can appear before or after the numeric value (e.g., "4.5gr HP-38" or "H-38 4.5gr").
    - `uniquetek_value` is the value that appears on the UnqiueTek powder bare measure. It is a decimal number with a precision of two decimal places.
    
    #### `projectile`
    - Projectile type = product name (e.g., "Berger VLD", "Hornady ELD-M")
    - Default unit: grains (gr)
    - If source states "g" or "grams": convert to grains using 1g = 15.4324 gr, round to 1 decimals
    - Preserve source precision (e.g., 168 stays 168, 168.5 stays 168.5)
    - Units field must be "gr" or "g" (use "gr" after conversion)
    - the manufacturer and type can appear before or after the numeric value
    
    #### `shot_velocities`
    - There must be at least one shot in each session. This is an array of the velocity for each shot in the session. 
    - The header for the velocity data is the line where the first field in the CSV is "﻿#". Each line after this contains data for a shot.
    - The first CSV value in the line will be the shot number.
    - The second CSV value in the line will be the shot velocity. It is a decimal value with a precision of one decimal place.
    - The sixth CSV value in the line is the shot time. This is always in the local timezone and it always occurs on the same day as the session.
    - The collection of velocity data ends with first CSV line that has a "-" in the first field.
        
    #### `case`
    - This holds information about the brass case.
    - Trim length is the trim length of the cartridge. It is a decimal value with a precision of three decimal places.
    
    ##### `primer`
    - A string that describes the manufacturer and product of the primer used.
    - If missing or unclear use "unknown". 
    
    ### `notes`
    - Notes: free-form string capturing any extra info not in other fields; use "" if none
    - The contents of the first line is the first thing to put in the `notes` JSON element.
    - Find the line that starts with "Session Note", and append the second field in the CSV to the `notes` element of the JSON file.
    - Do not include any shot velocity data in the notes.
    
    ## Error Handling
    - If input is malformed/incomplete: extract what you can, use "unknown"/"0" for missing values
    - Always return valid JSON matching the schema above
    - If multiple cartridges detected: parse only the first one
    
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

// ── clean up & validate ─────────────────────────────────────────────
// LLMs sometimes wrap the JSON in markdown fences; strip them.
reply = reply.Trim();
if (reply.StartsWith("```"))
{
    reply = reply.TrimStart('`');
    if (reply.StartsWith("json")) reply = reply[4..];
    reply = reply.TrimEnd('`').Trim();
}

// Re-serialize so the output is canonical, pretty-printed JSON.
try
{
    var parsed = JsonDocument.Parse(reply);

    // Update metadata section with model name and request time
    using var ms = new MemoryStream();
    await using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
    {
        CopyJsonWithUpdatedMetadata(parsed.RootElement, writer, model, requestTimeSeconds);
    }

    string pretty = Encoding.UTF8.GetString(ms.ToArray());

    Console.WriteLine(pretty);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error parsing JSON: {ex.Message}");
    Console.WriteLine(reply);
    return 1;
}

// ── helper method to copy JSON and update metadata ─────────────────
void CopyJsonWithUpdatedMetadata(JsonElement element, Utf8JsonWriter writer, string modelName, double requestTime)
{
    switch (element.ValueKind)
    {
        case JsonValueKind.Object:
            writer.WriteStartObject();
            foreach (JsonProperty property in element.EnumerateObject())
            {
                writer.WritePropertyName(property.Name);

                // If this is the metadata property, add the new fields
                if (property.Name == "metadata")
                {
                    writer.WriteStartObject();

                    // Copy existing metadata properties
                    if (property.Value.ValueKind == JsonValueKind.Object)
                        foreach (JsonProperty metaProp in property.Value.EnumerateObject())
                        {
                            writer.WritePropertyName(metaProp.Name);
                            CopyJsonWithUpdatedMetadata(metaProp.Value, writer, modelName, requestTime);
                        }

                    // Add new metadata fields
                    writer.WritePropertyName("model");
                    writer.WriteStringValue(modelName);

                    writer.WritePropertyName("request_time_seconds");
                    writer.WriteNumberValue(requestTime);

                    writer.WriteEndObject();
                }
                else
                {
                    CopyJsonWithUpdatedMetadata(property.Value, writer, modelName, requestTime);
                }
            }

            writer.WriteEndObject();
            break;

        case JsonValueKind.Array:
            writer.WriteStartArray();
            foreach (JsonElement item in element.EnumerateArray())
            {
                CopyJsonWithUpdatedMetadata(item, writer, modelName, requestTime);
            }

            writer.WriteEndArray();
            break;

        case JsonValueKind.String:
            writer.WriteStringValue(element.GetString());
            break;

        case JsonValueKind.Number:
            if (element.TryGetInt64(out long longValue))
                writer.WriteNumberValue(longValue);
            else if (element.TryGetDouble(out double doubleValue)) writer.WriteNumberValue(doubleValue);
            break;

        case JsonValueKind.True:
            writer.WriteBooleanValue(true);
            break;

        case JsonValueKind.False:
            writer.WriteBooleanValue(false);
            break;

        case JsonValueKind.Null:
            writer.WriteNullValue();
            break;
        case JsonValueKind.Undefined:
            break;
        default:
            throw new ArgumentOutOfRangeException();
    }
}