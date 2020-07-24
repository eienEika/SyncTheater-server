using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace SyncTheater
{
    internal static class Configuration
    {
        public const int MinPort = 1000;
        public const int MaxPort = 65535;
        public const string ValidServerIdChars = @"^[\w\d\-_]+$";

        static Configuration()
        {
            Directory.CreateDirectory(AppLogPath);
            Directory.CreateDirectory(AppProfilePath);
        }

        private static string AppDataPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "synctheater");

        private static string AppConfigPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "synctheater");

        private static string AppLogPath => Path.Combine(AppDataPath, "logs");
        public static string AppProfilePath => Path.Combine(AppConfigPath, "profiles");
        public static string DbConnectionString => $"Data Source={Path.Combine(AppDataPath, "db")};";

        public static void Logger(int verbosityLevel)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(levelSwitch: GetLevel(verbosityLevel))
                .WriteTo.File(Path.Combine(AppLogPath, "log.txt"),
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Debug,
                    outputTemplate: "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss zzz}: {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
        }

        private static LoggingLevelSwitch GetLevel(int level) =>
            new LoggingLevelSwitch(level switch
                {
                    1 => LogEventLevel.Fatal,
                    2 => LogEventLevel.Error,
                    3 => LogEventLevel.Warning,
                    4 => LogEventLevel.Information,
                    5 => LogEventLevel.Debug,
                    6 => LogEventLevel.Verbose,
                    _ => LogEventLevel.Information,
                }
            );
    }
}