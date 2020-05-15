using Grpc.Core;
using Postbus.Startup.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Interfaces
{
    internal interface IRepository
    {
        IDictionary<Guid, Subscriber<ResponseStream>> Subscribers { get; }

        Task<string> RegisterAsync(Guid guid, string username, IServerStreamWriter<ResponseStream> stream);

        Task<string> UnregisterAsync(Guid id);
    }
}
