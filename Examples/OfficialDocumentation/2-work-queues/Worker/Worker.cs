using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Receive
{
    public static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (sender, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    int dots = message.Split('.').Length - 1;
                    Thread.Sleep(dots * 1000);

                    


                    // Note: it is possible to access the channel via
                    //       ((EventingBasicConsumer)sender).Model here
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    Console.WriteLine($" [x] Message: {message} Done (Routing Key: {ea.RoutingKey})");

                };
                //We can disable autoAck to send ack automatically
                channel.BasicConsume(queue: "task_queue", autoAck: false, consumer: consumer);
                //To provide Quality of Service, we can assign fair fetch.
                channel.BasicQos(0, 1, false);
                Console.ReadLine();
            }
        }

    }
}