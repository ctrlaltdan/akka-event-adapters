# A sample application using akka persistence plugins and event adapters (using protobuf-net)

This is a test-bed playing with Akka.net event adapters to persist data using the protobuf-net library.

In short;

1. Akka calls save method on a type of `IDto`
2. Event adapter receives this method and wraps the dto in a single known contract (called `ProtobufContract`)
3. On restore, instances of `ProtobufContract` are received by the event adapter and deserialized to their correct type
4. The persistent actor correctly receives a `Recover<IDto>(state => {})` message again
