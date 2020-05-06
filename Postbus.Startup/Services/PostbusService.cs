using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Postbus.Startup.Services.Interfaces;

namespace Postbus.Startup
{
    public class PostbusService : Postbus.PostbusBase
    {
        private readonly ILogger<PostbusService> logger;
        private readonly IChatRoomService chatRoomService;

        public PostbusService(ILogger<PostbusService> logger, IChatRoomService chatRoomService)
        {
            this.logger = logger;
            this.chatRoomService = chatRoomService;
        }

        public override Task<ChatRoomsReply> RevealChatRooms(ChatRoomsRequest request, ServerCallContext context) =>
            Task.Run(() => 
            new ChatRoomsReply { Message = JsonConvert.SerializeObject(this.chatRoomService.GetAvailableChatRooms()) });

        public override Task<UsersReply> RevealUsers(UsersRequest request, ServerCallContext context) =>
            Task.Run(() =>
            new UsersReply { Message = JsonConvert.SerializeObject(this.chatRoomService.GetUsersPerChatRoom(request.Chatroom)) });

        public override async Task<ExitReply> ExitChatRoom(ExitRequest request, ServerCallContext context)
        {
            var message = await this.chatRoomService.UnRegisterAsync(request.Chatroom, request.Username);
            return await Task.Run(() => new ExitReply { Message = message });
        }

        public override async Task OpenConnection(IAsyncStreamReader<ChatRoomRequestStream> requestStream, IServerStreamWriter<ChatRoomResponseStream> responseStream, ServerCallContext context)
        {
            await foreach (var req in requestStream.ReadAllAsync())
            {
                if (!this.chatRoomService.IsRegistered(req.Chatroom, req.Username))
                {
                    await this.chatRoomService.RegisterAsync(req.Chatroom, req.Username, responseStream);
                }
                
                await this.chatRoomService.BroadcastMessageAsync(req.Chatroom, req.Username, req.Message);
            }
        }
    }
}
