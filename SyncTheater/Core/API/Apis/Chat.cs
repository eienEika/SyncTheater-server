using System;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Chat : IApiComponent
    {
        public Tuple<object, Api.SendTo> Request(string body, User user, Guid sessionId)
        {
            Log.Verbose($"Got request to chat with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Model>>(body);

            if (user == null)
            {
                return new Tuple<object, Api.SendTo>(new OutcomeData
                    {
                        Data = null,
                        Error = ApiError.AuthenticationRequired,
                        Method = request.Method,
                    },
                    Api.SendTo.Sender
                );
            }

            var (error, data, sendTo) = request.Method switch
            {
                Methods.Chat.NewMessage => NewMessage(user, request.Data.Text),
                _ => new Tuple<ApiError, object, Api.SendTo>(ApiError.UnknownMethod, null, Api.SendTo.Sender),
            };

            return new Tuple<object, Api.SendTo>(new OutcomeData
                {
                    Data = data,
                    Error = error,
                    Method = request.Method,
                },
                sendTo
            );
        }

        private static Tuple<ApiError, object, Api.SendTo> NewMessage(User user, string text)
        {
            Log.Debug($"Chat API: NewMessage request, text: \"{text}\"");

            if (user.IsAnonymous)
            {
                return new Tuple<ApiError, object, Api.SendTo>(ApiError.AuthenticationRequired, null, Api.SendTo.Sender);
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return new Tuple<ApiError, object, Api.SendTo>(ApiError.EmptyText, null, Api.SendTo.Sender);
            }

            return new Tuple<ApiError, object, Api.SendTo>(ApiError.NoError,
                new
                {
                    Text = text,
                },
                Api.SendTo.All
            );
        }

        [Serializable]
        private sealed class Model
        {
            public string Text { get; set; }
        }
    }
}