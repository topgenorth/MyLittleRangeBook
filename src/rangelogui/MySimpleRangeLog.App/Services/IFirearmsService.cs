using System.Threading.Tasks;
using MyLittleRangeBook.Gui.Models;

namespace MyLittleRangeBook.Gui.Services
{
    public interface IFirearmsService
    {
        Task<bool> SaveFirearmAsync(Firearm firearm);
        Task<bool> DeleteFirearmEvent(Firearm firearm);
    }
}
