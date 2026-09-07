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
string model         = args.Length > 2 ? args[2] : "llama3.2";

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
            "comments": "<string>"
            "original_string": "<string>"
        }
    }

    Rules:
    * data segments are separated by commas, semicolons, or periods.  Each segment may contain one or more data points.
    * the cartridge name a string that names the cartridge.  
    • projectile / powder weights are in grains (gr) or grams (g); keep the same decimal precision as the source. If not specified, assume grains. If the source explicitly states "g" or "grams", convert to grains (1 g = 15.4324 gr) and round to 2 decimal places.
    * the projectile manufacturer is the company that produced the projectile. If unknown, use "unknown".
    * The powder "manufacturer" is the company that produced the powder. If unknown, use "unknown".
    • The powder "type" should include the name of the product. If unknown, use "unknown".
    * the projectile type is the product name of the projectile. If unknown, use "unknown".
    * the powder uniquetek_value is a unitless value that used by the Uniquetek power meter bar to control the powder flow. If unknown, use 0. It is a decimal number with up to 2 decimal places.
    • COAL value is in inches or millimeters. Assume the units are in inches unless the source explicitly states "mm" or "millimeters".
    * The COAL value is identified with the string COAL. This identifier might appear after a numeric value or before for the numeric value.  For example 2.263" COAL or COAL 2.263".
    * CBTO value is in inches or millimeters. Assume the units are in inches unless the source explicitly states "mm" or "millimeters".
    * The CBTO value is identified with the string CBTO. This identifier might appear after a numeric value or before for the numeric value.  For example 2.263" CBTO or CBTO 2.263".
    * the case trim_length is in inches or millimeters. Assume the units are in inches unless the source explicitly states "mm" or "millimeters". The value is a decimal number with up to 3 decimal places. If missing, assume 0.
    * the case manufacturer is the company that produced the brass case. If unknown, use "unknown". 
    * the case primer is the manufacturer and type of primer as a single descriptive string. If unknown, use "unknown".
    * original_string is the original load data string that was parsed. THe original string must be JSON escaped so that it can be included in the JSON output.
    * comments is a free-form string that may contain any additional information that was not captured in the other fields. If there are no comments, use an empty string.


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
HttpResponseMessage resp    = await http.PostAsync(ollamaUrl, content);

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
    string pretty = JsonSerializer.Serialize(parsed.RootElement,
                                             new JsonSerializerOptions { WriteIndented = true });

    Console.WriteLine(pretty);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error parsing JSON: {ex.Message}");
    Console.WriteLine(reply);
    return 1;
}