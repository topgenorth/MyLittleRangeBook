using System.Data;
using FluentResults;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Services
{
    using ShotViewFileData = (EntityId EntityId, string FileName, string contents);

    public interface IShotViewFilesDbService
    {
        /// <summary>
        ///     Retrieves ShotView CSV file from the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        ///     A tuple that will hold the <c cref="EntityId" />, the filename of the ShotView file, and the contents of the
        ///     file.
        /// </returns>
        Task<Result<ShotViewFileData>> GetShotViewFileAsync(IDbConnection connection,
            string id,
            CancellationToken cancellationToken = default);

        Task<Result> DeleteShotViewFileAsync(IDbConnection connection,
            string id,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Adds or updates a ShotView CSV file in the database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="contents"></param>
        /// <param name="fileName">If null or empty string, then a synthetic filename will be created.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result<EntityId>> UpsertShotViewFileAsync(IDbConnection connection,
            string id,
            string contents,
            string? fileName,
            CancellationToken cancellationToken = default);

        Task<Result<long?>> AssociateWithRangeEvent(IDbConnection connection,
            string rangeEventId,
            string shotViewFileId,
            CancellationToken cancellationToken = default);
    }
}
