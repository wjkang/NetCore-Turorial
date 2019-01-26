using CustomHost.Startup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomHost.Internal.Implementation
{
    public class ServiceHost : IServiceHost
    {
        private readonly IServiceCollection _builder;
        private IStartup _startup;
        private IServiceProvider _applicationServices;
        private readonly IServiceProvider _hostingServiceProvider;
        public ServiceHost(IServiceCollection serviceCollection, IServiceProvider hostingServiceProvider)
        {
            _builder = serviceCollection;
            _hostingServiceProvider = hostingServiceProvider;
        }

        public void Dispose()
        {
            (_hostingServiceProvider as IDisposable)?.Dispose();
        }

        public IServiceProvider Initialize()
        {
            return _hostingServiceProvider;
        }

        public IDisposable Run()
        {
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
    }
}
