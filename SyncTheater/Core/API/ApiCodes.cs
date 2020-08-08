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

    internal enum ApiCode : short
    {
        State,
        Chat,
        Player,
        Authentication,
    }

    internal enum ApiError
    {
        NoError = 0,
        UnknownMethod,
        UnknownApi,
        AuthenticationRequired,
        EmptyText = 100,
        LoginOccupied,
        InvalidAuthKey,
    }

    internal enum StateUpdateCode
    {
        VideoUrl,
        Pause,
    }
}