using GemFire;
using GemFire.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Steeltoe.CloudFoundry.Connector.GemFire;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GemFireCacheServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedGemFireCache(this IServiceCollection services, IConfiguration configuration)
        {
            // add GemFire factories
            services.AddGemFireConnection(configuration, typeof(BasicAuthInitialize));

            // remove any other distributed cache
            var descriptors = services.Where(x => x.ServiceType == typeof(IDistributedCache));
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.TryAddSingleton<IDistributedCache, GemFireCache>();

            return services;
        }
    }
}
