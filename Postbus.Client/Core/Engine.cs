using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Postbus.Client.IO;
using Postbus.Startup;
using static Postbus.Startup.Postbus;

namespace Postbus.Client.Core
{
    class Engine
    {
        private readonly PostbusClient client;
        private readonly InputReader reader;
        private readonly OutputWriter writer;

        public Engine(PostbusClient client, InputReader reader, OutputWriter writer)
        {
            this.client = client;
            this.reader = reader;
            this.writer = writer;
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
                    this.writer.WriteLine(res.Message);
                }
            });
        }

        private async Task<string> Register()
        {
            this.writer.Clear();
            this.writer.Write("Type your username: ");
            string username = this.reader.ReadLine();
            var response = await this.client.RegisterAsync(new RegisterRequest { Username = username });

            if (response.Success)
            {
                this.writer.WriteLine("Successfully registered!!!");
                return username;
            }

            this.writer.WriteLine("Username is taken, please chose another one!!!");
            this.writer.Write("Type your username: ");

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

                this.writer.Clear();

                var response = await this.client.ExitChatRoomAsync(new ExitRequest { Chatroom = chatroom, Username = username });

                this.writer.WriteLine(response.Message);
            }
        }

        private async Task<string> EnterChatRoom()
        {
            var chatRoomsResponse = await this.client.RevealChatRoomsAsync(new ChatRoomsRequest());
            var chatRooms = JsonConvert.DeserializeObject<string[]>(chatRoomsResponse.Message);
            this.writer.WriteLine(string.Join(Environment.NewLine, chatRooms));
            
            while (true)
            {
                this.writer.Write("Enter Chat Room: ");
                var chatRoom = this.reader.ReadLine();

                if (chatRooms.Contains(chatRoom) || chatRoom == "exit")
                {
                    return chatRoom;
                }

                this.writer.WriteLine("Invalid Chat Room");
            }
        }

        private async Task RevealUsers(string chatroom, AsyncDuplexStreamingCall<RequestStream, ResponseStream> connection)
        {
            var usersResponse = await this.client.RevealUsersAsync(new UsersRequest() { Chatroom = chatroom });
            var users = JsonConvert.DeserializeObject<string[]>(usersResponse.Message);
            this.writer.Clear();
            this.writer.WriteLine(string.Join(Environment.NewLine, users));
            await connection.RequestStream.WriteAsync(new RequestStream { Chatroom = chatroom, Message = "Hello everyone :)", Toall = true });
        }

        private async Task StartTyping(string chatroom, AsyncDuplexStreamingCall<RequestStream, ResponseStream> connection)
        {
            string input;

            while ((input = this.reader.ReadLine()) != "exit")
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

            while ((input = this.reader.ReadLine()) != "exit")
            {
                await connection.RequestStream.WriteAsync(new RequestStream { Message = input, Toall = false, Username = username });
            }
        }

        private async Task TypeToMany(string input, string chatroom, AsyncDuplexStreamingCall<RequestStream, ResponseStream> connection) =>
            await connection.RequestStream.WriteAsync(new RequestStream { Chatroom = chatroom, Message = input, Toall = true });
    }
}