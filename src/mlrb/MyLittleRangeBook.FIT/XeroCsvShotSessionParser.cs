using System.Globalization;
using ByteAether.Ulid;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;
using MyLittleRangeBook.FIT.Model;

namespace MyLittleRangeBook.FIT
{
    public class XeroCsvShotSessionParser : IXeroCsvShotSessionParser
    {
        readonly ILogger _logger;

        public XeroCsvShotSessionParser(ILogger logger) => _logger = logger.ForContext<XeroCsvShotSessionParser>();

        public async Task<bool> IsShotViewCsvAsync(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                if (Path.GetExtension(filePath).ToLowerInvariant() != ".csv")
                {
                    return false;
                }

                using StreamReader reader = new(filePath);
                string?            firstLine = await reader.ReadLineAsync(cancellationToken);
                if (firstLine == null)
                {
                    return false;
                }

                string? secondLine = await reader.ReadLineAsync(cancellationToken);
                if (secondLine == null)
                {
                    return false;
                }

                // Check for characteristic headers in the second line
                return (secondLine.Contains("Speed (FPS)", StringComparison.OrdinalIgnoreCase) ||
                        secondLine.Contains("Speed (MPS)", StringComparison.OrdinalIgnoreCase)) &&
                       secondLine.Contains("KE (FT-LBS)",      StringComparison.OrdinalIgnoreCase) &&
                       secondLine.Contains("Power Factor",     StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public async Task<Result<ShotSession>> ParseCsvFileAsync(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return Result.Fail($"File not found: {filePath}");
                }

                using StreamReader reader = new(filePath);

                // Read the first line which contains Projectile info
                // Example: Rifle Bullet 36.0 gr
                string? firstLine = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(firstLine))
                {
                    return Result.Fail("CSV file is empty.");
                }

                ShotSession session = new(Guid.NewGuid().ToString());
                session.FileName = Path.GetFileName(filePath);

                ParseFirstLine(firstLine, session);

                CsvConfiguration config = new(CultureInfo.InvariantCulture)
                                          {
                                              HasHeaderRecord       = true,
                                              PrepareHeaderForMatch = args => args.Header.ToLower(),
                                              MissingFieldFound     = null,
                                              HeaderValidated       = null,
                                          };

                using CsvReader csv = new(reader, config);
                await csv.ReadAsync();
                csv.ReadHeader();

                string speedHeader =
                    (csv.HeaderRecord ?? [])
                   .FirstOrDefault(h => h.Contains("Speed", StringComparison.OrdinalIgnoreCase)) ?? "Speed (FPS)";
                string velocityUnits =
                    speedHeader.Contains("(MPS)", StringComparison.OrdinalIgnoreCase) ? "m/s" : "fps";
                session.VelocityUnits = velocityUnits;

                List<Shot> shots        = new();
                bool       readingShots = true;

                while (await csv.ReadAsync())
                {
                    if (readingShots)
                    {
                        string? firstField = csv.GetField(0);
                        if (string.IsNullOrWhiteSpace(firstField) || firstField == "-")
                        {
                            readingShots = false;
                            continue;
                        }

                        if (int.TryParse(firstField, out int shotNumber))
                        {
                            Shot shot = new()
                                        {
                                            ShotNumber = shotNumber,
                                            Speed = new ShotSpeed((double)ParseDecimal(csv.GetField(speedHeader)),
                                                                  velocityUnits),
                                            Notes = csv.GetField("Shot Notes"),
                                        };

                            string? timeStr = csv.GetField("Time");
                            // Full date/time for each shot is not in CSV, but we can combine with session date if needed.

                            string? cleanBore = csv.GetField("Clean Bore");
                            shot.CleanBore = !string.IsNullOrWhiteSpace(cleanBore) &&
                                             (cleanBore.Equals("yes",  StringComparison.OrdinalIgnoreCase) ||
                                              cleanBore.Equals("true", StringComparison.OrdinalIgnoreCase));

                            string? coldBore = csv.GetField("Cold Bore");
                            shot.ColdBore = !string.IsNullOrWhiteSpace(coldBore) &&
                                            (coldBore.Equals("yes",  StringComparison.OrdinalIgnoreCase) ||
                                             coldBore.Equals("true", StringComparison.OrdinalIgnoreCase));

                            session.AddShot(shot);
                        }
                    }
                    else
                    {
                        // Summary rows
                        string? label = csv.GetField(0);
                        string? value = csv.GetField(1);

                        if (string.IsNullOrWhiteSpace(label))
                        {
                            continue;
                        }

                        if (label.Contains("AVERAGE SPEED", StringComparison.OrdinalIgnoreCase))
                        {
                            session.AverageSpeed = (int)ParseDecimal(value);
                        }
                        else if (label.Contains("STD DEV", StringComparison.OrdinalIgnoreCase))
                        {
                            session.StandardDeviation = (double)ParseDecimal(value);
                        }
                        else if (label.Contains("SPREAD", StringComparison.OrdinalIgnoreCase))
                        {
                            session.ExtremeSpread = (int)ParseDecimal(value);
                        }
                        else if (label.Contains("Projectile Weight", StringComparison.OrdinalIgnoreCase))
                        {
                            session.ProjectileWeight = (int)ParseDecimal(value);
                        }
                        else if (label.Contains("Session Note", StringComparison.OrdinalIgnoreCase))
                        {
                            session.Notes = value ?? string.Empty;
                        }
                    }
                }

                // Attempt to parse date from filename if possible: Rifle_Bullet_2026-07-05_13-30-54.csv
                string   fileName   = Path.GetFileNameWithoutExtension(filePath);
                string[] parts      = fileName.Split('_');
                bool     dateParsed = false;
                if (parts.Length >= 2)
                {
                    string datePart = parts[^2];
                    string timePart = parts[^1].Replace("-", ":");
                    if (DateTimeOffset.TryParseExact($"{datePart} {timePart}", "yyyy-MM-dd HH:mm:ss",
                                                     CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal,
                                                     out DateTimeOffset dt))
                    {
                        session.DateTimeUtc = dt;
                        session.TimeCreated = dt;
                        dateParsed          = true;
                    }
                }

                if (!dateParsed)
                {
                    // Fallback to file creation time
                    DateTime creationTime = File.GetCreationTimeUtc(filePath);
                    session.DateTimeUtc = creationTime;
                    session.TimeCreated = creationTime;
                }

                session.Id = Ulid.New(session.TimeCreated).ToString();

                return Result.Ok(session);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to parse Xero CSV file {filePath}", filePath);
                return Result.Fail(new Error("Failed to parse Xero CSV file").CausedBy(ex));
            }
        }

        void ParseFirstLine(string line, ShotSession session)
        {
            // Example: Rifle Bullet 36.0 gr
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return;
            }

            int weightIndex = -1;
            for (int i = 0; i < parts.Length; i++)
            {
                if (double.TryParse(parts[i], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    weightIndex = i;
                    break;
                }
            }

            if (weightIndex > 0)
            {
                session.ProjectileType = string.Join(" ", parts.Take(weightIndex));
                if (double.TryParse(parts[weightIndex], NumberStyles.Any, CultureInfo.InvariantCulture,
                                    out double weight))
                {
                    session.ProjectileWeight = (int)weight;
                }

                if (parts.Length > weightIndex + 1)
                {
                    session.ProjectileUnits = parts[weightIndex + 1];
                }
            }
            else
            {
                session.ProjectileType = line.Trim();
            }
        }

        decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            // Remove quotes and commas
            string sanitized = value.Replace("\"", "").Replace(",", "");
            if (decimal.TryParse(sanitized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            return 0;
        }
    }
}