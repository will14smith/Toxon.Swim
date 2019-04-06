using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Serilog;
using Toxon.Swim.Models;

namespace Toxon.Swim.Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Async(a => a.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}"))
                // .MinimumLevel.Debug()
                .CreateLogger()
                ;

            var client1 = new SwimClient(
                new SwimHost(new IPEndPoint(IPAddress.Loopback, 18210)),
                new SwimMeta(new Dictionary<string, string>()),
                new SwimClientOptions { Logger = logger.ForContext("client", 18210) }
            );
            await client1.StartAsync();

            for (var i = 0; i < 5; i++)
            {
                var client2 = new SwimClient(
                    new SwimHost(new IPEndPoint(IPAddress.Loopback, 18211 + i)),
                    new SwimMeta(new Dictionary<string, string>()),
                    new SwimClientOptions {Logger = logger.ForContext("client", 18211 + i)}
                );
                await client2.StartAsync();
                await client2.JoinAsync(new[] {client1.Local});
            }

            Console.WriteLine("All clients are running...");
            Console.ReadLine();
        }
    }
}
