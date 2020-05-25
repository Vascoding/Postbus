using Grpc.Core;
using Postbus.Internals.Extentions;
using Postbus.Startup.Services.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Implementations
{
    internal class Repository : IRepository<ResponseStream>
    {
        private ConcurrentDictionary<string, IServerStreamWriter<ResponseStream>> subscribers;

        public Repository()
        {
            this.subscribers = new ConcurrentDictionary<string, IServerStreamWriter<ResponseStream>>();
        }

        public async Task<bool> Register(string username) =>
            await this.subscribers.TryAddAsync(username, null);

        public async Task<bool> SetStream(string username, IServerStreamWriter<ResponseStream> stream) =>
            await this.subscribers.TryUpdateAsync(username, stream);

        public async Task<bool> Unregister(string username) =>
            await this.subscribers.TryRemoveAsync(username);

        public IServerStreamWriter<ResponseStream> GetByUsername(string username)
        {
            this.subscribers.TryGetValue(username, out var value);

            return value;
        }
    }
}
