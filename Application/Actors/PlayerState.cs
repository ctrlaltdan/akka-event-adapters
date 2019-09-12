using System.Collections.Immutable;
using System.Linq;
using Application.Dtos;

namespace Application.Actors
{
    internal class PlayerState
    {
        public readonly ImmutableList<IDto> Events;

        public PlayerState(ImmutableList<IDto> events)
        {
            Events = events;
        }

        public PlayerState()
            : this(ImmutableList.Create<IDto>())
        {
        }

        public PlayerState Update(IDto state)
        {
            return new PlayerState(Events.Add(state));
        }

        public string Hits => string.Join(",", Events.Select(x => x.HitType));
    }
}
