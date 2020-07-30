using System;

namespace SyncTheater.Core.API
{
    [Serializable]
    internal class OutcomeData<T> where T : Enum
    {
        public T Method { get; set; }
        public ApiError Error { get; set; }
        public object Data { get; set; }
    }
}