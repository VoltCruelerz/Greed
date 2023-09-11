using System;
using System.Text.Json;
using static System.Text.Json.JsonSerializer;

namespace Greed.Extensions
{
    public static class Extensions
    {
        public static string JsonMinify(this string json)
            => Serialize(Deserialize<JsonDocument>(json, new JsonSerializerOptions
            {
                AllowTrailingCommas = true
            }));

        public static string JsonFormat(this string json)
            => Serialize(Deserialize<JsonDocument>(json, new JsonSerializerOptions
            {
                AllowTrailingCommas = true
            }), new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                WriteIndented = true
            });

        public static bool IsOlderThan(this Version version, Version other)
        {
            return version.CompareTo(other) < 0;
        }

        public static bool IsNewerThan(this Version version, Version other)
        {
            return version.CompareTo(other) > 0;
        }
    }
}
