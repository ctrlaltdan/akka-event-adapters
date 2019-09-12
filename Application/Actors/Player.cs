using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using Akka.Event;
using Akka.Logger.Serilog;
using Akka.Persistence;
using Application.Dtos;
using Application.Persistence;

namespace Application.Actors
{
    public class Player : ReceivePersistentActor
    {
        public override string PersistenceId { get; }
        private const int SnapshotInterval = 3;

        private PlayerState _state = new PlayerState();
        private static ILoggingAdapter Logger => Context.GetLogger<SerilogLoggingAdapter>();
        
        public Player(string id)
        {
            PersistenceId = id;

            var stopwatch = Stopwatch.StartNew();

            Recover<SnapshotOffer>(offer =>
            {
                if (!(offer.Snapshot is ProtobufContracts contracts))
                {
                    Log.Error("Unhandled snapshot type {Type} received.", offer.Snapshot.GetType());
                    return;
                }
                foreach (var dto in contracts.Dtos)
                    _state = _state.Update(dto);
            });

            Recover<IDto>(state =>
            {
                _state = _state.Update(state);
            });

            Recover<RecoveryCompleted>(_ =>
            {
                Logger.Info("Recovery completed in {Duration}ms.", stopwatch.ElapsedMilliseconds);
                Become(Ready);
            });
        }

        private void Ready()
        {
            SetReceiveTimeout(TimeSpan.FromSeconds(5));

            var stopwatch = Stopwatch.StartNew();

            Command<GetHits>(_ =>
            {
                Sender.Tell(_state.Hits);
            });

            Command<Kick>(message =>
            {
                Logger.Info("{Id} received {Type}.", PersistenceId, message.GetType().Name);

                var dto = new KickDto(Guid.NewGuid(), DateTime.UtcNow, message.Force);
                Save(dto, () =>
                {
                    Sender.Tell("DONE");
                });
            });

            Command<Punch>(message =>
            {
                Logger.Info("{Id} received {Type}.", PersistenceId, message.GetType().Name);

                var dto = new PunchDto(Guid.NewGuid(), DateTime.UtcNow, message.Speed);
                Save(dto, () =>
                {
                    Sender.Tell("DONE");
                });
            });

            Command<Slap>(message =>
            {
                Logger.Info("{Id} received {Type}.", PersistenceId, message.GetType().Name);

                var dto = new SlapDto(Guid.NewGuid(), DateTime.UtcNow, message.SassFactor);
                Save(dto, () =>
                {
                    Sender.Tell("DONE");
                });
            });
            
            Command<SaveSnapshotSuccess>(success => Logger.Debug("Saved snapshot {SequenceNr}.", success.Metadata.SequenceNr));
            Command<SaveSnapshotFailure>(failure => Logger.Error(failure.Cause, "Failed to save snapshot {SequenceNr}.", failure.Metadata));
            Command<ReceiveTimeout>(_ =>
            {
                Logger.Info("Received timeout after {Duration}ms. Stopping Actor.", stopwatch.ElapsedMilliseconds);
                Context.Stop(Self);
            });
        }
        
        private void Save(IDto state, Action onSuccess)
        {
            var stopwatch = Stopwatch.StartNew();
            Persist(state, _ =>
            {
                Logger.Info("State {Type} persisted in {Duration}ms.", state.GetType().Name, stopwatch.ElapsedMilliseconds);
                _state = _state.Update(state);
                onSuccess.Invoke();
                if (LastSequenceNr % SnapshotInterval == 0 && LastSequenceNr != 0)
                {
                    SaveSnapshot(_state.Events.ToProtobufContracts());
                }
            });
        }

        public static Props Props(string id)
            => Akka.Actor.Props.Create(() => new Player(id));

        #region Messages
        public interface IPlayerMessage
        {
            string PlayerId { get; }
        }
        public class GetHits : IPlayerMessage
        {
            public string PlayerId { get; }

            public GetHits(string playerId)
            {
                PlayerId = playerId;
            }
        }
        public class Kick : IPlayerMessage
        {
            public string PlayerId { get; }
            public int Force { get; }

            public Kick(string playerId, int force)
            {
                PlayerId = playerId;
                Force = force;
            }
        }
        public class Punch : IPlayerMessage
        {
            public string PlayerId { get; }
            public int Speed { get; }

            public Punch(string playerId, int speed)
            {
                PlayerId = playerId;
                Speed = speed;
            }
        }
        public class Slap : IPlayerMessage
        {
            public string PlayerId { get; }
            public double SassFactor { get; }

            public Slap(string playerId, double sassFactor)
            {
                PlayerId = playerId;
                SassFactor = sassFactor;
            }
        }
        #endregion
    }
}
