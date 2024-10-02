namespace net.opgenorth.xero.ImportFit
{
    /// <summary>
    /// This will copy files from the source directory to the destination directory. It will skip any files that already exist.
    /// </summary>
    public class ImportFitFile
    {
        readonly ILogger _logger;

        public ImportFitFile(ILogger logger)
        {
            _logger = logger.ForContext<ImportFitFile>();
        }

        public async Task<int> Import(string sourceDir, string targetDir, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.Debug("Cancellation is requested - no work will be done.");
                return 0;
            }
            var copiedCount = 0;
            var skippedCount = 0;           
            foreach (var filename in Directory.EnumerateFiles(sourceDir))
            {
  
                var sourceFile = Path.GetFileName(filename);
                var destinationFile = Path.Combine(targetDir, sourceFile);

                if (File.Exists(destinationFile))
                {
                    skippedCount++;
                    _logger.Verbose("File {Destination} exists, moving on to the next file", destinationFile);
                    continue;
                }

                await using var destinationStream = File.Create(destinationFile);
                await using var sourceStream = File.Open(filename, FileMode.Open);
                await sourceStream.CopyToAsync(destinationStream, cancellationToken);
                _logger.Verbose("Copied file {File} to {Destination}", filename, destinationFile);
                copiedCount++;
            }

            _logger.Information("Imported {FileCount}from {SourceDir} to {DestinationDir}, skipped {SkippedCount} files.", copiedCount, sourceDir, targetDir, skippedCount);
            
            return 0;
        }
    }
}
