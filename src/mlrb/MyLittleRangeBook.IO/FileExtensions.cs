namespace MyLittleRangeBook.IO
{
    public static class FileExtensions
    {
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
                return Result.Fail<ReadOnlyMemory<byte>>(new Error($"Failed to read file {filename}: {e.Message}").CausedBy(e));
            }
        }

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
    }
}
