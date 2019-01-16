using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware
{
    public static class MyMiddlewareExtensions
    {
        public static IApplicationBuilder UseMyMiddleware(this IApplicationBuilder app)
        {
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("MyMiddleware!\r\n");
            });
            return app;
        }
        public static IApplicationBuilder UseMyMiddlewareByClass(this IApplicationBuilder app,string otherParams="")
        {
            app.UseMiddleware<MyMiddleware>(otherParams);
            return app;
        }
    }
}
