using System.Windows.Controls;
using System.Windows.Media;

namespace Greed.Controls.Settings
{
    public class CheckBoxBox : SettingsBox
    {
        private readonly CheckBox ChkSetter;

        public CheckBoxBox(string name, int groupIndex, int subIndex, int posIndex, bool isChecked) : base(name, groupIndex, subIndex, posIndex)
        {
            ChkSetter = new CheckBox()
            {
                IsChecked = isChecked,
                Margin = new System.Windows.Thickness(10, 6, 0, 0),
                Foreground = new SolidColorBrush(Color.FromRgb(255, 157, 160)),
            };
            ChkSetter.Checked += ChkSetter_Checked;
            ChkSetter.Unchecked += ChkSetter_Unchecked;

            GrdContent.Children.Add(ChkSetter);
        }

        private void ChkSetter_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Greed.Utils.Settings.SetBool(GroupIndex, ArrayIndex, true);
        }

        private void ChkSetter_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            Greed.Utils.Settings.SetBool(GroupIndex, ArrayIndex, false);
        }
    }
}
