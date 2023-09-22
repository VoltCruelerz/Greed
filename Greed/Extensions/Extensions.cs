using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
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

        /// <summary>
        /// Adds one or more size-4 tabs to the start of a string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tabs"></param>
        /// <returns></returns>
        public static string TabLeft(this string text, int tabs)
        {
            return text.PadLeft(text.Length + tabs * 4);
        }

        /// <summary>
        /// JS-like stringiification.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static string Stringify(this IEnumerable sequence, int depth = 0)
        {
            if (sequence == null)
            {
                return string.Empty;
            }

            var lines = new List<string>();
            lines.Add(TabLeft("{", depth));
            foreach (var item in sequence)
            {
                var enumerable = item as IEnumerable;
                var entry = (enumerable != null) && (item is not string)
                    ? enumerable.Stringify(depth + 1)
                    : item?.ToString() ?? "null";
                lines.Add(TabLeft(entry, depth + 1));
            }
            lines.Add(TabLeft("}", depth));
            return string.Join("," + Environment.NewLine, lines);
        }

        /// <summary>
        /// Converts the enumerable to a bulleted list.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static string ToBulletedList(this IEnumerable sequence, int depth = 0)
        {
            if (sequence == null)
            {
                return string.Empty;
            }

            var lines = new List<string>();
            foreach (var item in sequence)
            {
                var enumerable = item as IEnumerable;
                var entry = (enumerable != null) && (item is not string)
                    ? enumerable.Stringify(depth + 1)
                    : item?.ToString() ?? "null";
                var content = $"{Constants.UNI_BULLET} {entry}";
                lines.Add(TabLeft(content, depth + 1));
            }
            return string.Join(Environment.NewLine, lines);
        }

        public static string GetDescription<T>(this T enumerationValue) where T : struct
        {
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", nameof(enumerationValue));
            }

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(enumerationValue!.ToString()!);
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the description value
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            //If we have no description attribute, just return the ToString of the enum
            return enumerationValue.ToString()!;
        }
    }
}
