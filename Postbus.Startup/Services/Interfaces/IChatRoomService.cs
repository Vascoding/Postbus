using Grpc.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Interfaces
{
    public interface IChatRoomService
    {
        Task RegisterAsync(string chatRoom, IServerStreamWriter<ChatRoomResponseStream> stream);

        Task<string[]> GetAvailableChatRoomsAsync();

        Task BroadcastMessageAsync(string chatRoom, string message);

        bool IsRegistered(string chatRoom, IServerStreamWriter<ChatRoomResponseStream> stream);
    }
}
