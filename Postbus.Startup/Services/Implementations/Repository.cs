using Grpc.Core;
using Postbus.Startup.Models;
using Postbus.Startup.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Implementations
{
    internal class Repository : IRepository
    {
        public Repository()
        {
            this.Subscribers = new ConcurrentDictionary<Guid, Subscriber<ChatRoomResponseStream>>();
        }

        public ConcurrentDictionary<Guid, Subscriber<ChatRoomResponseStream>> Subscribers { get; }

        public async Task<string> RegisterAsync(Guid guid, string username, IServerStreamWriter<ChatRoomResponseStream> stream) 
        {
            var success = await Task
                .Run(() =>
                    this.Subscribers.TryAdd(guid, new Subscriber<ChatRoomResponseStream>
                    {
                        Username = username,
                        ResponseStream = stream
                    }));

            return this.RegisterMessage(success);
        }

        public async Task<string> UnregisterAsync(Guid guid)
        {
            var success = await Task.Run(() => this.Subscribers.TryRemove(guid, out var stream));

            return UnregisterMessage(success);
        }

        private string RegisterMessage(bool success) =>
            $"{(success ? "Successfull" : "Unsuccessfull")} Registered!!!";

        private string UnregisterMessage(bool success) =>
            $"{(success ? "Successfull" : "Unsuccessfull")} Unegistered!!!";
    }
}
