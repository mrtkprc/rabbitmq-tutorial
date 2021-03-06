# Book:  RabbitMQ Essentials - Second Edition

## Messageing Queue

Messaging or message queuing is a method of communication between applications or components. 
Thanks to message queues, these applications can remain completely separate as they process their individual tasks. 

Messages are typically small requests, replies, status updates, or even just information.

RabbitMQ is an open source message broker that acts as the intermediary or middleman for independent applications, giving them a common platform to communicate. RabbitMQ mainly uses an Erlang-based implementation of the Advanced Message Queuing Protocol (AMQP), which supports advanced features such as clustering and the complex routing of messages.

Message queuing is a one-way style of interaction where one system asynchronously interacts with another system via messages, generally through a message broker. A requesting system in asynchronous communication mode does not wait for an answer or require return information; it continues processing no matter what.

The most common example of such an interaction is an email.

One big advantage of the messaging queuing approach is that systems become loosely coupled with each other. They do not need to know the location of other nodes on the network; a mere name is enough to reach them.

![Broker](https://learning.oreilly.com/api/v2/epubs/urn:orm:book:9781789131666/files/assets/09a9fe21-6b50-4db2-8496-81930a935750.png)

## Meet AMQP

AMQP is an open standard protocol that defines how a system can exchange messages. The protocol defines a set of rules that needs to be followed by the systems that are going to communicate with each other. In addition to defining the interaction that happens between a consumer/producer and a broker,

The following is a list of the core concepts of AMQP

- **Broker** or message broker: A broker is a piece of software that receives messages from one application or service, and delivers them to another application, service, or broker.

- **Virtual host, vhost**: A vhost exists within the broker. It's a way to separate applications that are using the same RabbitMQ instance, similar to a logical container inside a broker; for example, separating working environments into development on one vhost and staging on another, keeping them within the same broker instead of setting up multiple brokers. Users, exchanges, queues, and so on are isolated on one specific vhost. A user connected to a particular vhost cannot access any resources (queue, exchange, and so on) from another vhost. Users can have different access privileges to different vhosts

- Connection

- A channel is a virtual connection inside a connection. It reuses a connection, forgoing the need to reauthorize and open a new TCP stream. When messages are published or consumed, it is done over a channel. Many channels can be established within a single connection.

- Exchange: The exchange entity is in charge of applying routing rules for messages, making sure that messages are reaching their final destination. In other words, the exchange ensures that the received message ends up in the correct queues. Which queue the message ends up in depends on the rules defined by the exchange type. A queue needs to be bound to at least one exchange to be able to receive messages. Routing rules include direct (point-to-point), topic (publish-subscribe), fanout (multicast), and header exchanges.

- Queue: A queue is a sequence of items; in this case, messages. The queue exists within the broker.

- A binding is a virtual link between an exchange and a queue within the broker. It enables messages to flow from an exchange to a queue

![Basic concepts](https://learning.oreilly.com/api/v2/epubs/urn:orm:book:9781789131666/files/assets/eb85d456-ba34-460a-ba06-4a916b6296f3.png)

**Rabbitmq supports fedaration and clustering mechanisms.**

Exploring the benefits of message queuing

- Development and maintenance made easier: Dividing an application across multiple services allows separate responsibilities and gives developers the freedom to write code for a specific service in any chosen language.

- Fault isolation: A fault can be isolated to a single module and will thereby not affect other services. For example, an application with a reporting service temporarily out of function will not affect the authenticate or payment services. 

- Enhanced levels of speed and productivity: Different developers are able to work on different modules at the same time. In addition to speeding up the development cycle, the testing phase is also impacted by the use of microservices and message queues.

- Improved scalability: Microservices also allow for effortless scale-out at will. It's possible to add more consumers if the message queue is growing. Adding new components to just one service is easy to do without changing any other service.

- Easy to understand: Since each module in a microservice architecture represents a single functionality, getting to know the relevant details for a task is easy. 

```bash
docker run -d --hostname my-rabbit --name my-rabbit -p 5672:5672 -p 15672:15672 -e RABBITMQ_ERLANG_COOKIE='cookie_for_clustering' -e RABBITMQ_DEFAULT_USER=user -e RABBITMQ_DEFAULT_PASS=password  --name some-rabbit rabbitmq:3-management
```

![Channel](https://learning.oreilly.com/api/v2/epubs/urn:orm:book:9781789131666/files/assets/43ce057b-af5f-4164-95ff-ee965b9a21b7.png)

Unlike creating channels, creating connections is a costly operation, very much like it is with database connections. Typically, database connections are pooled, where each instance of the pool is used by a single execution thread. AMQP is different in the sense that a single connection can be used by many threads through many multiplexed channels.

The handshake process for an AMQP connection requires at least seven TCP packets, and even more when using TLS. Channels can be opened and closed more frequently if needed:

- AMQP connections: 7 TCP packages
- AMQP channel: 2 TCP packages
- AMQP publish: 1 TCP package (more for larger messages)
- AMQP close channel: 2 TCP packages
- AMQP close connection: 2 TCP packages
- **Total 14-19 packages (plus Acks)**

![Channel Open](https://learning.oreilly.com/api/v2/epubs/urn:orm:book:9781789131666/files/assets/f23a471f-89ff-43ca-a643-e779f4492462.png)

**All operations performed by a client happen on a channel, queues are declared on channels, and messages are sent over channels.**

###??Routing strategy

A routing strategy determines which queue (or queues) the message will be routed to. The routing strategy bases its decision on a routing key (a free-form string) and potentially on message meta-information. Think of the routing key as an address that the exchange uses to decide how the message should be routed. It also needs to be a binding between an exchange and the queue to enable a message to flow from the former to the latter.

###??The direct exchange
A direct exchange delivers messages to queues based on a message routing key. 

![Direct exchange](https://learning.oreilly.com/api/v2/epubs/urn:orm:book:9781789131666/files/assets/40c44f43-019d-4d0d-8880-d292a31a6a19.png)

### Create exchange for every publish.
If the exchange doesn't exist when something is published to it, it will raise exceptions. If the exchange already exists, it will do nothing; otherwise, it will actually create one. This is why it's safe to declare queues every time the application starts or before publishing a message.

An exchange declaration is only idempotent if the exchange properties are the same. Trying to declare an already-existing exchange with different properties will fail. Always use consistent properties in an exchange declaration. If you're making a change to the properties, delete the exchange before declaring it with the new properties. The same rule applies to a queue declaration.

- Fanout: Messages are routed to all queues bound to the fanout exchange.
- Topic: Wildcards must form a match between the routing key and the binding's specific routing pattern.
- Headers: Use the message header attributes for routing.

**Conversely, the check-then-act pattern is discouraged;**

Ensure that messages are not lost by declaring a queue as durable and setting the message delivery mode to persistent.

### Consuming Nessages

There's the polling-based `basic.get` method and the push-based `basic.consume` method. Make sure that the consumer consumes messages from the queue instead of using basic GET actions. The basic.get command is comparatively expensive when it comes to resources.

### Acknowledgment and negative acknowledgment

RabbitMQ needs to know when a message can be considered successful in terms of being sent to the consumer as expected. The broker should then delete messages from the queue once the broker receives the response; otherwise, the queue would overflow. The client can reply to the broker by either acking (acknowledge) the message when it receives it or when the consumer has completely processed the message. In either situation, once the message has been ack:ed, it's removed from the queue.

To avoid a situation where a message could be forever lost (for example, worker crashed, exceptions, and so on), the consuming application should not acknowledge a message until it is completely finished with it.

Channels are full-duplex, meaning that one channel can be used to both publish and consume messages.

A low prefetch value is recommended in situations where there are many consumers and a short processing time. If the prefetch value is set too low, the consumers will be idle much of the time, waiting for messages to arrive. On the other hand, if the prefetch value is too high, one consumer may be very busy while the others are idle.

In scenarios where there are many consumers and/or a longer time to process messages, it is recommended that the prefetch value is set to one (1) to evenly distribute messages among all consumers.

**If the client is set to auto-ack messages, prefetch settings will have no effect.**

### Dead Letter Messages:

A dead letter is a message that can't be delivered, either because the intended target cannot be accessed or because it has expired. In the case of CC, messages that reach their TTL will become dead letters.

```ruby
# Declare a queue for a taxi inbox 1
queue1 = channel.queue('taxi-inbox.1', 
  durable: true, 
  arguments:{
    'x-message-ttl'=> 604800000, 
    'x-dead-letter-exchange'=> 'taxi-dlx', 
    'x-dead-letter-routing-key'=> 'taxi-inbox.1'
  }
)
```

This strategy to refactor the existing queues is achieved with the following single command-line operation:

`sudo rabbitmqctl set_policy -p cc-dev-vhost Q_TTL_DLX "taxi\.\d+" '{"message-ttl":604800000, "dead-letter-exchange":"taxi-dlx"}' 
--apply-to queues`

### Applying Dead-Letter Queue:

1. Start by declaring two queues with x-message-ttl set to 604800000:
```ruby
queue1 = channel.queue('taxi-inbox.1', durable: true,
  arguments: {'x-message-ttl'=> 604800000, 'x-dead-letter-exchange'=> 'taxi-dlx'})

queue2 = channel.queue('taxi-inbox.2', durable: true,
  arguments: {'x-message-ttl'=> 604800000, 'x-dead-letter-exchange'=> 'taxi-dlx'})
```
2. Declare a fanout exchange taxi-fanout:
```ruby
exchange = channel.fanout('taxi-fanout')
```
3. Bind both queues to the exchange:
```ruby
queue1.bind(exchange, routing_key: "")
queue2.bind(exchange, routing_key: "")
```

4. Declare a dead letter queue, taxi-dlq:
```ruby
taxi_dlq = channel.queue('taxi-dlq', durable: true)
```

5. Declare a dead letter fanout exchange, taxi-dlx:
```ruby
dlx_exchange = channel.fanout('taxi-dlx')
```
6. Now taxi-dlx needs to be bound to taxi-dlq:
```ruby
taxi_dlq.bind(dlx_exchange, routing_key: "")
```
Finally, publish a message:
```ruby
exchange.publish("Hello! This is an information message!",   key: "")
```
As you can see, this is just a standard fanout exchange declaration

```bash

$ sudo rabbitmqctl set_policy 
-p cc-dev-vhost Q_TTL_DLX "taxi-inbox\.\d+ " '{"message-ttl":604800000, "dead-letter-exchange":"taxi-dlx"}' --apply-to queues

Setting policy "Q_TTL_DLX" for pattern "taxi-inbox\.\d+ " to "{\"message-ttl\":604800000, \"dead-letter-exchange\":\"taxi-dlx\"}" with priority "0" ...
...done.
```

The Delayed Message Plugin is available for RabbitMQ 3.5.3 and later versions of RabbitMQ. The Delayed Message Plugin adds a new exchange type to RabbitMQ.

## Mirroring Queues:
In the case of CC, the data in the queues needs to be highly available. Mirrored queues provide this type of security. Queue mirroring uses a master-mirror design pattern. All message queuing and dequeuing actions happen with the master, and the mirrors receive the updates periodically from the master. If a master becomes unavailable, RabbitMQ promotes a mirror to a master; usually, the oldest mirror becomes the new master, as long as it is synchronized.

## How to avoid losing messages
For the most part, CC has followed the best practice of keeping queues short and efficient. Queues that contain too many messages have a negative impact on the broker's performance. An identified high RAM usage could indicate that the number of queued messages rapidly went up
