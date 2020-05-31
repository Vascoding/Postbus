using System.Threading.Tasks;
using Grpc.Net.Client;
using Postbus.Client.Core;
using Postbus.Client.Enums;
using Postbus.Client.IO;

namespace Postbus.Client
{
    class Program
    {
        static async Task Main()
        {
            using var channel = GrpcChannel.ForAddress(Configuration.GetHostName(AppMode.Development));

            await new Engine(
                new Startup.Postbus.PostbusClient(channel),
                new InputReader(),
                new OutputWriter())
                .Run();
        }
    }
}