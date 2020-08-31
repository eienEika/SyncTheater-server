using System.Collections.Generic;
using System.Linq;

namespace SyncTheater.Core.API.Types
{
    internal sealed class ApiResult
    {
        public ApiResult(ApiRequestResponse response)
        {
            Response = response;
            Triggers = Enumerable.Empty<NotificationTrigger>();
        }

        public ApiResult(ApiRequestResponse response, IEnumerable<NotificationTrigger> triggers)
        {
            Response = response;
            Triggers = triggers;
        }

        public ApiRequestResponse Response { get; }
        public IEnumerable<NotificationTrigger> Triggers { get; }
    }
}