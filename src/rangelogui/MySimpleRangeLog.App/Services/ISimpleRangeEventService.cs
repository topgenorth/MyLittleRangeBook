using System.Threading.Tasks;
using MyLittleRangeBook.Gui.Models;

namespace MyLittleRangeBook.Gui.Services
{
    public interface ISimpleRangeEventService
    {
        Task<bool> SaveRangeEventAsync(SimpleRangeEvent rangeEvent);
        Task<bool> DeleteRangeEvent(SimpleRangeEvent rangeEvent);
    }
}
