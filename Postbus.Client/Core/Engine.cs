using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
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
            using var streaming = client.ToChatRoom();
            Console.Write("Type your username: ");
            var username = Console.ReadLine();
            var input = string.Empty;
            while ((input = Console.ReadLine()) != "end")
            {
                var response = Task.Run(async () =>
                {
                    await foreach (var res in streaming.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine(res.Message);
                    }
                });

                await streaming.RequestStream.WriteAsync(new ChatRoomRequestStream { Chatroom = "General", Username = username, Message = input });
            }

            await streaming.RequestStream.CompleteAsync();
        }
    }
}
