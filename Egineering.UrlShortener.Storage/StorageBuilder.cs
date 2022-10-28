using Egineering.UrlShortener.Storage.Interfaces;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Egineering.UrlShortener.Services
{
    public static class StorageBuilder
    {
        static private List<PluginLoader> LoadtorageOptions()
        {
            var loaders = new List<PluginLoader>();

            // create plugin loaders
            var pluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");
            foreach (var dir in Directory.GetDirectories(pluginsDir))
            {
                var dirName = Path.GetFileName(dir);
                var pluginDll = Path.Combine(dir, dirName + ".dll");
                if (File.Exists(pluginDll))
                {
                    var loader = PluginLoader.CreateFromAssemblyFile(
                        pluginDll,
                        sharedTypes: new[] { typeof(IPluginFactory), typeof(IServiceCollection) });
                    loaders.Add(loader);
                }
            }
            return loaders;
        }
        
        public static void ConfigureStorageProvider(IServiceCollection services, string storagePluginName)
        {
            var loaders = LoadtorageOptions();
            // Create an instance of plugin types
            foreach (var loader in loaders)
            {
                foreach (var pluginType in loader
                    .LoadDefaultAssembly()
                    .GetTypes()
                    .Where(t => typeof(IPluginFactory).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    // This assumes the implementation of IPluginFactory has a parameterless constructor
                    var plugin = Activator.CreateInstance(pluginType) as IPluginFactory;
                    if(plugin?.StorageType() == storagePluginName)
                        plugin?.Configure(services);
                }
            }
        }
    }
}
