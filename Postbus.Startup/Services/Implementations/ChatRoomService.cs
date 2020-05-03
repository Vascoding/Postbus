using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Postbus.Startup.Services.Interfaces;

namespace Postbus.Startup.Services.Implementations
{
    public class ChatRoomService : IChatRoomService
    {
        private readonly IRepository repository;
        private readonly ConcurrentDictionary<string, List<IServerStreamWriter<ChatRoomResponseStream>>> subscribersByChatRoom;

        public ChatRoomService(IRepository repository)
        {
            this.repository = repository;
            this.subscribersByChatRoom = this.InitChatRooms();
        }

        public bool IsRegistered(string chatRoom, IServerStreamWriter<ChatRoomResponseStream> stream)
        {
            if (!this.subscribersByChatRoom.ContainsKey(chatRoom))
            {
                return false;
            }

            return this.subscribersByChatRoom[chatRoom].Contains(stream);
        }

        public async Task RegisterAsync(string chatRoom, IServerStreamWriter<ChatRoomResponseStream> stream)
        {
            await Task
                .Run(() => this.subscribersByChatRoom
                .AddOrUpdate(chatRoom, new List<IServerStreamWriter<ChatRoomResponseStream>> { stream }, (key, value) => {
                    value.Add(stream);
                    return value;
                }));
        }

        public async Task BroadcastMessageAsync(string chatRoom, string message)
        {
            if (!this.subscribersByChatRoom.ContainsKey(chatRoom))
            {
                return;
            }

            foreach (var subscriber in this.subscribersByChatRoom[chatRoom])
            {
                try
                {
                    await subscriber.WriteAsync(new ChatRoomResponseStream { Message = message });
                }
                catch (Exception)
                {
                    Console.WriteLine($"the connection for user {subscriber} was lost");
                }
            }
        }

        public async Task<string[]> GetAvailableChatRoomsAsync() =>
            await Task.Run(() => this.subscribersByChatRoom.Keys.ToArray());

        private ConcurrentDictionary<string, List<IServerStreamWriter<ChatRoomResponseStream>>> InitChatRooms()
        {
            var availableChatRooms = new ConcurrentDictionary<string, List<IServerStreamWriter<ChatRoomResponseStream>>>();
            availableChatRooms.TryAdd("Team", new List<IServerStreamWriter<ChatRoomResponseStream>>());
            availableChatRooms.TryAdd("General", new List<IServerStreamWriter<ChatRoomResponseStream>>());
            availableChatRooms.TryAdd("Spam", new List<IServerStreamWriter<ChatRoomResponseStream>>());
            return availableChatRooms;
        }
    }
}
