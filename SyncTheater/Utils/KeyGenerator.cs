using System;
using System.Security.Cryptography;
using System.Text;

namespace SyncTheater.Utils
{
    internal static class KeyGenerator
    {
        private static readonly char[] Chars = "qwertyuiopasdfghjklzxcvbnm1234567890".ToCharArray();

        public static string GetKey(int size = 128)
        {
            var data = new byte[4 * size];
            using (var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }

            var result = new StringBuilder(size);
            for (var i = 0; i < size; ++i)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % Chars.Length;

                result.Append(Chars[idx]);
            }

            return result.ToString();
        }
    }
}