using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DependencyInjection.LifetimeOptions
{
    public class MySingletonService
    {
        private IOperationTransient _operationTransient;
        public MySingletonService(IOperationTransient operationTransient)
        {
            _operationTransient = operationTransient;
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }

        public Guid OperationTransientId
        {
            get { return _operationTransient.OperationId; }
        }
    }
}
