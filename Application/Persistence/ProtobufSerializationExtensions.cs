using System.Collections.Generic;
using System.Linq;
using Application.Dtos;

namespace Application.Persistence
{
    public static class ProtobufSerializationExtensions
    {
        public static ProtobufContracts ToProtobufContracts(this IEnumerable<IDto> dtos) => new 
                ProtobufContracts(dtos
                    .Select(ProtobufSerialization.ToProtobufContract)
                    .ToList());

        public static List<IDto> ToDtos(this IEnumerable<ProtobufContract> protoContracts) => protoContracts
            .Select(ProtobufSerialization.FromProtobufContract)
            .ToList();
    }
}
