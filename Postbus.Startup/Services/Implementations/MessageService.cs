using Microsoft.Extensions.Logging;
using Postbus.Startup.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Implementations
{
    internal class MessageService : IMessageService
    {
        private readonly ILogger<MessageService> logger;
        private readonly IRepository<ResponseStream> repository;
        private readonly IChatRoomService chatRoomService;
        public MessageService(ILogger<MessageService> logger, IRepository<ResponseStream> repository, IChatRoomService chatRoomService)
        {
            this.logger = logger;
            this.repository = repository;
            this.chatRoomService = chatRoomService;
        }

        public async Task BroadcastSingleDirection(string to, string message, string from) =>
            await this.WriteToOne(to, $"{from}: {message}");

        public async Task BroadcastDualDirection(string to, string message, string from)
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

        public async Task BroadcastToAll(string chatRoom, string message, string from) =>
            await this.WriteToAll(chatRoom, $"{from}: {message}");

        private async Task WriteToAll(string chatRoom, string message)
        {
            var subscribersToRemove = new List<string>();

            foreach (var username in await this.chatRoomService.GetUsersPerChatRoom(chatRoom))
            {
                var success = await this.WriteToOne(username, message);

                if (!success)
                {
                    subscribersToRemove.Add(username);
                }
            }

            await this.chatRoomService.RemoveSubscribersFromChatRoom(chatRoom, subscribersToRemove);
        }

        private async Task<bool> WriteToOne(string username, string message)
        {
            try
            {
                await this.repository.GetByUsername(username).WriteAsync(new ResponseStream { Message = message });
                return true;
            }
            catch
            {
                this.logger.LogInformation($"the connection for user {username} was lost");
                await this.repository.Unregister(username);
                return false;
            }
        }
    }
}
