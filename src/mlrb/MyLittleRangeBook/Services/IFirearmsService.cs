using System.Data;
using FluentResults;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Services
{
    public interface IFirearmsService
    {
        Task<Result<bool>> DeleteAsync(IDbConnection connection,
            Firearm firearm,
            CancellationToken cancellationToken = default);

        Task<Result<long?>> UpsertAsync(IDbConnection connection,
            Firearm firearm,
            CancellationToken cancellationToken = default);

        Task<Result<IEnumerable<Firearm>>> GetFirearmsAsync(IDbConnection connection,
            CancellationToken cancellationToken = default);
    }
}
