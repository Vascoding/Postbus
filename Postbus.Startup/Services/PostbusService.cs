using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Postbus.Startup.Services.Interfaces;

namespace Postbus.Startup
{
    internal class PostbusService : Postbus.PostbusBase
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

        public override Task<ChatRoomsReply> RevealChatRooms(ChatRoomsRequest request, ServerCallContext context) =>
            Task.Run(() => 
                new ChatRoomsReply { Message = JsonConvert.SerializeObject(this.chatRoomService.GetAvailableChatRooms()) });

        public override Task<UsersReply> RevealUsers(UsersRequest request, ServerCallContext context) =>
            Task.Run(() =>
                new UsersReply { Message = JsonConvert.SerializeObject(this.chatRoomService.GetUsersPerChatRoom(request.Chatroom)) });

        public override async Task<ExitReply> ExitChatRoom(ExitRequest request, ServerCallContext context)
        {
            Guid.TryParse(request.Guid, out var guid);

            var message = await this.chatRoomService.UnRegisterAsync(request.Chatroom, guid);

            return await Task.Run(() => new ExitReply { Message = message });
        }

        public override async Task OpenConnection(IAsyncStreamReader<ChatRoomRequestStream> requestStream, IServerStreamWriter<ChatRoomResponseStream> responseStream, ServerCallContext context)
        {
            var metadata = context.RequestHeaders.ToDictionary(k => k.Key, v => v.Value);

            Guid.TryParse(metadata["guid"], out var guid);

            var success = await this.repository.RegisterAsync(guid, metadata["username"], responseStream);

            await responseStream.WriteAsync(new ChatRoomResponseStream { Message = success });

            await this.ProccessStream(requestStream, guid);
        }

        private async Task ProccessStream(IAsyncStreamReader<ChatRoomRequestStream> requestStream, Guid guid)
        {
            await foreach (var req in requestStream.ReadAllAsync())
            {
                if (req.Toall)
                {
                    if (!this.chatRoomService.IsRegistered(req.Chatroom, guid))
                    {
                        await this.chatRoomService.RegisterAsync(req.Chatroom, guid);
                    }

                    await this.chatRoomService.BroadcastMessageAsync(req.Chatroom, guid, req.Message);
                }
            }
        }
    }
}
