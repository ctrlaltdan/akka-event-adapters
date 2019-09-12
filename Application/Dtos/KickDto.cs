using System;
using Application.Persistence;
using ProtoBuf;

namespace Application.Dtos
{
    [ProtoContract, JournalEntry("KICK_V1")]
    public class KickDto : IDto
    {
        public string HitType => "KICK";

        [ProtoMember(1)]
        public Guid Id { get; }

        [ProtoMember(2)]
        public DateTime Timestamp { get; }

        [ProtoMember(3)]
        public int Force { get; }

        public KickDto()
        {
        }

        public KickDto(Guid id, DateTime timestamp, int force)
        {
            Id = id;
            Timestamp = timestamp;
            Force = force;
        }
    }
}
