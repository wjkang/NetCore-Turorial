using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DependencyInjection.LifetimeOptions
{
    public class OperationMiddleware2
    {
        private readonly RequestDelegate _next;

        public OperationMiddleware2(RequestDelegate next)
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
            await context.Response.WriteAsync(string.Format("In OperationMiddleware2 operationTransient is:{0}!\r\n", operationTransient.OperationId.ToString()));
            await context.Response.WriteAsync(string.Format("In OperationMiddleware2 operationScoped is:{0}!\r\n", operationScoped.OperationId.ToString()));
            await context.Response.WriteAsync(string.Format("In OperationMiddleware2 operationSingleton is:{0}!\r\n", operationSingleton.OperationId.ToString()));
            await context.Response.WriteAsync(string.Format("In OperationMiddleware2 operationSingletonInstance is:{0}!\r\n", operationSingletonInstance.OperationId.ToString()));
            await _next(context);
        }
    }
}
