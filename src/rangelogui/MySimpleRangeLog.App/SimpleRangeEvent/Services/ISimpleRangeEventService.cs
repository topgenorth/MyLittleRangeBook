using System.Threading.Tasks;
using MySimpleRangeLog.Models;

namespace MySimpleRangeLog.Services
{
    public interface ISimpleRangeEventService
    {
        Task<bool> SaveRangeEventAsync(SimpleRangeEvent rangeEvent);
        Task<bool> DeleteRangeEvent(SimpleRangeEvent rangeEvent);
    }
}
