using Egineering.UrlShortener.Services.StorageServices;
using Egineering.UrlShortener.Storage.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Egineering.UrlShortener.Storage.AzureTableStorage
{
    public class PluginConfiguration : IPluginFactory
    {
        public void Configure(IServiceCollection services)
        {
            services.AddSingleton<IStorageService, AzureTableStorageService>();
        }

        public string StorageType()
        {
            return AzureTableStorageService.TypeName;
        }
    }
}
