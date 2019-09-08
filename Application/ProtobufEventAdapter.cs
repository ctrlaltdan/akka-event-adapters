using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.Persistence.Journal;

namespace Application
{
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
}
