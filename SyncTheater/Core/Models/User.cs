using System;
using SyncTheater.Utils;

namespace SyncTheater.Core.Models
{
    internal sealed class User
    {
        public User(Guid sessionId)
        {
            SessionId = sessionId;
            IsAnonymous = true;
        }

        public User(Guid sessionId, string login)
        {
            Login = login;
            SessionId = sessionId;
            AuthKey = KeyGenerator.GetKey();
        }

        public User(Guid sessionId, string login, string authKey)
        {
            Login = login;
            SessionId = sessionId;
            AuthKey = authKey;
        }

        public string Login { get; }
        public string AuthKey { get; }
        public Guid SessionId { get; }
        public bool IsAnonymous { get; }

        public override bool Equals(object obj) => SessionId == (obj as User)?.SessionId;
        public override int GetHashCode() => SessionId.GetHashCode();
    }
}