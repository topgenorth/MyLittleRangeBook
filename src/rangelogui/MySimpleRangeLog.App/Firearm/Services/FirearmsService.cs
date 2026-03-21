using System.Threading.Tasks;
using MySimpleRangeLog.Models;

namespace MySimpleRangeLog.Services
{
    public class FirearmsService : IFirearmsService
    {
        public Task<bool> SaveFirearmAsync(Firearm firearm)
        {
            return firearm.SaveAsync();
        }

        public Task<bool> DeleteFirearmEvent(Firearm firearm)
        {
            return firearm.DeleteAsync();
        }
    }
}
