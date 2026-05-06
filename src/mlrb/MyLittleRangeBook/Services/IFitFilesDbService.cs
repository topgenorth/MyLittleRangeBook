using System.Data;
using FluentResults;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Services
{
    using FitFileData = (EntityId EntityId, string FileName, ReadOnlyMemory<byte> contents);

    public interface IFitFilesDbService
    {
        /// <summary>
        ///     Retrieves FIT file from the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A tuple that will hold the <c cref="EntityId" />, the filename of the FIT file, and the contents of the file.</returns>
        Task<Result<FitFileData>> GetFitFileAsync(IDbConnection connection,
            string id,
            CancellationToken cancellationToken = default);

        Task<Result> DeleteFitFileAsync(IDbConnection connection,
            string id,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="contents"></param>
        /// <param name="fileName">If null or empty string, then a synethic filename will be created.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<EntityId>> UpsertFitFileAsync(IDbConnection connection,
            string id,
            ReadOnlyMemory<byte> contents,
            string? fileName,
            CancellationToken cancellationToken = default);

        Task<Result<long?>> AssociateWithRangeEvent(IDbConnection connection, string rangeEventId, string fitFileId, CancellationToken cancellationToken = default);
    }
}
