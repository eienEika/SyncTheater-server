using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using SyncTheater.Core.Models;

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

        protected override bool AuthenticateRequired { get; } = true;

        protected override Tuple<ApiError, object, IEnumerable<NotificationTrigger>> MethodSwitch(
            string method, object data, User user)
        {
            var castedData = data as Model;

            return method switch
            {
                Methods.Chat.NewMessage => NewMessage(user, castedData?.Text),
                _ => UnknownMethodError,
            };
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