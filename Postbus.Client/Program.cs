using System.Threading.Tasks;
using Grpc.Net.Client;
using Postbus.Client.Core;

namespace Postbus.Client
{
    class Program
    {
        static async Task Main()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Startup.Postbus.PostbusClient(channel);

            await new Engine(client).Run();
        }
    }
}
