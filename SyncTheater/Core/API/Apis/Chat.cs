using System;
using Serilog;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Chat : IApiComponent
    {
        public string Request(string body)
        {
            Log.Verbose($"Got request to chat with body {body}.");

            var data = SerializationUtils.Deserialize<IApiComponent.IncomeData<Method, Model>>(body);
            return data.Method switch
            {
                Method.NewMessage => NewMessage(data.Data),
                _ => Api.UnknownMethodResponse(data.Method),
            };
        }

        private string NewMessage(Model data)
        {
            Log.Debug($"Chat API: NewMessage request, text: \"{data.Text}\"");

            if (string.IsNullOrWhiteSpace(data.Text))
            {
                return new IApiComponent.OutcomeData<Method, Error>
                {
                    Error = Error.EmptyText,
                    Method = Method.NewMessage,
                }.ToJson();
            }

            return new IApiComponent.OutcomeData<Method, Api.ErrorCommon>
            {
                Error = Api.ErrorCommon.NoError,
                Method = Method.NewMessage,
                Data = new
                {
                    data.Text,
                },
            }.ToJson();
        }

        private enum Method
        {
            NewMessage = 100,
        }

        private enum Error
        {
            EmptyText = 100,
        }

        private sealed class State
        {
        }

        [Serializable]
        private sealed class Model
        {
            public string Text { get; set; }
        }
    }
}