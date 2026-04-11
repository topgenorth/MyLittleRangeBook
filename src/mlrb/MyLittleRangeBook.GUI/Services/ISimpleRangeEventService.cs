using System.Threading;
using System.Threading.Tasks;
using MyLittleRangeBook.GUI.Models;

namespace MyLittleRangeBook.GUI.Services
{
    public interface ISimpleRangeEventService
    {
        Task<bool> SaveRangeEventAsync(SimpleRangeEvent rangeEvent, CancellationToken cancellationToken);
        Task<bool> DeleteRangeEvent(SimpleRangeEvent rangeEvent, CancellationToken cancellationToken);
    }
}
