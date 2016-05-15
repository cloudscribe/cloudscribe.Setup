using cloudscribe.Core.Models.Setup;
using cloudscribe.Setup.Web;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCloudscribeSetup(
            this IServiceCollection services,
            IConfigurationRoot configuration)
        {
            services.Configure<SetupOptions>(configuration.GetSection("SetupOptions"));
            services.AddScoped<SetupManager, SetupManager>();
            services.AddScoped<IVersionProvider, SetupVersionProvider>();

            return services;
        }
    }
}
