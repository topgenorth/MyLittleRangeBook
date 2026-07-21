using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.Notes;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    // [TO20260611] Keep this in the root MyLittleRangeBook namespace.
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterNotes(this IServiceCollection services)
        {
            services.TryAddKeyedScoped<INotesService, NotesService>(SqliteHelperExtensions.DI_KEY);
            services.TryAddScoped<INotesService, NotesService>();
            return services;
        }
    }
}