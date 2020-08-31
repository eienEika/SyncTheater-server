using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly Tuple<ApiError, object, IEnumerable<NotificationTrigger>> LoginOccupiedError =
            new Tuple<ApiError, object, IEnumerable<NotificationTrigger>>(
                ApiError.LoginOccupied,
                null,
                Enumerable.Empty<NotificationTrigger>()
            );

        private static readonly Tuple<ApiError, object, IEnumerable<NotificationTrigger>> InvalidAuthKeyError =
            new Tuple<ApiError, object, IEnumerable<NotificationTrigger>>(
                ApiError.InvalidAuthKey,
                null,
                Enumerable.Empty<NotificationTrigger>()
            );

        public override ApiResult Request(string body, User user)
        {
            Log.Verbose($"Got request to authentication with body {body}.");

            var request = SerializationUtils.Deserialize<IncomeData<Model>>(body);

            var (error, data, triggers) = request.Method switch
            {
                Methods.Authentication.AuthAnon => AuthenticateAnonymous(user),
                Methods.Authentication.Disconnect => Disconnect(user),
                Methods.Authentication.Register => Register(user.SessionId, request.Data.Login),
                Methods.Authentication.AuthLogin => AuthenticateLogined(user, request.Data.AuthKey),
                _ => UnknownMethodError,
            };

            var response = new ApiRequestResponse
            {
                Data = data,
                Error = error,
                Method = request.Method,
            };

            return new ApiResult(response, triggers);
        }

        private static Tuple<ApiError, object, IEnumerable<NotificationTrigger>> AuthenticateAnonymous(User user)
        {
            Room.GetState.AuthenticateUser(user);

            var triggers = new[]
            {
                new NotificationTrigger(Notifications.State, Room.GetState.State, user.SessionId),
            };

            return new Tuple<ApiError, object, IEnumerable<NotificationTrigger>>(ApiError.NoError, null, triggers);
        }

        private static Tuple<ApiError, object, IEnumerable<NotificationTrigger>> Disconnect(User user)
        {
            Room.GetState.DisconnectUser(user);

            return NoError;
        }

        private static Tuple<ApiError, object, IEnumerable<NotificationTrigger>> Register(Guid sessionId, string login)
        {
            var user = new User(sessionId, login);

            var added = Db.AddUser(
                new UserDto
                {
                    Login = user.Login,
                    AuthKey = user.AuthKey,
                }
            );

            if (!added)
            {
                return LoginOccupiedError;
            }

            Room.GetState.RegisterUser(user);

            var response = new
            {
                Login = login,
                user.AuthKey,
            };

            return new Tuple<ApiError, object, IEnumerable<NotificationTrigger>>(
                ApiError.NoError,
                response,
                Enumerable.Empty<NotificationTrigger>()
            );
        }

        private static Tuple<ApiError, object, IEnumerable<NotificationTrigger>> AuthenticateLogined(
            User fakeUser, string authKey)
        {
            var user = Db.GetUserByAuthKey(authKey)?.Entity(fakeUser.SessionId);

            if (user == null)
            {
                return InvalidAuthKeyError;
            }

            Room.GetState.AuthenticateUser(user);

            Api.SendNotification(Notifications.State, Room.GetState.State, user.SessionId);

            var triggers = new[]
            {
                new NotificationTrigger(Notifications.State, Room.GetState.State, user.SessionId),
            };

            return new Tuple<ApiError, object, IEnumerable<NotificationTrigger>>(ApiError.NoError, null, triggers);
        }

        [Serializable]
        private sealed class Model
        {
            public string Login { get; set; }
            public string AuthKey { get; set; }
        }
    }
}