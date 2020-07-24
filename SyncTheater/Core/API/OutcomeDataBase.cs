using System;

namespace SyncTheater.Core.API
{
    [Serializable]
    internal abstract class OutcomeDataBase<TMethod, TError> where TMethod : Enum where TError : Enum
    {
        public TMethod Method { get; set; }
        public TError Error { get; set; }
    }
}