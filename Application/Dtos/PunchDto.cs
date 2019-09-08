using System;
using ProtoBuf;

namespace Application.Dtos
{
    [ProtoContract, JournalEntry("PUNCH_V1")]
    public class PunchDto : IDto
    {
        public string HitType => "PUNCH";

        [ProtoMember(1)]
        public Guid Id { get; }

        [ProtoMember(2)]
        public DateTime Timestamp { get; }

        [ProtoMember(3)]
        public int Speed { get; }

        public PunchDto()
        {
        }

        public PunchDto(Guid id, DateTime timestamp, int speed)
        {
            Id = id;
            Timestamp = timestamp;
            Speed = speed;
        }
    }
}
