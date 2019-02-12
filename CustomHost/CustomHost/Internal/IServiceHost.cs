using System;

namespace CustomHost.Internal
{
    public interface IServiceHost : IDisposable
    {
        IDisposable Run();
        IServiceProvider Initialize();
    }
}
