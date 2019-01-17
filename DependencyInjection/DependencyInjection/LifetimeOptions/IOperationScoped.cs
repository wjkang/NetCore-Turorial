using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DependencyInjection.LifetimeOptions
{
    public interface IOperationScoped
    {
        Guid OperationId { get; }
    }
}
