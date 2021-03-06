using System;
using System.Collections.Generic;
using SyncTheater.Core.API.Apis;
using SyncTheater.Core.API.Types;
using SyncTheater.Utils;

namespace SyncTheater.Core.API
{
    internal static class Api
    {
        private static readonly ApiResult UnknownApiResponse = new ApiResult(
            new ApiRequestResponse
            {
                Data = null,
                Error = ApiError.UnknownApi,
                Method = null,
            }
        );

        private static readonly ApiComponentBase Authentication = new Authentication();
        private static readonly ApiComponentBase Chat = new Chat();
        private static readonly ApiComponentBase Player = new Player();

        public static void ReadAndExecute(byte[] data, Guid sender)
        {
            var (apiCode, jsonData) = Packet.Read(data);

            var user = Room.GetState.User(sender);

            var result = apiCode switch
            {
                ApiCode.Authentication => Authentication.Request(jsonData, user),
                ApiCode.Chat => Chat.Request(jsonData, user),
                ApiCode.Player => Player.Request(jsonData, user),
                _ => UnknownApiResponse,
            };

            Send(apiCode, result.Response, sender);

            if (result.Response.Error != ApiError.NoError)
            {
                return;
            }

            foreach (var trigger in result.Triggers)
            {
                trigger.Execute();
            }
        }

        public static void SendNotification(string type, object data)
        {
            var notification = new ServerNotification
            {
                Data = data,
                Type = type,
            };
            Send(ApiCode.Notification, notification, Room.GetState.UserSessions);
        }

        public static void SendNotification(string type, object data, Guid sendTo)
        {
            var notification = new ServerNotification
            {
                Data = data,
                Type = type,
            };
            Send(ApiCode.Notification, notification, sendTo);
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