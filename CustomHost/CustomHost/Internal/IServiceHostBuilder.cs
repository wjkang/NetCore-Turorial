using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CustomHost.Internal
{
    public interface IServiceHostBuilder
    {
        IServiceHost Build();
        IServiceHostBuilder RegisterServices(Action<IServiceCollection> configureServices);
        IServiceHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);
        IServiceHostBuilder MapServices(Action<IServiceProvider> mapper);
        IServiceHostBuilder Configure(Action<IConfigurationBuilder> builder);
    }
}
