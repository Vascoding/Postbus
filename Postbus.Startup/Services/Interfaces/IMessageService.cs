using System.Threading.Tasks;

namespace Postbus.Startup.Services.Interfaces
{
    internal interface IMessageService
    {
        Task BroadcastToAllAsync(string chatRoom, string message, string from);

        Task BroadcastToOneSingleDirectionAsync(string to, string message, string from);

        Task BroadcastToOneDualDirectionAsync(string to, string message, string from);
    }
}