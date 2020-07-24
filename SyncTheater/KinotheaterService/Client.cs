using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using NetCoreServer;
using Serilog;

namespace SyncTheater.KinotheaterService
{
    internal sealed class Client : SslClient
    {
        private bool _connectedPrev;
        private bool _forceDisconnect;

        public Client(IPEndPoint endPoint) : base(
            new SslContext(SslProtocols.Tls12, new X509Certificate2(), CertificateValidationCallback),
            endPoint
        )
        {
            OptionKeepAlive = true;
        }

        public event EventHandler ConnectedEvent;

        public void ForceDisconnect()
        {
            _forceDisconnect = true;

            Send("stop");
            Disconnect();
        }

        protected override void OnHandshaked()
        {
            Log.Information("Connected to service.");

            _connectedPrev = true;
            ConnectedEvent?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnDisconnected()
        {
            if (_forceDisconnect)
            {
                return;
            }

            if (_connectedPrev)
            {
                Log.Error("Disconnected from service. Trying reconnect...");

                _connectedPrev = false;
            }
            else
            {
                Log.Verbose("Failed to connect to service.");
            }

            Thread.Sleep(TimeSpan.FromSeconds(10));

            ConnectAsync();
        }

        protected override void OnError(SocketError error)
        {
            Log.Fatal($"Service client: {error}");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            var message = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
            Log.Information($"Message from service: {message}");
        }

        private static bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslpolicyerrors) =>
            true;
    }
}