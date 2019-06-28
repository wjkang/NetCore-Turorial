### 使用RabbitMQ进行传输

`option.UseRabbitMQTransport();`

```csharp
public static IServiceBuilder UseRabbitMQTransport(this IServiceBuilder builder)
{
    var services = builder.Services;
    builder.Services.RegisterType(typeof(Implementation.EventBusRabbitMQ)).As(typeof(IEventBus)).SingleInstance();
    builder.Services.RegisterType(typeof(DefaultConsumeConfigurator)).As(typeof(IConsumeConfigurator)).SingleInstance();
    builder.Services.RegisterType(typeof(InMemoryEventBusSubscriptionsManager)).As(typeof(IEventBusSubscriptionsManager)).SingleInstance();
    builder.Services.Register(provider =>
    {
        var logger = provider.Resolve<ILogger<DefaultRabbitMQPersistentConnection>>();
        var HostName = AppConfig.Configuration["EventBusConnection"];
        var rabbitUserName= AppConfig.Configuration["EventBusUserName"]??"guest";;
        var rabbitPassword= AppConfig.Configuration["EventBusPassword"] ??"guest";
        var factory = new ConnectionFactory()
        {
            HostName = HostName,
            UserName = rabbitUserName,
            Password = rabbitPassword
        };
        return new DefaultRabbitMQPersistentConnection(factory, logger);
    }).As<IRabbitMQPersistentConnection>();
    return builder;
}
```