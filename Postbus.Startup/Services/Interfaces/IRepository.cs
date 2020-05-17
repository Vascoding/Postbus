using Grpc.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Interfaces
{
    internal interface IRepository<T>
    {
        Task<bool> Register(string username);

        Task<bool> SetStream(string username, IServerStreamWriter<T> stream);

        Task<bool> UnregisterAsync(string username);

        IServerStreamWriter<T> GetByUsername(string username);

        IDictionary<string, IServerStreamWriter<T>> GetAll();
    }
}