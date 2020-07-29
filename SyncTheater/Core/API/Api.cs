using System;
using SyncTheater.Core.API.Apis;
using SyncTheater.Utils;

namespace SyncTheater.Core.API
{
    internal static class Api
    {
        public enum ErrorCommon
        {
            NoError = 0,
            UnknownMethod,
            UnknownApi,
        }

        public enum SendTo
        {
            All,
            Sender,
        }

        private static readonly IApiComponent Chat = new Chat();
        private static readonly IApiComponent Player = new Player();

        private static readonly object UnknownApiResponse = new
        {
            Error = ErrorCommon.UnknownApi,
        };

        public static object UnknownMethodResponse<T>(T method) where T : Enum =>
            new
            {
                Error = ErrorCommon.UnknownMethod,
                Method = method,
            };

        public static void ReadAndExecute(byte[] data)
        {
            var (code, jsonData) = Packet.Read(data);
            Execute((ApiCode) code, jsonData);
        }

        private static void Execute(ApiCode apiCode, string body)
        {
            var (data, sendTo) = apiCode switch
            {
                ApiCode.Chat => Chat.Request(body),
                ApiCode.Player => Player.Request(body),
                _ => new Tuple<object, SendTo>(UnknownApiResponse, SendTo.Sender),
            };

            Room.GetInstance.Send(Packet.Write((short) apiCode, data.ToJson()), sendTo);
        }

        private enum ApiCode : short
        {
            Chat = 0,
            Player,
        }
    }
}