using System;
using Serilog;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Chat : IApiComponent
    {
        public Tuple<object, SendTo> Request(string body)
        {
            Log.Verbose($"Got request to chat with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Method, Model>>(body);

            return request.Method switch
            {
                Method.NewMessage => NewMessage(request.Data.Text),
                _ => new Tuple<object, SendTo>(Api.UnknownMethodResponse(request.Method), SendTo.Sender),
            };
        }

        private Tuple<object, SendTo> NewMessage(string text)
        {
            Log.Debug($"Chat API: NewMessage request, text: \"{text}\"");

            if (string.IsNullOrWhiteSpace(text))
            {
                return new Tuple<object, SendTo>(new OutcomeData<Method, Error>
                    {
                        Data = null,
                        Error = Error.EmptyText,
                        Method = Method.NewMessage,
                    },
                    SendTo.Sender
                );
            }

            return new Tuple<object, SendTo>(new OutcomeData<Method, ErrorCommon>
                {
                    Data = new
                    {
                        Text = text,
                    },
                    Error = ErrorCommon.NoError,
                    Method = Method.NewMessage,
                },
                SendTo.All
            );
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