using System.Net.Sockets;
using NetCoreServer;
using Serilog;
using SyncTheater.Core.API;

namespace SyncTheater.Core.Servers
{
    internal sealed class Session : TcpSession
    {
        public Session(TcpServer server) : base(server)
        {
        }

        protected override void OnConnected()
        {
            // Console.WriteLine(Id);
            // Console.WriteLine(Server.BytesReceived);
        }

        protected override void OnDisconnected()
        {
            Room.GetState.UserDisconnect(Id);
        }

        protected override void OnError(SocketError error)
        {
            Log.Fatal($"Core session: {error}.");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            Log.Verbose($"Received data from {Socket.RemoteEndPoint}.");

            Api.ReadAndExecute(buffer, Id);
        }

        protected override void OnSent(long sent, long pending)
        {
            Log.Verbose($"Sending data to {Socket.RemoteEndPoint}...");
        }
    }
}