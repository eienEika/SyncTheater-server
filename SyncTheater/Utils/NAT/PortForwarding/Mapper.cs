using System.Threading.Tasks;

namespace SyncTheater.Utils.NAT.PortForwarding
{
    internal abstract class Mapper
    {
        protected readonly bool Pmp;
        protected readonly int PrivatePort;
        protected readonly bool Upnp;

        protected Mapper(int privatePort, bool upnp, bool pmp)
        {
            PrivatePort = privatePort;
            PublicPort = privatePort;
            Upnp = upnp;
            Pmp = pmp;
        }

        public bool NatFound { get; protected set; }
        public int PublicPort { get; protected set; }
        public bool Success { get; protected set; }

        public abstract Task MapAsync();
    }
}