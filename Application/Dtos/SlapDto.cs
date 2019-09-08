using System;
using ProtoBuf;

namespace Application.Dtos
{
    [ProtoContract, JournalEntry("SLAP_V1")]
    public class SlapDto : IDto
    {
        public string HitType => "SLAP";

        [ProtoMember(1)]
        public Guid Id { get; }

        [ProtoMember(2)]
        public DateTime Timestamp { get; }

        [ProtoMember(3)]
        public double SassFactor { get; }

        public SlapDto()
        {
        }

        public SlapDto(Guid id, DateTime timestamp, double sassFactor)
        {
            Id = id;
            Timestamp = timestamp;
            SassFactor = sassFactor;
        }
    }
}
