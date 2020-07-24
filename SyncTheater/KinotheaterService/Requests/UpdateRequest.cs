using System;

namespace SyncTheater.KinotheaterService.Requests
{
    [Serializable]
    public class UpdateRequest
    {
        public Server Server { get; set; }
        public string Secret { get; set; }
    }
}