[自定义Host](/自定义Host)就是根据`Surging.Core.ServiceHosting`去掉`autofac`依赖修改来的。

### MapServices

在`surging`中，使用某个模块其实就是调用`MapServices`方法注册委托，委托的逻辑就是从容器中取出服务进行相应操作。比如`Log4net`模块：
```csharp
public static IServiceHostBuilder UseLog4net(this IServiceHostBuilder hostBuilder,string log4NetConfigFile)
{
    return hostBuilder.MapServices(mapper =>
    {
        mapper.Resolve<ILoggerFactory>().AddProvider(new Log4NetProvider(log4NetConfigFile));
    });
}
```

在之前自定义Host的基础上加上这个：

`IServiceHostBuilder.cs`
```csharp
IServiceHostBuilder MapServices(Action<IServiceProvider> mapper);
```

`ServiceHostBuilder.cs`
```csharp
...
private readonly List<Action<IServiceProvider>> _mapServicesDelegates;
...
public IServiceHost Build()
{
    var services = BuildCommonServices();
    var hostingServices = RegisterServices();
    var hostingServiceProvider = services.BuildServiceProvider();
    var host = new ServiceHost(hostingServices, hostingServiceProvider, _mapServicesDelegates);
    host.Initialize();
    return host;
}
...
public IServiceHostBuilder MapServices(Action<IServiceProvider> mapper)
{
    if (mapper == null)
    {
        throw new ArgumentNullException(nameof(mapper));
    }
    _mapServicesDelegates.Add(mapper);
    return this;
}
...
```
`ServiceHost.cs`
```csharp
...
public IDisposable Run()
{
    if (_applicationServices != null)
        MapperServices(_applicationServices);
    return this;
}
...
private void MapperServices(IServiceProvider mapper)
{
    foreach (var mapServices in _mapServicesDelegates)
    {
        mapServices(mapper);
    }
}
...
```
>所以，`MapServices`方法注册的委托中，mapper容器就是_applicationServices，取出来的服务是通过`ServiceHostBuilder`的`RegisterServices(Action<IServiceCollection> configureServices)`方法注册的，而不是`ConfigureServices(Action<IServiceCollection> configureServices)`

### 全局IOC容器

`Startup`的`ConfigureServices`方法
```csharp
public IContainer ConfigureServices(ContainerBuilder builder)
{
    var services = new ServiceCollection();
    ConfigureLogging(services);
    builder.Populate(services);
    _builder = builder;
    ServiceLocator.Current = builder.Build();
    return ServiceLocator.Current;
}
```

`ServiceLocator.cs`
```csharp
using Autofac;
using System;

namespace Surging.Core.CPlatform.Utilities
{
    public class ServiceLocator
    {
        public static IContainer Current { get; set; }

        public static T GetService<T>()
        {
            return Current.Resolve<T>();
        }

        public static T GetService<T>(string key)
        {
            return Current.ResolveKeyed<T>(key);
        }

        public static object GetService(Type type)
        {
            return Current.Resolve(type);
        }

        public static object GetService(string key, Type type)
        {
            return Current.ResolveKeyed(key, type);
        }
    }
}
```

>通过`ServiceHostBuilder`的`RegisterServices(Action<IServiceCollection> configureServices)`方法注册的服务，以及`Startup`的`ConfigureServices`方法注册的服务，都可以通过`ServiceLocator`取到