using Greed.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Greed.Controls
{
    public class SliderBox : GroupBox
    {
        public int GroupIndex;
        public int ScalarIndex;

        //<GroupBox Header = "Gravity Well Size" Height="53" VerticalAlignment="Top" BorderBrush="#FFC8D213">
        //    <Grid>
        //        <Label x:Name="LblGravityWellSize" Content="100%" HorizontalAlignment="Left" Margin="10,0,0,2" VerticalAlignment="Bottom" Width="60" RenderTransformOrigin="0.489,0.056"/>
        //        <Slider x:Name="SldGravityWellSize" Margin="75,0,10,0" TickPlacement="Both" IsSnapToTickEnabled="True" Minimum="0" Maximum="20" TickFrequency="1" Foreground="#FF9DA000"/>
        //    </Grid>
        //</GroupBox>
        Label LblPercentage;

        Slider SldSetter;

        public SliderBox(string name, int groupIndex, int scalarIndex)
        {
            GroupIndex = groupIndex;
            ScalarIndex = scalarIndex;

            Header = name;
            Height = 53;
            VerticalAlignment = System.Windows.VerticalAlignment.Top;
            BorderBrush = new SolidColorBrush(Color.FromRgb(255, 200, 210));
            Margin = new System.Windows.Thickness(0, 50 * scalarIndex, 0, 0);

            LblPercentage = new Label()
            {
                Content = "100%",
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
                Value = Settings.SliderOne
            };
            SldSetter.ValueChanged += Slider_ValueChanged;

            var grid = new Grid();
            grid.Children.Add(LblPercentage);
            grid.Children.Add(SldSetter);
            AddChild(grid);
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Settings.SetSlider(GroupIndex, ScalarIndex, LblPercentage, e.NewValue);
        }
    }
}
