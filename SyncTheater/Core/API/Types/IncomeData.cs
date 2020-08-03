using System;

namespace SyncTheater.Core.API.Types
{
    [Serializable]
    internal class IncomeData<TMethod, TData> where TMethod : Enum where TData : class
    {
        public TMethod Method { get; set; }
        public TData Data { get; set; }
    }
}