using System;
using System.Collections.Generic;
using SyncTheater.Core.API;
using SyncTheater.Core.API.Types;

namespace SyncTheater.Core
{
    internal sealed class RoomState
    {
        private readonly HashSet<Guid> _anonymousUsers = new HashSet<Guid>();
        private string _currentVideoUrl;
        private bool _pause;

        public IEnumerable<Guid> Users => _anonymousUsers;

        public void UserDisconnect(Guid user)
        {
            _anonymousUsers.Remove(user);
        }

        public void NewAnonymousUser(Guid user)
        {
            _anonymousUsers.Add(user);
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