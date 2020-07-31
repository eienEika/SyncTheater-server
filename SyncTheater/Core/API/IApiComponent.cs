using System;

namespace SyncTheater.Core.API
{
    internal interface IApiComponent
    {
        public Tuple<object, SendTo> Request(string body, Guid sender);
    }
}