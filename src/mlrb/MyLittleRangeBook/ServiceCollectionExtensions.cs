using Microsoft.Extensions.DependencyInjection;

namespace MyLittleRangeBook
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers domain event serializers for supported event types.
        /// </summary>
        /// <param name="services">
        /// The service collection to which the domain event serializers should be registered.
        /// </param>
        /// <returns>
        /// The updated service collection with the registered domain event serializers.
        /// </returns>
        public static IServiceCollection RegisterDomainEventSerializers(this IServiceCollection services)
        {
            services.AddScoped<IEventSerializer, SystemTextJsonEventSerializer>(serviceProvider =>
                                                                                        {
                                                                                            var l = new List<Type>();
                                                                                            l.AddRange(SupportedRangeAssetEvents);
                                                                                            l.AddRange(SupportedFirearmsEvents);

                                                                                            return new SystemTextJsonEventSerializer(l);
                                                                                        });

            return services;
        }
    }
}