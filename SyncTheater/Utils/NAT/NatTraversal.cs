using System.Threading.Tasks;
using Serilog;
using SyncTheater.Utils.NAT.PortForwarding;

namespace SyncTheater.Utils.NAT
{
    internal static class NatTraversal
    {
        public static async Task<ForwardingResult> ForwardPortAsync(bool upnp, bool pmp, int port)
        {
            Log.Debug($"Permit UPnP: {upnp}. Permit PMP: {pmp}.");
            Log.Information($"Trying to forward port {port}, it's may take a while...");

            if (!upnp && !pmp)
            {
                Log.Information("Both UPnP and PMP are disabled, canceling port forwarding.");
                return null;
            }

            Mapper mapper = new OpenNat(port, upnp, pmp);
            await mapper.MapAsync();
            if (!mapper.Success)
            {
                mapper = new MonoNat(port, upnp, pmp);
                await mapper.MapAsync();
            }

            if (!mapper.NatFound)
            {
                Log.Information("NAT device was not found.");
            }
            else
            {
                if (mapper.Success)
                {
                    Log.Information(
                        $"Port forwarding completed successfully. Public port {mapper.PublicPort} forwarded to private {port}."
                    );
                }
                else
                {
                    Log.Error("Port forwarding failed.");
                }
            }

            return new ForwardingResult(mapper.NatFound, mapper.Success, mapper.PublicPort);
        }

        public static async Task ClearAsync(int port)
        {
            if (port != 0)
            {
                Log.Verbose("Clearing port forwarding...");
                await OpenNat.ClearAsync(port);
                MonoNat.Clear(port);
                Log.Verbose("Port forwarding cleared.");
            }
        }
    }
}