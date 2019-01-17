using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DependencyInjection.LifetimeOptions
{
    public class OperationMiddleware1
    {
        private readonly RequestDelegate _next;

        public OperationMiddleware1(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context, 
            IOperationTransient operationTransient,
            IOperationScoped operationScoped,
            IOperationSingleton operationSingleton,
            IOperationSingletonInstance operationSingletonInstance
            )
        {
            await context.Response.WriteAsync(string.Format("In OperationMiddleware1 operationTransient is:{0}\r\n",operationTransient.OperationId.ToString()));
            await context.Response.WriteAsync(string.Format("In OperationMiddleware1 operationScoped is:{0}\r\n", operationScoped.OperationId.ToString()));
            await context.Response.WriteAsync(string.Format("In OperationMiddleware1 operationSingleton is:{0}\r\n", operationSingleton.OperationId.ToString()));
            await context.Response.WriteAsync(string.Format("In OperationMiddleware1 operationSingletonInstance is:{0}\r\n", operationSingletonInstance.OperationId.ToString()));
            await _next(context);
        }
    }
}
