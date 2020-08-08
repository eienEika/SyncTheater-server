using System;
using Serilog;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils;
using SyncTheater.Utils.DB;
using SyncTheater.Utils.DB.DTOs;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Authentication : ApiComponentBase
    {
        private static readonly Tuple<ApiError, object, Api.SendTo> LoginOccupiedError =
            new Tuple<ApiError, object, Api.SendTo>(ApiError.LoginOccupied, null, Api.SendTo.Sender);

        private static readonly Tuple<ApiError, object, Api.SendTo> InvalidAuthKeyError =
            new Tuple<ApiError, object, Api.SendTo>(ApiError.InvalidAuthKey, null, Api.SendTo.Sender);

        public override Tuple<object, Api.SendTo> Request(string body, User user, Guid sessionId)
        {
            Log.Verbose($"Got request to authentication with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Model>>(body);

            var (error, data, sendTo) = request.Method switch
            {
                Methods.Authentication.AuthAnon => AuthenticateAnonymous(sessionId),
                Methods.Authentication.Disconnect => Disconnect(sessionId),
                Methods.Authentication.Register => Register(sessionId, request.Data.Login),
                Methods.Authentication.AuthLogin => AuthenticateLogined(sessionId, request.Data.AuthKey),
                _ => new Tuple<ApiError, object, Api.SendTo>(ApiError.UnknownMethod, null, Api.SendTo.Sender),
            };

            return new Tuple<object, Api.SendTo>(new OutcomeData
                {
                    Data = data,
                    Error = error,
                    Method = request.Method,
                },
                sendTo
            );
        }

        private static Tuple<ApiError, object, Api.SendTo> AuthenticateAnonymous(Guid sessionId)
        {
            Room.GetState.UserConnected(sessionId, new User());

            return NoError;
        }

        private static Tuple<ApiError, object, Api.SendTo> Disconnect(Guid sessionId)
        {
            Room.GetState.UserDisconnect(sessionId);

            return Nothing;
        }

        private static Tuple<ApiError, object, Api.SendTo> Register(Guid sessionId, string login)
        {
            var user = new User(login);

            var added = Db.AddUser(new UserDto
                {
                    Login = user.Login,
                    AuthKey = user.AuthKey,
                }
            );

            if (!added)
            {
                return LoginOccupiedError;
            }

            Room.GetState.UserRegistered(sessionId, user);

            return new Tuple<ApiError, object, Api.SendTo>(ApiError.NoError,
                new
                {
                    Login = login,
                    user.AuthKey,
                },
                Api.SendTo.Sender
            );
        }

        private static Tuple<ApiError, object, Api.SendTo> AuthenticateLogined(Guid sessionId, string authKey)
        {
            var user = Db.GetUserByAuthKey(authKey)?.Entity;

            if (user == null)
            {
                return InvalidAuthKeyError;
            }

            Room.GetState.UserConnected(sessionId, user);

            return NoError;
        }

        [Serializable]
        private sealed class Model
        {
            public string Login { get; set; }
            public string AuthKey { get; set; }
        }
    }
}