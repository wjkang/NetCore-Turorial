using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DependencyInjection
{
    public class MyDependency : IMyDependency
    {
        public Task<string> GetMessage(string message)
        {
            return Task.FromResult(message);
        }
    }
}
