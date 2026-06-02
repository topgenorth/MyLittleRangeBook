using System.Security.Cryptography;
using System.Text;

namespace MyLittleRangeBook.IO
{
    public static class FileExtensions
    {
        const int CopyFileBufferSize = 81920;
        public const string MIME_TYPE_GARMIN_FIT_FILE = "application/vnd.mlrb.garmin-fit";
        public const string MIME_TYPE_GARMIN_SHOTVIEW_FILE = "application/vnd.mlrb.garmin-shotview+csv";
        public const string MIME_TYPE_GORDONS_RELOADING_TOOL_LOAD_FILE = "application/vnd.mlrb.grt-load+xml";
        public const string MIME_TYPE_QUICKLOAD_DATA_FILE = "application/vnd.quickload.dat";
        public const string MIME_TYPE_LABRADAR_SERIES_FILE = "application/vnd.labradar.series+csv";

        public static async Task<Result<ReadOnlyMemory<byte>>> LoadFileBytesAsync(this FileInfo fitFile,
            CancellationToken ct = default)
        {
            if (!fitFile.Exists)
            {
                return Result.Fail<ReadOnlyMemory<byte>>(new FailedToLoadFileError(fitFile.FullName))
                    .WithError(new FailedToLoadFileError(fitFile.FullName));
            }


            string filename = fitFile.FullName;

            return await filename.LoadFileBytesAsync(ct).ConfigureAwait(false);
        }

        public static async Task<string> ComputeSha256HashAsync(this FileInfo file, CancellationToken ct = default)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException($"File not found: {file.FullName}");
            }

            return await ComputeSha256HashAsync(file.FullName, ct).ConfigureAwait(false);
        }

        public static async Task<string> ComputeSha256HashAsync(string path,
            CancellationToken cancellationToken = default)
        {
            await using FileStream stream = File.OpenRead(path);
            using var sha256 = SHA256.Create();

            byte[] hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);

            var sb = new StringBuilder(64);
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Asynchronously reads the content of a file and returns its bytes wrapped in a result object.
        /// </summary>
        /// <param name="filename">The full path of the file to be read.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the file read operation.</param>
        /// <returns>
        ///     A result object containing the read-only memory representation of the file bytes if the operation succeeds.
        ///     If the operation fails or is canceled, the result object will contain an error with details about the failure.
        /// </returns>
        public static async Task<Result<ReadOnlyMemory<byte>>> LoadFileBytesAsync(this string filename,
            CancellationToken ct)
        {
            try
            {
                byte[] result = await File.ReadAllBytesAsync(filename, ct).ConfigureAwait(false);

                return Result.Ok<ReadOnlyMemory<byte>>(result);
            }
            catch (OperationCanceledException oce)
            {
                Error? err = new Error($"Operation was cancelled by user; did not read  {filename}").CausedBy(oce);

                return Result.Fail<ReadOnlyMemory<byte>>(err);
            }
            catch (Exception e)
            {
                return Result.Fail<ReadOnlyMemory<byte>>(new Error($"Failed to read file {filename}: {e.Message}")
                    .CausedBy(e));
            }
        }

        /// <summary>
        ///     Asynchronously reads the content of a text file and returns its content as a string wrapped in a result object.
        /// </summary>
        /// <param name="filename">The full path of the text file to be read.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the text file read operation.</param>
        /// <returns>
        ///     A result object containing the string representation of the text file's content if the operation succeeds.
        ///     If the operation fails or is canceled, the result object will contain an error with details about the failure.
        /// </returns>
        public static async Task<Result<string>> LoadFileTextAsync(this string filename,
            CancellationToken ct)
        {
            try
            {
                string result = await File.ReadAllTextAsync(filename, ct).ConfigureAwait(false);

                return Result.Ok(result);
            }
            catch (OperationCanceledException oce)
            {
                Error? err = new Error($"Operation was cancelled by user; did not read  {filename}").CausedBy(oce);

                return Result.Fail<string>(err);
            }
            catch (Exception e)
            {
                return Result.Fail<string>(new Error($"Failed to read file {filename}: {e.Message}").CausedBy(e));
            }
        }

        /// <summary>
        ///     Determines the MIME type based on the file extension provided.
        /// </summary>
        /// <param name="extension">The file extension, including the leading period (e.g., ".jpg", ".png").</param>
        /// <returns>
        ///     The MIME type that corresponds to the given file extension.
        ///     Returns "application/octet-stream" if the extension is not recognized.
        /// </returns>
        public static string GetMimeType(string extension)
        {
            // TODO [TO20260601] Maybe this should be a MimeTypeService and a config file/embedded resource?
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".csv" => "text/csv", // [TO20260601] Might also be MIME_TYPE_GARMIN_SHOTVIEW_FILE or MIME_TYPE_LABRADAR_SERIES_FILE
                ".xml" => "application/xml",
                ".grtload" => MIME_TYPE_GORDONS_RELOADING_TOOL_LOAD_FILE,
                ".fit" => MIME_TYPE_GARMIN_FIT_FILE,
                ".dat" => MIME_TYPE_QUICKLOAD_DATA_FILE,
                _ => "application/octet-stream"
            };
        }
        public static async Task CopyFileAsync(string sourceFile,
            string destinationFile,
            CancellationToken cancellationToken)
        {
            await using var source = new FileStream(
                sourceFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                CopyFileBufferSize,
                FileOptions.Asynchronous);

            await using var destination = new FileStream(
                destinationFile,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                CopyFileBufferSize,
                FileOptions.Asynchronous);

            await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
            await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
