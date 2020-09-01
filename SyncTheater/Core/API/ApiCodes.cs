namespace SyncTheater.Core.API
{
    internal static class Methods
    {
        public static class Authentication
        {
            public const string AuthAnon = "AnonymousAuth";
            public const string Disconnect = "Disconnect";
            public const string Register = "Register";
            public const string AuthLogin = "LoginedAuth";
        }

        public static class Chat
        {
            public const string NewMessage = "NewMessage";
        }

        public static class Player
        {
            public const string SetVideo = "SetVideo";
            public const string PauseCycle = "PauseCycle";
        }
    }

    public static class Notifications
    {
        public const string State = "State";
        public const string VideoUrl = "VideoUrl";
        public const string VideoPause = "VideoPause";
        public const string UserConnected = "UserConnected";
        public const string UserDisconnected = "UserDisconnected";
        public const string NewChatMessage = "NewChatMessage";
    }

    internal enum ApiCode : short
    {
        InError = -123,
        Notification = 0,
        Chat,
        Player,
        Authentication,
    }

    internal enum ApiError
    {
        NoError = 0,
        UnknownError,
        UnknownMethod,
        UnknownApi,
        AuthenticationRequired,
        LoginRequired,
        EmptyText = 100,
        LoginOccupied,
        InvalidAuthKey,
        AlreadyAuthenticated,
        AlreadyRegistered,
        BadLogin,
    }
}