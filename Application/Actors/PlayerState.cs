using System.Collections.Immutable;
using System.Linq;
using Application.Dtos;

namespace Application.Actors
{
    internal class PlayerState
    {
        private readonly ImmutableList<object> _events;

        public PlayerState(ImmutableList<object> events)
        {
            _events = events;
        }

        public PlayerState()
            : this(ImmutableList.Create<object>())
        {
        }

        public PlayerState Update(object state)
        {
            return new PlayerState(_events.Add(state));
        }

        public string Hits => string.Join(",", _events.OfType<IDto>().Select(x => x.HitType));
    }
}
