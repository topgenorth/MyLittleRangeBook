using System.Threading.Tasks;
using MyLittleRangeBook.GUI.Models;

namespace MyLittleRangeBook.GUI.Services
{
    public interface ISimpleRangeEventService
    {
        Task<bool> SaveRangeEventAsync(SimpleRangeEvent rangeEvent);
        Task<bool> DeleteRangeEvent(SimpleRangeEvent rangeEvent);
    }
}
