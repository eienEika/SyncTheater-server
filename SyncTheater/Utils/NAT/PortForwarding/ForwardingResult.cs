namespace SyncTheater.Utils.NAT.PortForwarding
{
    internal sealed class ForwardingResult
    {
        public ForwardingResult(bool nat, bool success, int port)
        {
            NatFound = nat;
            Success = success;
            PublicPort = port;
        }

        public bool NatFound { get; }
        public bool Success { get; }
        public int PublicPort { get; }
    }
}