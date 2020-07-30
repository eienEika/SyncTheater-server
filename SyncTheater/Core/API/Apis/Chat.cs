using System;
using Serilog;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Chat : IApiComponent
    {
        public enum Method
        {
            NewMessage = 100,
        }

        public Tuple<object, SendTo> RemoteRequest(string body, Guid sender)
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

        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        public Tuple<ApiError, object> LocalRequest(Enum method, params dynamic[] args) =>
            (Method) method switch
            {
                _ => throw new NotSupportedException(),
            };

        private Tuple<ApiError, object, SendTo> NewMessage(string text)
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