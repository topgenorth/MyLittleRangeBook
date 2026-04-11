using System.Threading;
using System.Threading.Tasks;
using MyLittleRangeBook.GUI.Models;

namespace MyLittleRangeBook.GUI.Services
{
    public interface IFirearmsService
    {
        Task<bool> SaveFirearmAsync(Firearm firearm, CancellationToken cancellationToken);
        Task<bool> DeleteFirearmEvent(Firearm firearm, CancellationToken cancellationToken);
    }
}
