namespace net.opgenorth.xero.ImportFitFiles;

/// <summary>
///     This will copy files from the source directory to the destination directory. It will skip any files that already
///     exist.
/// </summary>
public class ImportFitFile
{
    private readonly ILogger _logger;

    public ImportFitFile(ILogger logger) => _logger = logger.ForContext<ImportFitFile>();

    /// <summary>
    ///     Copies new FIT from the source directory to the target directory.
    /// </summary>
    /// <param name="sourceDir"></param>
    /// <param name="targetDir"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> Import(string sourceDir, string targetDir, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.Debug("Cancellation is requested - no work will be done.");

            return 0;
        }

        int copiedCount = 0;
        int skippedCount = 0;
        foreach (string filename in Directory.EnumerateFiles(sourceDir))
        {
            (skippedCount, copiedCount) = await AddNewFilesToDirectory(targetDir, cancellationToken, filename,
                skippedCount, copiedCount);
        }

        _logger.Information(
            "Imported {FileCount}from {SourceDir} to {DestinationDir}, skipped {SkippedCount} files.", copiedCount,
            sourceDir, targetDir, skippedCount);

        return 0;
    }

    private async Task<(int skippedCount, int copiedCount)> AddNewFilesToDirectory(string targetDir,
        CancellationToken cancellationToken,
        string filename,
        int skippedCount,
        int copiedCount)
    {
        if (!"FIT".Equals(Path.GetExtension(filename), StringComparison.OrdinalIgnoreCase))
        {
            skippedCount++;
            _logger.Verbose("Not a FIT file, ignoring {file}", filename);

            return (skippedCount, copiedCount);
        }

        string sourceFile = Path.GetFileName(filename);
        string destinationFile = Path.Combine(targetDir, sourceFile);

        if (File.Exists(destinationFile))
        {
            skippedCount++;
            _logger.Verbose("File {Destination} exists, moving on to the next file", destinationFile);

            return (skippedCount, copiedCount);
        }

        await using FileStream destinationStream = File.Create(destinationFile);
        await using FileStream sourceStream = File.Open(filename, FileMode.Open);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
        _logger.Verbose("Copied file {File} to {Destination}", filename, destinationFile);
        copiedCount++;

        return (skippedCount, copiedCount);
    }
}
