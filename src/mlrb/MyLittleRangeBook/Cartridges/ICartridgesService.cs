using System.Data;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Cartridges
{
    public interface ICartridgesService
    {
        Task<Result<bool>> DeleteAsync(IDbConnection connection,
            Cartridge cartridge,
            CancellationToken cancellationToken = default);

        Task<Result<EntityId>> UpsertAsync(IDbConnection connection,
            Cartridge cartridge,
            CancellationToken cancellationToken = default);

        Task<Result<IEnumerable<Cartridge>>> GetCartridgesAsync(IDbConnection connection,
            bool activeOnly = true,
            CancellationToken cancellationToken = default);

        Task<Result<Cartridge>> GetCartridgeAsync(IDbConnection connection,
            string id,
            CancellationToken cancellationToken = default);
    }
}
