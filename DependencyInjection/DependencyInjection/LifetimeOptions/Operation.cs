using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DependencyInjection.LifetimeOptions
{
    public class Operation : IOperationTransient, IOperationScoped, IOperationSingleton, IOperationSingletonInstance
    {
        public Operation()
        {
            OperationId = Guid.NewGuid();
        }
        public Guid OperationId { get; private set; }
    }
}
