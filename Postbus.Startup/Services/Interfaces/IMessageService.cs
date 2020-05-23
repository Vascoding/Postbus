using System.Threading.Tasks;

namespace Postbus.Startup.Services.Interfaces
{
    internal interface IMessageService
    {
        Task BroadcastToAll(string chatRoom, string message, string from);

        Task BroadcastSingleDirection(string to, string message, string from);

        Task BroadcastDualDirection(string to, string message, string from);
    }
}