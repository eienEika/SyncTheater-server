using System;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Chat : ApiComponentBase
    {
        private static readonly MethodResult EmptyText = new MethodResult(ApiError.EmptyText);

        protected override bool AuthenticateRequired { get; } = true;

        protected override MethodResult MethodSwitch(string method, object data, User user)
        {
            var castedData = data as Model;

            return method switch
            {
                Methods.Chat.NewMessage => NewMessage(user, castedData?.Text),
                _ => MethodResult.UnknownMethod,
            };
        }

        private static MethodResult NewMessage(User user, string text)
        {
            Log.Debug($"Chat API: NewMessage request, text: \"{text}\"");

            if (user.IsAnonymous)
            {
                return MethodResult.LoginRequired;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return EmptyText;
            }

            var triggers = new[]
            {
                new NotificationTrigger(Notifications.NewChatMessage, text),
            };

            return new MethodResult(ApiError.NoError, triggers);
        }

        [Serializable]
        private sealed class Model
        {
            public string Text { get; set; }
        }
    }
}