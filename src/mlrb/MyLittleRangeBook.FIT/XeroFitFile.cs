using FluentResults;

namespace MyLittleRangeBook.FIT
{
    public class XeroFitFile
    {
        public XeroFitFile(string fileName, ReadOnlyMemory<byte> contents)
        {
            FileName = fileName;
            Contents = contents;
        }

        public string               FileName { get; }
        public ReadOnlyMemory<byte> Contents { get; }


        /// <summary>
        ///     Will load the contents of the file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<Result<XeroFitFile>> New(string fileName, CancellationToken ct = default)
        {
            try
            {
                byte[]      contents = await File.ReadAllBytesAsync(fileName, ct).ConfigureAwait(false);
                XeroFitFile x        = new(fileName, contents);

                return Result.Ok(x);
            }
            catch (OperationCanceledException oce)
            {
                Error? err = new Error($"Operation was cancelled by user; did not read  {fileName}").CausedBy(oce);

                return Result.Fail(err);
            }
            catch (Exception e)
            {
                return Result.Fail(new Error($"Failed to read file {fileName}: {e.Message}").CausedBy(e));
            }
        }


        public override string ToString() => FileName;
    }
}