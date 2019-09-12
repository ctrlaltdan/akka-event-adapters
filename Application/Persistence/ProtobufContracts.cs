using System.Collections.Generic;
using Application.Dtos;

namespace Application.Persistence
{
    public class ProtobufContracts
    {
        public List<ProtobufContract> Contracts { get; }
        public IEnumerable<IDto> Dtos => Contracts.ToDtos();

        public ProtobufContracts(List<ProtobufContract> contracts)
        {
            Contracts = contracts;
        }
    }
}