using System;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils;
using SyncTheater.Utils.DB;
using SyncTheater.Utils.DB.DTOs;

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
                Method.Register => Register(sender, request.Data.Login),
                Method.LoginedAuth => AuthenticateLogined(sender, request.Data.AuthKey),
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

        private static Tuple<ApiError, object, SendTo> AuthenticateAnonymous(Guid sessionId)
        {
            Room.GetState.UserConnected(new User(sessionId));

            return new Tuple<ApiError, object, SendTo>(ApiError.NoError, null, SendTo.Sender);
        }

        private static Tuple<ApiError, object, SendTo> Disconnect(Guid sessionId)
        {
            Room.GetState.UserDisconnect(sessionId);

            return new Tuple<ApiError, object, SendTo>(ApiError.NoError, null, SendTo.None);
        }

        private static Tuple<ApiError, object, SendTo> Register(Guid sessionId, string login)
        {
            var user = new User(sessionId, login);

            var added = Db.AddUser(new UserDto
                {
                    Login = user.Login,
                    AuthKey = user.AuthKey,
                }
            );

            if (!added)
            {
                return new Tuple<ApiError, object, SendTo>(ApiError.LoginOccupied, null, SendTo.Sender);
            }

            Room.GetState.UserRegistered(user);

            return new Tuple<ApiError, object, SendTo>(ApiError.NoError,
                new
                {
                    Login = login,
                    user.AuthKey,
                },
                SendTo.Sender
            );
        }

        private static Tuple<ApiError, object, SendTo> AuthenticateLogined(Guid sessionId, string authKey)
        {
            var user = Db.GetUserByAuthKey(authKey)?.Entity(sessionId);

            if (user == null)
            {
                return new Tuple<ApiError, object, SendTo>(ApiError.InvalidAuthKey, null, SendTo.Sender);
            }

            Room.GetState.UserConnected(user);

            return new Tuple<ApiError, object, SendTo>(ApiError.NoError, null, SendTo.Sender);
        }

        private enum Method
        {
            AnonymousAuth,
            Disconnect,
            Register,
            LoginedAuth,
        }

        [Serializable]
        private sealed class Model
        {
            public string Login { get; set; }
            public string AuthKey { get; set; }
        }
    }
}