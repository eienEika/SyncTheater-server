using System;

namespace SyncTheater.Core.API
{
    internal interface IApiComponent
    {
        public string Request(string body);

        [Serializable]
        protected class IncomeData<TMethod, TData> where TMethod : Enum where TData : class
        {
            public TMethod Method { get; set; }
            public TData Data { get; set; }
        }

        [Serializable]
        protected class OutcomeData<TMethod, TError> where TMethod : Enum where TError : Enum
        {
            public TMethod Method { get; set; }
            public TError Error { get; set; }
            public object Data { get; set; }
        }
    }
}