using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Postbus.Startup.Extensions
{
    internal static class AwaiterExtensions
    {
        public static TaskAwaiter<RegisterReply> GetAwaiter(this RegisterReply registerReply) =>
            Task.Run(() => registerReply).GetAwaiter();

        public static TaskAwaiter<ChatRoomsReply> GetAwaiter(this ChatRoomsReply chatRoomsReply) =>
            Task.Run(() => chatRoomsReply).GetAwaiter();

        public static TaskAwaiter<UsersReply> GetAwaiter(this UsersReply usersReply) =>
            Task.Run(() => usersReply).GetAwaiter();

        public static TaskAwaiter<ExitReply> GetAwaiter(this ExitReply exitReply) =>
            Task.Run(() => exitReply).GetAwaiter();
    }
}
