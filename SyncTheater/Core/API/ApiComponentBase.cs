using System;
using System.Collections.Generic;
using System.Linq;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;

namespace SyncTheater.Core.API
{
    internal abstract class ApiComponentBase
    {
        protected static readonly Tuple<ApiError, object, IEnumerable<NotificationTrigger>> NoError =
            new Tuple<ApiError, object, IEnumerable<NotificationTrigger>>(
                ApiError.NoError,
                null,
                Enumerable.Empty<NotificationTrigger>()
            );

        protected static readonly Tuple<ApiError, object, IEnumerable<NotificationTrigger>> LoginRequiredError =
            new Tuple<ApiError, object, IEnumerable<NotificationTrigger>>(
                ApiError.LoginRequired,
                null,
                Enumerable.Empty<NotificationTrigger>()
            );

        protected static readonly Tuple<ApiError, object, IEnumerable<NotificationTrigger>> UnknownMethodError =
            new Tuple<ApiError, object, IEnumerable<NotificationTrigger>>(
                ApiError.UnknownMethod,
                null,
                Enumerable.Empty<NotificationTrigger>()
            );

        protected static ApiResult AuthenticationRequiredResult(string method) =>
            new ApiResult(
                new ApiRequestResponse
                {
                    Data = null,
                    Error = ApiError.AuthenticationRequired,
                    Method = method,
                }
            );

        public abstract ApiResult Request(string body, User user);
    }
}