using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DependencyInjection.LifetimeOptions
{
    public class OperationMiddleware3
    {
        private readonly RequestDelegate _next;
        private IOperationSingleton _operationSingleton;
        private IOperationSingletonInstance _operationSingletonInstance;
        private MySingletonService _mySingletonService;
        public OperationMiddleware3(
            RequestDelegate next,
            IOperationSingleton operationSingleton,
            IOperationSingletonInstance operationSingletonInstance,
            MySingletonService mySingletonService
            )
        {
            _next = next;
            _operationSingleton = operationSingleton;
            _operationSingletonInstance = operationSingletonInstance;
            _mySingletonService = mySingletonService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await context.Response.WriteAsync(string.Format("In OperationMiddleware3 operationSingleton is:{0}\r\n", _operationSingleton.OperationId.ToString()));
            await context.Response.WriteAsync(string.Format("In OperationMiddleware3 operationSingletonInstance is:{0}\r\n", _operationSingletonInstance.OperationId.ToString()));
            await context.Response.WriteAsync(string.Format("In OperationMiddleware3 MySingletonService is:{0}\r\n", _mySingletonService.Id.ToString()));
            await context.Response.WriteAsync(string.Format("In OperationMiddleware3 MySingletonService's _operationTransient is:{0}\r\n", _mySingletonService.OperationTransientId.ToString()));
            await _next(context);
        }
    }
}
