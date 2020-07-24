using System;
using Serilog;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Chat
    {
        public static string Request(string body)
        {
            Log.Verbose($"Got request to chat with body {body}.");

            var data = SerializationUtils.Deserialize<Model>(body);
            return data.Method switch
            {
                Method.NewMessage => NewMessage(data),
                _ => Api.UnknownMethodResponse(data.Method),
            };
        }

        private static string NewMessage(Model data)
        {
            Log.Debug($"Chat API: NewMessage request, text: \"{data.Text}\"");

            if (string.IsNullOrWhiteSpace(data.Text))
            {
                return new OutcomeData
                {
                    Error = Error.EmptyText,
                    Method = data.Method,
                }.ToJson();
            }

            return new OutcomeData
            {
                Error = (Error) Api.ErrorCommon.NoError,
                Method = Method.NewMessage,
                Text = data.Text,
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

        [Serializable]
        private sealed class Model : IncomeDataBase<Method>
        {
            public string Text { get; set; }
        }

        [Serializable]
        private sealed class OutcomeData : OutcomeDataBase<Method, Error>
        {
            public string Text { get; set; }
        }
    }
}