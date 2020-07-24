using System.Net;
using System.Net.Sockets;
using Serilog;

namespace SyncTheater.KinotheaterService
{
    internal static class ServiceUtils
    {
        public const string Domain = "synctheater.space";

        static ServiceUtils()
        {
            IPAddress[] ips;
            try
            {
                ips = Dns.GetHostAddresses(Domain);
            }
            catch (SocketException)
            {
                Log.Fatal("Fail to resolve service domain.");

                Ipv4 = IPAddress.None;
                Ipv6 = IPAddress.IPv6None;

                return;
            }

            foreach (var ip in ips)
            {
                Log.Verbose($"Found {ip} in service domain info.");

                switch (ip.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        Ipv4 = ip;
                        break;

                    case AddressFamily.InterNetworkV6:
                        Ipv6 = ip;
                        break;

                    default:
                        continue;
                }
            }
        }

        public static IPAddress Ipv4 { get; }
        public static IPAddress Ipv6 { get; }
    }
}