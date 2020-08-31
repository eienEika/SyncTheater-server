using System.Collections.Generic;
using System.Linq;

namespace SyncTheater.Core.API.Types
{
    internal sealed class MethodResult
    {
        public MethodResult(ApiError error)
        {
            Error = error;
            Triggers = Enumerable.Empty<NotificationTrigger>();
        }

        public MethodResult(ApiError error, object data) : this(error)
        {
            Data = data;
        }

        public MethodResult(ApiError error, IEnumerable<NotificationTrigger> triggers)
        {
            Error = error;
            Triggers = triggers;
        }

        public MethodResult(ApiError error, object data, IEnumerable<NotificationTrigger> triggers) : this(
            error,
            triggers
        )
        {
            Data = data;
        }

        public ApiError Error { get; }
        public object Data { get; }
        public IEnumerable<NotificationTrigger> Triggers { get; }

        public static MethodResult Ok { get; } = new MethodResult(ApiError.NoError);
        public static MethodResult LoginRequired { get; } = new MethodResult(ApiError.LoginRequired);
        public static MethodResult UnknownMethod { get; } = new MethodResult(ApiError.UnknownMethod);
    }
}