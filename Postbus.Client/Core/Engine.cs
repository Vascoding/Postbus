using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Postbus.Startup;
using static Postbus.Startup.Postbus;

namespace Postbus.Client.Core
{
    class Engine
    {
        private readonly PostbusClient client;
        private List<string> channels;

        public Engine(PostbusClient client)
        {
            this.client = client;
            this.channels = new List<string>();
        }

        public async Task Run()
        {
            var response = await client.RevealChannelsAsync(new ChannelsRequest());
            this.channels = JsonConvert.DeserializeObject<List<string>>(response.Message);
            Console.WriteLine(string.Join(Environment.NewLine, this.channels));
            var input = string.Empty;
            while ((input = Console.ReadLine()) != "exit")
            {
                Console.WriteLine($"You entered {input}");
                while ((input = Console.ReadLine()) != "back")
                {
                    Console.WriteLine("type");
                }

                Console.WriteLine(string.Join(Environment.NewLine, this.channels));
            }
        }
    }
}
