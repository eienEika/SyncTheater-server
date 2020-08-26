using System;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Chat : ApiComponentBase
    {
        private static readonly Tuple<ApiError, object> EmptyTextError =
            new Tuple<ApiError, object>(ApiError.EmptyText, null);

        public override object Request(string body, User user)
        {
            Log.Verbose($"Got request to chat with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Model>>(body);

            if (user == null)
            {
                return new ApiRequestResponse
                {
                    Data = null,
                    Error = ApiError.AuthenticationRequired,
                    Method = request.Method,
                };
            }

            var (error, data) = request.Method switch
            {
                Methods.Chat.NewMessage => NewMessage(user, request.Data.Text),
                _ => new Tuple<ApiError, object>(ApiError.UnknownMethod, null),
            };

            return new ApiRequestResponse
            {
                Data = data,
                Error = error,
                Method = request.Method,
            };
        }

        private static Tuple<ApiError, object> NewMessage(User user, string text)
        {
            Log.Debug($"Chat API: NewMessage request, text: \"{text}\"");

            if (user.IsAnonymous)
            {
                return AuthenticationRequiredError;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return EmptyTextError;
            }

            var notification = new ServerNotification
            {
                Data = new
                {
                    Text = text,
                },
                Type = Notifications.NewChatMessage,
            };
            Api.SendNotification(notification);

            return new Tuple<ApiError, object>(ApiError.NoError, null);
        }

        [Serializable]
        private sealed class Model
        {
            public string Text { get; set; }
        }
    }
}