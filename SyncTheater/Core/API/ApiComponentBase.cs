using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils;

namespace SyncTheater.Core.API
{
    internal abstract class ApiComponentBase
    {
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

            var result = MethodSwitch(request.Method, request.Data, user);

            var response = new ApiRequestResponse
            {
                Data = result.Data,
                Error = result.Error,
                Method = request.Method,
            };

            return new ApiResult(response, result.Triggers);
        }

        protected abstract MethodResult MethodSwitch(string method, object data, User user);
    }
}