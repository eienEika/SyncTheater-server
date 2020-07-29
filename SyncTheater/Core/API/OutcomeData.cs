using System;

namespace SyncTheater.Core.API
{
    [Serializable]
    internal class OutcomeData<TMethod, TError> where TMethod : Enum where TError : Enum
    {
        public TMethod Method { get; set; }
        public TError Error { get; set; }
        public object Data { get; set; }
    }
}