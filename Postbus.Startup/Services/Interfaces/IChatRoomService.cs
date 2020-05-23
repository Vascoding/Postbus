using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Interfaces
{
    internal interface IChatRoomService
    {
        Task Register(string chatRoom, string username);

        Task<string> UnRegister(string chatRoom, string username);

        Task<string[]> GetAvailableChatRooms();

        Task<IList<string>> GetUsersPerChatRoom(string chatRoom);

        Task RemoveSubscribersFromChatRoom(string chatRoom, List<string> username);

        Task<bool> IsRegistered(string chatRoom, string username);
    }
}
