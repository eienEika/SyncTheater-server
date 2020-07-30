using System;

namespace SyncTheater.Core.API
{
    internal interface IApiComponent
    {
        public Tuple<object, SendTo> RemoteRequest(string body, Guid sender);
        public Tuple<ApiError, object> LocalRequest(Enum method, params dynamic[] args);
    }
}