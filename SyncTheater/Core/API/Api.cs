using System;
using System.Collections.Generic;
using SyncTheater.Core.API.Apis;
using SyncTheater.Core.API.Types;
using SyncTheater.Utils;

namespace SyncTheater.Core.API
{
    internal static class Api
    {
        private static readonly object UnknownApiResponse = new ApiRequestResponse
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

            var response = apiCode switch
            {
                ApiCode.Authentication => Authentication.Request(jsonData, user),
                ApiCode.Chat => Chat.Request(jsonData, user),
                ApiCode.Player => Player.Request(jsonData, user),
                _ => UnknownApiResponse,
            };

            Send(apiCode, response, sender);
        }

        public static void SendNotification(ServerNotification notification)
        {
            Send(ApiCode.Notification, notification, Room.GetState.UserSessions);
        }

        private static void Send(ApiCode code, object data, Guid sendTo)
        {
            Room.GetInstance.Send(Packet.Write(code, data.ToJson()), sendTo);
        }

        private static void Send(ApiCode code, object data, IEnumerable<Guid> sendTo)
        {
            foreach (var sessionId in sendTo)
            {
                Room.GetInstance.Send(Packet.Write(code, data.ToJson()), sessionId);
            }
        }
    }
}