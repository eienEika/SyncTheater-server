using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using SyncTheater.KinotheaterService;
using SyncTheater.Utils.DB;
using YamlDotNet.Serialization;

namespace SyncTheater.Core
{
    [Serializable]
    public sealed class Profile
    {
        [YamlIgnore]
        public string ServerId { get; private set; }

        [YamlIgnore]
        public string Secret { get; private set; }

        public static async Task<bool> CreateAsync(Cli.NewProfileOptions options)
        {
            Log.Verbose("Creating new profile...");

            if (!string.IsNullOrWhiteSpace(options.Id)
                && Directory.Exists(Path.Combine(Configuration.AppProfilePath, options.Id)))
            {
                Log.Warning($"Profile `{options.Id}` is already exists. Overwrite? y/n.");

                if (!YesNo())
                {
                    return false;
                }
            }

            var registerResponse = await Service.RegisterAsync(options.Id);

            if (registerResponse == null)
            {
                Log.Fatal("Error while registering server.");

                return false;
            }

            await Db.AddServerAsync(registerResponse.Id, registerResponse.SuperSecretCode);

            var profilePath = Path.Combine(Configuration.AppProfilePath, registerResponse.Id);
            Directory.CreateDirectory(profilePath);
            await using var file = File.CreateText(Path.Combine(profilePath, "config.yaml"));
            new Serializer().Serialize(file, new Profile());

            Log.Information($"Profile `{registerResponse.Id}` created.");

            if (options.Default)
            {
                await Db.SetDefaultServerAsync(registerResponse.Id);

                Log.Information($"Server `{registerResponse.Id}` marked as default.");
            }

            return true;
        }

        public static async Task<Profile> GetAsync(string serverId)
        {
            if (string.IsNullOrWhiteSpace(serverId))
            {
                Log.Information("Using default server.");

                serverId = await Db.GetDefaultServerAsync();

                if (string.IsNullOrWhiteSpace(serverId))
                {
                    Log.Fatal("Default server was not found. Please, create a new server with option `--default`.");

                    return null;
                }
            }

            Log.Information($"Loading `{serverId}` profile.");

            var profilePath = Path.Combine(Configuration.AppProfilePath, serverId);

            if (!Directory.Exists(profilePath))
            {
                Log.Warning($"Profile {serverId} is not exists. It will be created.");

                var profileOptions = new Cli.NewProfileOptions
                {
                    Id = serverId,
                };
                if (!await CreateAsync(profileOptions))
                {
                    return null;
                }
            }

            using var file = File.OpenText(Path.Combine(profilePath, "config.yaml"));
            var profile = new Deserializer().Deserialize<Profile>(file);

            profile.ServerId = serverId;
            profile.Secret = await Db.GetSecretAsync(serverId);

            return profile;
        }

        private static bool YesNo()
        {
            while (true)
            {
                switch (Console.Read())
                {
                    case 'y':
                        return true;
                    case 'n':
                        return false;
                    default:
                        Console.WriteLine("Wrong letter. Please, try again.");
                        continue;
                }
            }
        }
    }
}