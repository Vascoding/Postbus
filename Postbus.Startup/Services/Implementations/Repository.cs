using Grpc.Core;
using Postbus.Startup.Services.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Implementations
{
    public class Repository : IRepository
    {
        private readonly IDictionary<string, IServerStreamWriter<ChatRoomResponseStream>> subscribers;

        public Repository()
        {
            this.subscribers = new ConcurrentDictionary<string, IServerStreamWriter<ChatRoomResponseStream>>();
        }

        public IDictionary<string, IServerStreamWriter<ChatRoomResponseStream>> Subscribers => this.subscribers;

        public async Task<bool> Register(string username, IServerStreamWriter<ChatRoomResponseStream> stream)
        {
            return await Task.Run(() => subscribers.TryAdd(username, stream));
        }
    }
}
