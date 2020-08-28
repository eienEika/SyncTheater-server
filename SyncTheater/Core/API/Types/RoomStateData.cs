using System;
using System.Collections.Generic;

namespace SyncTheater.Core.API.Types
{
    [Serializable]
    internal sealed class RoomStateData
    {
        public string VideoUrl { get; set; }
        public bool Pause { get; set; }
        public IEnumerable<string> UserLogins { get; set; }
        public int UserCount { get; set; }
    }
}