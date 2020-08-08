using System;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Player : IApiComponent
    {
        public Tuple<object, Api.SendTo> Request(string body, User user, Guid sessionId)
        {
            Log.Verbose($"Got request to player with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Model>>(body);

            if (user == null)
            {
                return new Tuple<object, Api.SendTo>(new OutcomeData
                    {
                        Data = null,
                        Error = ApiError.AuthenticationRequired,
                        Method = request.Method,
                    },
                    Api.SendTo.Sender
                );
            }

            var (error, data, sendTo) = request.Method switch
            {
                Methods.Player.PauseCycle => PauseCycle(user),
                Methods.Player.SetVideo => SetVideo(user, request.Data.Url),
                _ => new Tuple<ApiError, object, Api.SendTo>(ApiError.UnknownMethod, null, Api.SendTo.Sender),
            };

            return new Tuple<object, Api.SendTo>(new OutcomeData
                {
                    Data = data,
                    Error = error,
                    Method = request.Method,
                },
                sendTo
            );
        }

        private static Tuple<ApiError, object, Api.SendTo> SetVideo(User user, string url)
        {
            if (user.IsAnonymous)
            {
                return new Tuple<ApiError, object, Api.SendTo>(ApiError.AuthenticationRequired, null, Api.SendTo.Sender);
            }

            Room.GetState.SetVideoUrl(url);

            return new Tuple<ApiError, object, Api.SendTo>(ApiError.NoError, null, Api.SendTo.Sender);
        }

        private static Tuple<ApiError, object, Api.SendTo> PauseCycle(User user)
        {
            if (user.IsAnonymous)
            {
                return new Tuple<ApiError, object, Api.SendTo>(ApiError.AuthenticationRequired, null, Api.SendTo.Sender);
            }

            Room.GetState.PauseCycle();

            return new Tuple<ApiError, object, Api.SendTo>(ApiError.NoError, null, Api.SendTo.Sender);
        }

        [Serializable]
        private sealed class Model
        {
            public string Url { get; set; }
        }
    }
}