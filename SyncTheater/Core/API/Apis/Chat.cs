using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Chat : ApiComponentBase
    {
        private static readonly Tuple<ApiError, object, IEnumerable<NotificationTrigger>> EmptyTextError =
            new Tuple<ApiError, object, IEnumerable<NotificationTrigger>>(
                ApiError.EmptyText,
                null,
                Enumerable.Empty<NotificationTrigger>()
            );

        public override ApiResult Request(string body, User user)
        {
            Log.Verbose($"Got request to chat with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Model>>(body);

            if (!user.IsAuthenticated)
            {
                return AuthenticationRequiredResult(request.Method);
            }

            var (error, data, triggers) = request.Method switch
            {
                Methods.Chat.NewMessage => NewMessage(user, request.Data.Text),
                _ => UnknownMethodError,
            };

            var response = new ApiRequestResponse
            {
                Data = data,
                Error = error,
                Method = request.Method,
            };

            return new ApiResult(response, triggers);
        }

        private static Tuple<ApiError, object, IEnumerable<NotificationTrigger>> NewMessage(User user, string text)
        {
            Log.Debug($"Chat API: NewMessage request, text: \"{text}\"");

            if (user.IsAnonymous)
            {
                return LoginRequiredError;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return EmptyTextError;
            }

            var triggers = new[]
            {
                new NotificationTrigger(Notifications.NewChatMessage, text),
            };

            return new Tuple<ApiError, object, IEnumerable<NotificationTrigger>>(ApiError.NoError, null, triggers);
        }

        [Serializable]
        private sealed class Model
        {
            public string Text { get; set; }
        }
    }
}