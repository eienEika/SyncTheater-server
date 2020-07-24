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

        private static readonly string UnknownApiResponse = new
        {
            Error = ErrorCommon.UnknownApi,
        }.ToJson();

        public static string UnknownMethodResponse<T>(T method) where T : Enum =>
            new
            {
                Error = ErrorCommon.UnknownMethod,
                Method = method,
            }.ToJson();

        public static void ReadAndExecute(byte[] data)
        {
            var (code, jsonData) = Packet.Read(data);
            Execute((ApiCode) code, jsonData);
        }

        private static void Execute(ApiCode apiCode, string body)
        {
            Send(apiCode,
                apiCode switch
                {
                    ApiCode.Chat => Chat.Request(body),
                    ApiCode.Player => Room.GetInstance.Player.Request(body),
                    _ => UnknownApiResponse,
                }
            );
        }

        private static void Send(ApiCode apiCode, string body)
        {
            Room.GetInstance.SendTo(Packet.Write((short) apiCode, body));
        }

        private enum ApiCode : short
        {
            Chat = 0,
            Player,
        }
    }
}