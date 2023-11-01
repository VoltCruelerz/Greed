using System.Windows.Controls;
using System.Windows.Media;

namespace Greed.Controls.Settings
{
    public class SliderBox : SettingsBox
    {
        private readonly Label LblPercentage;
        private readonly Slider SldSetter;

        public SliderBox(string name, int groupIndex, int subIndex, int posIndex, int tickIndex) : base(name, groupIndex, subIndex, posIndex)
        {
            LblPercentage = new Label()
            {
                Content = Greed.Utils.Settings.SliderValue[tickIndex].ToString("P0"),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                Width = 60,
                Margin = new System.Windows.Thickness(10, 0, 0, 2)
            };

            SldSetter = new Slider()
            {
                Margin = new System.Windows.Thickness(75, 0, 10, 0),
                IsSnapToTickEnabled = true,
                TickPlacement = System.Windows.Controls.Primitives.TickPlacement.Both,
                Minimum = 0,
                Maximum = 20,
                TickFrequency = 1,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 157, 160)),
                Value = tickIndex
            };
            SldSetter.ValueChanged += Slider_ValueChanged;

            GrdContent.Children.Add(LblPercentage);
            GrdContent.Children.Add(SldSetter);
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Greed.Utils.Settings.SetSlider(GroupIndex, ArrayIndex, LblPercentage, e.NewValue);
        }
    }
}
