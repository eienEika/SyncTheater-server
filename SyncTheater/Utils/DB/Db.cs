using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using Dapper;
using Serilog;
using SyncTheater.Utils.DB.DTOs;

namespace SyncTheater.Utils.DB
{
    internal static class Db
    {
        private static readonly SQLiteConnection Conn = new SQLiteConnection(Configuration.DbConnectionString);

        static Db()
        {
            CreateTables();
        }

        public static async Task AddServiceSecretAsync(string serverId, string secret)
        {
            Log.Verbose($"Writing secret of server `{serverId}`...");

            try
            {
                await Conn.ExecuteAsync(
                    @"INSERT INTO secrets (server_id, secret) VALUES (@ServerId, @Secret);",
                    new
                    {
                        ServerId = serverId,
                        Secret = secret,
                    }
                );
            }
            catch (SQLiteException)
            {
                await UpdateServiceSecret(serverId, secret);
            }
        }

        public static async Task<string> GetServiceSecretAsync(string serverId) =>
            await Conn.QueryFirstAsync<string>(
                @"SELECT secret FROM secrets WHERE server_id = @ServerId;",
                new
                {
                    ServerId = serverId,
                }
            );

        public static async Task SetDefaultServerAsync(string serverId)
        {
            Log.Information($"Setting `{serverId}` as default...");

            var defaultServer = await GetDefaultServerAsync();
            if (string.IsNullOrWhiteSpace(defaultServer))
            {
                await Conn.ExecuteAsync(
                    @"INSERT INTO default_server (val) VALUES (@ServerId);",
                    new
                    {
                        ServerId = serverId,
                    }
                );
            }
            else
            {
                Log.Information($"Changing default server from `{defaultServer}` to `{serverId}`.");

                await Conn.ExecuteAsync(
                    @"UPDATE default_server SET val = @ServerId;",
                    new
                    {
                        ServerId = serverId,
                    }
                );
            }
        }

        public static async Task<string> GetDefaultServerAsync()
        {
            try
            {
                return await Conn.QueryFirstAsync<string>(@"SELECT val FROM default_server");
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private static async Task UpdateServiceSecret(string serverId, string secret)
        {
            Log.Debug($"Overwriting server `{serverId}`...");

            await Conn.ExecuteAsync(
                @"UPDATE secrets SET secret = @Secret WHERE server_id = @ServerId;",
                new
                {
                    Secret = secret,
                    ServerId = serverId,
                }
            );
        }

        public static bool AddUser(UserDto user)
        {
            try
            {
                Conn.Execute(
                    @"INSERT INTO users (login, authKey) VALUES (@Login, @AuthKey);",
                    new
                    {
                        Nickname = user.Login,
                        user.AuthKey,
                    }
                );

                return true;
            }
            catch (SQLiteException)
            {
                return false;
            }
        }

        public static UserDto GetUser(string login) =>
            Conn.QueryFirst<UserDto>(
                @"SELECT * FROM users WHERE login = @Login;",
                new
                {
                    Login = login,
                }
            );

        public static UserDto GetUserByAuthKey(string authKey) =>
            Conn.QueryFirst<UserDto>(
                @"SELECT * FROM users WHERE authKey = @AuthKey;",
                new
                {
                    AuthKey = authKey,
                }
            );

        private static void CreateTables()
        {
            Conn.Execute(@"CREATE TABLE IF NOT EXISTS default_server (val TEXT);");
            Conn.Execute(
                @"CREATE TABLE IF NOT EXISTS service_secrets (server_id TEXT PRIMARY KEY, secret TEXT UNIQUE NOT NULL);"
            );
            Conn.Execute(@"CREATE TABLE IF NOT EXISTS users (login TEXT PRIMARY KEY, authKey TEXT UNIQUE NOT NULL);");
        }
    }
}