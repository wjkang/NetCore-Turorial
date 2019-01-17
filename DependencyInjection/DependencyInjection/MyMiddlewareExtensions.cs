using DependencyInjection.LifetimeOptions;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DependencyInjection
{
    public static class MyMiddlewareExtensions
    {
        public static IApplicationBuilder UseMyMiddlewareByClass(this IApplicationBuilder app, string otherParams = "")
        {
            app.UseMiddleware<MyMiddleware>(otherParams);
            return app;
        }

        public static IApplicationBuilder UseOperation(this IApplicationBuilder app)
        {
            app.UseMiddleware<OperationMiddleware1>();
            app.UseMiddleware<OperationMiddleware2>();
            app.UseMiddleware<OperationMiddleware3>();
            return app;
        }
    }
}
