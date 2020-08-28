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

        private RoomStateData State =>
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
            Update(Notifications.UserDisconnected, user.Login);
        }

        public void UserConnected(User user)
        {
            _users.Add(user.SessionId, user);
            Update(Notifications.UserConnected, user.Login);
            Api.SendNotification(new ServerNotification
                {
                    Data = State,
                    Type = Notifications.State,
                },
                user.SessionId
            );
        }

        public void UserRegistered(User user)
        {
            _users[user.SessionId] = user;
            Update(Notifications.UserDisconnected, null);
            Update(Notifications.UserConnected, user.Login);
        }

        public void SetVideoUrl(string url)
        {
            _currentVideoUrl = url;
            Update(Notifications.VideoUrl, _currentVideoUrl);

            _pause = false;
            Update(Notifications.VideoPause, _pause);
        }

        public void PauseCycle()
        {
            _pause = !_pause;
            Update(Notifications.VideoPause, _pause);
        }

        private static void Update(string type, object data)
        {
            Api.SendNotification(new ServerNotification
                {
                    Data = data,
                    Type = type,
                }
            );
        }
    }
}