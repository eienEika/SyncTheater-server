using System;

namespace SyncTheater.Core.API.Types
{
    [Serializable]
    internal class IncomeData<TData> where TData : class
    {
        public string Method { get; set; }
        public TData Data { get; set; }
    }
}