using System;
using System.Collections.Generic;
using SyncTheater.Core.API.Apis;
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

        public static IApiComponent Authentication { get; } = new Authentication();
        public static IApiComponent Chat { get; } = new Chat();
        public static IApiComponent Player { get; } = new Player();

        public static void ReadAndExecute(byte[] data, Guid sender)
        {
            var (code, jsonData) = Packet.Read(data);
            Execute((ApiCode) code, jsonData, sender);
        }

        private static void Execute(ApiCode apiCode, string body, Guid sender)
        {
            var (data, sendTo) = apiCode switch
            {
                ApiCode.Authentication => Authentication.RemoteRequest(body, sender),
                ApiCode.Chat => Chat.RemoteRequest(body, sender),
                ApiCode.Player => Player.RemoteRequest(body, sender),
                _ => new Tuple<object, SendTo>(UnknownApiResponse, SendTo.Sender),
            };

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            var sendTos = sendTo switch
            {
                SendTo.All => Authentication.LocalRequest(Apis.Authentication.Method.GetUsers).Item2 as
                    IEnumerable<Guid>,
                SendTo.Sender => new[]
                {
                    sender,
                },
                _ => new Guid[0],
            };

            Room.GetInstance.Send(Packet.Write((short) apiCode, data.ToJson()), sendTos);
        }

        private enum ApiCode : short
        {
            Chat = 0,
            Player,
            Authentication,
        }
    }

    internal enum SendTo
    {
        None,
        All,
        Sender,
    }
}