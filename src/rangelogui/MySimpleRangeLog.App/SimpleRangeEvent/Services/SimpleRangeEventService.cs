using System.Threading.Tasks;
using MySimpleRangeLog.Models;

namespace MySimpleRangeLog.Services
{
    public class SimpleRangeEventService : ISimpleRangeEventService
    {
        public async Task<bool> SaveRangeEventAsync(SimpleRangeEvent rangeEvent)
        {
            return await rangeEvent.SaveAsync();
        }

        public async Task<bool> DeleteRangeEvent(SimpleRangeEvent rangeEvent)
        {
            return await rangeEvent.DeleteAsync();
        }
    }
}
