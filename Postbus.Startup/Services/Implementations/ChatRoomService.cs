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
        private readonly ConcurrentDictionary<string, IList<string>> subscribersByChatRoom;

        public ChatRoomService(IRepository<ResponseStream> repository)
        {
            this.subscribersByChatRoom = this.InitChatRooms();
        }

        public bool IsRegistered(string chatRoom, string username)
        {
            if (!this.HasChatRoom(chatRoom))
            {
                return false;
            }

            return this.subscribersByChatRoom[chatRoom].Any(u => u == username);
        }

        public async Task RegisterAsync(string chatRoom, string username)
        {
            await Task
                .Run(() => this.subscribersByChatRoom
                .AddOrUpdate(chatRoom, new List<string> { username }, (key, value) => {
                    value.Add(username);
                    return value;
                }));
        }

        public string UnRegister(string chatRoom, string username)
        {
            if (!this.HasChatRoom(chatRoom))
            {
                return "Chat room not found!!!";
            }

            var subscriber = this.subscribersByChatRoom[chatRoom].FirstOrDefault(u => u == username);

            if (subscriber == null)
            {
                return "User not found!!!";
            }

            this.subscribersByChatRoom[chatRoom].Remove(subscriber);

            return $"You left chat room {chatRoom}!!!";
        }

        public string[] GetAvailableChatRooms() =>
            this.subscribersByChatRoom.Keys.ToArray();

        public IList<string> GetUsersPerChatRoom(string chatRoom)
        {
            if (!this.HasChatRoom(chatRoom))
            {
                return new List<string>();
            }

            return this.subscribersByChatRoom[chatRoom];
        }

        private ConcurrentDictionary<string, IList<string>> InitChatRooms()
        {
            var availableChatRooms = new ConcurrentDictionary<string, IList<string>>();
            availableChatRooms.TryAdd("Team", new List<string>());
            availableChatRooms.TryAdd("General", new List<string>());
            availableChatRooms.TryAdd("Spam", new List<string>());
            return availableChatRooms;
        }

        private bool HasChatRoom(string chatRoom) =>
            this.subscribersByChatRoom.ContainsKey(chatRoom);
    }
}
