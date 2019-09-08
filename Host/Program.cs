using System;
using System.Runtime.Loader;
using Application;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace Host
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            ProtobufEventAdapter.Warm();

            LocalSystem
                .Bootstrap()
                .Build();

            host.Run();

            Console.CancelKeyPress += (sender, eventArgs) => LocalSystem.Instance.Shutdown();
            AssemblyLoadContext.Default.Unloading += _ => LocalSystem.Instance.Shutdown();

            LocalSystem.Instance
                .KeepAlive()
                .Wait();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration))
            .UseKestrel()
            .UseStartup<Startup>();
    }
}
