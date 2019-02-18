`Surging.Core.ProxyGenerator`模块的功能是根据接口创建代理提供远程RPC调用或者直接通过`routePath`进行调用。

### IServiceProxyFactory

旧的surging源码例子中，会调用`UseProxy`方法配置热启动，减少调用生成代理产生的性能损耗。
```csharp
using Surging.Core.ServiceHosting.Internal;
using Autofac;
using Surging.Core.ProxyGenerator.Implementation;

namespace Surging.Core.ProxyGenerator
{
   public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseProxy(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<IServiceProxyFactory>();
            });
        }
    }
}
```
这个扩展方法的作用就是实例化`IServiceProxyFactory`的实现`ServiceProxyFactory`。

>最新的surging源码例子里不再显式调用`UseProxy`方法，使用`ServiceLocator.GetService<IServiceProxyFactory>()`时才会进行相应的初始化。

### ServiceProxyFactory

`ServiceProxyFactory`是`IServiceProxyFactory`的实现。

服务是通过扩展方法`AddClientProxy`进行注册的。

```csharp
//Surging.Core.ProxyGenerator ContainerBuilderExtensions.cs

public static IServiceBuilder AddClientProxy(this IServiceBuilder builder)
{
    var services = builder.Services;
    services.RegisterType<ServiceProxyGenerater>().As<IServiceProxyGenerater>().SingleInstance();
    services.RegisterType<ServiceProxyProvider>().As<IServiceProxyProvider>().SingleInstance();
    builder.Services.Register(provider =>new ServiceProxyFactory(
            provider.Resolve<IRemoteInvokeService>(),
            provider.Resolve<ITypeConvertibleService>(),
            provider.Resolve<IServiceProvider>(),
            builder.GetInterfaceService()
            )).As<IServiceProxyFactory>().SingleInstance();
    return builder;
}
...
public static IServiceBuilder AddClient(this IServiceBuilder builder)
{
    return builder
        .RegisterServices()
        .RegisterRepositories()
        .RegisterServiceBus()
        .AddClientRuntime()
        .AddClientProxy();
}
```

`AddClient`调用：
```csharp
...
var host = new ServiceHostBuilder()
.RegisterServices(builder =>
{
    builder.AddMicroService(option =>
    {
        option.AddClient();
        option.AddClientIntercepted(typeof(CacheProviderInterceptor));
        //option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181"));
        option.UseConsulManager(new ConfigInfo("127.0.0.1:8500"));
        option.UseDotNettyTransport();
        option.UseRabbitMQTransport();
        //option.UseProtoBufferCodec();
        option.UseMessagePackCodec();
        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
    });
})
.UseProxy()
.UseLog4net()
.UseClient()
.UseStartup<Startup>()
.Build();
...
```
`option`为`IServiceBuilder`类型。这个服务是如何注册的？进入`AddMicroService`的实现：
```csharp
//Surging.Core.CPlatform ContainerBuilderExtensions.cs

public static void AddMicroService(this ContainerBuilder builder, Action<IServiceBuilder> option)
{
    option.Invoke(builder.AddCoreService());
}
...
public static IServiceBuilder AddCoreService(this ContainerBuilder services)
{
    Check.NotNull(services, "services");
    services.RegisterType<DefaultServiceIdGenerator>().As<IServiceIdGenerator>().SingleInstance();
    services.Register(p => new CPlatformContainer(p));
    services.RegisterType(typeof(DefaultTypeConvertibleProvider)).As(typeof(ITypeConvertibleProvider)).SingleInstance();
    services.RegisterType(typeof(DefaultTypeConvertibleService)).As(typeof(ITypeConvertibleService)).SingleInstance();
    services.RegisterType(typeof(AuthorizationAttribute)).As(typeof(IAuthorizationFilter)).SingleInstance();
    services.RegisterType(typeof(AuthorizationAttribute)).As(typeof(IFilter)).SingleInstance();
    services.RegisterType(typeof(DefaultServiceRouteProvider)).As(typeof(IServiceRouteProvider)).SingleInstance();
    services.RegisterType(typeof(DefaultServiceRouteFactory)).As(typeof(IServiceRouteFactory)).SingleInstance();
    services.RegisterType(typeof(DefaultServiceSubscriberFactory)).As(typeof(IServiceSubscriberFactory)).SingleInstance();
    return new ServiceBuilder(services)
        .AddJsonSerialization()
        .UseJsonCodec();

}
```
所以`AddMicroService`的作用就是立即调用传入的委托。

