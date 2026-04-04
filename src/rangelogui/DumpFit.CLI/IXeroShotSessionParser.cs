using FluentResults;
using MyLittleRangeBook.Cli.Model;

namespace MyLittleRangeBook.Cli
{
    public interface IXeroShotSessionParser
    {
        /// <summary>
        /// Decodes a FIT file into a ShotSession.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Result<ShotSession>> DecodeShotSessionAsync(string filePath, CancellationToken ct);

        Result<ShotSession> Decode(Stream input);
    }
}
