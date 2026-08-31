using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.Firearms.Firearm;

// ReSharper disable once CheckNamespace
namespace MyLittleRangeBook
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// This should match the JsonSerializeble attributes in MlrbJsonSerializerContext.cs
        /// </summary>
        static readonly Type[] s_supportedFirearmsEvents = [
            typeof(FirearmActivated),
            typeof(FirearmAssociatedWithAsset),
            typeof(FirearmAssociatedWithRangeEvent),
            typeof(FirearmBarrelChanged),
            typeof(FirearmCleaned),
            typeof(FirearmCreated),
            typeof(FirearmRoundCountAltered),
            typeof(FirearmDeactivated),
            typeof(FirearmModified),
            typeof(FirearmNoteAdded),
            typeof(FirearmSightingSystemChanged),
        ];

        public static IServiceCollection RegisterFirearmEventSourcing(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddKeyedScoped<IFirearmsService, FirearmsService>(SqliteHelperExtensions.DI_KEY);
            services.TryAddScoped<IFirearmsService, FirearmsService>();
            return services;
        }
    }
}
