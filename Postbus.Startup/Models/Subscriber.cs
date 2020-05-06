using Grpc.Core;

namespace Postbus.Startup.Models
{
    public class Subscriber<T>
    {
        public string Username { get; set; }

        public IServerStreamWriter<T> ResponseStream { get; set; }
    }
}
