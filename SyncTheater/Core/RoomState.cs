using System;
using System.Collections.Generic;
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

        public IEnumerable<User> Users => _users.Values;
        public IEnumerable<Guid> UserSessions => _users.Keys;
        public User User(Guid sessionId) => _users[sessionId];

        public void UserDisconnect(Guid sessionId)
        {
            _users.Remove(sessionId);
        }

        public void UserConnected(Guid sessionId, User user)
        {
            _users.Add(sessionId, user);
        }

        public void UserRegistered(Guid sessionId, User user)
        {
            _users[sessionId] = user;
        }

        public void SetVideoUrl(string url)
        {
            _currentVideoUrl = url;
            Update(StateUpdateCode.VideoUrl, _currentVideoUrl);

            _pause = false;
            Update(StateUpdateCode.Pause, _pause);
        }

        public void PauseCycle()
        {
            _pause = !_pause;
            Update(StateUpdateCode.Pause, _pause);
        }

        private void Update(StateUpdateCode code, object data)
        {
            Api.Send(ApiCode.State,
                new StateUpdate
                {
                    Data = data,
                    UpdateCode = code,
                },
                UserSessions
            );
        }
    }
}