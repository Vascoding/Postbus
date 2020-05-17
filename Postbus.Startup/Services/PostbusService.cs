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
        private readonly IMessageService messageService;
        private readonly IRepository<ResponseStream> repository;

        public PostbusService(ILogger<PostbusService> logger, IChatRoomService chatRoomService, IMessageService messageService, IRepository<ResponseStream> repository)
        {
            this.logger = logger;
            this.chatRoomService = chatRoomService;
            this.messageService = messageService;
            this.repository = repository;
        }

        public override async Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            var success = await this.repository.Register(request.Username);

            return await Task.Run(() =>
                new RegisterReply { Success = success });
        }

        public override Task<ChatRoomsReply> RevealChatRooms(ChatRoomsRequest request, ServerCallContext context) =>
            Task.Run(() => 
                new ChatRoomsReply { Message = JsonConvert.SerializeObject(this.chatRoomService.GetAvailableChatRooms()) });

        public override Task<UsersReply> RevealUsers(UsersRequest request, ServerCallContext context) =>
            Task.Run(() =>
                new UsersReply { Message = JsonConvert.SerializeObject(this.chatRoomService.GetUsersPerChatRoom(request.Chatroom)) });

        public override Task<ExitReply> ExitChatRoom(ExitRequest request, ServerCallContext context) =>
            Task.Run(() =>
                new ExitReply { Message = this.chatRoomService.UnRegister(request.Chatroom, request.Username) });

        public override async Task OpenConnection(IAsyncStreamReader<RequestStream> requestStream, IServerStreamWriter<ResponseStream> responseStream, ServerCallContext context)
        {
            var username = context.RequestHeaders.ToDictionary(k => k.Key, v => v.Value)["username"];

            var success = await this.repository.SetStream(username, responseStream);

            if (!success)
            {
                return;
            }

            await this.ProccessStream(requestStream, username);
        }

        private async Task ProccessStream(IAsyncStreamReader<RequestStream> requestStream, string sender)
        {
            await foreach (var req in requestStream.ReadAllAsync())
            {
                if (req.Toall)
                {
                    if (!this.chatRoomService.IsRegistered(req.Chatroom, sender))
                    {
                        await this.chatRoomService.RegisterAsync(req.Chatroom, sender);
                    }

                    await this.messageService.BroadcastToAllAsync(req.Chatroom, req.Message, sender);
                }
                else
                {
                    await this.messageService.BroadcastToOneDualDirectionAsync(req.Username, req.Message, sender);
                }
            }
        }
    }
}
