using System;
using Serilog;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Player
    {
        public bool Pause { get; private set; }
        public string Url { get; private set; }

        public string Request(string body)
        {
            Log.Verbose($"Got request to player with body {body}.");

            var data = SerializationUtils.Deserialize<Model>(body);
            return data.Method switch
            {
                Method.SetVideo => SetVideo(data),
                Method.PauseCycle => PauseCycle(data),
                _ => Api.UnknownMethodResponse(data.Method),
            };
        }

        private string SetVideo(Model data)
        {
            Log.Debug($"Player API: SetVideo request, new url: {data.Url}.");

            Url = data.Url;
            Pause = false;
            return new OutcomeData
            {
                Error = (Error) Api.ErrorCommon.NoError,
                Method = data.Method,
                Url = data.Url,
            }.ToJson();
        }

        private string PauseCycle(Model data)
        {
            Log.Debug($"Player API: PauseCycle request, paused before: {Pause}.");

            Pause = !Pause;
            return new OutcomeData
            {
                Error = (Error) Api.ErrorCommon.NoError,
                Method = data.Method,
                Pause = Pause,
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

        [Serializable]
        private sealed class Model : IncomeDataBase<Method>
        {
            public string Url { get; set; }
        }

        [Serializable]
        private sealed class OutcomeData : OutcomeDataBase<Method, Error>
        {
            public bool? Pause { get; set; }
            public string Url { get; set; }
        }
    }
}