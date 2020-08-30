using System;
using SyncTheater.Utils;

namespace SyncTheater.Core.Models
{
    internal sealed class User : IEquatable<User>
    {
        public User(Guid sessionId)
        {
            SessionId = sessionId;
            IsAnonymous = true;
        }

        public User(Guid sessionId, string login)
        {
            SessionId = sessionId;
            Login = login;
            AuthKey = KeyGenerator.GetKey();
        }

        public User(Guid sessionId, string login, string authKey)
        {
            SessionId = sessionId;
            Login = login;
            AuthKey = authKey;
        }

        public Guid SessionId { get; }
        public string Login { get; }
        public string AuthKey { get; }
        public bool IsAnonymous { get; }
        public bool IsAuthenticated { get; set; }

        public bool Equals(User other) =>
            !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || SessionId.Equals(other.SessionId));

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is User other && Equals(other);

        public override int GetHashCode() => SessionId.GetHashCode();
    }
}