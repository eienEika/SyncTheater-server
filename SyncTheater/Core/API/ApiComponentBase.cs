using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils;

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

        private static readonly ApiResult AuthenticationRequiredResult = new ApiResult(
            new ApiRequestResponse
            {
                Data = null,
                Error = ApiError.AuthenticationRequired,
                Method = null,
            }
        );

        protected abstract bool AuthenticateRequired { get; }

        public ApiResult Request(string body, User user)
        {
            Log.Verbose($"Got request with body {body}.");

            if (AuthenticateRequired && !user.IsAuthenticated)
            {
                return AuthenticationRequiredResult;
            }

            var request = SerializationUtils.Deserialize<IncomeData<object>>(body);

            var (error, data, triggers) = MethodSwitch(request.Method, request.Data, user);

            var response = new ApiRequestResponse
            {
                Data = data,
                Error = error,
                Method = request.Method,
            };

            return new ApiResult(response, triggers);
        }

        protected abstract Tuple<ApiError, object, IEnumerable<NotificationTrigger>> MethodSwitch(
            string method, object data, User user);
    }
}