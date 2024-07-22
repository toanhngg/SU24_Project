using RabbitMQ.Client;
using System.Text;

namespace Project_API.Service
{
    public class RabbitMQService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQService()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "logQueuePayment",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        public void SendMessage(string message)
        {

            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "exchangelogQueuePayment",
                                  routingKey: "logQueuePayment",
                                  basicProperties: null,
                                  body: body);
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
