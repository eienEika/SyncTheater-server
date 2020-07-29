using System;
using Serilog;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Player : IApiComponent
    {
        private readonly State _state = new State();

        public Tuple<object, Api.SendTo> Request(string body)
        {
            Log.Verbose($"Got request to player with body {body}.");

            var request = SerializationUtils.Deserialize<IApiComponent.IncomeData<Method, Model>>(body);

            return request.Method switch
            {
                Method.PauseCycle => PauseCycle(),
                Method.SetVideo => SetVideo(request.Data.Url),
                _ => new Tuple<object, Api.SendTo>(Api.UnknownMethodResponse(request.Method), Api.SendTo.Sender),
            };
        }

        private Tuple<object, Api.SendTo> SetVideo(string url)
        {
            Log.Debug($"Player API: SetVideo request, new url: {url}.");

            _state.Url = url;
            _state.Pause = false;

            return new Tuple<object, Api.SendTo>(new IApiComponent.OutcomeData<Method, Api.ErrorCommon>
                {
                    Data = new
                    {
                        _state.Url,
                        Pause = false,
                    },
                    Error = Api.ErrorCommon.NoError,
                    Method = Method.SetVideo,
                },
                Api.SendTo.All
            );
        }

        private Tuple<object, Api.SendTo> PauseCycle()
        {
            Log.Debug($"Player API: PauseCycle request, paused before: {_state.Pause}.");

            _state.Pause = !_state.Pause;

            return new Tuple<object, Api.SendTo>(new IApiComponent.OutcomeData<Method, Api.ErrorCommon>
                {
                    Data = new
                    {
                        _state.Pause,
                    },
                    Error = Api.ErrorCommon.NoError,
                    Method = Method.PauseCycle,
                },
                Api.SendTo.All
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