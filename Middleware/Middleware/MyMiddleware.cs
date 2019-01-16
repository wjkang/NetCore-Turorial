using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware
{
    public class MyMiddleware
    {
        private readonly RequestDelegate _next;
        private string _otherParams;

        public MyMiddleware(RequestDelegate next,string otherParams)
        {
            _next = next;
            _otherParams = otherParams;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await context.Response.WriteAsync(string.Format("otherParams:{0}\r\n", _otherParams));
            await context.Response.WriteAsync("MyMiddleware Build In Class!\r\n");
            await _next(context);//不调用next，则阻断管道
        }
    }
}
