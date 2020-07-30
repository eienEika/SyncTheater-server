using System;
using Serilog;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Player : IApiComponent
    {
        public enum Method
        {
            SetVideo = 100,
            PauseCycle,
        }

        private readonly State _state = new State();

        public Tuple<object, SendTo> RemoteRequest(string body, Guid sender)
        {
            Log.Verbose($"Got request to player with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Method, Model>>(body);

            var ((error, data), sendTo) = request.Method switch
            {
                Method.PauseCycle => (PauseCycle(), SendTo.All),
                Method.SetVideo => (SetVideo(request.Data.Url), SendTo.All),
                _ => (new Tuple<ApiError, object>(ApiError.UnknownMethod, null), SendTo.Sender),
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

        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        public Tuple<ApiError, object> LocalRequest(Enum method, params dynamic[] args) =>
            (Method) method switch
            {
                _ => throw new NotSupportedException(),
            };

        private Tuple<ApiError, object> SetVideo(string url)
        {
            Log.Debug($"Player API: SetVideo request, new url: {url}.");

            _state.Url = url;
            _state.Pause = false;

            return new Tuple<ApiError, object>(ApiError.NoError,
                new
                {
                    _state.Url,
                    _state.Pause,
                }
            );
        }

        private Tuple<ApiError, object> PauseCycle()
        {
            Log.Debug($"Player API: PauseCycle request, paused before: {_state.Pause}.");

            _state.Pause = !_state.Pause;

            return new Tuple<ApiError, object>(ApiError.NoError,
                new
                {
                    _state.Pause,
                }
            );
        }

        private sealed class State
        {
            public bool Pause { get; set; }
            public string Url { get; set; }
        }

        [Serializable]
        private sealed class Model
        {
            public string Url { get; set; }
        }
    }
}