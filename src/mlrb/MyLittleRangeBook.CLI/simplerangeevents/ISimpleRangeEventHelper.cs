using System.Runtime.CompilerServices;
using FluentResults;

namespace MyLittleRangeBook.CLI
{
    public interface ISimpleRangeEventHelper
    {
        Task<Result<(List<string>, List<string>)>> GetFirearmsAndRangesAsync(
            CancellationToken cancellationToken);

        Task<Result<List<string>>> GetAmmoDescriptionsForFirearmAsync(string firearmName,
            CancellationToken cancellationToken = default);
    }
}
