using System;
using System.Collections.Generic;
using SyncTheater.Core.API;
using SyncTheater.Core.API.Types;
using SyncTheater.Core.Models;

namespace SyncTheater.Core
{
    internal sealed class RoomState
    {
        private readonly HashSet<User> _users = new HashSet<User>();
        private string _currentVideoUrl;
        private bool _pause;

        public IEnumerable<User> Users => _users;

        public void UserDisconnect(Guid sessionId)
        {
            _users.Remove(new User(sessionId));
        }

        public void UserConnected(User user)
        {
            _users.Add(user);
        }

        public void UserRegistered(User user)
        {
            _users.Remove(new User(user.SessionId));
            _users.Add(user);
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
                Users
            );
        }
    }
}