using System;
using SyncTheater.Utils;

namespace SyncTheater.Core.Models
{
    internal sealed class User : IEquatable<User>
    {
        public User()
        {
            IsAnonymous = true;
        }

        public User(string login)
        {
            Login = login;
            AuthKey = KeyGenerator.GetKey();
        }

        public User(string login, string authKey)
        {
            Login = login;
            AuthKey = authKey;
        }

        public string Login { get; }
        public string AuthKey { get; }
        public bool IsAnonymous { get; }

        public bool Equals(User other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Login == other.Login
                   && AuthKey == other.AuthKey;
        }

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is User other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Login, AuthKey);
    }
}