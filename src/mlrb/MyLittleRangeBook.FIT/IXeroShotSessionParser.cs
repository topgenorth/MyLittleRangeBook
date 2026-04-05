using FluentResults;
using MyLittleRangeBook.FIT.Model;

namespace MyLittleRangeBook.FIT
{
    public interface IXeroShotSessionParser
    {
        /// <summary>
        ///     Decodes a FIT file into a ShotSession.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Result<ShotSession>> DecodeFITFileAsync(string filePath, CancellationToken ct);

        /// <summary>
        /// Decodes a FIT byte stream into a ShotSession.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Result<ShotSession> Decode(Stream input);
    }
}
