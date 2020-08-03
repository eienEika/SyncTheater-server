using System;
using SyncTheater.Core.Models;

namespace SyncTheater.Utils.DB.DTOs
{
    [Serializable]
    internal sealed class UserDto
    {
        public string Login { get; set; }
        public string AuthKey { get; set; }

        public User Entity => new User(Login, AuthKey);
    }
}