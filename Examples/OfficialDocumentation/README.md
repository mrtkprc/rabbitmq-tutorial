# General Notes:

`The core idea in the messaging model in RabbitMQ is that the producer never sends any messages directly to a queue. Actually, quite often the producer doesn't even know if a message will be delivered to any queue at all.`

### Exchange
Instead, the producer can only send messages to an exchange. An exchange is a very simple thing. On one side it receives messages from producers and the other side it pushes them to queues

#### Exchange Types:
-   Direct
-   Topic
-   Headers
-   Fanout

`A binding is a relationship between an exchange and a queue`

`The fanout exchanges, which we used previously, simply ignored its value.`

`We will use a direct exchange instead. The routing algorithm behind a direct exchange is simple - a message goes to the queues whose binding key exactly matches the routing key of the message.`

`It is perfectly legal to bind multiple queues with the same binding key. In our example we could add a binding between X and Q1 with binding key black. In that case, the direct exchange will behave like fanout and will broadcast the message to all the matching queues.`

`Direct exchange can't do routing based on multiple criteria.`

For topics:

You might know this concept from the syslog unix tool, which routes logs based on both severity (info/warn/crit...) and facility (auth/cron/kern...).

**To implement that in our logging system we need to learn about a more complex topic exchange.**

### Topic exchange
```Messages sent to a topic exchange can't have an arbitrary routing_key - it must be a list of words, delimited by dots. The words can be anything, but usually they specify some features connected to the message```

A few valid routing key examples: "stock.usd.nyse", "nyse.vmw", "quick.orange.rabbit". There can be as many words in the routing key as you like, up to the limit of 255 bytes.

The binding key must also be in the same form. The logic behind the topic exchange is similar to a direct one - a message sent with a particular routing key will be delivered to all the queues that are bound with a matching binding key. However there are two important special cases for binding keys:


```
* (star) can substitute for exactly one word.
# (hash) can substitute for zero or more words.
```


### Topic exchange is powerful and can behave like other exchanges.

```
Topic exchange

When a queue is bound with "#" (hash) binding key - it will receive all the messages, regardless of the routing key - like in fanout exchange.

When special characters "*" (star) and "#" (hash) aren't used in bindings, the topic exchange will behave just like a direct one.
```

### RPC

But what if we need to run a function on a remote computer and wait for the result? Well, that's a different story. This pattern is commonly known as Remote Procedure Call or RPC.

**A note on RPC**
```
Although RPC is a pretty common pattern in computing, it's often criticised. The problems arise when a programmer is not aware whether a function call is local or if it's a slow RPC. Confusions like that result in an unpredictable system and adds unnecessary complexity to debugging. Instead of simplifying software, misused RPC can result in unmaintainable spaghetti code
Bearing that in mind, consider the following advice:

Make sure it's obvious which function call is local and which is remote.
Document your system. Make the dependencies between components clear.
Handle error cases. How should the client react when the RPC server is down for a long time?
```

```
Message properties
The AMQP 0-9-1 protocol predefines a set of 14 properties that go with a message. Most of the properties are rarely used, with the exception of the following:

Persistent: Marks a message as persistent (with a value of true) or transient (any other value). Take a look at the second tutorial.
DeliveryMode: those familiar with the protocol may choose to use this property instead of Persistent. They control the same thing.
ContentType: Used to describe the mime-type of the encoding. For example for the often used JSON encoding it is a good practice to set this property to: application/json.
ReplyTo: Commonly used to name a callback queue.
CorrelationId: Useful to correlate RPC responses with requests.
```

If we see an unknown CorrelationId value, we may safely discard the message - it doesn't belong to our requests.

Our RPC will work like this:

![Reply Queue](./img/reply-queue.png)

* When the Client starts up, it creates an anonymous exclusive callback queue.
* For an RPC request, the Client sends a message with two properties: ReplyTo, which is set to the callback queue and CorrelationId, which is set to a unique value for every request.
* The request is sent to an rpc_queue queue.
* The RPC worker (aka: server) is waiting for requests on that queue. When a request appears, it does the job and sends a message with the result back to the Client, using the queue from the ReplyTo property.
* The client waits for data on the callback queue. When a message appears, it checks the CorrelationId property. If it matches the value from the request it returns the response to the application.
