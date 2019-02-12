using Microsoft.Extensions.DependencyInjection;
using System;

namespace CustomHost.Internal
{
    public interface IServiceHostBuilder
    {
        IServiceHost Build();
        IServiceHostBuilder RegisterServices(Action<IServiceCollection> configureServices);
        IServiceHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);
    }
}
