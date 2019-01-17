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
    }
}
