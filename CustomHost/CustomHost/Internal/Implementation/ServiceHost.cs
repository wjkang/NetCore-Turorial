using CustomHost.Startup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace CustomHost.Internal.Implementation
{
    public class ServiceHost : IServiceHost
    {
        private readonly IServiceCollection _builder;
        private IStartup _startup;
        private IServiceProvider _applicationServices;
        private readonly IServiceProvider _hostingServiceProvider;
        private readonly List<Action<IServiceProvider>> _mapServicesDelegates;
        public ServiceHost(IServiceCollection serviceCollection, IServiceProvider hostingServiceProvider, List<Action<IServiceProvider>> mapServicesDelegate)
        {
            _builder = serviceCollection;
            _hostingServiceProvider = hostingServiceProvider;
            _mapServicesDelegates = mapServicesDelegate;
        }

        public void Dispose()
        {
            (_hostingServiceProvider as IDisposable)?.Dispose();
        }

        public IServiceProvider Initialize()
        {
            if (_applicationServices == null)
            {
                _applicationServices = BuildApplication();
            }
            return _applicationServices;
        }

        public IDisposable Run()
        {
            if (_applicationServices != null)
                MapperServices(_applicationServices);
            return this;
        }

        private void EnsureApplicationServices()
        {
            if (_applicationServices == null)
            {
                EnsureStartup();
                _applicationServices = _startup.ConfigureServices(_builder);
            }
        }
        private void EnsureStartup()
        {
            if (_startup != null)
            {
                return;
            }

            _startup = _hostingServiceProvider.GetRequiredService<IStartup>();
        }
        private IServiceProvider BuildApplication()
        {
            try
            {
                EnsureApplicationServices();
                Action<IServiceProvider> configure = _startup.Configure;
                if (_applicationServices == null)
                    _applicationServices = _builder.BuildServiceProvider();
                configure(_applicationServices);
                return _applicationServices;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("应用程序启动异常: " + ex.ToString());
                throw;
            }
        }
        private void MapperServices(IServiceProvider mapper)
        {
            foreach (var mapServices in _mapServicesDelegates)
            {
                mapServices(mapper);
            }
        }
    }
}
