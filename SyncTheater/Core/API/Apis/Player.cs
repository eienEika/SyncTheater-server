using System;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Player : IApiComponent
    {
        public Tuple<object, SendTo> Request(string body, User user, Guid sessionId)
        {
            Log.Verbose($"Got request to player with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Method, Model>>(body);

            if (user == null)
            {
                return new Tuple<object, SendTo>(new OutcomeData<Method>
                    {
                        Data = null,
                        Error = ApiError.AuthenticationRequired,
                        Method = request.Method,
                    },
                    SendTo.Sender
                );
            }

            var (error, data, sendTo) = request.Method switch
            {
                Method.PauseCycle => PauseCycle(user),
                Method.SetVideo => SetVideo(user, request.Data.Url),
                _ => new Tuple<ApiError, object, SendTo>(ApiError.UnknownMethod, null, SendTo.Sender),
            };

            return new Tuple<object, SendTo>(new OutcomeData<Method>
                {
                    Data = data,
                    Error = error,
                    Method = request.Method,
                },
                sendTo
            );
        }

        private static Tuple<ApiError, object, SendTo> SetVideo(User user, string url)
        {
            if (user.IsAnonymous)
            {
                return new Tuple<ApiError, object, SendTo>(ApiError.AuthenticationRequired, null, SendTo.Sender);
            }

            Room.GetState.SetVideoUrl(url);

            return new Tuple<ApiError, object, SendTo>(ApiError.NoError, null, SendTo.Sender);
        }

        private static Tuple<ApiError, object, SendTo> PauseCycle(User user)
        {
            if (user.IsAnonymous)
            {
                return new Tuple<ApiError, object, SendTo>(ApiError.AuthenticationRequired, null, SendTo.Sender);
            }

            Room.GetState.PauseCycle();

            return new Tuple<ApiError, object, SendTo>(ApiError.NoError, null, SendTo.Sender);
        }

        private enum Method
        {
            SetVideo,
            PauseCycle,
        }

        [Serializable]
        private sealed class Model
        {
            public string Url { get; set; }
        }
    }
}