using System;

namespace SyncTheater.Core.API
{
    internal sealed class NotificationTrigger
    {
        private readonly object _data;
        private readonly Guid? _sessionId;
        private readonly string _type;

        public NotificationTrigger(string notificationType, object data)
        {
            _type = notificationType;
            _data = data;
        }

        public NotificationTrigger(string notificationType, object data, Guid sessionId) : this(notificationType, data)
        {
            _sessionId = sessionId;
        }

        public void Execute()
        {
            if (_sessionId.HasValue)
            {
                Api.SendNotification(_type, _data, _sessionId.Value);
            }
            else
            {
                Api.SendNotification(_type, _data);
            }
        }
    }
}