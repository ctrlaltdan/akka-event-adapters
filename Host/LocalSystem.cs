using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Application.Actors;

namespace Host
{
    public class LocalSystem
    {
        public static readonly Config InMemory;
        public static readonly Config MongoDb;
        public static readonly Config Sql;

        static LocalSystem()
        {
            InMemory = ConfigurationFactory.ParseString(@"
                akka {
                    persistence {
                        journal {
                            plugin = ""akka.persistence.journal.application""
                            auto-start-journals = [""akka.persistence.journal.application""]

				            application {
                                class = ""Akka.Persistence.Journal.MemoryJournal, Akka.Persistence""
				                plugin-dispatcher = ""akka.actor.default-dispatcher""
				                auto-initialize = on

                                event-adapters {
                                    protobuf = ""Application.Persistence.ProtobufEventAdapter, Application""
                                }
                                event-adapter-bindings
                                {
                                    ""Application.Dtos.IDto, Application"" = protobuf
                                    ""Application.Persistence.ProtobufContract, Application"" = protobuf
                                }
                            }
                        }

                        snapshot-store {
                            plugin = ""akka.persistence.snapshot-store.application""
                            auto-start-snapshot-stores = [""akka.persistence.snapshot-store.application""]

			                application {
                                class = ""Akka.Persistence.Snapshot.MemorySnapshotStore, Akka.Persistence""
				                plugin-dispatcher = ""akka.actor.default-dispatcher""
				                auto-initialize = on
			                }
                        }
                    }
                }");

            MongoDb = ConfigurationFactory.ParseString(@"
                akka {
                    persistence {
                        journal {
                            plugin = ""akka.persistence.journal.mongodb""
                            mongodb {
                                class = ""Akka.Persistence.MongoDb.Journal.MongoDbJournal, Akka.Persistence.MongoDb""
                                connection-string = ""mongodb://localhost:xxxxxx@localhost:10255/admin?ssl=true""
                        
                                event-adapters {
                                    protobuf = ""Application.Persistence.ProtobufEventAdapter, Application""
                                }
                                event-adapter-bindings
                                {
                                     ""Application.Dtos.IDto, Application"" = protobuf
                                     ""Application.Persistence.ProtobufContract, Application"" = protobuf
                                }
                            }
                        }
                        snapshot-store {
                            plugin = ""akka.persistence.snapshot-store.mongodb""
                            mongodb {
                                class = ""Akka.Persistence.MongoDb.Snapshot.MongoDbSnapshotStore, Akka.Persistence.MongoDb""
                                connection-string = ""mongodb://localhost:xxxxxx@localhost:10255/admin?ssl=true""
                            }
                        }
                    }
                }
                ");

            Sql = ConfigurationFactory.ParseString(@"
                akka {
                    persistence {
                        journal {
                            plugin = ""akka.persistence.journal.application""
                            auto-start-journals = [""akka.persistence.journal.application""]

				            application {
				                class = ""Akka.Persistence.SqlServer.Journal.SqlServerJournal, Akka.Persistence.SqlServer""
				                plugin-dispatcher = ""akka.actor.default-dispatcher""
				                connection-string = ""Data Source=localhost;Initial Catalog=test;Integrated Security=True;MultipleActiveResultSets=true""
				                connection-timeout = 30s
				                schema-name = dbo
				                table-name = EventJournal
				                auto-initialize = on
				                timestamp-provider = ""Akka.Persistence.Sql.Common.Journal.DefaultTimestampProvider, Akka.Persistence.Sql.Common""
				                metadata-table-name = Metadata

                                event-adapters {
                                    protobuf = ""Application.Persistence.ProtobufEventAdapter, Application""
                                }
                                event-adapter-bindings
                                {
                                     ""Application.Dtos.IDto, Application"" = protobuf
                                     ""Application.Persistence.ProtobufContract, Application"" = protobuf
                                }
			                }
                        }

                        snapshot-store {
                            plugin = ""akka.persistence.snapshot-store.application""
                            auto-start-snapshot-stores = [""akka.persistence.snapshot-store.application""]

			                application {
				                class = ""Akka.Persistence.SqlServer.Snapshot.SqlServerSnapshotStore, Akka.Persistence.SqlServer""
				                plugin-dispatcher = ""akka.actor.default-dispatcher""
				                connection-string = ""Data Source=localhost;Initial Catalog=test;Integrated Security=True;MultipleActiveResultSets=true""
				                connection-timeout = 30s
				                schema-name = dbo
				                table-name = SnapshotStore
				                auto-initialize = on
			                }
                        }
                    }
                }");
        }

        public static SystemBuilder Bootstrap()
        {
            return new SystemBuilder();
        }

        public class SystemBuilder
        {
            private readonly Config _config;

            public SystemBuilder()
            {
                var hocon = ConfigurationFactory.ParseString(@"
                petabridge {
                    cmd {
                        host = ""0.0.0.0""
                        port = 9110
                    }
                }

                akka {
                    loggers = [""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]

                    loglevel = DEBUG

                    actor {
                        debug {
                            receive = on 
                            autoreceive = on
                            lifecycle = on
                            event-stream = on
                            unhandled = on
                        }
	                }
                }");

                

                _config = Config.Empty
                    //.WithFallback(InMemory)
                    //.WithFallback(MongoDb)
                    .WithFallback(Sql)
                    .WithFallback(hocon);
            }

            public void Build()
            {
                Instance = new LocalSystem(_config);
            }
        }

        public static LocalSystem Instance { get; private set; }

        public ActorSystem System { get; }
        public IActorRef Supervisor { get; }
        
        private LocalSystem(Config config)
        {
            System = ActorSystem.Create("test", config);

            //MongoDbPersistence.Get(System);

            Supervisor = System.ActorOf(Props.Create(() => new PlayerSupervisor()));
        }

        public Task KeepAlive() => System.WhenTerminated;

        private static readonly object Gate = new object();
        private static bool _isDown;

        public void Shutdown()
        {
            lock (Gate)
            {
                if (_isDown)
                    return;

                CoordinatedShutdown
                    .Get(System)
                    .Run(CoordinatedShutdown.ClrExitReason.Instance)
                    .Wait();

                _isDown = true;
            }
        }
    }
}
