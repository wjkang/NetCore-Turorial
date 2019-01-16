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
            app.UseMyMiddlewareByClass("123");
            app.UseMyMiddleware();
            #region Use
            //app.Use(async (context, next) =>
            //{
            //    await context.Response.WriteAsync("进入第1个委托，执行下一个委托之前\r\n");
            //    await next();
            //    await context.Response.WriteAsync("结束第1个委托，执行下一个委托之后\r\n");
            //});
            //app.Use(async (context, next) =>
            //{
            //    await context.Response.WriteAsync("进入第2个委托，执行下一个委托之前\r\n");
            //    await next();
            //    await context.Response.WriteAsync("结束第2个委托，执行下一个委托之后\r\n");
            //});
            #endregion
            #region Map
            //app.Map("/map1", (level1app) =>
            //{
            //    level1app.Use(async (context, next) =>
            //    {
            //        await context.Response.WriteAsync("进入第1个委托，执行下一个委托之前\r\n");
            //        await next();
            //        await context.Response.WriteAsync("结束第1个委托，执行下一个委托之后\r\n");
            //    });
            //    level1app.Use(async (context, next) =>
            //    {
            //        await context.Response.WriteAsync("进入第2个委托，执行下一个委托之前\r\n");
            //        await next();
            //        await context.Response.WriteAsync("结束第2个委托，执行下一个委托之后\r\n");
            //    });
            //    level1app.Map("/map12", (level2app) =>
            //    {
            //        //level1app.use 注册的委托仍生效
            //        level2app.Run(async (context) =>
            //        {
            //            await context.Response.WriteAsync("map2\r\n");
            //        });
            //    });
            //    level1app.Run(async (context) =>
            //    {
            //        await context.Response.WriteAsync("map1\r\n");
            //    });
            //    //不管app.run 在 level1app.Run之前还是之后，分支管道都会被阻断，没有任何输出
            //    //app.Run(async (context) =>
            //    //{
            //    //    await context.Response.WriteAsync("Hello World!\r\n");
            //    //});
            //});
            #endregion

            #region MapWhen
            app.MapWhen(context => context.Request.Query.ContainsKey("branch"), (level1app) =>
            {
                level1app.Run(async (context) =>
                {
                    var branchVer = context.Request.Query["branch"];
                    await context.Response.WriteAsync($"Branch used = {branchVer}");
                });
            });
            #endregion

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
