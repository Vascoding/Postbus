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
            var username = await this.Register();

            using var connection = client.OpenConnection(new Metadata { { "username", username } });

            this.ReadResponses(connection);

            await this.EnterChat(username, connection);

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

        private async Task<string> Register()
        {
            Console.Clear();
            Console.Write("Type your username: ");
            string username = Console.ReadLine();
            var resp = await this.client.RegisterAsync(new RegisterRequest { Username = username });

            if (resp.Success)
            {
                Console.WriteLine("Successfully registered!!!");
                return username;
            }

            Console.WriteLine("Username is taken, please chose another one!!!");
            Console.Write("Type your username: ");

            return await this.Register();
        }

        private async Task EnterChat(string username, AsyncDuplexStreamingCall<RequestStream, ResponseStream> connection)
        {
            while (true)
            {
                var chatroom = await this.EnterChatRoom();
                if (chatroom == "exit")
                {
                    break;
                }

                await this.RevealUsers(chatroom, connection);

                await this.StartTyping(chatroom, connection);

                Console.Clear();

                var response = await this.client.ExitChatRoomAsync(new ExitRequest { Chatroom = chatroom, Username = username });

                Console.WriteLine(response.Message);
            }
        }

        private async Task<string> EnterChatRoom()
        {
            var chatRoomsResponse = await this.client.RevealChatRoomsAsync(new ChatRoomsRequest());
            var chatRooms = JsonConvert.DeserializeObject<string[]>(chatRoomsResponse.Message);
            Console.WriteLine(string.Join(Environment.NewLine, chatRooms));
            
            while (true)
            {
                Console.Write("Enter Chat Room: ");
                var chatRoom = Console.ReadLine();

                if (chatRooms.Contains(chatRoom) || chatRoom == "exit")
                {
                    return chatRoom;
                }

                Console.WriteLine("Invalid Chat Room");
            }
        }

        private async Task RevealUsers(string chatroom, AsyncDuplexStreamingCall<RequestStream, ResponseStream> connection)
        {
            var usersResponse = await this.client.RevealUsersAsync(new UsersRequest() { Chatroom = chatroom });
            var users = JsonConvert.DeserializeObject<string[]>(usersResponse.Message);
            Console.Clear();
            Console.WriteLine(string.Join(Environment.NewLine, users));
            await connection.RequestStream.WriteAsync(new RequestStream { Chatroom = chatroom, Message = "Hello everyone :)", Toall = true });
        }

        private async Task StartTyping(string chatroom, AsyncDuplexStreamingCall<RequestStream, ResponseStream> connection)
        {
            string input;

            while ((input = Console.ReadLine()) != "exit")
            {
                await this.ProcessInput(input, chatroom, connection);
            }
        }

        private async Task ProcessInput(string input, string chatroom, AsyncDuplexStreamingCall<RequestStream, ResponseStream> connection)
        {
            if (input.StartsWith("/msg"))
            {
                await this.TypeToOne(input, connection);
            }
            else
            {
                await this.TypeToMany(input, chatroom, connection);
            }
        }

        private async Task TypeToOne(string input, AsyncDuplexStreamingCall<RequestStream, ResponseStream> connection) 
        {
            var username = input.Replace("/msg", "").Trim();

            while ((input = Console.ReadLine()) != "exit")
            {
                await connection.RequestStream.WriteAsync(new RequestStream { Message = input, Toall = false, Username = username });
            }
        }

        private async Task TypeToMany(string input, string chatroom, AsyncDuplexStreamingCall<RequestStream, ResponseStream> connection) =>
            await connection.RequestStream.WriteAsync(new RequestStream { Chatroom = chatroom, Message = input, Toall = true });
    }
}