using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Postbus.Startup.Services.Interfaces;

namespace Postbus.Startup
{
    public class PostbusService : Postbus.PostbusBase
    {
        private readonly ILogger<PostbusService> logger;
        private readonly IChatRoomService chatRoomService;
        private readonly IRepository repository;

        public PostbusService(ILogger<PostbusService> logger, IChatRoomService chatRoomService, IRepository repository)
        {
            this.logger = logger;
            this.chatRoomService = chatRoomService;
            this.repository = repository;
        }

        public override async Task ToChatRoom(IAsyncStreamReader<ChatRoomRequestStream> requestStream, IServerStreamWriter<ChatRoomResponseStream> responseStream, ServerCallContext context)
        {
            await foreach (var req in requestStream.ReadAllAsync())
            {
                if (!this.chatRoomService.IsRegistered(req.Chatroom, responseStream))
                {
                    await this.chatRoomService.RegisterAsync(req.Chatroom, responseStream);
                }
                
                await this.chatRoomService.BroadcastMessageAsync(req.Chatroom, $"{req.Username} :{req.Message}");
            }
        }
    }
}
