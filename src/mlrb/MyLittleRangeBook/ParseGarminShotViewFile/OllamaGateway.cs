using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MyLittleRangeBook
{
    /// <summary>
    ///     A gateway that will combine a prompt with the ShotView CSV file and send it to Ollama.
    /// </summary>
    public class OllamaGateway
    {
        internal const string  LOCAL_OLLAMA_URL       = "http://localhost:11434/api/chat";
        internal const string  DEFAULT_MODEL          = "llama3.2";
        internal const int     OLLAMA_TIMEOUT_SECONDS = 400;
        readonly       ILogger _logger;

        public OllamaGateway(ILogger logger) => _logger = logger;


        public static async Task<Result<string>> LoadPrompt() => "";

        /// <summary>
        ///     Send a request to Ollama to change the Garmin ShotView CSV file into a JSON structure.
        /// </summary>
        /// <param name="fileContents"></param>
        /// <param name="prompt"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public async Task<Result<string>> TransmorgifyShotViewCsv(string            fileContents,
                                                                  string            promptTemplate,
                                                                  CancellationToken cancellationToken = default)
        {
            string ollamaResponse = await SendRequestToOllama(fileContents, promptTemplate, cancellationToken);

            using JsonDocument doc = JsonDocument.Parse(ollamaResponse);
            string reply = doc.RootElement.GetProperty("message")
                              .GetProperty("content")
                              .GetString()!;
            // ── clean up & validate ─────────────────────────────────────────────
            // LLMs sometimes wrap the JSON in markdown fences; strip them.
            reply = reply.Trim();
            return Result.Ok(reply);
        }

        async Task<string> SendRequestToOllama(string            fileContents, string promptTemplate,
                                               CancellationToken cancellationToken)
        {
            // [TO20260914] Build the Ollama request body.
            string prompt = new StringBuilder(promptTemplate).Append(fileContents).ToString();
            var request = new
                          {
                              model  = DEFAULT_MODEL,
                              stream = false,
                              messages = new[]
                                         {
                                             new
                                             {
                                                 role = "system", content = "You output only raw JSON. No code fences.",
                                             },
                                             new { role = "user", content = prompt },
                                         },
                          };
#pragma warning disable IL2026, IL3050
            string ollamaRequest = JsonSerializer.Serialize(request);
#pragma warning restore IL2026, IL3050


            // [TO20260914] Send the request to Ollama
            using HttpClient http    = new() { Timeout = TimeSpan.FromSeconds(OLLAMA_TIMEOUT_SECONDS) };
            StringContent    content = new(ollamaRequest, Encoding.UTF8, "application/json");
            _logger.Debug("Sending request to Ollama.");
            Stopwatch stopwatch = Stopwatch.StartNew();
            HttpResponseMessage resp = await http.PostAsync(LOCAL_OLLAMA_URL, content, cancellationToken)
                                                 .ConfigureAwait(false);

            string             x   = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using JsonDocument doc = JsonDocument.Parse(x);
            stopwatch.Stop();
            double requestTimeSeconds = stopwatch.Elapsed.TotalSeconds;
            _logger.Debug("Received the request from Ollama.");
            _logger.Debug($"Request time: {requestTimeSeconds} seconds");
            return x;
        }

        /// <summary>
        ///     Copies a JSON element and writes it to a specified writer, adding or updating metadata fields based on the provided
        ///     model name and request time.
        /// </summary>
        /// <param name="element">The source JSON element to be copied and potentially modified.</param>
        /// <param name="writer">The target Utf8JsonWriter where the JSON will be written.</param>
        /// <param name="modelName">The name of the model to include in the metadata.</param>
        /// <param name="requestTime">The request processing time to include in the metadata.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a JSON value kind is unsupported or unhandled.</exception>
        void CopyJsonWithUpdatedMetadata(JsonElement element, Utf8JsonWriter writer, string modelName,
                                         double      requestTime)
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
                            {
                                foreach (JsonProperty metaProp in property.Value.EnumerateObject())
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
                case JsonValueKind.Undefined:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}