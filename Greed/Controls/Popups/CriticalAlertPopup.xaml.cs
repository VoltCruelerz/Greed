using Greed.Extensions;
using System;
using System.Diagnostics;
using System.Media;
using System.Windows;

namespace Greed.Controls.Popups
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CriticalAlertPopup : Window
    {
        public CriticalAlertPopup(string title, string body, bool blameJson)
        {
            Debug.WriteLine($"CriticalAlertPopup: {title}");
            InitializeComponent();
            Log.Error(title + Environment.NewLine + body);
            Title = title;
            TxtCriticalError.Text = body;
            SystemSounds.Beep.Play();
            Topmost = true;

            if (blameJson)
            {
                CmdIgnoreError.Content = "I'll go check my JSON.";
                CmdReportError.Content = "I checked. It's not on my end.";
                Title = "It appears there's a JSON error in your code. Please double check it before submitting.";
            }
        }

        private void CmdIgnoreError_Click(object sender, RoutedEventArgs e)
        {
            Log.Info($"CmdIgnoreError_Click()");
            Close();
        }

        private void CmdReportError_Click(object sender, RoutedEventArgs e)
        {
            Log.Info($"CmdReportError_Click()");
            var navigable = $"https://github.com/VoltCruelerz/Greed/issues/new?title={Title}&body={TxtCriticalError.Text.UrlEncode()}&labels[]=bug";
            navigable.NavigateToUrl();
        }

        private static string ParseException(Exception ex)
        {
            var str = "";
            Exception? tempEx = ex;
            do
            {
                str += tempEx.GetType() + ": " + tempEx.Message + Environment.NewLine + tempEx.StackTrace;
                if (tempEx.InnerException != null)
                {
                    str += Environment.NewLine + "=====================" + Environment.NewLine + Environment.NewLine;
                }
                tempEx = tempEx.InnerException;
            } while (tempEx != null);
            return str;
        }

        public static void Throw(string title, Exception ex)
        {
            try
            {
                var str = ParseException(ex);
                new CriticalAlertPopup(title, str, str.Contains("Newtonsoft.Json")).Show();
            }
            catch (Exception oops)
            {
                Log.Critical("Incorrectly called CriticalAlertPopup.Throw()", oops);
            }
        }

        public static void ThrowAsync(string title, Exception ex)
        {
            MainWindow.Instance!.Dispatcher.Invoke(() => Throw(title, ex));
        }
    }
}
