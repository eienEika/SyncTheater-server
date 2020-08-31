using System;
using System.Collections.Generic;
using SyncTheater.Core.Models;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Player : ApiComponentBase
    {
        protected override bool AuthenticateRequired { get; } = true;

        protected override Tuple<ApiError, object, IEnumerable<NotificationTrigger>> MethodSwitch(
            string method, object data, User user)
        {
            var castedData = data as Model;

            return method switch
            {
                Methods.Player.PauseCycle => PauseCycle(user),
                Methods.Player.SetVideo => SetVideo(user, castedData?.Url),
                _ => UnknownMethodError,
            };
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