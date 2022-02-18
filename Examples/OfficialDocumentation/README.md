# General Notes:

`The core idea in the messaging model in RabbitMQ is that the producer never sends any messages directly to a queue. Actually, quite often the producer doesn't even know if a message will be delivered to any queue at all.`

### Exchange
Instead, the producer can only send messages to an exchange. An exchange is a very simple thing. On one side it receives messages from producers and the other side it pushes them to queues

#### Exchange Types:
-   Direct
-   Topic
-   Headers
-   Fanout


