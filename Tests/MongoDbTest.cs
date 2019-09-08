using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Application.Actors;
using Host;
using Xunit;

namespace Tests
{
    public class MongoDbTest : TestKit
    {
        public MongoDbTest()
            : base(LocalSystem.MongoDb)
        {
        }
        
        [Fact]
        public async Task Test()
        {
            const string playerId = "Yoshimitsu";

            var actor1 = Sys.ActorOf(Player.Props(playerId));

            var hit1 = await actor1.Ask<string>(new Player.Kick(playerId, new Random().Next(0,10)), TimeSpan.FromSeconds(3));
            var hit2 = await actor1.Ask<string>(new Player.Slap(playerId, new Random().NextDouble()), TimeSpan.FromSeconds(3));

            Assert.Equal("DONE", hit1);
            Assert.Equal("DONE", hit2);

            Sys.Stop(actor1);


            var actor2 = Sys.ActorOf(Player.Props(playerId));
            
            var hits = await actor2.Ask<string>(new Player.GetHits(playerId), TimeSpan.FromSeconds(3));

            Assert.Equal("KICK,SLAP", hits);
        }
    }
}