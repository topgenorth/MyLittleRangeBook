using FluentResults;
using MyLittleRangeBook.IO;

namespace MyLittleRangeBook.FIT
{
    public class XeroFitFile
    {
        public XeroFitFile(string fileName, ReadOnlyMemory<byte> contents)
        {
            FileName = fileName;
            Contents = contents;
        }

        public string FileName { get; }
        public ReadOnlyMemory<byte> Contents { get; }


        /// <summary>
        ///     Will load the contents of the file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<XeroFitFile> New(string fileName, CancellationToken cancellationToken = default)
        {
            Result<ReadOnlyMemory<byte>> result = await fileName.LoadFileBytesAsync(cancellationToken);

            return result.IsSuccess
                ? new XeroFitFile(fileName, result.Value)
                : throw new InvalidOperationException($"Failed to load file '{fileName}'");
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
