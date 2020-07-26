using System;
using Serilog;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Player : IApiComponent
    {
        private readonly State _state = new State();

        public string Request(string body)
        {
            Log.Verbose($"Got request to player with body {body}.");

            var request = SerializationUtils.Deserialize<IApiComponent.IncomeData<Method, Model>>(body);
            return request.Method switch
            {
                Method.SetVideo => SetVideo(request.Data),
                Method.PauseCycle => PauseCycle(),
                _ => Api.UnknownMethodResponse(request.Method),
            };
        }

        private string SetVideo(Model data)
        {
            Log.Debug($"Player API: SetVideo request, new url: {data.Url}.");

            _state.Url = data.Url;
            _state.Pause = false;

            return new IApiComponent.OutcomeData<Method, Api.ErrorCommon>
            {
                Error = Api.ErrorCommon.NoError,
                Method = Method.SetVideo,
                Data = new
                {
                   _state.Url,
                    Pause = false,
                },
            }.ToJson();
        }

        private string PauseCycle()
        {
            Log.Debug($"Player API: PauseCycle request, paused before: {_state.Pause}.");

            _state.Pause = !_state.Pause;

            return new IApiComponent.OutcomeData<Method, Api.ErrorCommon>
            {
                Error = Api.ErrorCommon.NoError,
                Method = Method.PauseCycle,
                Data = new
                {
                    _state.Pause,
                },
            }.ToJson();
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