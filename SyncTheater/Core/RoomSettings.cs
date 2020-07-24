namespace SyncTheater.Core
{
    internal sealed class RoomSettings
    {
        public RoomSettings(Profile profile)
        {
            ServerId = profile.ServerId;
            Secret = profile.Secret;
        }

        public string ServerId { get; }
        public string Secret { get; }
    }
}