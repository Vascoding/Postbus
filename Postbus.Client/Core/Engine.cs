using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
        public Engine(PostbusClient client)
        {
            this.client = client;
        }

        public async Task Run()
        {
            Console.Write("Type your username: ");
            var guid = Guid.NewGuid().ToString();
            var username = Console.ReadLine();
            using var connection = client.OpenConnection(new Metadata 
            { 
                { "guid",  guid },
                { "username", username }

            });

            this.ReadResponses(connection);
            var input = string.Empty;
            while (true)
            {
                var chatRoomsResponse = await this.client.RevealChatRoomsAsync(new ChatRoomsRequest());
                var chatRooms = JsonConvert.DeserializeObject<string[]>(chatRoomsResponse.Message);
                Console.WriteLine(string.Join(Environment.NewLine, chatRooms));
                Console.Write("Enter Chat Room: ");
                var chatroom = Console.ReadLine();
                if (chatroom == "exit")
                {
                    break;
                }

                var usersResponse = await this.client.RevealUsersAsync(new UsersRequest() { Chatroom = chatroom });
                var users = JsonConvert.DeserializeObject<string[]>(usersResponse.Message);
                Console.Clear();
                Console.WriteLine(string.Join(Environment.NewLine, users));
                await connection.RequestStream.WriteAsync(new ChatRoomRequestStream { Chatroom = chatroom, Message = "Hello everyone :)", Toall = true });
                while ((input = Console.ReadLine()) != "exit")
                {
                    await connection.RequestStream.WriteAsync(new ChatRoomRequestStream { Chatroom = chatroom, Message = input, Toall = true });
                }

                Console.Clear();

                var response = await this.client.ExitChatRoomAsync(new ExitRequest { Chatroom = chatroom, Guid = guid });

                Console.WriteLine(response.Message);
            }

            await connection.RequestStream.CompleteAsync();
        }

        private void ReadResponses(AsyncDuplexStreamingCall<ChatRoomRequestStream, ChatRoomResponseStream> connection)
        {
            Task.Run(async () =>
            {
                await foreach (var res in connection.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine(res.Message);
                }
            });
        }
    }
}
