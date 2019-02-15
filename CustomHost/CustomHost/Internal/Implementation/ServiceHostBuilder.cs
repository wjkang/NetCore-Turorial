using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomHost.Internal.Implementation
{
    public class ServiceHostBuilder : IServiceHostBuilder
    {
        private readonly List<Action<IServiceCollection>> _configureServicesDelegates;
        private readonly List<Action<IServiceCollection>> _registerServicesDelegates;
        private readonly List<Action<IServiceProvider>> _mapServicesDelegates;
        private readonly List<Action<IConfigurationBuilder>> _configureDelegates;
        public ServiceHostBuilder()
        {
            _configureServicesDelegates = new List<Action<IServiceCollection>>();
            _registerServicesDelegates = new List<Action<IServiceCollection>>();
            _mapServicesDelegates = new List<Action<IServiceProvider>>();
        }

        public IServiceHost Build()
        {
            var services = BuildCommonServices();
            var config = Configure();
            services.AddSingleton(typeof(IConfigurationBuilder), config);
            var hostingServices = RegisterServices();
            var hostingServiceProvider = services.BuildServiceProvider();
            var host = new ServiceHost(hostingServices, hostingServiceProvider, _mapServicesDelegates);
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

        public IServiceHostBuilder MapServices(Action<IServiceProvider> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            _mapServicesDelegates.Add(mapper);
            return this;
        }

        public IServiceHostBuilder Configure(Action<IConfigurationBuilder> builder)
        {
            throw new NotImplementedException();
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

        private IConfigurationBuilder Configure()
        {
            var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory);
            foreach (var configure in _configureDelegates)
            {
                configure(config);
            }
            return config;
        }
    }
}
