using System;

namespace SyncTheater.Core.API.Types
{
    [Serializable]
    internal sealed class StateUpdate
    {
        public StateUpdateCode UpdateCode { get; set; }
        public object Data { get; set; }
    }
}