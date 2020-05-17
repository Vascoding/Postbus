using System;
using System.Linq;
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
            string username;
            while (true)
            {
                username = Console.ReadLine();
                var resp = await this.client.RegisterAsync(new RegisterRequest { Username = username });
                if (resp.Success)
                {
                    Console.WriteLine("Successfully registered!!!");
                    break;
                }

                Console.WriteLine("Username is taken, please chose another one!!!");
                Console.Write("Type your username: ");
            }
            using var connection = client.OpenConnection(new Metadata 
            { 
                { "username", username }
            });

            this.ReadResponses(connection);
            string input;
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
                await connection.RequestStream.WriteAsync(new RequestStream { Chatroom = chatroom, Message = "Hello everyone :)", Toall = true });
                while ((input = Console.ReadLine()) != "exit")
                {
                    if (input.StartsWith("/msg"))
                    {
                        //await connection.RequestStream.WriteAsync(new RequestStream { Chatroom = chatroom, Message = input, Toall = false, Username = user });
                    }
                    else
                    {
                        await connection.RequestStream.WriteAsync(new RequestStream { Chatroom = chatroom, Message = input, Toall = true });
                    }
                }

                Console.Clear();

                var response = await this.client.ExitChatRoomAsync(new ExitRequest { Chatroom = chatroom, Username = username });

                Console.WriteLine(response.Message);
            }

            await connection.RequestStream.CompleteAsync();
        }

        private void ReadResponses(AsyncDuplexStreamingCall<RequestStream, ResponseStream> connection)
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
