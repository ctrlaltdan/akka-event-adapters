# A sample application assessing the differences between akka persistence plugins and event adapters (using protobuf-net)

This is a test-bed playing with Akka.net event adapters to persist data using the protobuf-net library.

In short, the following event adapter is being used to persist data [here](Application/ProtobufEventAdapter.cs).

```
    public class ProtobufEventAdapter : IEventAdapter
    {
        public static object Warm() => null;
        private static readonly Dictionary<string, Type> Dtos;

        /// <summary>
        /// Scans the defined assemblies for classes decorated with a [JournalEntry(manifest)] attribute
        /// and adds the manifest property, and the class type to the static Dtos dictionary.
        /// </summary>
        static ProtobufEventAdapter()
        {
            var assemblies = new[]
            {
                typeof(ProtobufEventAdapter).Assembly
            };

            Dtos = assemblies
                .SelectMany(assembly => assembly.GetTypes(), (assembly, type) => new { assembly, type })
                .Select(assemblyAndType => new
                {
                    assemblyAndType.type,
                    attribute = assemblyAndType
                        .type
                        .GetCustomAttributes(typeof(JournalEntryAttribute), false)
                        .SingleOrDefault()
                })
                .Where(assemblyAndTypeAndAttributes => assemblyAndTypeAndAttributes.attribute != null)
                .ToDictionary(key => ((JournalEntryAttribute)key.attribute).Manifest, value => value.type);
        }

        public string Manifest(object evt)
        {
            var attribute = evt
                .GetType()
                .GetCustomAttributes(typeof(JournalEntryAttribute), false)
                .SingleOrDefault();

            if (attribute == null)
                throw new Exception("Attribute not found on Journal entry.");

            return ((JournalEntryAttribute)attribute).Manifest;
        }

        public object ToJournal(object evt)
        {
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, evt);
                return stream.ToArray();
            }
        }

        public IEventSequence FromJournal(object evt, string manifest)
        {
            if (!(evt is byte[] bytes))
                throw new Exception("Cannot cast journal entry as byte array.");

            if (!Dtos.TryGetValue(manifest, out var type))
                throw new Exception($"Unknown manifest type [{manifest}].");

            using (var stream = new MemoryStream(bytes))
            {
                return EventSequence.Single(ProtoBuf.Serializer.Deserialize(type, stream));
            }
        }
    }
``` 

### Results

**InMemory persistence**

✔️ `public string Manifest(object evt)` returns `Manifest` property from `JournalEntry` attribute.
✔️ `public object ToJournal(object evt)` converts object to byte[].
✔️ `public IEventSequence FromJournal(object evt, string manifest)` object is byte[], manifest is string declared above.

**Kafka persistence**

✔️ `public string Manifest(object evt)` returns `Manifest` property from `JournalEntry` attribute.
✔️ `public object ToJournal(object evt)` converts object to byte[].
❌ `public IEventSequence FromJournal(object evt, string manifest)` returns exception:
```
MongoDb: System.TypeLoadException: Could not load type '<my type defined above in Manifest(obj) above>' from assembly 'Akka.Persistence.MongoDb, Version=1.3.12.0
```

**Sql persistence**

✔️ `public string Manifest(object evt)` returns `Manifest` property from `JournalEntry` attribute.
✔️ `public object ToJournal(object evt)` converts object to byte[].
❌ `public IEventSequence FromJournal(object evt, string manifest)` object is byte[], manifest is empty string (empty string is what is stored in sql table).
