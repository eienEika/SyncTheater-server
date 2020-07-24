using System;

namespace SyncTheater.KinotheaterService.Requests
{
    [Serializable]
    public sealed class Server
    {
        public string Id { get; set; }
        public int PrivatePort { get; set; }
        public int PublicPort { get; set; }
        public string Ipv4 { get; set; }
        public string Ipv6 { get; set; }
    }
}