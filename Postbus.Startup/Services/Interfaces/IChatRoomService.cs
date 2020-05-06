using Grpc.Core;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Interfaces
{
    public interface IChatRoomService
    {
        Task RegisterAsync(string chatRoom, string username, IServerStreamWriter<ChatRoomResponseStream> stream);

        Task<string> UnRegisterAsync(string chatRoom, string username);

        string[] GetAvailableChatRooms();

        string[] GetUsersPerChatRoom(string chatRoom);

        Task BroadcastMessageAsync(string chatRoom, string username, string message);

        bool IsRegistered(string chatRoom, string username);
    }
}
