using Grpc.Core;
using Postbus.Internals.Extentions;
using Postbus.Startup.Models;
using Postbus.Startup.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postbus.Startup.Services.Implementations
{
    internal class Repository : IRepository
    {
        public Repository()
        {
            this.Subscribers = new Dictionary<Guid, Subscriber<ResponseStream>>();
        }

        public IDictionary<Guid, Subscriber<ResponseStream>> Subscribers { get; }

        public async Task<string> RegisterAsync(Guid guid, string username, IServerStreamWriter<ResponseStream> stream) 
        {
            var success = await this.Subscribers.TryAddAsync(guid, new Subscriber<ResponseStream>
            {
                Username = username,
                ResponseStream = stream
            });

            return this.RegisterMessage(success);
        }

        public async Task<string> UnregisterAsync(Guid guid)
        {
            var success = await this.Subscribers.TryRemoveAsync(guid);

            return UnregisterMessage(success);
        }

        private string RegisterMessage(bool success) =>
            $"{(success ? "Successfull" : "Unsuccessfull")} Registered!!!";

        private string UnregisterMessage(bool success) =>
            $"{(success ? "Successfull" : "Unsuccessfull")} Unegistered!!!";
    }
}
