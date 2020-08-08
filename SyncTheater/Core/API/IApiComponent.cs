using System;
using SyncTheater.Core.Models;

namespace SyncTheater.Core.API
{
    internal interface IApiComponent
    {
        public Tuple<object, Api.SendTo> Request(string body, User user, Guid sessionId);
    }
}