using System;
using SyncTheater.Core.Models;

namespace SyncTheater.Core.API
{
    internal abstract class ApiComponentBase
    {
        protected static readonly Tuple<ApiError, object, Api.SendTo> Nothing =
            new Tuple<ApiError, object, Api.SendTo>(ApiError.NoError, null, Api.SendTo.None);

        protected static readonly Tuple<ApiError, object, Api.SendTo> NoError =
            new Tuple<ApiError, object, Api.SendTo>(ApiError.NoError, null, Api.SendTo.Sender);

        protected static readonly Tuple<ApiError, object, Api.SendTo> AuthenticationRequiredError =
            new Tuple<ApiError, object, Api.SendTo>(ApiError.AuthenticationRequired, null, Api.SendTo.Sender);

        public abstract Tuple<object, Api.SendTo> Request(string body, User user, Guid sessionId);
    }
}