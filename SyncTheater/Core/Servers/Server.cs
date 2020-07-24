using System.Net;
using System.Net.Sockets;
using NetCoreServer;
using Serilog;

namespace SyncTheater.Core.Servers
{
    internal sealed class Server : TcpServer
    {
        public Server(IPAddress address, int port) : base(address, port)
        {
            OptionKeepAlive = true;
            OptionReuseAddress = true;
            if (Endpoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                OptionDualMode = true;
            }
        }

        protected override TcpSession CreateSession() => new Session(this);

        protected override void OnError(SocketError error)
        {
            Log.Fatal($"Core server: {error}.");
        }
    }
}