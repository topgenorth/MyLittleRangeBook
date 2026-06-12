using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleAppFramework;
using MyLittleRangeBook.Console;

namespace MyLittleRangeBook
{
    [RegisterCommands("update")]
    public class UpdateCommands : MlrbCommandBase
    {
        private const string WorkflowName = "Build MyLittleRangeBook";

        public UpdateCommands(ILogger logger, ICliDisplay cliDisplay)
            : base(logger, cliDisplay)
        {
        }

        /// <summary>
        /// Updates the mlrb executable from the latest GitHub artifacts.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("from-artifacts")]
        public async Task<int> UpdateAsync(CancellationToken ct = default)
        {
            CliDisplay.PrintCommandHeader("Update MyLittleRangeBook from Artifacts");

            if (!await IsGhInstalledAsync(ct).ConfigureAwait(false))
            {
                CliDisplay.PrintFailure("GitHub CLI (gh) is required but was not found in PATH.");
                return ReturnCodes.FAILURE;
            }

            string? repoRoot = await GetRepoRootAsync(ct).ConfigureAwait(false);
            if (string.IsNullOrEmpty(repoRoot))
            {
                CliDisplay.PrintFailure("This command must be run from inside the MyLittleRangeBook git repository.");
                return ReturnCodes.FAILURE;
            }

            string artifactPattern;
            string destinationDir;
            string executableName;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                artifactPattern = "mlrb-*-windows-executables";
                destinationDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".bin");
                executableName = "mlrb.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                artifactPattern = "*-linux-executables";
                destinationDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin");
                executableName = "mlrb";
            }
            else
            {
                CliDisplay.PrintFailure("Unsupported platform. This command supports Windows and Linux only.");
                return ReturnCodes.FAILURE;
            }

            CliDisplay.Console.MarkupLine($"[blue]Finding latest successful run for workflow '{WorkflowName}'...[/]");
            string? runId = await GetLatestRunIdAsync(WorkflowName, repoRoot, ct).ConfigureAwait(false);
            if (string.IsNullOrEmpty(runId) || runId == "null")
            {
                CliDisplay.PrintFailure($"Could not find a successful run for workflow '{WorkflowName}'.");
                return ReturnCodes.FAILURE;
            }
            CliDisplay.Console.MarkupLine($"[green]Found run ID: {runId}[/]");

            CliDisplay.Console.MarkupLine($"[blue]Finding newest artifact matching '{artifactPattern}' in run {runId}...[/]");
            string? newestArtifact = await GetNewestArtifactNameAsync(runId, artifactPattern, repoRoot, ct).ConfigureAwait(false);
            if (string.IsNullOrEmpty(newestArtifact))
            {
                CliDisplay.PrintFailure($"Could not find any artifacts matching pattern '{artifactPattern}' in run {runId}.");
                return ReturnCodes.FAILURE;
            }
            CliDisplay.Console.MarkupLine($"[green]Found newest artifact: {newestArtifact}[/]");

            string tempRoot = Path.Combine(Path.GetTempPath(), "mlrb-cli-install");
            try
            {
                if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Failed to clean up temporary directory {tempRoot}", tempRoot);
            }
            
            string downloadDir = Path.Combine(tempRoot, "download");
            Directory.CreateDirectory(downloadDir);

            CliDisplay.Console.MarkupLine($"[blue]Downloading artifact to {downloadDir}...[/]");
            if (!await DownloadArtifactAsync(runId, newestArtifact, downloadDir, repoRoot, ct).ConfigureAwait(false))
            {
                CliDisplay.PrintFailure("Failed to download artifact.");
                return ReturnCodes.FAILURE;
            }

            string? downloadedExecutable = Directory.GetFiles(downloadDir, executableName, SearchOption.AllDirectories).FirstOrDefault();
            if (string.IsNullOrEmpty(downloadedExecutable))
            {
                CliDisplay.PrintFailure($"Could not find {executableName} in downloaded artifact.");
                return ReturnCodes.FAILURE;
            }

