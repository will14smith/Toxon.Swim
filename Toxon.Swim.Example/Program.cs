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
            var clients = new Dictionary<int, SwimClient>();

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
            clients.Add(18210, client1);

            for (var i = 0; i < 5; i++)
            {
                var client2 = new SwimClient(
                    new SwimHost(new IPEndPoint(IPAddress.Loopback, 18211 + i)),
                    new SwimMeta(new Dictionary<string, string>()),
                    new SwimClientOptions { Logger = logger.ForContext("client", 18211 + i) }
                );
                await client2.StartAsync();
                await client2.JoinAsync(new[] { client1.Local });
                clients.Add(18211 + i, client2);
            }

            Console.WriteLine("All clients are running...");
            while (true)
            {
                var line = Console.ReadLine()?.Trim().ToLower();
                if (line == "m")
                {
                    foreach (var (id, client) in clients)
                    {
                        Console.WriteLine($"Client {id}:");
                        foreach (var member in client.Members.GetAll())
                        {
                            Console.WriteLine($"- {member.Host}");
                        }

                        Console.WriteLine();
                    }
                }
                else
                {
                    break;
                }
            }

            foreach (var (_, client) in clients)
            {
                await client.LeaveAsync();
            }
        }
    }
}
