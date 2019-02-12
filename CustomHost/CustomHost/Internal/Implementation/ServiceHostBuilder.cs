using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace CustomHost.Internal.Implementation
{
    public class ServiceHostBuilder : IServiceHostBuilder
    {
        private readonly List<Action<IServiceCollection>> _configureServicesDelegates;
        private readonly List<Action<IServiceCollection>> _registerServicesDelegates;

        public ServiceHostBuilder()
        {
            _configureServicesDelegates = new List<Action<IServiceCollection>>();
            _registerServicesDelegates = new List<Action<IServiceCollection>>();
        }

        public IServiceHost Build()
        {
            var services = BuildCommonServices();
            var hostingServices = RegisterServices();
            var hostingServiceProvider = services.BuildServiceProvider();
            var host = new ServiceHost(hostingServices, hostingServiceProvider);
            host.Initialize();
            return host;
        }

        public IServiceHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            if (configureServices == null)
            {
                throw new ArgumentNullException(nameof(configureServices));
            }
            _configureServicesDelegates.Add(configureServices);
            return this;
        }

        public IServiceHostBuilder RegisterServices(Action<IServiceCollection> configureServices)
        {
            if (configureServices == null)
            {
                throw new ArgumentNullException(nameof(configureServices));
            }
            _registerServicesDelegates.Add(configureServices);
            return this;
        }

        private IServiceCollection BuildCommonServices()
        {
            var services = new ServiceCollection();
            foreach (var configureServices in _configureServicesDelegates)
            {
                configureServices(services);
            }
            return services;
        }
        private IServiceCollection RegisterServices()
        {
            var hostingServices = new ServiceCollection();
            foreach (var registerServices in _registerServicesDelegates)
            {
                registerServices(hostingServices);
            }
            return hostingServices;
        }
    }
}
