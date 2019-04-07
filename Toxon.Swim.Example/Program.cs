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
                .MinimumLevel.Debug()
                .CreateLogger()
                ;

            AppDomain.CurrentDomain.UnhandledException += (_, ex) => { logger.Error((Exception)ex.ExceptionObject, "Unhandled Exception"); };


            var client1 = new SwimClient(
                new SwimHost(new IPEndPoint(IPAddress.Loopback, 18210)),
                new SwimMeta(new Dictionary<string, string>()),
                new SwimClientOptions { Logger = logger.ForContext("client", 18210) }
            );
            client1.UpdateMeta(new SwimMeta(new Dictionary<string, string> { { "client", "18210" } }));
            await client1.StartAsync();
            clients.Add(18210, client1);

            for (var i = 0; i < 5; i++)
            {
                var id = 18211 + i;
                var client2 = new SwimClient(
                    new SwimHost(new IPEndPoint(IPAddress.Loopback, id)),
                    new SwimMeta(new Dictionary<string, string>()),
                    new SwimClientOptions { Logger = logger.ForContext("client", id) }
                );
                await client2.StartAsync();
                await client2.JoinAsync(new[] { client1.Local });
                client2.UpdateMeta(new SwimMeta(new Dictionary<string, string> { { "client", $"{id}" } }));
                clients.Add(id, client2);
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
                            Console.WriteLine($"- {member.Host} - {member.Meta}");
                        }

                        Console.WriteLine();
                    }
                }
                else if (line == "c")
                {
                    foreach (var (id, client) in clients)
                    {
                        Console.WriteLine($"Client {id} - {client.Members.Count()}");
                    }
                }
                else if (line == "i")
                {
                    Console.WriteLine("Client Id: ");
                    var id = int.Parse(Console.ReadLine());
                    Console.WriteLine("New Client Id: ");
                    var newId = int.Parse(Console.ReadLine());

                    clients.Remove(id, out var client);
                    client.UpdateMeta(new SwimMeta(new Dictionary<string, string> { { "client", $"{newId}" } }));
                    clients.Add(newId, client);
                }
                else if (line == "k")
                {
                    Console.WriteLine("Client Id: ");
                    var id = int.Parse(Console.ReadLine());

                    clients.Remove(id, out var client);
                    await client.LeaveAsync();
                }
                else if (line == "s")
                {
                    Console.WriteLine("New Client Id: ");
                    var id = int.Parse(Console.ReadLine());

                    var client = new SwimClient(
                        new SwimHost(new IPEndPoint(IPAddress.Loopback, id)),
                        new SwimMeta(new Dictionary<string, string>()),
                        new SwimClientOptions { Logger = logger.ForContext("client", id) }
                    );
                    await client.StartAsync();
                    await client.JoinAsync(new[] { client1.Local });
                    client.UpdateMeta(new SwimMeta(new Dictionary<string, string> { { "client", $"{id}" } }));
                    clients.Add(id, client);
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
