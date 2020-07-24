using System;
using System.Threading;
using System.Threading.Tasks;
using Open.Nat;
using Serilog;

namespace SyncTheater.Utils.NAT.PortForwarding
{
    internal sealed class OpenNat : Mapper
    {
        private static NatDevice _device;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        private readonly NatDiscoverer _discoverer = new NatDiscoverer();

        public OpenNat(int port, bool upnp, bool pmp) : base(port, upnp, pmp)
        {
        }

        public static async Task ClearAsync(int port)
        {
            try
            {
                await _device.DeletePortMapAsync(await _device.GetSpecificMappingAsync(Protocol.Tcp, port));
            }
            catch
            {
                // ignored
            }
        }

        public override async Task MapAsync()
        {
            if (Upnp)
            {
                Success = await CreateMapAsync(PortMapper.Upnp);
            }

            if (Pmp && !Success)
            {
                Success = await CreateMapAsync(PortMapper.Pmp);
            }
        }

        private async Task<bool> CreateMapAsync(PortMapper mapProtocol)
        {
            try
            {
                _device = await _discoverer.DiscoverDeviceAsync(mapProtocol, _cts);
            }
            catch (NatDeviceNotFoundException)
            {
                Log.Debug($"NAT device with {mapProtocol} wasn't found.");
                
                return false;
            }

            Log.Information($"Found {mapProtocol} NAT device with ip: {await _device.GetExternalIPAsync()}.");
            NatFound = true;

            try
            {
                await _device.CreatePortMapAsync(new Mapping(Protocol.Tcp,
                        PrivatePort,
                        PublicPort,
                        "Synctheater's server."
                    )
                );
            }
            catch (MappingException ex)
            {
                Log.Error($"NAT: {ex.ErrorText}.");
                return false;
            }

            return true;
        }
    }
}