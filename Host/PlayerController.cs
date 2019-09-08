using System;
using System.Threading.Tasks;
using Akka.Actor;
using Application.Actors;
using Microsoft.AspNetCore.Mvc;

namespace Host
{
    [ApiController, Route("player/{id}")]
    public class PlayerController : ControllerBase
    {
        private static readonly Guid TestId = new Guid("11e1a600-b7b1-47a1-b4f2-0d4f3abbc87a");

        [HttpGet("")]
        public async Task<IActionResult> GetHits(string id)
        {
            var result = await LocalSystem.Instance
                .Supervisor
                .Ask<string>(new Player.GetHits(id), TimeSpan.FromSeconds(3));

            return Ok(result);
        }

        [HttpGet("kick")]
        public async Task<IActionResult> Kick(string id)
        {
            var result = await LocalSystem.Instance
                .Supervisor
                .Ask<string>(new Player.Kick(id, new Random().Next(0, 10)), TimeSpan.FromSeconds(3));

            return Ok(result);
        }

        [HttpGet("punch")]
        public async Task<IActionResult> Punch(string id)
        {
            var result = await LocalSystem.Instance
                .Supervisor
                .Ask<string>(new Player.Punch(id, new Random().Next(0, 10)), TimeSpan.FromSeconds(3));

            return Ok(result);
        }

        [HttpGet("slap")]
        public async Task<IActionResult> Slap(string id)
        {
            var result = await LocalSystem.Instance
                .Supervisor
                .Ask<string>(new Player.Slap(id, new Random().NextDouble()), TimeSpan.FromSeconds(3));

            return Ok(result);
        }
    }
}
