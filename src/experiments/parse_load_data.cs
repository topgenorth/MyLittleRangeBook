// Run with:  dotnet run parse_load_data.cs

#:property JsonSerializerIsReflectionEnabledByDefault=true
using System.Text;
using System.Text.Json;

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
    Console.Error.WriteLine("  loadingString (required) - The firearm load data string to parse");
    Console.Error.WriteLine("  ollamaUrl (optional)     - Ollama API URL (default: http://localhost:11434/api/chat)");
    Console.Error.WriteLine("  model (optional)         - Ollama model name (default: llama3.2)");
    return 1;
}

string loadingString = args[0];
string ollamaUrl     = args.Length > 1 ? args[1] : "http://localhost:11434/api/chat";
string model         = args.Length > 2 ? args[2] : "mistral";

// ── prompt ──────────────────────────────────────────────────────────
string prompt =
    """
    You are a data-extraction assistant.  Parse the firearm load data string below
    and return ONLY a JSON object — no prose, no markdown fences — in exactly
    this shape:
    {  
        recipe : {
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
                        "uniquetek_value": <number>}
            "coal":   { "value": <number>, "unit": "inches" },
            "cbto":   { "value": <number>, "unit": "inches" },
            "case": { "trim_length": <number>, 
                        "trim_length_units": "inches",
                        "manufacturer": "<string>",
                        "primer": "<string>" }
            "comments": "<string>",
            "metadata" : {
                "original_string": "<string>"
            }
        }
    }

    ## Parsing Rules

    ### Cartridge & Components
    - Extract the cartridge name as a single string (e.g., "6.5 Creedmoor", ".308 Winchester")
    - If the cartridge is "9mm", then change it to "9mm Parabellum"
    - If the cartridge is "45" or ".45" then change it to ".45ACP"
    
    ### Powder
    - Powder type = product name (e.g., "H4895", "IMR 4064")
    - If the powder manufacturer is "VV", then replace it with "Vihtavouri"
    - If there is no powder manufacturer and the powder type starts with an "N", then the powder manufacturer is "Vihtavouri".
    - Default unit: grains (gr)
    - If the powder type starts with "IMR" or "H", then the powder manufacturer is "Hodgdon".
    - If source states "g" or "grams": convert to grains using 1g = 15.4324 gr, round to 1 decimals
    - Units field must be "gr" or "g" (use "gr" after conversion)
    - the manufacturer and type can appear before or after the numeric value (e.g., "4.5gr HP-38" or "H-38 4.5gr").
    
    ### Projectile
    - Projectile type = product name (e.g., "Berger VLD", "Hornady ELD-M")
    - Default unit: grains (gr)
    - If source states "g" or "grams": convert to grains using 1g = 15.4324 gr, round to 1 decimals
    - Preserve source precision (e.g., 168 stays 168, 168.5 stays 168.5)
    - Units field must be "gr" or "g" (use "gr" after conversion)
    - the manufacturer and type can appear before or after the numeric value

    ### COAL & CBTO (Cartridge Overall Length / Cartridge Base to Ogive)
    - Default unit: inches
    - If source explicitly states "mm" or "millimeters": keep as-is, unit = "mm"
    - Identifier "COAL" or "CBTO" may appear before or after value (e.g., "COAL 2.263"" or "2.263" COAL")

    ### Case Trim Length
    - Default unit: inches
    - If missing from source: set value to 0, unit = "inches"
    - If source states "mm" or "millimeters": keep as-is, unit = "mm"

    ### Manufacturers & Unknown Values
    - If manufacturer unknown: use "unknown"
    - If the manufacturer is "VV" then replace that with "Vihtavuori".
    - if the manufacturer is "IMR", then replace that with "Hodgdon".
    - If data missing or unclear: use "unknown" for string fields, 0 for numeric fields
    - Uniquetek value (unitless): default to 0 if not present

    ### Comments
    - Comments: free-form string capturing any extra info not in other fields; use "" if none

    ### Metadata
    - original_string: the exact input string, JSON-escaped for inclusion in output

    ### Primer
    - A string that describes the manufacturer and product of the primer used.
    - If missing or unclear use "unknown". 

    ### Data Segmentation
    - Segments separated by: commas, semicolons, periods, forward slash (/)
    - Process each segment for relevant data points

    ## Examples

    Input: "Random stuff at the beginning. 6.5 x 55 Swedish Mauser, Berger 140gr VLD, VV N566 44.5gr, COAL 2.8125, Lapua brass, CCI200 primer.  Random stuff at the end."

    Output (partial):
    {
        "cartridge": {"name": "6.5 x 55 Swedish Mauser"},
        "projectile": {"manufacturer": "Berger", "type": "VLD", "weight": 140, "units": "gr"},
        "powder": {"manufacturer": "VV", "type": "N560", "weight": 44.5, "units": "gr", "uniquetek_value": 0},
        "coal": {"value": 2.8125, "unit": "inches"},
        "case": {"trim_length": 0, "trim_length_units": "inches", "manufacturer": "Lapua", "primer": "CCI200"}
    }

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

string json = JsonSerializer.Serialize(request);

// ── call Ollama ─────────────────────────────────────────────────────
using var           http    = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
var                 content = new StringContent(json, Encoding.UTF8, "application/json");

var stopwatch = System.Diagnostics.Stopwatch.StartNew();
HttpResponseMessage resp    = await http.PostAsync(ollamaUrl, content);
stopwatch.Stop();
double requestTimeSeconds = stopwatch.Elapsed.TotalSeconds;

if (!resp.IsSuccessStatusCode)
{
    Console.Error.WriteLine($"Ollama error {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
    return 1;
}

// ── extract the assistant's reply ───────────────────────────────────
using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
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
            foreach (var property in element.EnumerateObject())
            {
                writer.WritePropertyName(property.Name);

                // If this is the metadata property, add the new fields
                if (property.Name == "metadata")
                {
                    writer.WriteStartObject();

                    // Copy existing metadata properties
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var metaProp in property.Value.EnumerateObject())
                        {
                            writer.WritePropertyName(metaProp.Name);
                            CopyJsonWithUpdatedMetadata(metaProp.Value, writer, modelName, requestTime);
                        }
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
            foreach (var item in element.EnumerateArray())
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
            {
                writer.WriteNumberValue(longValue);
            }
            else if (element.TryGetDouble(out double doubleValue))
            {
                writer.WriteNumberValue(doubleValue);
            }
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
    }
}
