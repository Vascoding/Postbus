using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Interfaces
{
    internal interface IChatRoomService
    {
        Task RegisterAsync(string chatRoom, string username);

        string UnRegister(string chatRoom, string username);

        string[] GetAvailableChatRooms();

        IList<string> GetUsersPerChatRoom(string chatRoom);

        bool IsRegistered(string chatRoom, string username);
    }
}
