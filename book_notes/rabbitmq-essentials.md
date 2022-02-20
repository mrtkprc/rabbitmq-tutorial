# Book:  RabbitMQ Essentials - Second Edition

## Messageing Queue
Messaging or message queuing is a method of communication between applications or components. 
Thanks to message queues, these applications can remain completely separate as they process their individual tasks. 

Messages are typically small requests, replies, status updates, or even just information.

RabbitMQ is an open source message broker that acts as the intermediary or middleman for independent applications, giving them a common platform to communicate. RabbitMQ mainly uses an Erlang-based implementation of the Advanced Message Queuing Protocol (AMQP), which supports advanced features such as clustering and the complex routing of messages.

Message queuing is a one-way style of interaction where one system asynchronously interacts with another system via messages, generally through a message broker. A requesting system in asynchronous communication mode does not wait for an answer or require return information; it continues processing no matter what.

The most common example of such an interaction is an email.

One big advantage of the messaging queuing approach is that systems become loosely coupled with each other. They do not need to know the location of other nodes on the network; a mere name is enough to reach them.

![Broker](https://learning.oreilly.com/api/v2/epubs/urn:orm:book:9781789131666/files/assets/09a9fe21-6b50-4db2-8496-81930a935750.png)

## Meet AMQP

