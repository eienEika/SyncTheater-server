using System;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Player : ApiComponentBase
    {
        protected override bool AuthenticateRequired { get; } = true;

        protected override MethodResult MethodSwitch(string method, object data, User user)
        {
            var castedData = data as Model;

            return method switch
            {
                Methods.Player.PauseCycle => PauseCycle(user),
                Methods.Player.SetVideo => SetVideo(user, castedData?.Url),
                _ => MethodResult.UnknownMethod,
            };
        }

        private static MethodResult SetVideo(User user, string url)
        {
            if (user.IsAnonymous)
            {
                return MethodResult.LoginRequired;
            }

            var res = Room.GetState.SetVideoUrl(url);

            if (res != ApiError.NoError)
            {
                return new MethodResult(res);
            }

            return MethodResult.Ok;
        }

        private static MethodResult PauseCycle(User user)
        {
            if (user.IsAnonymous)
            {
                return MethodResult.LoginRequired;
            }

            var res = Room.GetState.PauseCycle();

            if (res != ApiError.NoError)
            {
                return new MethodResult(res);
            }

            return MethodResult.Ok;
        }

        [Serializable]
        private sealed class Model
        {
            public string Url { get; set; }
        }
    }
}