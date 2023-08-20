using System.Text.Json;
using static System.Text.Json.JsonSerializer;

namespace Greed.Extensions
{
    public static class Extensions
    {
        public static string JsonMinify(this string json)
            => Serialize(Deserialize<JsonDocument>(json));
    }
}
