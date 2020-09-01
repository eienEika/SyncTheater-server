using System;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;
using SyncTheater.Utils.DB;
using SyncTheater.Utils.DB.DTOs;

namespace SyncTheater.Core.API.Apis
{
    internal sealed class Authentication : ApiComponentBase
    {
        private static readonly MethodResult LoginOccupied = new MethodResult(ApiError.LoginOccupied);
        private static readonly MethodResult InvalidAuthKey = new MethodResult(ApiError.InvalidAuthKey);
        private static readonly MethodResult BadLogin = new MethodResult(ApiError.BadLogin);

        protected override bool AuthenticateRequired { get; } = false;

        protected override MethodResult MethodSwitch(string method, object data, User user)
        {
            var castedData = data as Model;

            return method switch
            {
                Methods.Authentication.AuthAnon => AuthenticateAnonymous(user),
                Methods.Authentication.Disconnect => Disconnect(user),
                Methods.Authentication.Register => Register(user.SessionId, castedData?.Login),
                Methods.Authentication.AuthLogin => AuthenticateLogined(user, castedData?.AuthKey),
                _ => MethodResult.UnknownMethod,
            };
        }

        private static MethodResult AuthenticateAnonymous(User user)
        {
            var res = Room.GetState.AuthenticateUser(user);

            if (res != ApiError.NoError)
            {
                return new MethodResult(res);
            }

            var triggers = new[]
            {
                new NotificationTrigger(Notifications.State, Room.GetState.State, user.SessionId),
            };

            return new MethodResult(ApiError.NoError, triggers);
        }

        private static MethodResult Disconnect(User user)
        {
            Room.GetState.DisconnectUser(user);

            return MethodResult.Ok;
        }

        private static MethodResult Register(Guid sessionId, string login)
        {
            if (User.CheckLoginIsValid(login))
            {
                return BadLogin;
            }
            
            if (Db.GetUser(login) != null)
            {
                return LoginOccupied;
            }

            var user = new User(sessionId, login);

            var res = Room.GetState.RegisterUser(user);

            if (res != ApiError.NoError)
            {
                return new MethodResult(res);
            }

            Db.AddUser(
                new UserDto
                {
                    Login = user.Login,
                    AuthKey = user.AuthKey,
                }
            );

            var response = new
            {
                Login = login,
                user.AuthKey,
            };

            return new MethodResult(ApiError.NoError, response);
        }

        private static MethodResult AuthenticateLogined(User fakeUser, string authKey)
        {
            var user = Db.GetUserByAuthKey(authKey)?.Entity(fakeUser.SessionId);

            if (user == null)
            {
                return InvalidAuthKey;
            }

            var res = Room.GetState.AuthenticateUser(user);

            if (res != ApiError.NoError)
            {
                return new MethodResult(res);
            }

            Api.SendNotification(Notifications.State, Room.GetState.State, user.SessionId);

            var triggers = new[]
            {
                new NotificationTrigger(Notifications.State, Room.GetState.State, user.SessionId),
            };

            return new MethodResult(ApiError.NoError, triggers);
        }

        [Serializable]
        private sealed class Model
        {
            public string Login { get; set; }
            public string AuthKey { get; set; }
        }
    }
}