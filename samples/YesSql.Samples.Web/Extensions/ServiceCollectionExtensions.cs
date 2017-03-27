using System;
using YesSql.Core.Services;
using YesSql.Data;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbProvider(
            this IServiceCollection services,
            Action<IDbProviderOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            var options = new DbProviderOptions();
            setupAction.Invoke(options);
            services.AddSingleton<IStore>(new Store(options.Configuration));

            return services;
        }
    }
}
