using System.Threading.Tasks;
using MyLittleRangeBook.GUI.Models;

namespace MyLittleRangeBook.GUI.Services
{
    public interface IFirearmsService
    {
        Task<bool> SaveFirearmAsync(Firearm firearm);
        Task<bool> DeleteFirearmEvent(Firearm firearm);
    }
}
