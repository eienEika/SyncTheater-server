using System;

namespace SyncTheater.Core.API.Types
{
    [Serializable]
    internal class OutcomeData
    {
        public string Method { get; set; }
        public ApiError Error { get; set; }
        public object Data { get; set; }
    }
}