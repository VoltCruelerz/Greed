using Greed.Extensions;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Greed.Controls
{
    public class Log : RichTextBox
    {
        public static readonly string LogPath = Directory.GetCurrentDirectory() + "\\log.log";
        public static readonly string LogPrevPath = Directory.GetCurrentDirectory() + "\\log_prev.log";

        private static Log? Instance;

        private static readonly SolidColorBrush DebugBack = new(Colors.LightGreen);
        private static readonly SolidColorBrush InfoBack = new(Colors.LightBlue);
        private static readonly SolidColorBrush WarnBack = new(Colors.Yellow);
        private static readonly SolidColorBrush ErrorBack = new(Colors.Red);
        private static readonly SolidColorBrush CriticalBack = new(Colors.DarkRed);

        private static readonly SolidColorBrush NormalText = new(Colors.Black);
        private static readonly SolidColorBrush CriticalText = new(Colors.White);

        private static int LogCount = 0;

        public Log()
        {
            Instance = this;

            // Shift the log history.
            if (File.Exists(LogPath))
            {
                if (File.Exists(LogPrevPath))
                {
                    File.Delete(LogPrevPath);
                }
                File.Move(LogPath, LogPrevPath);
            }
        }

        /// <summary>
        /// Print synchronously.
        /// </summary>
        /// <param name="str"></param>
        private static void Print(string str, LogLevel type)
        {
            var timePrefix = DateTime.Now.ToString("[yyyy/MM/dd | hh:mm:ss.fff] ");

            var background = type switch
            {
                LogLevel.DEBUG => DebugBack,
                LogLevel.INFO => InfoBack,
                LogLevel.WARN => WarnBack,
                LogLevel.ERROR => ErrorBack,
                LogLevel.CRITICAL => CriticalBack,
                _ => throw new NotImplementedException("Unrecognized log type: " + type)
            };

            var foreground = type == LogLevel.CRITICAL ? CriticalText : NormalText;

            var p = new Paragraph()
            {
                Margin = new System.Windows.Thickness(0)
            };

            // Add Colored Timestamp and Level
            p.Inlines.Add(new Run(timePrefix + type.GetDescription())
            {
                Foreground = foreground,
                Background = background
            });

            // Add Content
            p.Inlines.Add(new Run(" " + str));

            // Update the text field.
            if (LogCount++ == 0)
            {
                Instance!.Document = new FlowDocument(p);
            }
            else
            {
                Instance!.Document.Blocks.Add(p);
            }
            Instance!.ScrollToEnd();

            // IO last because it's slow and this is async.
            var line = timePrefix + type.GetDescription() + " " + str;
            System.Diagnostics.Debug.WriteLine(line);
            File.AppendAllText(LogPath, line + Environment.NewLine);
        }

        #region Sync Overload
        private static void Print(Exception ex, LogLevel type)
        {
            Print(ex.Message + Environment.NewLine + ex.StackTrace, type);
        }

        private static void Print(string str, Exception ex, LogLevel type)
        {
            Print(str + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, type);
        }
        #endregion

        #region Async Overload
        /// <summary>
        /// Invoke the print function when possible.
        /// </summary>
        /// <param name="str"></param>
        private static async Task PrintAsync(string str, LogLevel type)
        {
            Instance!.Dispatcher.Invoke(() => Print(str, type));
            await Task.Delay(0).ConfigureAwait(false);
        }

        /// <summary>
        /// Invoke the print function when possible.
        /// </summary>
        /// <param name="str"></param>
        private static async Task PrintAsync(Exception ex, LogLevel type)
        {
            Instance!.Dispatcher.Invoke(() => Print(ex, type));
            await Task.Delay(0).ConfigureAwait(false);
        }

        /// <summary>
        /// Invoke the print function when possible.
        /// </summary>
        /// <param name="str"></param>
        private static async Task PrintAsync(string str, Exception ex, LogLevel type)
        {
            Instance!.Dispatcher.Invoke(() => Print(str, ex, type));
            await Task.Delay(0).ConfigureAwait(false);
        }
        #endregion

        #region Public Access
        public static void Debug(string str)
        {
            Task.Run(() => PrintAsync(str, LogLevel.DEBUG).ConfigureAwait(false));
        }

        public static void Info(string str)
        {
            Task.Run(() => PrintAsync(str, LogLevel.INFO).ConfigureAwait(false));
        }

        public static void Warn(string str)
        {
            Task.Run(() => PrintAsync(str, LogLevel.WARN).ConfigureAwait(false));
        }

        public static void Warn(Exception ex)
        {
            Task.Run(() => PrintAsync(ex, LogLevel.WARN).ConfigureAwait(false));
        }

        public static void Error(string str)
        {
            Task.Run(() => PrintAsync(str, LogLevel.ERROR).ConfigureAwait(false));
        }

        public static void Error(Exception ex)
        {
            Task.Run(() => PrintAsync(ex, LogLevel.ERROR).ConfigureAwait(false));
        }

        public static void Error(string str, Exception ex)
        {
            Task.Run(() => PrintAsync(str, ex, LogLevel.ERROR).ConfigureAwait(false));
        }

        public static void Critical(string str, Exception ex)
        {
            Task.Run(() => PrintAsync(str, ex, LogLevel.CRITICAL).ConfigureAwait(false));
        }
        #endregion

        public static string GetLogs()
        {
            var sb = new StringBuilder();

            sb.AppendLine("========== CONFIG ==========");
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "Greed.dll.config");
            if (File.Exists(configPath))
            {
                sb.AppendLine(File.ReadAllText(configPath));
            }

            sb.AppendLine("========== PREV LOG ==========");
            if (File.Exists(LogPrevPath))
            {
                sb.AppendLine(File.ReadAllText(LogPrevPath));
            }

            sb.AppendLine("========== ACTIVE LOG ==========");
            if (File.Exists(LogPath))
            {
                sb.AppendLine(File.ReadAllText(LogPath));
            }

            return sb.ToString();
        }
    }

    public enum LogLevel
    {
        [Description("[DEBUG]")]
        DEBUG,
        [Description("[INFO]")]
        INFO,
        [Description("[WARN]")]
        WARN,
        [Description("[ERROR]")]
        ERROR,
        [Description("[CRITICAL]")]
        CRITICAL
    }
}
