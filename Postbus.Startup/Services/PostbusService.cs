using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Postbus.Internals.Interfaces;

namespace Postbus.Startup
{
    public class PostbusService : Postbus.PostbusBase
    {
        private readonly ILogger<PostbusService> logger;
        private readonly IChannelsService channelsService;

        public PostbusService(ILogger<PostbusService> logger, IChannelsService channelsService)
        {
            this.logger = logger;
            this.channelsService = channelsService;
        }

        public override async Task<ChannelsReply> RevealChannels(ChannelsRequest request, ServerCallContext context)
        {
            var channels = await this.channelsService.GetAvailableChannels();
            return new ChannelsReply { Message = JsonConvert.SerializeObject(channels) };
        }
    }
}
