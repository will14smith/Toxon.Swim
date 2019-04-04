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
                .WriteTo.Async(a => a.Console())
                .MinimumLevel.Debug()
                .CreateLogger();

            var client1 = new SwimClient(
                new SwimHost(new IPEndPoint(IPAddress.Loopback, 18210)),
                new SwimMeta(new Dictionary<string, string>()),
                new SwimClientOptions { Logger = logger }
            );
            await client1.StartAsync();

            var client2 = new SwimClient(
                new SwimHost(new IPEndPoint(IPAddress.Loopback, 18211)),
                new SwimMeta(new Dictionary<string, string>()),
                new SwimClientOptions { Logger = logger }
            );
            await client2.StartAsync();
            await client2.JoinAsync(new[] { client1.Local });

            Console.WriteLine("Both clients are running...");
            Console.ReadLine();
        }
    }
}
