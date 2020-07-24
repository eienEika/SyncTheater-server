using System;

namespace SyncTheater.KinotheaterService.Requests
{
    [Serializable]
    public sealed class RegisterResponse
    {
        public string Id { get; set; }
        public string SuperSecretCode { get; set; }
    }
}