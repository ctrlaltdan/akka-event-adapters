using Akka.Actor;

namespace Application.Actors
{
    public class PlayerSupervisor : ReceiveActor
    {
        public PlayerSupervisor()
        {
            Receive<Player.IPlayerMessage>(message =>
            {
                var name = $"{message.PlayerId}";

                var childDoesntExist = Context
                    .Child(name)
                    .Equals(ActorRefs.Nobody);

                if (childDoesntExist)
                    Context.ActorOf(Player.Props(message.PlayerId), name);

                Context
                    .Child(name)
                    .Forward(message);
            });
        }
    }
}
