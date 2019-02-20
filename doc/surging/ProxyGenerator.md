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

经过上文的分析，已经知道`IServiceBuilder`实例是如何产生的，以及其中的属性`Services`就是`ContainerBuilder`实例，整个应用的服务都会往里注册。

以委托的方式注册`IServiceProxyFactory`服务，当获取服务的时候，调用委托实例化`ServiceProxyFactory`，分别取出`IRemoteInvokeService`，`ITypeConvertibleService`，`IServiceProvider`服务，作为`ServiceProxyFactory`构造函数参数。

>`IServiceProvider`不需要注册，autofac会默认生成，直接从IOC容器取出即可。

`GetInterfaceService`方法会扫描全部程序集，获取继承了`IServiceKey`接口，并且类型为接口类型的类型。后续会使用这些接口类型创建RPC代理服务。

```csharp
public ServiceProxyFactory(IRemoteInvokeService remoteInvokeService, ITypeConvertibleService typeConvertibleService,
            IServiceProvider serviceProvider, IEnumerable<Type> types)
{
    _remoteInvokeService = remoteInvokeService;
    _typeConvertibleService = typeConvertibleService;
    _serviceProvider = serviceProvider;
    if (types != null)
        _serviceTypes = _serviceProvider.GetService<IServiceProxyGenerater>().GenerateProxys(types).ToArray();
}
```
前三个参数赋值给`ServiceProxyFactory`内的三个属性，会面用到的时候再说。

如果`types`不为空，就需要创建代理服务。创建代理服务需要调用`IServiceProxyGenerater`的`GenerateProxys`方法。

### IServiceProxyGenerater
`IServiceProxyGenerater.cs`
```csharp
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Surging.Core.ProxyGenerator
{
    /// <summary>
    /// 一个抽象的服务代理生成器。
    /// </summary>
    public interface IServiceProxyGenerater
    {
        /// <summary>
        /// 生成服务代理。
        /// </summary>
        /// <param name="interfacTypes">需要被代理的接口类型。</param>
        /// <returns>服务代理实现。</returns>
        IEnumerable<Type> GenerateProxys(IEnumerable<Type> interfacTypes);

        /// <summary>
        /// 生成服务代理代码树。
        /// </summary>
        /// <param name="interfaceType">需要被代理的接口类型。</param>
        /// <returns>代码树。</returns>
        SyntaxTree GenerateProxyTree(Type interfaceType);
    }
}
```
`IServiceProxyGenerater`服务是与`IServiceProxyFactory`服务同时注册的：
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

### ServiceProxyGenerater

