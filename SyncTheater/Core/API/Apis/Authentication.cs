using System;
using System.Collections.Generic;
using Serilog;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Authentication : IApiComponent
    {
        public enum Method
        {
            Anonymous = 100,
            GetUsers,
            Disconnect,
        }

        private readonly State _state = new State();

        public Tuple<object, SendTo> RemoteRequest(string body, Guid sender)
        {
            Log.Verbose($"Got request to authentication with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Method, Model>>(body);

            var ((error, data), sendTo) = request.Method switch
            {
                Method.Anonymous => (AuthenticateAnonymous(sender), SendTo.Sender),
                _ => (new Tuple<ApiError, object>(ApiError.UnknownMethod, null), SendTo.Sender),
            };

            return new Tuple<object, SendTo>(new OutcomeData<Method>
                {
                    Data = data,
                    Error = error,
                    Method = request.Method,
                },
                sendTo
            );
        }

        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        public Tuple<ApiError, object> LocalRequest(Enum method, params dynamic[] args) =>
            (Method) method switch
            {
                Method.GetUsers => GetUsers(),
                Method.Disconnect => Disconnect(args[0]),
                _ => throw new NotSupportedException(),
            };

        private Tuple<ApiError, object> AuthenticateAnonymous(Guid sender)
        {
            _state.AuthenticatedUsers.Add(sender);

            return new Tuple<ApiError, object>(ApiError.NoError, null);
        }

        private Tuple<ApiError, object> GetUsers() =>
            new Tuple<ApiError, object>(ApiError.NoError, _state.AuthenticatedUsers);

        private Tuple<ApiError, object> Disconnect(Guid user)
        {
            _state.AuthenticatedUsers.Remove(user);
            
            return new Tuple<ApiError, object>(ApiError.NoError, null);
        }

        private sealed class State
        {
            public List<Guid> AuthenticatedUsers { get; } = new List<Guid>();
        }

        [Serializable]
        private sealed class Model
        {
        }
    }
}