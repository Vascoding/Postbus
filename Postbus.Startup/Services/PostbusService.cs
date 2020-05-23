using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Postbus.Startup.Extensions;
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

            return await new RegisterReply { Success = success };
        }
            

        public override async Task<ChatRoomsReply> RevealChatRooms(ChatRoomsRequest request, ServerCallContext context)
        {
            var message = JsonConvert.SerializeObject(await this.chatRoomService.GetAvailableChatRooms());

            return await new ChatRoomsReply { Message = message };
        }

        public override async Task<UsersReply> RevealUsers(UsersRequest request, ServerCallContext context)
        {
            var message = JsonConvert.SerializeObject(await this.chatRoomService.GetUsersPerChatRoom(request.Chatroom));

            return await new UsersReply { Message = message };
        }

        public override async Task<ExitReply> ExitChatRoom(ExitRequest request, ServerCallContext context)
        {
            var message = await this.chatRoomService.UnRegister(request.Chatroom, request.Username);

            return await new ExitReply { Message = message };
        }

        public override async Task OpenConnection(IAsyncStreamReader<RequestStream> requestStream, IServerStreamWriter<ResponseStream> responseStream, ServerCallContext context)
        {
            var username = context.RequestHeaders.ToDictionary(k => k.Key, v => v.Value)["username"];

            if (username == null)
            {
                await responseStream.WriteAsync(new ResponseStream { Message = "Username not provided!!!" });
                return;
            }

            var success = await this.repository.SetStream(username, responseStream);

            if (!success)
            {
                await responseStream.WriteAsync(new ResponseStream { Message = "Unable to establish connection!!!" });
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
                    if (!await this.chatRoomService.IsRegistered(req.Chatroom, sender))
                    {
                        await this.chatRoomService.Register(req.Chatroom, sender);
                    }

                    await this.messageService.BroadcastToAll(req.Chatroom, req.Message, sender);
                }
                else
                {
                    await this.messageService.BroadcastDualDirection(req.Username, req.Message, sender);
                }
            }
        }
    }
}