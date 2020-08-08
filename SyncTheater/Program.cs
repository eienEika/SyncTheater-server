using System;
using System.Threading.Tasks;
using CommandLine;
using Serilog;
using SyncTheater.Core;

namespace SyncTheater
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.CancelKeyPress += CancelHandler;

            Parser.Default.ParseArguments<Cli.NewProfileOptions, Cli.RunServerOptions>(args)
                .WithParsed<Cli.NewProfileOptions>(options => CreateProfile(options).GetAwaiter().GetResult())
                .WithParsed<Cli.RunServerOptions>(options => Run(options).GetAwaiter().GetResult());

            Stop();
        }

        public static async Task CreateProfile(Cli.NewProfileOptions options)
        {
            if (!options.Validate())
            {
                return;
            }

            Configuration.Logger(options.Verbosity);

            await Profile.CreateAsync(options);
        }

        public static async Task Run(Cli.RunServerOptions options)
        {
            if (!options.Validate())
            {
                return;
            }

            Configuration.Logger(options.Verbosity);

            var profile = await Profile.GetAsync(options.Server);

            if (profile == null)
            {
                Log.Fatal("Error while loading profile.");

                return;
            }

            Room.GetInstance
                .Initialize(new ServerSettings(options, !profile.Private), new RoomSettings(profile))
                .Open();
        }

        public static void Stop()
        {
            var room = Room.GetInstance;

            if (room.IsOpen)
            {
                room.Close();
            }
        }

        private static void CancelHandler(object sender, ConsoleCancelEventArgs args)
        {
            Log.Information("Received cancel signal, stopping program...");

            Stop();
        }
    }
}