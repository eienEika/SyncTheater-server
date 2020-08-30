using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using SyncTheater.Core.API;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;

namespace SyncTheater.Core
{
    internal sealed class RoomState
    {
        private readonly Dictionary<Guid, User> _users = new Dictionary<Guid, User>();
        private string _currentVideoUrl;
        private bool _pause;

        public RoomStateData State =>
            new RoomStateData
            {
                Pause = _pause,
                VideoUrl = _currentVideoUrl,
                UserCount = _users.Count,
                UserLogins = _users.Values.Select(u => u.Login),
            };

        public IEnumerable<Guid> UserSessions => _users.Keys;
        public User User(Guid sessionId) => _users.ContainsKey(sessionId) ? _users[sessionId] : new User(sessionId);

        public ApiError DisconnectUser(User user)
        {
            if (!_users.ContainsKey(user.SessionId))
            {
                Log.Warning($"Error while user ({user.Login}|{user.SessionId}) disconnecting. User wasn't found.");

                return ApiError.UnknownError;
            }

            _users.Remove(user.SessionId);
            Api.SendNotification(Notifications.UserDisconnected, user.Login);

            return ApiError.NoError;
        }

        public ApiError AuthenticateUser(User user)
        {
            if (_users.ContainsKey(user.SessionId))
            {
                Log.Debug($"User ({user.Login}|{user.SessionId}) already authenticated.");

                return ApiError.AlreadyAuthenticated;
            }

            user.IsAuthenticated = true;
            _users.Add(user.SessionId, user);
            Api.SendNotification(Notifications.UserConnected, user.Login);

            return ApiError.NoError;
        }

        public ApiError RegisterUser(User user)
        {
            if (!_users.ContainsKey(user.SessionId))
            {
                Log.Debug($"User ({user.Login}|{user.SessionId}) not authenticated.");

                return ApiError.AuthenticationRequired;
            }

            if (!_users[user.SessionId].IsAnonymous)
            {
                Log.Debug($"User ({user.Login}|{user.SessionId}) already registered.");
                
                return ApiError.AlreadyRegistered;
            }

            _users[user.SessionId] = user;

            Api.SendNotification(Notifications.UserDisconnected, null);
            Api.SendNotification(Notifications.UserConnected, user.Login);

            return ApiError.NoError;
        }

        public ApiError SetVideoUrl(string url)
        {
            _currentVideoUrl = url;
            Api.SendNotification(Notifications.VideoUrl, _currentVideoUrl);

            _pause = false;
            Api.SendNotification(Notifications.VideoPause, _pause);

            return ApiError.NoError;
        }

        public ApiError PauseCycle()
        {
            _pause = !_pause;
            Api.SendNotification(Notifications.VideoPause, _pause);

            return ApiError.NoError;
        }
    }
}