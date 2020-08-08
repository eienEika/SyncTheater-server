using System;
using System.Collections.Generic;
using SyncTheater.Core.API.Apis;
using SyncTheater.Core.API.Types;
using SyncTheater.Utils;

namespace SyncTheater.Core.API
{
    internal static class Api
    {
        public enum SendTo
        {
            None,
            All,
            Sender,
        }

        private static readonly object UnknownApiResponse = new OutcomeData
        {
            Data = null,
            Error = ApiError.UnknownApi,
            Method = null,
        };

        private static readonly ApiComponentBase Authentication = new Authentication();
        private static readonly ApiComponentBase Chat = new Chat();
        private static readonly ApiComponentBase Player = new Player();

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
}