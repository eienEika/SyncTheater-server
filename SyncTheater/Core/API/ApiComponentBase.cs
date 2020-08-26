using System;
using SyncTheater.Core.Models;

namespace SyncTheater.Core.API
{
    internal abstract class ApiComponentBase
    {
        protected static readonly Tuple<ApiError, object> Nothing =
            new Tuple<ApiError, object>(ApiError.NoError, null);

        protected static readonly Tuple<ApiError, object> NoError =
            new Tuple<ApiError, object>(ApiError.NoError, null);

        protected static readonly Tuple<ApiError, object> AuthenticationRequiredError =
            new Tuple<ApiError, object>(ApiError.AuthenticationRequired, null);

        public abstract object Request(string body, User user);
    }
}