using System;

namespace SyncTheater.Core.API
{
    [Serializable]
    internal abstract class IncomeDataBase<TMethod> where TMethod : Enum
    {
        public TMethod Method { get; set; }
    }
}