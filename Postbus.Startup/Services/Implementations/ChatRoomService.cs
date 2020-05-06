using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Postbus.Startup.Models;
using Postbus.Startup.Services.Interfaces;

namespace Postbus.Startup.Services.Implementations
{
    public class ChatRoomService : IChatRoomService
    {
        private readonly ConcurrentDictionary<string, List<Subscriber<ChatRoomResponseStream>>> subscribersByChatRoom;

        public ChatRoomService()
        {
            this.subscribersByChatRoom = this.InitChatRooms();
        }

        public bool IsRegistered(string chatRoom, string username)
        {
            if (!this.HasChatRoom(chatRoom))
            {
                return false;
            }

            return this.subscribersByChatRoom[chatRoom].Any(u => u.Username == username);
        }

        public async Task RegisterAsync(string chatRoom, string username, IServerStreamWriter<ChatRoomResponseStream> stream)
        {
            var subscriber = new Subscriber<ChatRoomResponseStream>
            {
                Username = username,
                ResponseStream = stream
            };

            await Task
                .Run(() => this.subscribersByChatRoom
                .AddOrUpdate(chatRoom, new List<Subscriber<ChatRoomResponseStream>> { subscriber }, (key, value) => {
                    value.Add(subscriber);
                    return value;
                }));

            await this.WriteToAll(chatRoom, $"{username} has joined the chat room");
        }

        public async Task<string> UnRegisterAsync(string chatRoom, string username)
        {
            if (!this.HasChatRoom(chatRoom))
            {
                return await Task.Run(() => "Chat room not found!!!");
            }

            var subscriber = this.subscribersByChatRoom[chatRoom].FirstOrDefault(u => u.Username == username);

            if (subscriber == null)
            {
                return await Task.Run(() => "User not found!!!");
            }

            this.subscribersByChatRoom[chatRoom].Remove(subscriber);

            await this.WriteToAll(chatRoom, $"{username} has left the chat room");

            return await Task.Run(() => $"You left chat room {chatRoom}!!!");
        }

        public async Task BroadcastMessageAsync(string chatRoom, string username, string message)
        {
            if (!this.HasChatRoom(chatRoom))
            {
                return;
            }

            await this.WriteToAll(chatRoom, $"{username}: {message}");
        }

        public string[] GetAvailableChatRooms() =>
            this.subscribersByChatRoom.Keys.ToArray();

        public string[] GetUsersPerChatRoom(string chatRoom)
        {
            if (!this.HasChatRoom(chatRoom))
            {
                return Array.Empty<string>();
            }

            return this.subscribersByChatRoom[chatRoom].Select(s => s.Username).ToArray();
        }

        private ConcurrentDictionary<string, List<Subscriber<ChatRoomResponseStream>>> InitChatRooms()
        {
            var availableChatRooms = new ConcurrentDictionary<string, List<Subscriber<ChatRoomResponseStream>>>();
            availableChatRooms.TryAdd("Team", new List<Subscriber<ChatRoomResponseStream>>());
            availableChatRooms.TryAdd("General", new List<Subscriber<ChatRoomResponseStream>>());
            availableChatRooms.TryAdd("Spam", new List<Subscriber<ChatRoomResponseStream>>());
            return availableChatRooms;
        }

        private bool HasChatRoom(string chatRoom) =>
            this.subscribersByChatRoom.ContainsKey(chatRoom);

        private async Task WriteToAll(string chatRoom, string message)
        {
            var subscribersToRemove = new List<Subscriber<ChatRoomResponseStream>>();

            foreach (var subscriber in this.subscribersByChatRoom[chatRoom])
            {
                try
                {
                    await subscriber.ResponseStream.WriteAsync(new ChatRoomResponseStream { Message = message });
                }
                catch (Exception)
                {
                    subscribersToRemove.Add(subscriber);
                    Console.WriteLine($"the connection for user {subscriber} was lost");
                }
            }

            subscribersToRemove.ForEach(s => this.subscribersByChatRoom[chatRoom].Remove(s));
        }
    }
}
