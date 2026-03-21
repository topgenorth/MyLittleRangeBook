using System.Threading.Tasks;
using MySimpleRangeLog.Models;

namespace MySimpleRangeLog.Services
{
    public interface IFirearmsService
    {
        Task<bool> SaveFirearmAsync(Firearm firearm);
        Task<bool> DeleteFirearmEvent(Firearm firearm);
    }
}
