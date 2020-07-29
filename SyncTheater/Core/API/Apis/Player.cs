using System;
using Serilog;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Player : IApiComponent
    {
        private readonly State _state = new State();

        public Tuple<object, SendTo> Request(string body)
        {
            Log.Verbose($"Got request to player with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Method, Model>>(body);

            return request.Method switch
            {
                Method.PauseCycle => PauseCycle(),
                Method.SetVideo => SetVideo(request.Data.Url),
                _ => new Tuple<object, SendTo>(Api.UnknownMethodResponse(request.Method), SendTo.Sender),
            };
        }

        private Tuple<object, SendTo> SetVideo(string url)
        {
            Log.Debug($"Player API: SetVideo request, new url: {url}.");

            _state.Url = url;
            _state.Pause = false;

            return new Tuple<object, SendTo>(new OutcomeData<Method, ErrorCommon>
                {
                    Data = new
                    {
                        _state.Url,
                        Pause = false,
                    },
                    Error = ErrorCommon.NoError,
                    Method = Method.SetVideo,
                },
                SendTo.All
            );
        }

        private Tuple<object, SendTo> PauseCycle()
        {
            Log.Debug($"Player API: PauseCycle request, paused before: {_state.Pause}.");

            _state.Pause = !_state.Pause;

            return new Tuple<object, SendTo>(new OutcomeData<Method, ErrorCommon>
                {
                    Data = new
                    {
                        _state.Pause,
                    },
                    Error = ErrorCommon.NoError,
                    Method = Method.PauseCycle,
                },
                SendTo.All
            );
        }

        private enum Method
        {
            SetVideo = 100,
            PauseCycle,
        }

        private enum Error
        {
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