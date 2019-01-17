using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DependencyInjection
{
    public interface IMyDependency
    {
        Task<string> GetMessage(string message);
    }
}
