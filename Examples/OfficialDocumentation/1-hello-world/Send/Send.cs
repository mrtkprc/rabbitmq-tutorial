using System;
using System.Text;
using RabbitMQ.Client;

class Send
{
    public static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                //Declaring Queue is idempotent - it will only be created if it doesn't exist already.
                channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

                var message = "Hello World";

                //The message content is a byte array, so you can encode whatever you like there
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange:"", routingKey: "hello", basicProperties: null, body: body);

                Console.WriteLine($" [X] Sent: {message}");
            }
        }

        Console.WriteLine(" Press [enter] to exit");
        Console.ReadLine();
    }
}