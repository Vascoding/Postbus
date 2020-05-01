using System.Threading.Tasks;
using Postbus.Internals.Interfaces;

namespace Postbus.Internals.Implementations
{
    public class ChannelsService : IChannelsService
    {
        public async Task<string[]> GetAvailableChannels() =>
            await Task.Run(() => new string[]
            {
                "CoronaTeam",
                "CoronaRequirements",
                "CoronaSpam"
            });
    }
}
