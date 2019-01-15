using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Middleware
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.Use(async (context, next) =>
            {
                await context.Response.WriteAsync("进入第1个委托，执行下一个委托之前\r\n");
                await next();
                await context.Response.WriteAsync("结束第1个委托，执行下一个委托之后\r\n");
            });
            app.Use(async (context, next) =>
            {
                await context.Response.WriteAsync("进入第2个委托，执行下一个委托之前\r\n");
                await next();
                await context.Response.WriteAsync("结束第2个委托，执行下一个委托之后\r\n");
            });
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!\r\n");
            });
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("管道已经被终止!");
            });
        }
    }
}