`ServiceProxyGenerater`需要注入`IServiceIdGenerator`服务与`ILogger`服务。
```csharp
public ServiceProxyGenerater(IServiceIdGenerator serviceIdGenerator, ILogger<ServiceProxyGenerater> logger)
{
    _serviceIdGenerator = serviceIdGenerator;
    _logger = logger;
}
```
`IServiceIdGenerator`服务是在Surging.Core.CPlatform ContainerBuilderExtensions.cs 方法`AddCoreService`中注册的。
```csharp
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
实现为`DefaultServiceIdGenerator`，只有一个方法`GenerateServiceId`，返回字符串，调用的结果例子为

**Surging.IModuleServices.Common.IUserService.Authentication_requestData**


`ServiceProxyGenerater`中通过`GenerateProxyTree`方法生成服务代理代码树。
```csharp
public SyntaxTree GenerateProxyTree(Type interfaceType)
{
    var className = interfaceType.Name.StartsWith("I") ? interfaceType.Name.Substring(1) : interfaceType.Name;
    className += "ClientProxy";

    var members = new List<MemberDeclarationSyntax>
    {
        GetConstructorDeclaration(className)
    };

    members.AddRange(GenerateMethodDeclarations(interfaceType.GetMethods()));
    return CompilationUnit()
        .WithUsings(GetUsings())
        .WithMembers(
            SingletonList<MemberDeclarationSyntax>(
                NamespaceDeclaration(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName("Surging"),
                            IdentifierName("Cores")),
                        IdentifierName("ClientProxys")))
        .WithMembers(
            SingletonList<MemberDeclarationSyntax>(
                ClassDeclaration(className)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBaseList(
                        BaseList(
                            SeparatedList<BaseTypeSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    SimpleBaseType(IdentifierName("ServiceProxyBase")),
                                    Token(SyntaxKind.CommaToken),
                                    SimpleBaseType(GetQualifiedNameSyntax(interfaceType))
                                })))
                    .WithMembers(List(members))))))
        .NormalizeWhitespace().SyntaxTree;
}
```
传入需要代理的接口，返回实现的代理服务的代码树。

`GenerateProxys`方法，使用生成的代码树，动态编译，得到服务代理实现。
```csharp
public IEnumerable<Type> GenerateProxys(IEnumerable<Type> interfacTypes)
{
#if NET
    var assemblys = AppDomain.CurrentDomain.GetAssemblies();
#else
    var assemblys = DependencyContext.Default.RuntimeLibraries.SelectMany(i => i.GetDefaultAssemblyNames(DependencyContext.Default).Select(z => Assembly.Load(new AssemblyName(z.Name))));
#endif
    assemblys = assemblys.Where(i => i.IsDynamic == false).ToArray();
    var trees = interfacTypes.Select(p=>GenerateProxyTree(p)).ToList();
    var stream = CompilationUtilitys.CompileClientProxy(trees,
        assemblys
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Concat(new[]
            {
                MetadataReference.CreateFromFile(typeof(Task).GetTypeInfo().Assembly.Location)
            }),
        _logger);

    using (stream)
    {
#if NET
        var assembly = Assembly.Load(stream.ToArray());
#else
        var assembly = AssemblyLoadContext.Default.LoadFromStream(stream);
#endif

        return assembly.GetExportedTypes();
    }
}
```
期间还调用了`CompilationUtilitys.CompileClientProxy`方法。

>构建代码树，动态编译，使用的是[roslyn](https://github.com/dotnet/roslyn)相关API，一个根据现成代码转成代码树的[在线工具](http://roslynquoter.azurewebsites.net/)，一个[roslyn简单例子](https://dev.tencent.com/u/jaycewu/p/RoslynTest/git/tree/master/src/01)。

### 动态生成的代理服务
下面是surging中动态生成的代理服务例子（Surging.IModuleServices.Common.IUserService代理实现）：

```csharp
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.ProxyGenerator.Implementation;
namespace Surging.Cores.ClientProxys
{
    public class UserServiceClientProxy : ServiceProxyBase, Surging.IModuleServices.Common.IUserService
    {
        public UserServiceClientProxy(IRemoteInvokeService remoteInvokeService, ITypeConvertibleService typeConvertibleService, String serviceKey, CPlatformContainer serviceProvider) : base(remoteInvokeService, typeConvertibleService, serviceKey, serviceProvider)
        {
        }
        public async Task<Surging.IModuleServices.Common.Models.UserModel> Authentication(Surging.IModuleServices.Common.Models.AuthenticationRequestData requestData)
        {
            return await Invoke<Surging.IModuleServices.Common.Models.UserModel>(new Dictionary<string, object>
            {
                {
                    "requestData", requestData
                }
            }
            , "Surging.IModuleServices.Common.IUserService.Authentication_requestData");
        }
        public async Task<System.String> GetUserName(System.Int32 id)
        {
            return await Invoke<System.String>(new Dictionary<string, object>
            {
                {
                    "id", id
                }
            }
            , "Surging.IModuleServices.Common.IUserService.GetUserName_id");
        }
        public async Task<System.Boolean> Exists(System.Int32 id)
        {
            return await Invoke<System.Boolean>(new Dictionary<string, object> { { "id", id } }, "Surging.IModuleServices.Common.IUserService.Exists_id");
        }
        public async Task<Surging.IModuleServices.Common.Models.IdentityUser> Save(Surging.IModuleServices.Common.Models.IdentityUser requestData)
        {
            return await Invoke<Surging.IModuleServices.Common.Models.IdentityUser>(new Dictionary<string, object>
            {
                {
                    "requestData", requestData
                }
            }
            , "Surging.IModuleServices.Common.IUserService.Save_requestData");
        }
        public async Task<System.Int32> GetUserId(System.String userName)
        {
            return await Invoke<System.Int32>(new Dictionary<string, object>
            {
                {
                    "userName", userName
                }
            }
            , "Surging.IModuleServices.Common.IUserService.GetUserId_userName");
        }
        public async Task<System.DateTime> GetUserLastSignInTime(System.Int32 id)
        {
            return await Invoke<System.DateTime>(new Dictionary<string, object>
            {
                {
                    "id", id
                }
            }
            , "Surging.IModuleServices.Common.IUserService.GetUserLastSignInTime_id");
        }
        public async Task<Surging.IModuleServices.Common.Models.UserModel> GetUser(Surging.IModuleServices.Common.Models.UserModel user)
        {
            return await Invoke<Surging.IModuleServices.Common.Models.UserModel>(new Dictionary<string, object>
            {
                {
                    "user", user
                }
            }
            , "Surging.IModuleServices.Common.IUserService.GetUser_user");
        }
        public async Task<System.Boolean> Update(System.Int32 id, Surging.IModuleServices.Common.Models.UserModel model)
        {
            return await Invoke<System.Boolean>(new Dictionary<string, object> { { "id", id }, { "model", model } }, "Surging.IModuleServices.Common.IUserService.Update_id_model");
        }
        public async Task<System.Boolean> Get(List<Surging.IModuleServices.Common.Models.UserModel> users)
        {
            return await Invoke<System.Boolean>(new Dictionary<string, object> { { "users", users } }, "Surging.IModuleServices.Common.IUserService.Get_users");
        }
        public async Task<System.Boolean> GetDictionary()
        {
            return await Invoke<System.Boolean>(new Dictionary<string, object> { }, "Surging.IModuleServices.Common.IUserService.GetDictionary");
        }
        public async System.Threading.Tasks.Task TryThrowException()
        {
            await Invoke(new Dictionary<string, object>
            {
            }
            , "Surging.IModuleServices.Common.IUserService.TryThrowException");
        }
        public async System.Threading.Tasks.Task PublishThroughEventBusAsync(Surging.Core.CPlatform.EventBus.Events.IntegrationEvent evt)
        {
            await Invoke(new Dictionary<string, object>
            {
                {
                    "evt", evt
                }
            }
            , "Surging.IModuleServices.Common.IUserService.PublishThroughEventBusAsync_evt");
        }
    }
}
```
**assemblyInfo tree**
```csharp
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[assembly: TargetFramework(".NETFramework,Version=v4.5", FrameworkDisplayName = ".NET Framework 4.5")]
[assembly: AssemblyTitle("Surging.Cores.ClientProxys")]
[assembly: AssemblyProduct("Surging.Cores.ClientProxys")]
[assembly: AssemblyCopyright("Copyright ©  Surging")]
[assembly: ComVisible(false)]
[assembly: Guid("a57f5783-8a1c-4919-8f95-eadc3f8517dd")]
[assembly: AssemblyVersion("0.0.0.1")]
[assembly: AssemblyFileVersion("0.0.0.1")]
```
实现的方法中，都是调用基类的`Invoke`方法。后面再分析Invoke的实现，接着回到`ServiceProxyFactory`，分析生成代理服务后的操作。

### 回到ServiceProxyFactory
生成服务代理实现后，会赋值给`_serviceTypes`属性。

```csharp
_serviceTypes = _serviceProvider.GetService<IServiceProxyGenerater>().GenerateProxys(types).ToArray();
```

代理调用测试：
```csharp
var serviceProxyFactory = ServiceLocator.GetService<IServiceProxyFactory>();
var userProxy = serviceProxyFactory.CreateProxy<IUserService>("User");
var userId = userProxy.GetUserId("fanly").GetAwaiter().GetResult();
```

`CreateProxy<IUserService>("User")`声明实现：
```csharp
public T CreateProxy<T>(string key) where T:class
{
    var instanceType = typeof(T);
    var instance = ServiceResolver.Current.GetService(instanceType, key);
    if (instance == null)
    {
        var proxyType = _serviceTypes.Single(typeof(T).GetTypeInfo().IsAssignableFrom);
            instance = proxyType.GetTypeInfo().GetConstructors().First().Invoke(new object[] { _remoteInvokeService, _typeConvertibleService,key,
        _serviceProvider.GetService<CPlatformContainer>() });
        ServiceResolver.Current.Register(key, instance, instanceType);
    }
    return instance as T;
}
```

尝试从自建的IOC容器中取出服务实例，`GetService`实现：

`Surging.Core.CPlatform.DependencyResolution ServiceResolver.cs`
```csharp
...
private static readonly ServiceResolver _defaultInstance = new ServiceResolver();
private readonly ConcurrentDictionary<ValueTuple<Type, string>, object> _initializers =
            new ConcurrentDictionary<ValueTuple<Type, string>, object>();
...
public static ServiceResolver Current
{
    get { return _defaultInstance; }
}
...
public virtual object GetService(Type type, object key)
{
    object result;
    _initializers.TryGetValue(ValueTuple.Create(type, key == null ? null : key.ToString()), out result);
    return result;
}
```