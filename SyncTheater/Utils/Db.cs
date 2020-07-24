using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using Dapper;
using Serilog;

namespace SyncTheater.Utils
{
    internal static class Db
    {
        static Db()
        {
            CreateTables();
        }

        public static async Task AddServerAsync(string serverId, string secret)
        {
            Log.Verbose($"Writing secret of server `{serverId}`...");

            await using var conn = new SQLiteConnection(Configuration.DbConnectionString);
            try
            {
                await conn.ExecuteAsync(@"INSERT INTO secrets (server_id, secret) VALUES (@ServerId, @Secret);",
                    new
                    {
                        ServerId = serverId,
                        Secret = secret,
                    }
                );
            }
            catch (SQLiteException)
            {
                await UpdateServerSecret(serverId, secret);
            }
        }

        public static async Task<string> GetSecretAsync(string serverId)
        {
            await using var conn = new SQLiteConnection(Configuration.DbConnectionString);
            return await conn.QueryFirstAsync<string>(@"SELECT secret FROM secrets WHERE server_id = @ServerId;",
                new
                {
                    ServerId = serverId,
                }
            );
        }

        public static async Task SetDefaultServerAsync(string serverId)
        {
            Log.Information($"Setting `{serverId}` as default...");

            await using var conn = new SQLiteConnection(Configuration.DbConnectionString);
            var defaultServer = await GetDefaultServerAsync();
            if (string.IsNullOrWhiteSpace(defaultServer))
            {
                await conn.ExecuteAsync(@"INSERT INTO default_server (val) VALUES (@ServerId);",
                    new
                    {
                        ServerId = serverId,
                    }
                );
            }
            else
            {
                Log.Information($"Changing default server from `{defaultServer}` to `{serverId}`.");

                await conn.ExecuteAsync(@"UPDATE default_server SET val = @ServerId;",
                    new
                    {
                        ServerId = serverId,
                    }
                );
            }
        }

        public static async Task<string> GetDefaultServerAsync()
        {
            await using var conn = new SQLiteConnection(Configuration.DbConnectionString);
            try
            {
                return await conn.QueryFirstAsync<string>(@"SELECT val FROM default_server");
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private static async Task UpdateServerSecret(string serverId, string secret)
        {
            Log.Debug($"Overwriting server `{serverId}`...");

            await using var conn = new SQLiteConnection(Configuration.DbConnectionString);
            await conn.ExecuteAsync(@"UPDATE secrets SET secret = @Secret WHERE server_id = @ServerId;",
                new
                {
                    Secret = secret,
                    ServerId = serverId,
                }
            );
        }

        private static void CreateTables()
        {
            using var conn = new SQLiteConnection(Configuration.DbConnectionString);
            conn.Execute(@"CREATE TABLE IF NOT EXISTS default_server (val TEXT);");
            conn.Execute(
                @"CREATE TABLE IF NOT EXISTS secrets (server_id TEXT PRIMARY KEY, secret TEXT UNIQUE NOT NULL);"
            );
        }
    }
}