这里的`ContainerBuilder`是在`ServiceHostBuilder`中进行实例化的：
```csharp
...
public IServiceHostBuilder RegisterServices(Action<ContainerBuilder> builder)
{
    if (builder == null)
    {
        throw new ArgumentNullException(nameof(builder));
    }
    _registerServicesDelegates.Add(builder);
    return this;
}
...
private ContainerBuilder RegisterServices()
{
    var hostingServices = new ContainerBuilder();
    foreach (var registerServices in _registerServicesDelegates)
    {
        registerServices(hostingServices);
    }
    return hostingServices;
}
```
>调用ServiceHostBuilder `Build`方法的时候，才会执行委托调用`AddMicroService`方法，通过`AddMicroService`传入的委托才会调用。

`AddMicroService`方法会在`new ServiceHost`之前执行。先使用`ContainerBuilder`实例注册一些服务（`AddCoreService`方法），然后实例化`ServiceBuilder`。

`ServiceBuilder`全部功能就是通过一个属性引用构造函数传入的`ContainerBuilder`实例：
```csharp
internal sealed class ServiceBuilder : IServiceBuilder
{
    public ServiceBuilder(ContainerBuilder services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        Services = services;
    }

    #region Implementation of IServiceBuilder

    /// <summary>
    /// 服务集合。
    /// </summary>
    public ContainerBuilder Services { get; set; }

    #endregion Implementation of IServiceBuilder
}
```
surging 中很多`ServiceBuilder`的扩展方法都是调`ServiceBuilder`属性的方法注册服务。

### 流程小结

* `ServiceHostBuilder`的`RegisterServices`方法传入委托。
* 执行`ServiceHostBuilder`的`Build`方法，实例化`ContainerBuilder`作为委托调用参数。
* 委托调用就会执行`AddMicroService`方法，`AddMicroService`方法直接调用传入的委托，委托的调用需要`IServiceBuilder`实例作为参数。
* 通过`ContainerBuilder`扩展方法，返回`ServiceBuilder`实例，期间还在`ContainerBuilder`实例上注册了一些服务。
* `ServiceBuilder`中的`Services`属性直接引用`ContainerBuilder`实例（通过构造函数传入）。
* `ServiceHostBuilder`的`Build`方法中，将`ConfigureServices`方法注册的服务，也注册到`ContainerBuilder`实例上，然后将实例传入`ServiceHost`中。
* `ServiceHost`的`Initialize`方法使用传入的`ContainerBuilder`实例，作为`Startup`的`ConfigureServices`方法的调用参数。
* `Startup`的`ConfigureServices`方法中可以实例化`ServiceCollection`并注册服务，再Populate到`ContainerBuilder`实例上，也可以直接调用`ContainerBuilder`实例方法进行注册。`ContainerBuilder`的`Build`方法，得到IOC容器实例，并且ServiceLocator.Current进行引用。
* `ServiceHost`中执行完`Startup`的`ConfigureServices`方法，会返回IOC容器实例。接着调用`Startup`的`Configure`方法，IOC容器实例作为参数传入。

### 继续ServiceProxyFactory

```csharp
public static IServiceBuilder AddClientProxy(this IServiceBuilder builder)
{
    var services = builder.Services;
    services.RegisterType<ServiceProxyGenerater>().As<IServiceProxyGenerater>().SingleInstance();
    services.RegisterType<ServiceProxyProvider>().As<IServiceProxyProvider>().SingleInstance();
    builder.Services.Register(provider =>new ServiceProxyFactory(
            provider.Resolve<IRemoteInvokeService>(),
            provider.Resolve<ITypeConvertibleService>(),
            provider.Resolve<IServiceProvider>(),
            builder.GetInterfaceService()
            )).As<IServiceProxyFactory>().SingleInstance();
    return builder;
}
```

经过上文的分析，已经知道`IServiceBuilder`实例是如何产生的，以及其中的属性`Services`就是`ContainerBuilder`实例。



