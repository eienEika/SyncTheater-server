using System;
using System.Text.RegularExpressions;
using CommandLine;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SyncTheater
{
    public static class Cli
    {
        [Verb("new", HelpText = "Create new profile.")]
        public sealed class NewProfileOptions
        {
            [Option("default", Default = false, HelpText = "Create as default server.")]
            public bool Default { get; set; }

            [Option('i', "id", Required = true, HelpText = "Server id.")]
            public string Id { get; set; }

            [Option("private", Default = false, HelpText = "Don't use SyncTheater's service.")]
            public bool Private { get; set; }

            [Option('v', "verbosity", Default = 4, HelpText = "Verbosity level, from 1 to 6.")]
            public int Verbosity { get; set; }

            public bool Validate()
            {
                if (!new Regex(Configuration.ValidServerIdChars).IsMatch(Id))
                {
                    Console.Error.WriteLine(
                        $"Invalid '--id' value: {Id}. Allowed symbols are: latin alphabet, digits and `_`, `-`."
                    );
                    return false;
                }

                if (Verbosity < 1 && Verbosity > 6)
                {
                    Console.Error.WriteLine(
                        $"Invalid '--verbosity' level: {Verbosity}. Value must be in range from 1 to 6."
                    );
                    return false;
                }

                return true;
            }
        }

        [Verb("run", HelpText = "Run server.")]
        public sealed class RunServerOptions
        {
            [Value(0, Default = "", HelpText = "Server to run.")]
            public string Server { get; set; }

            [Option("no-ipv6", Default = false, HelpText = "Disable IPv6.")]
            public bool NoIpv6 { get; set; }

            [Option("no-nat-traversal", Default = false, HelpText = "Disable NAT traversal.")]
            public bool NoNatTraversal { get; set; }

            [Option("no-pmp", Default = false, HelpText = "Disable NAT Port Mapping Protocol.")]
            public bool NoPmp { get; set; }

            [Option("no-upnp", Default = false, HelpText = "Disable Universal Plug and Play.")]
            public bool NoUpnp { get; set; }

            [Option('p', "port", Default = 9128, HelpText = "Port to bind.")]
            public int Port { get; set; }

            [Option('v', "verbosity", Default = 4, HelpText = "Verbosity level, from 1 to 6.")]
            public int Verbosity { get; set; }

            public bool Validate()
            {
                if (Port < Configuration.MinPort || Port > Configuration.MaxPort)
                {
                    Console.Error.WriteLine(
                        $"Invalid '--port' value: {Port}. Port must be between {Configuration.MinPort} and {Configuration.MaxPort}."
                    );
                    return false;
                }

                if (Verbosity < 1 && Verbosity > 6)
                {
                    Console.Error.WriteLine(
                        $"Invalid '--verbosity' level: {Verbosity}. Value must be in range from 1 to 6."
                    );
                    return false;
                }

                return true;
            }
        }
    }
}