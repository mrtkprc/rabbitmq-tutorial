# RabbitMQ Cookbook

This file is taken notes while reading RabbitMQ Cookbook which written by Sigismondo Boschi, Gabriele Santomaggio.

## Introduction

Advanced Message Queuing Protocol (AMQP) has been developed because of the need for interoperability among the many different messaging solutions, that were developed a few years ago by many different vendors such as IBM MQ-Series, TIBCO, or Microsoft MSMQ.

**RabbitMQ is a free and complete AMQP broker implementation.**

It implements version 0-9-1 of the AMQP specification; this is the most widespread version today and it is the last version that focuses on the client API.

## Connection to broker

```java
ConnectionFactory factory = new ConnectionFactory();
factory.setHost(rabbitMQhostname);
Connection connection = factory.newConnection();
Channel channel = connection.createChannel();



```

**In this recipe we have used the default connection parameters user: guest, password: guest, and vhost: /; we will discuss these parameters later.**

## Important: If you are developing multithreaded applications, it is highly recommended to use a different channel for each thread. If many threads use the same channel, they will serialize their execution in the channel method calls, leading to possible performance degradation.

## Important: The best practice is to open a connection and share it with different threads. Each thread creates, uses, and destroys its own independent channel(s).

### What is the Virtual Host?

Virtual hosts are administrative containers; they allow to configure many logically independent brokers hosts within one single RabbitMQ instance, to let many different independent applications share the same RabbitMQ server. Each virtual host can be configured with its independent set of permissions, exchanges, and queues and will work in a logically separated environment.

## Set Uri

```java
ConnectionFactory factory = new ConnectionFactory();
String uri="amqp://user:pass@hostname:port/vhost";
factory.setUri(uri);
```

## Producing Message

```java
String myQueue = "myFirstQueue";
channel.queueDeclare(myQueue, true, false, false, null);

//Send message
String message = "My message to myFirstQueue";
channel.basicPublish("",myQueue, null, message.getBytes());
```

## Important: If the queue already exists but has been created with different parameters, queueDeclare() will raise an exception.

### All the operations that need interactions with the broker are carried out through channels.

### Durable: This specifies whether the queue will survive server restarts. Note that it is required for a queue to be declared as durable if you want persistent messages to survive a server restart.

### Exclusive: This specifies whether the queue is restricted to only this connection.

### Important: If the queue has been declared with the durable flag set to true and the message has been marked as persistent, it is stored on the disk by the broker. 

### Consuming Messages:

By allowing both producers and consumers to declare the same queue, we are decoupling their existence; the order in which we start them is not important.

### Broadcasting Messages:

**Publisher**
```java
channel.exchangeDeclare(myExchange, "fanout");
channel.basicPublish(myExchange, "", null, jsonmessage.getBytes());
```

**Consumer**

```java
channel.exchangeDeclare(myExchange, "fanout");

//Autocreate a new temporary queue:
String queueName = channel.queueDeclare().getQueue();

//Bind the queue to the exchange:
channel.queueBind(queueName, myExchange, "");
```

### Topic Exchange

**Direct and topic exchanges are conceptually very similar to each other.**


```java
channel.exchangeDeclare(exchangeName, "topic", false, false, null);
channel.basicPublish(exchangeName, routingKey, null, jsonBook.getBytes());

//Consumer.cs
String myQueue = channel.queueDeclare().getQueue();
channel.queueBind(myQueue,exchangeName,bindingKey);
```

#: This matches zero or more words
*: This matches exactly one word

### Guaranteeing Message Processing

```java
channel.queueDeclare(myQueue, true, false, false,null);
ActualConsumer consumer = new ActualConsumer(channel);
boolean autoAck = false; // n.b.
channel.basicConsume(MyQueue, autoAck, consumer);

public void handleDelivery(String consumerTag,Envelope envelope, BasicPropertiesproperties,byte[] body) throws java.io.IOException {
String message = new String(body);
this.getChannel().basicAck(envelope.getDeliveryTag(),false);
```

### Important: If you forget to send ack mechanism:

If you don't send the ack back, the consumer continues to fetch subsequent messages; however, when you disconnect the consumer, all the messages will still be in the queue. Messages are not consumed until RabbitMQ receives the corresponding ack. Try to comment out the basicAck() call in the example to experiment this behavior.

### Messaging with transactions

You can use transactional messages by performing the following steps:

```java
channel.queueDeclare(myQueue, true, false, false, null);

//Set the channel to the transactional mode using:
channel.txSelect();
channel.basicPublish("", myQueue, MessageProperties.PERSISTENT_TEXT_PLAIN, message.getBytes()); 
channel.txCommit();
```

Note: The method txSelect()must be called at least once before txCommit() or txRollback().


### How to let messages expire

```java
channel.exchangeDeclare(exchange, "direct", false);
channel.queueDeclare(queue, false, false, false, null);
channel.queueBind(queue, exchange, routingKey);

BasicPropertiesmsgProperties = new BasicProperties.Builder().expiration("20000").build();
channel.basicPublish(exchange, routingKey, msgProperties, statMsg.getBytes());

```

### How to let messages expire on specific queues

```java
channel.exchangeDeclare(exchange, "direct", false);
Map<String, Object> arguments = new HashMap<String, Object>();
arguments.put("x-message-ttl", 10000);
channel.queueDeclare(queue, false, false, false, arguments);
channel.queueBind(queue, exchange, routingKey);

```

### How to let queues expire

In this third case, the TTL is not related to messages anymore, but to queues. This case is a perfect fit to manage server restarts and updates. 

```java
channel.exchangeDeclare(exchange, "direct", false);
Map<String, Object> arguments = new HashMap<String, Object>();
arguments.put("x-expires", 30000);
channel.queueDeclare(queue, false, false, false, arguments);
channel.queueBind(queue, exchange, routingKey);

```

### Managing rejected or expired messages
In this example, we show how to manage expired or rejected messages using dead letter exchanges. The dead letter exchange is a normal exchange where dead messages are redirected; if not specified, dead messages are just dropped by the broker.

```java
channel.exchangeDeclare(Constants.exchange, "direct", false);
channel.exchangeDeclare(Constants.exchange_dead_letter, "direct", false);

arguments.put("x-message-ttl", 10000);
arguments.put("x-dead-letter-exchange",exchange_dead_letter);
channel.queueDeclare(queue, false, false, false, arguments);
channel.queueBind(queue, exchange, "");

```

### Understanding the validated user-ID extension

According to AMQP, when a consumer gets a message, it doesn't know the sender. Generally, consumers should not care about who produced the messages; that's good for the producer-consumer decoupling. However, sometimes it's necessary for authentication, and RabbitMQ provides the validated user-ID extension for this purpose.

```java
channel.queueDeclare(queue, true, false, false, null);
BasicProperties messageProperties = new BasicProperties.Builder()
.timestamp(new Date())
.userId("guest");
channel.basicPublish("",queue, messageProperties, bookOrderMsg.getBytes());

```

### Using SSL

This recipe is very important but it has some detail. If you need to get more detail, may you can read the related book chapter. Moreover, the book covers management of rabbitmq server, replication of broker and other management stuff. With intent to not to copy/paste codes/notes from the book so much, this file does not comprises of that sections.

