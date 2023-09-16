using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Linq;
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

        public static void SetPopDuration(this Popup popup, int durationMs)
        {
            popup.IsOpen = true;

            // Close the popup after a delay.
            var timer = new DispatcherTimer();
            timer.Tick += (s, args) =>
            {
                popup.IsOpen = false;
                timer.Stop();
            };
            timer.Interval = TimeSpan.FromMilliseconds(durationMs);
            timer.Start();
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> sequence, Func<T, Task> action)
        {
            return Task.WhenAll(sequence.Select(action));
        }

        public static bool EqualsAll<T>(this T subject, params T[] values)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            return values.All(v => subject.Equals(v));
        }

        public static bool EqualsAny<T>(this T subject, params T[] values)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            return values.Any(v => subject.Equals(v));
        }
    }
}
