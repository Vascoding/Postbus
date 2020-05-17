using Postbus.Internals.Extentions;
using Postbus.Startup.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Implementations
{
    internal class MessageService : IMessageService
    {
        private readonly IRepository<ResponseStream> repository;
        private readonly IChatRoomService chatRoomService;
        public MessageService(IRepository<ResponseStream> repository, IChatRoomService chatRoomService)
        {
            this.repository = repository;
            this.chatRoomService = chatRoomService;
        }

        public async Task BroadcastToOneSingleDirectionAsync(string to, string message, string from) =>
            await this.WriteToOne(to, $"{from}: {message}");

        public async Task BroadcastToOneDualDirectionAsync(string to, string message, string from)
        {
            var success = await this.WriteToOne(to, $"{from}: {message}");

            if (success)
            {
                await this.WriteToOne(from, $"{from}: {message}");
            }
            else
            {
                await this.WriteToOne(from, $"User has been disconected!");
            }
        }

        public async Task BroadcastToAllAsync(string chatRoom, string message, string from)
        {
            await this.WriteToAll(chatRoom, $"{from}: {message}");
        }

        private async Task WriteToAll(string chatRoom, string message)
        {
            var subscribersToRemove = new List<string>();

            foreach (var username in this.chatRoomService.GetUsersPerChatRoom(chatRoom))
            {
                var success = await this.WriteToOne(username, message);

                if (!success)
                {
                    subscribersToRemove.Add(username);
                }
            }

            subscribersToRemove.ForEach(s => this.chatRoomService.GetUsersPerChatRoom(chatRoom).Remove(s));
        }

        private async Task<bool> WriteToOne(string username, string message)
        {
            try
            {
                await this.repository.GetByUsername(username).WriteAsync(new ResponseStream { Message = message });
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine($"the connection for user {username} was lost");
                this.repository.UnregisterAsync(username);
                return false;
            }
        }
    }
}
