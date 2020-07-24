using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mono.Nat;
using Serilog;

namespace SyncTheater.Utils.NAT.PortForwarding
{
    internal sealed class MonoNat : Mapper
    {
        private static readonly List<INatDevice> Devices = new List<INatDevice>(2);

        public MonoNat(int port, bool upnp, bool pmp) : base(port, upnp, pmp)
        {
        }

        public static void Clear(int port)
        {
            foreach (var device in Devices)
            {
                try
                {
                    device.DeletePortMap(device.GetSpecificMapping(Protocol.Tcp, port));
                }
                catch (MappingException)
                {
                }
            }
        }

        public override async Task MapAsync()
        {
            NatUtility.DeviceFound += DeviceFound;

            try
            {
                await Task.Run(() => NatUtility.StartDiscovery(),
                    new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token
                );
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                NatUtility.StopDiscovery();
            }
        }

        private void DeviceFound(object sender, DeviceEventArgs args)
        {
            NatFound = true;

            var device = args.Device;
            
            Devices.Add(device);
            
            if (device.NatProtocol == NatProtocol.Upnp && !Upnp
                || device.NatProtocol == NatProtocol.Pmp && !Pmp)
            {
                return;
            }

            Log.Debug($"Found {device.NatProtocol} NAT device with ip {device.GetExternalIP()}.");

            try
            {
                device.CreatePortMap(new Mapping(Protocol.Tcp, PrivatePort, PublicPort, 0, "Synctheater's server"));
            }
            catch (MappingException ex)
            {
                Log.Error($"NAT: {ex.ErrorText}.");
                
                return;
            }

            Success = true;
        }
    }
}