using System.Threading.Tasks;

namespace MyLittleRangeBook.Gui.Services
{
    public interface IDatabaseService
    {
        /// <summary>
        ///     Returns the connection string for the database.
        /// </summary>
        /// <returns></returns>
        string GetConnectionString();

        Task SaveAsync();
    }
}
