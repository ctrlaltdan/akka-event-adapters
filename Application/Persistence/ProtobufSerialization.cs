using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.Persistence.Journal;
using Application.Dtos;

namespace Application.Persistence
{
    public static class ProtobufSerialization
    {
        public static object Warm() => null;
        public static readonly string ProtoContractType = typeof(ProtobufContract).AssemblyQualifiedName;

        private static readonly Dictionary<string, Type> Dtos;

        /// <summary>
        /// Scans the defined assemblies for classes decorated with a [JournalEntry(manifest)] attribute
        /// and adds the manifest property, and the class type to the static Dtos dictionary.
        /// </summary>
        static ProtobufSerialization()
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

        public static ProtobufContract ToProtobufContract(object evt)
        {
            var attribute = evt
                .GetType()
                .GetCustomAttributes(typeof(JournalEntryAttribute), false)
                .SingleOrDefault();

            if (attribute == null)
                throw new Exception("Attribute not found on Journal entry.");

            var type = ((JournalEntryAttribute)attribute).Manifest;

            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, evt);
                return new ProtobufContract(type, stream.ToArray());
            }
        }

        public static IDto FromProtobufContract(object evt)
        {
            if (!(evt is ProtobufContract protobuf))
                throw new Exception("Cannot cast journal entry as protobuf contract.");

            if (!Dtos.TryGetValue(protobuf.Type, out var type))
                throw new Exception($"Unknown dto type [{protobuf.Type}].");

            using (var stream = new MemoryStream(protobuf.Body))
            {
                return ProtoBuf.Serializer.Deserialize(type, stream) as IDto;
            }
        }
    }
}
