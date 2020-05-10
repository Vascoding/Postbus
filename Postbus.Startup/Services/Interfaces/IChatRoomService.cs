using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Interfaces
{
    internal interface IChatRoomService
    {
        Task RegisterAsync(string chatRoom, Guid guid);

        Task<string> UnRegisterAsync(string chatRoom, Guid guid);

        string[] GetAvailableChatRooms();

        string[] GetUsersPerChatRoom(string chatRoom);

        Task BroadcastMessageAsync(string chatRoom, Guid guid, string message);

        bool IsRegistered(string chatRoom, Guid guid);
    }
}
