using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Postbus.Internals.Extentions;
using Postbus.Startup.Services.Interfaces;

namespace Postbus.Startup.Services.Implementations
{
    internal class ChatRoomService : IChatRoomService
    {
        private readonly ConcurrentDictionary<string, IList<string>> subscribersByChatRoom;

        public ChatRoomService()
        {
            this.subscribersByChatRoom = this.InitChatRooms();
        }

        public async Task<bool> IsRegistered(string chatRoom, string username)
        {
            var subscribers = await this.subscribersByChatRoom.TryGetValueAsync(chatRoom);

            if (subscribers == null)
            {
                return false;
            }

            return subscribers.Any(u => u == username);
        }

        public async Task Register(string chatRoom, string username)
        {
            await this.subscribersByChatRoom.AddOrUpdateAsync(chatRoom, new List<string> { username }, (key, value) =>
            {
                value.Add(username);
                return value;
            });
        }

        public async Task<string> UnRegister(string chatRoom, string username)
        {
            var subscribers = await this.subscribersByChatRoom.TryGetValueAsync(chatRoom);

            var subscriber = subscribers.FirstOrDefault(u => u == username);

            if (subscriber == null)
            {
                return "User not found!!!";
            }

            subscribers.Remove(subscriber);

            return $"You left chat room {chatRoom}!!!";
        }

        public async Task<string[]> GetAvailableChatRooms() =>
            await Task.FromResult(this.subscribersByChatRoom.Keys.ToArray());

        public async Task<IList<string>> GetUsersPerChatRoom(string chatRoom)
        {
            return await this.subscribersByChatRoom.TryGetValueAsync(chatRoom);
        }

        private ConcurrentDictionary<string, IList<string>> InitChatRooms() =>
            new ConcurrentDictionary<string, IList<string>>
            {
                { "Team", new List<string>() },
                { "General", new List<string>() },
                { "Spam", new List<string>() },
            };

        public async Task RemoveSubscribersFromChatRoom(string chatRoom, List<string> usernames)
        {
            var subscribers = await this.subscribersByChatRoom.TryGetValueAsync(chatRoom);

            if (subscribers == null)
            {
                return;
            }

            usernames.ForEach(a => subscribers.Remove(a));
        }
    }
}
