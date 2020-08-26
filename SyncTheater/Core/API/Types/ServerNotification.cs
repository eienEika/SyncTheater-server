using System;

namespace SyncTheater.Core.API.Types
{
    [Serializable]
    internal sealed class ServerNotification
    {
        public string Type { get; set; }
        public object Data { get; set; }
    }
}