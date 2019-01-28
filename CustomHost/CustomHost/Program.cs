using CustomHost.Internal;
using CustomHost.Internal.Implementation;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CustomHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var host=
            BuildHost().Run();
        }

        public static IServiceHost BuildHost()=>
            new ServiceHostBuilder()
                .RegisterServices(builder =>
                {
                    builder.AddScoped<MyService>();
                })
                .UseStartup<StartupImplementation>()
                .Build();
    }
}
