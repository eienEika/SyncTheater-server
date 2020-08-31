using System;
using System.Collections.Generic;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Player : ApiComponentBase
    {
        public override ApiResult Request(string body, User user)
        {
            Log.Verbose($"Got request to player with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Model>>(body);

            if (!user.IsAuthenticated)
            {
                return AuthenticationRequiredResult(request.Method);
            }

            var (error, data, triggers) = request.Method switch
            {
                Methods.Player.PauseCycle => PauseCycle(user),
                Methods.Player.SetVideo => SetVideo(user, request.Data.Url),
                _ => UnknownMethodError,
            };

            var response = new ApiRequestResponse
            {
                Data = data,
                Error = error,
                Method = request.Method,
            };

            return new ApiResult(response, triggers);
        }

        private static Tuple<ApiError, object, IEnumerable<NotificationTrigger>> SetVideo(User user, string url)
        {
            if (user.IsAnonymous)
            {
                return LoginRequiredError;
            }

            Room.GetState.SetVideoUrl(url);

            return NoError;
        }

        private static Tuple<ApiError, object, IEnumerable<NotificationTrigger>> PauseCycle(User user)
        {
            if (user.IsAnonymous)
            {
                return LoginRequiredError;
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