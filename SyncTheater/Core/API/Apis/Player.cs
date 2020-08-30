using System;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Player : ApiComponentBase
    {
        public override object Request(string body, User user)
        {
            Log.Verbose($"Got request to player with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Model>>(body);

            if (!user.IsAuthenticated)
            {
                return new ApiRequestResponse
                {
                    Data = null,
                    Error = ApiError.AuthenticationRequired,
                    Method = request.Method,
                };
            }

            var (error, data) = request.Method switch
            {
                Methods.Player.PauseCycle => PauseCycle(user),
                Methods.Player.SetVideo => SetVideo(user, request.Data.Url),
                _ => new Tuple<ApiError, object>(ApiError.UnknownMethod, null),
            };

            return new ApiRequestResponse
            {
                Data = data,
                Error = error,
                Method = request.Method,
            };
        }

        private static Tuple<ApiError, object> SetVideo(User user, string url)
        {
            if (user.IsAnonymous)
            {
                return AuthenticationRequiredError;
            }

            Room.GetState.SetVideoUrl(url);

            return NoError;
        }

        private static Tuple<ApiError, object> PauseCycle(User user)
        {
            if (user.IsAnonymous)
            {
                return AuthenticationRequiredError;
            }

            Room.GetState.PauseCycle();

            return NoError;
        }

        [Serializable]
        private sealed class Model
        {
            public string Url { get; set; }
        }
    }
}