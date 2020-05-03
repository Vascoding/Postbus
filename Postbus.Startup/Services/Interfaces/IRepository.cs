using Grpc.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Interfaces
{
    public interface IRepository
    {
        IDictionary<string, IServerStreamWriter<ChatRoomResponseStream>> Subscribers { get; }
        Task<bool> Register(string username, IServerStreamWriter<ChatRoomResponseStream> stream);
    }
}