using System.Threading.Tasks;

namespace Postbus.Internals.Interfaces
{
    public interface IChannelsService
    {
        Task<string[]> GetAvailableChannels();
    }
}
