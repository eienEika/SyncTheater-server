using SyncTheater.Utils.NAT.PortForwarding;

namespace SyncTheater.Utils.NAT
{
    internal sealed class NatResult
    {
        public NatResult(bool traverse, ForwardingResult forwardingResult)
        {
            Traverse = traverse;
            ForwardingResult = forwardingResult;
        }

        public bool Traverse { get; }
        public bool NatFound => ForwardingResult.NatFound;
        public ForwardingResult ForwardingResult { get; }
    }
}