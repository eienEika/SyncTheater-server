using System.Net.Sockets;
using Serilog;
using SyncTheater.Utils.NAT;

namespace SyncTheater.Core
{
    internal sealed class ServerSettings
    {
        public ServerSettings(Cli.RunServerOptions options)
        {
            Port = options.Port;
            UseIpv6 = !options.NoIpv6 && Socket.OSSupportsIPv6;
            UseService = !options.NoService;

            if (options.NoNatTraversal)
            {
                Nat = new NatResult(false, null);
            }
            else
            {
                var result = NatTraversal.ForwardPortAsync(!options.NoUpnp, !options.NoPmp, options.Port).Result;
                Nat = new NatResult(true, result);
            }

            PrintLog();
        }

        public int Port { get; }
        public bool UseIpv6 { get; }
        public NatResult Nat { get; }
        public bool UseService { get; }
        public string Ipv4 { get; set; }
        public string Ipv6 { get; set; }

        private void PrintLog()
        {
            Log.Debug($"Using port: {Port}.");
            Log.Debug($"Using IPv6: {UseIpv6}.");
            Log.Debug($"Using service: {UseService}.");
            // ReSharper disable once InvertIf
            if (Nat.Traverse)
            {
                Log.Debug($"Port forwarding success: {Nat.ForwardingResult.Success}.");
                if (Nat.ForwardingResult.Success)
                {
                    Log.Debug($"NAT public port: {Nat.ForwardingResult.PublicPort}.");
                }
            }
        }
    }
}