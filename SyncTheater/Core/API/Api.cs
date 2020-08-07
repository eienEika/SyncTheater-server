using System;
using System.Collections.Generic;
using SyncTheater.Core.API.Apis;
using SyncTheater.Core.API.Types;
using SyncTheater.Utils;

namespace SyncTheater.Core.API
{
    internal static class Api
    {
        private static readonly object UnknownApiResponse = new OutcomeData<Enum>
        {
            Data = null,
            Error = ApiError.UnknownApi,
            Method = null,
        };

        private static readonly IApiComponent Authentication = new Authentication();
        private static readonly IApiComponent Chat = new Chat();
        private static readonly IApiComponent Player = new Player();

        public static void ReadAndExecute(byte[] data, Guid sender)
        {
            var (apiCode, jsonData) = Packet.Read(data);

            var user = Room.GetState.User(sender);

            var (response, sendTo) = (ApiCode) apiCode switch
            {
                ApiCode.Authentication => Authentication.Request(jsonData, user, sender),
                ApiCode.Chat => Chat.Request(jsonData, user, sender),
                ApiCode.Player => Player.Request(jsonData, user, sender),
                _ => new Tuple<object, SendTo>(UnknownApiResponse, SendTo.Sender),
            };

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            var sendTos = sendTo switch
            {
                SendTo.All => Room.GetState.UserSessions,
                SendTo.Sender => new[]
                {
                    sender,
                },
                _ => new Guid[0],
            };

            Send((ApiCode) apiCode, response, sendTos);
        }

        public static void Send(ApiCode code, object data, IEnumerable<Guid> sendTo)
        {
            Room.GetInstance.Send(Packet.Write((short) code, data.ToJson()), sendTo);
        }
    }

    internal enum ApiCode : short
    {
        State,
        Chat,
        Player,
        Authentication,
    }

    internal enum SendTo
    {
        None,
        All,
        Sender,
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