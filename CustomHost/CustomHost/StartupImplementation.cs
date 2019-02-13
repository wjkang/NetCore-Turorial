using CustomHost.Startup;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CustomHost
{
    public class StartupImplementation: IStartup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //services.AddScoped<MyService>();
            return services.BuildServiceProvider();
        }

        public void Configure(IServiceProvider app)
        {
            var myService = app.GetService<MyService>();
            myService.WriteMessage("This is a message");
        }
    }
}
