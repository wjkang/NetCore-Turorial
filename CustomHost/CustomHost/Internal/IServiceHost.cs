using System;
using System.Collections.Generic;
using System.Text;

namespace CustomHost.Internal
{
    public interface IServiceHost : IDisposable
    {
        IDisposable Run();

        IServiceProvider Initialize();
    }
}
