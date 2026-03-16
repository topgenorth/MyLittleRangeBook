using System.Threading.Tasks;

namespace MySimpleRangeLog.Services
{
    public interface IDatabaseService
    {
        string GetDatabasePath();
        Task SaveAsync();
    }
}
