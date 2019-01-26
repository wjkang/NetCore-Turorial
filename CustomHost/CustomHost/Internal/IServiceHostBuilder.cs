using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomHost.Internal
{
    public interface IServiceHostBuilder
    {
        IServiceHost Build();
        IServiceHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);


    }
}
