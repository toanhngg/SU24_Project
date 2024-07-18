using RabbitMQ.Client;

public static class RabbitMQExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, string hostName, string userName, string password)
    {
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            DispatchConsumersAsync = true
        };

        var connection = factory.CreateConnection();
        services.AddSingleton<IConnection>(connection);

        return services;
    }
}