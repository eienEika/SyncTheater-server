using System;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Player : IApiComponent
    {
        public Tuple<object, SendTo> Request(string body, Guid sender)
        {
            Log.Verbose($"Got request to player with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Method, Model>>(body);

            var (error, data, sendTo) = request.Method switch
            {
                Method.PauseCycle => PauseCycle(),
                Method.SetVideo => SetVideo(request.Data.Url),
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

        private static Tuple<ApiError, object, SendTo> SetVideo(string url)
        {
            Room.GetState.SetVideoUrl(url);

            return new Tuple<ApiError, object, SendTo>(ApiError.NoError, null, SendTo.Sender);
        }

        private static Tuple<ApiError, object, SendTo> PauseCycle()
        {
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