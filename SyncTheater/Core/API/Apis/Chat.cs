using System;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Chat : IApiComponent
    {
        public Tuple<object, SendTo> Request(string body, User user, Guid sessionId)
        {
            Log.Verbose($"Got request to chat with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Method, Model>>(body);

            if (user == null)
            {
                return new Tuple<object, SendTo>(new OutcomeData<Method>
                    {
                        Data = null,
                        Error = ApiError.AuthenticationRequired,
                        Method = request.Method,
                    },
                    SendTo.Sender
                );
            }

            var (error, data, sendTo) = request.Method switch
            {
                Method.NewMessage => NewMessage(user, request.Data.Text),
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

        private static Tuple<ApiError, object, SendTo> NewMessage(User user, string text)
        {
            Log.Debug($"Chat API: NewMessage request, text: \"{text}\"");

            if (user.IsAnonymous)
            {
                return new Tuple<ApiError, object, SendTo>(ApiError.AuthenticationRequired, null, SendTo.Sender);
            }

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