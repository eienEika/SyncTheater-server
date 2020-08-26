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
        private static readonly Tuple<ApiError, object> LoginOccupiedError =
            new Tuple<ApiError, object>(ApiError.LoginOccupied, null);

        private static readonly Tuple<ApiError, object> InvalidAuthKeyError =
            new Tuple<ApiError, object>(ApiError.InvalidAuthKey, null);

        public override object Request(string body, User user)
        {
            Log.Verbose($"Got request to authentication with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Model>>(body);

            var (error, data) = request.Method switch
            {
                Methods.Authentication.AuthAnon => AuthenticateAnonymous(user),
                Methods.Authentication.Disconnect => Disconnect(user),
                Methods.Authentication.Register => Register(user.SessionId, request.Data.Login),
                Methods.Authentication.AuthLogin => AuthenticateLogined(user, request.Data.AuthKey),
                _ => new Tuple<ApiError, object>(ApiError.UnknownMethod, null),
            };

            return new ApiRequestResponse
            {
                Data = data,
                Error = error,
                Method = request.Method,
            };
        }

        private static Tuple<ApiError, object> AuthenticateAnonymous(User user)
        {
            Room.GetState.UserConnected(user);

            return NoError;
        }

        private static Tuple<ApiError, object> Disconnect(User user)
        {
            Room.GetState.UserDisconnect(user);

            return Nothing;
        }

        private static Tuple<ApiError, object> Register(Guid sessionId, string login)
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
                return LoginOccupiedError;
            }

            Room.GetState.UserRegistered(user);

            return new Tuple<ApiError, object>(ApiError.NoError,
                new
                {
                    Login = login,
                    user.AuthKey,
                }
            );
        }

        private static Tuple<ApiError, object> AuthenticateLogined(User fakeUser, string authKey)
        {
            var user = Db.GetUserByAuthKey(authKey)?.Entity(fakeUser.SessionId);

            if (user == null)
            {
                return InvalidAuthKeyError;
            }

            Room.GetState.UserConnected(user);

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