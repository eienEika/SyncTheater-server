using System;
using System.Collections.Generic;
using System.Linq;
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

        public void UserDisconnect(User user)
        {
            _users.Remove(user.SessionId);
            Api.SendNotification(Notifications.UserDisconnected, user.Login);
        }

        public void UserConnected(User user)
        {
            user.IsAuthenticated = true;
            _users.Add(user.SessionId, user);
            Api.SendNotification(Notifications.UserConnected, user.Login);
        }

        public void UserRegistered(User user)
        {
            _users[user.SessionId] = user;
            Api.SendNotification(Notifications.UserDisconnected, null);
            Api.SendNotification(Notifications.UserConnected, user.Login);
        }

        public void SetVideoUrl(string url)
        {
            _currentVideoUrl = url;
            Api.SendNotification(Notifications.VideoUrl, _currentVideoUrl);

            _pause = false;
            Api.SendNotification(Notifications.VideoPause, _pause);
        }

        public void PauseCycle()
        {
            _pause = !_pause;
            Api.SendNotification(Notifications.VideoPause, _pause);
        }
    }
}