using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Postbus.Startup.Services.Interfaces;

namespace Postbus.Startup.Services.Implementations
{
    internal class ChatRoomService : IChatRoomService
    {
        private readonly ConcurrentDictionary<string, List<Guid>> subscribersByChatRoom;
        private readonly IRepository repository;

        public ChatRoomService(IRepository repository)
        {
            this.repository = repository;
            this.subscribersByChatRoom = this.InitChatRooms();
        }

        public bool IsRegistered(string chatRoom, Guid guid)
        {
            if (!this.HasChatRoom(chatRoom))
            {
                return false;
            }

            return this.subscribersByChatRoom[chatRoom].Any(u => u == guid);
        }

        public async Task RegisterAsync(string chatRoom, Guid guid)
        {
            await Task
                .Run(() => this.subscribersByChatRoom
                .AddOrUpdate(chatRoom, new List<Guid> { guid }, (key, value) => {
                    value.Add(guid);
                    return value;
                }));
        }

        public async Task<string> UnRegisterAsync(string chatRoom, Guid guid)
        {
            if (!this.HasChatRoom(chatRoom))
            {
                return "Chat room not found!!!";
            }

            var subscriber = this.subscribersByChatRoom[chatRoom].FirstOrDefault(u => u == guid);

            if (subscriber == null)
            {
                return "User not found!!!";
            }

            this.subscribersByChatRoom[chatRoom].Remove(subscriber);

            await this.WriteToAll(chatRoom, $"{this.repository.Subscribers[guid].Username} has left the chat room");

            return $"You left chat room {chatRoom}!!!";
        }

        public async Task BroadcastMessageAsync(string chatRoom, Guid guid, string message)
        {
            if (!this.HasChatRoom(chatRoom))
            {
                return;
            }

            await this.WriteToAll(chatRoom, $"{this.repository.Subscribers[guid].Username}: {message}");
        }

        public string[] GetAvailableChatRooms() =>
            this.subscribersByChatRoom.Keys.ToArray();

        public string[] GetUsersPerChatRoom(string chatRoom)
        {
            if (!this.HasChatRoom(chatRoom))
            {
                return Array.Empty<string>();
            }

            return this.repository
                .Subscribers
                .Where(s => this.subscribersByChatRoom[chatRoom].Contains(s.Key))
                .Select(a => a.Value.Username).ToArray();
        }

        private ConcurrentDictionary<string, List<Guid>> InitChatRooms()
        {
            var availableChatRooms = new ConcurrentDictionary<string, List<Guid>>();
            availableChatRooms.TryAdd("Team", new List<Guid>());
            availableChatRooms.TryAdd("General", new List<Guid>());
            availableChatRooms.TryAdd("Spam", new List<Guid>());
            return availableChatRooms;
        }

        private bool HasChatRoom(string chatRoom) =>
            this.subscribersByChatRoom.ContainsKey(chatRoom);

        private async Task WriteToAll(string chatRoom, string message)
        {
            var subscribersToRemove = new List<Guid>();

            foreach (var guid in this.subscribersByChatRoom[chatRoom])
            {
                try
                {
                    await this.repository.Subscribers[guid].ResponseStream.WriteAsync(new ChatRoomResponseStream { Message = message });
                }
                catch (Exception)
                {
                    subscribersToRemove.Add(guid);
                    this.repository.Subscribers.TryRemove(guid, out var subscriber);
                    Console.WriteLine($"the connection for user {guid} was lost");
                }
            }

            subscribersToRemove.ForEach(s => this.subscribersByChatRoom[chatRoom].Remove(s));
        }
    }
}
