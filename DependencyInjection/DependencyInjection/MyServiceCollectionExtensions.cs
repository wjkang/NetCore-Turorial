using DependencyInjection;
using DependencyInjection.LifetimeOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MyServiceCollectionExtensions
    {
        public static void AddMyService(this IServiceCollection services)
        {
            services.AddScoped<IMyDependency, MyDependency>();
        }

        public static void AddOperation(this IServiceCollection services)
        {
            services.AddTransient<IOperationTransient, Operation>();
            services.AddScoped<IOperationScoped, Operation>();
            services.AddSingleton<IOperationSingleton, Operation>();
            services.AddSingleton<IOperationSingletonInstance>(new Operation());
        }
    }
}
