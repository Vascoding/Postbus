using Grpc.Core;
using Postbus.Startup.Models;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Interfaces
{
    internal interface IRepository
    {
        ConcurrentDictionary<Guid, Subscriber<ChatRoomResponseStream>> Subscribers { get; }

        Task<string> RegisterAsync(Guid guid, string username, IServerStreamWriter<ChatRoomResponseStream> stream);

        Task<string> UnregisterAsync(Guid id);
    }
}