            Directory.CreateDirectory(destinationDir);
            string destinationPath = Path.Combine(destinationDir, executableName);
            CliDisplay.Console.MarkupLine($"[blue]Installing to {destinationPath}...[/]");

            try
            {
                if (File.Exists(destinationPath))
                {
                    // Move the existing executable to a .old file to avoid "Text file busy" (Linux) or access errors (Windows)
                    string oldPath = destinationPath + ".old";
                    try
                    {
                        if (File.Exists(oldPath)) File.Delete(oldPath);
                        File.Move(destinationPath, oldPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex, "Failed to move existing executable to {oldPath}. Attempting direct overwrite.", oldPath);
                    }
                }
                
                File.Copy(downloadedExecutable, destinationPath, true);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    await RunProcessAsync("chmod", $"+x \"{destinationPath}\"", ct, repoRoot);
                }
            }
            catch (Exception ex)
            {
                CliDisplay.PrintFailure($"Failed to copy executable: {ex.Message}");
                return ReturnCodes.FAILURE;
            }

            CliDisplay.Console.MarkupLine("[blue]Migrating database and running maintenance tasks...[/]");
            
            // Run post-update tasks using the newly installed executable
            await RunProcessAsync(destinationPath, "db migrate", ct, repoRoot, true);
            await RunProcessAsync(destinationPath, "firearms recalculate-round-count", ct, repoRoot, true);
            await RunProcessAsync(destinationPath, "db maintenance", ct, repoRoot, true);

            CliDisplay.PrintSuccess($"Installed {executableName} to: {destinationPath}");
            return ReturnCodes.SUCCESS;
        }

        private async Task<bool> IsGhInstalledAsync(CancellationToken ct)
        {
            try
            {
                var result = await RunProcessAsync("gh", "--version", ct);
                return result.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string?> GetRepoRootAsync(CancellationToken ct)
        {
            try
            {
                var result = await RunProcessAsync("git", "rev-parse --show-toplevel", ct);
                return result.ExitCode == 0 ? result.Output.Trim() : null;
            }
            catch
            {
                return null;
            }
        }

        private async Task<string?> GetLatestRunIdAsync(string workflowName, string repoRoot, CancellationToken ct)
        {
            var result = await RunProcessAsync("gh", $"run list --workflow \"{workflowName}\" --limit 1 --json databaseId --jq \".[0].databaseId\"", ct, repoRoot);
            return result.Output.Trim();
        }

        private async Task<string?> GetNewestArtifactNameAsync(string runId, string pattern, string repoRoot, CancellationToken ct)
        {
            var result = await RunProcessAsync("gh", $"api \"repos/{{owner}}/{{repo}}/actions/runs/{runId}/artifacts\" --jq \".artifacts | sort_by(.created_at) | reverse | map(.name) | .[]\"", ct, repoRoot).ConfigureAwait(false);
            var names = result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            var regexPattern = "^" + pattern.Replace("*", ".*") + "$";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            return names.FirstOrDefault(n => regex.IsMatch(n));
        }

        private async Task<bool> DownloadArtifactAsync(string runId, string artifactName, string downloadDir, string repoRoot, CancellationToken ct)
        {
            var result = await RunProcessAsync("gh", $"run download {runId} --name \"{artifactName}\" --dir \"{downloadDir}\"", ct, repoRoot).ConfigureAwait(false);
            return result.ExitCode == 0;
        }

        private async Task<(int ExitCode, string Output)> RunProcessAsync(string fileName, string arguments, CancellationToken ct, string? workingDirectory = null, bool echoOutput = false)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
            };

            using var process = new Process();
            process.StartInfo = psi;
            var output = new StringBuilder();
            
            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    output.AppendLine(e.Data);
                    if (echoOutput) CliDisplay.Console.WriteLine(e.Data);
                }
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    if (echoOutput) CliDisplay.Console.MarkupLine($"[red]{Markup.Escape(e.Data)}[/]");
                }
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(ct).ConfigureAwait(false);

                return (process.ExitCode, output.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to run process {fileName}", fileName);
                return (-1, string.Empty);
            }
        }
    }
}
