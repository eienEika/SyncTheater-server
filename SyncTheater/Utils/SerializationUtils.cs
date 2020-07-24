using System.Text.Encodings.Web;
using System.Text.Json;

namespace SyncTheater.Utils
{
    internal static class SerializationUtils
    {
        private static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public static string ToJson(this object obj) => JsonSerializer.Serialize(obj, JsonOptions);

        public static T Deserialize<T>(string str) => JsonSerializer.Deserialize<T>(str, JsonOptions);
    }
}