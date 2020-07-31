using System;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Chat : IApiComponent
    {
        public Tuple<object, SendTo> Request(string body, Guid sender)
        {
            Log.Verbose($"Got request to chat with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Method, Model>>(body);

            var (error, data, sendTo) = request.Method switch
            {
                Method.NewMessage => NewMessage(request.Data.Text),
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

        private static Tuple<ApiError, object, SendTo> NewMessage(string text)
        {
            Log.Debug($"Chat API: NewMessage request, text: \"{text}\"");

            if (string.IsNullOrWhiteSpace(text))
            {
                return new Tuple<ApiError, object, SendTo>(ApiError.EmptyText, null, SendTo.Sender);
            }

            return new Tuple<ApiError, object, SendTo>(ApiError.NoError,
                new
                {
                    Text = text,
                },
                SendTo.All
            );
        }

        private enum Method
        {
            NewMessage,
        }

        [Serializable]
        private sealed class Model
        {
            public string Text { get; set; }
        }
    }
}