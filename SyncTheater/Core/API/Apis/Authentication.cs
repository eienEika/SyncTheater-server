using System;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Utils;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Authentication : IApiComponent
    {
        public Tuple<object, SendTo> Request(string body, Guid sender)
        {
            Log.Verbose($"Got request to authentication with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Method, Model>>(body);

            var (error, data, sendTo) = request.Method switch
            {
                Method.AnonymousAuth => AuthenticateAnonymous(sender),
                Method.Disconnect => Disconnect(sender),
                _ => new Tuple<ApiError, object, SendTo>(ApiError.UnknownMethod, null, SendTo.Sender),
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

        private static Tuple<ApiError, object, SendTo> AuthenticateAnonymous(Guid user)
        {
            Room.GetState.NewAnonymousUser(user);

            return new Tuple<ApiError, object, SendTo>(ApiError.NoError, null, SendTo.Sender);
        }

        private static Tuple<ApiError, object, SendTo> Disconnect(Guid user)
        {
            Room.GetState.UserDisconnect(user);

            return new Tuple<ApiError, object, SendTo>(ApiError.NoError, null, SendTo.None);
        }

        private enum Method
        {
            AnonymousAuth,
            Disconnect,
        }

        [Serializable]
        private sealed class Model
        {
        }
    }
}