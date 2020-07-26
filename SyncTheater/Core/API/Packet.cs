using System;
using System.Text;
using Serilog;

namespace SyncTheater.Core.API
{
    internal static class Packet
    {
        private const int ApiCodeSize = 2;
        private const int DataLengthSize = 2;

        public static Tuple<short, string> Read(byte[] data)
        {
            Log.Verbose("Reading new packet...");

            var apiByte = data[..ApiCodeSize];
            var sizeByte = data[ApiCodeSize..(ApiCodeSize + DataLengthSize)];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(apiByte);
                Array.Reverse(sizeByte);
            }

            short api;
            string jsonData;
            try
            {
                api = BitConverter.ToInt16(apiByte);
                var size = BitConverter.ToInt16(sizeByte);
                jsonData = Encoding.UTF8.GetString(data, ApiCodeSize + DataLengthSize, size);
            }
            catch (ArgumentOutOfRangeException)
            {
                Log.Debug("Cannot read data.");

                return new Tuple<short, string>(-1, "");
            }

            return new Tuple<short, string>(api, jsonData);
        }

        public static byte[] Write(short apiCode, string body)
        {
            Log.Verbose($"Writing {body} to packet...");

            var api = BitConverter.GetBytes(apiCode);
            var data = Encoding.UTF8.GetBytes(body);
            var size = BitConverter.GetBytes((short) data.Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(api);
                Array.Reverse(size);
            }

            var packet = new byte[ApiCodeSize + DataLengthSize + data.Length];

            Buffer.BlockCopy(api, 0, packet, 0, ApiCodeSize);
            Buffer.BlockCopy(size, 0, packet, ApiCodeSize, DataLengthSize);
            Buffer.BlockCopy(data, 0, packet, ApiCodeSize + DataLengthSize, data.Length);

            return packet;
        }
    }
}