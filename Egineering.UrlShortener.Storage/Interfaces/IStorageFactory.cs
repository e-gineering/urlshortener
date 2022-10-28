using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Egineering.UrlShortener.Storage.Interfaces
{

    public interface IPluginFactory
    {
        void Configure(IServiceCollection services);
        string StorageType();
    }
}
