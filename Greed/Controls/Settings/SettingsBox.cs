using System.Windows.Controls;
using System.Windows.Media;

namespace Greed.Controls.Settings
{
    public class SettingsBox : GroupBox
    {
        public const int BoxSpacing = 50;
        public int GroupIndex;
        public int ArrayIndex;
        public int PositionIndex;
        protected readonly Grid GrdContent;

        public SettingsBox(string name, int groupIndex, int subIndex, int positionIndex)
        {
            GroupIndex = groupIndex;
            ArrayIndex = subIndex;
            PositionIndex = positionIndex;

            Header = name;
            Height = 53;
            VerticalAlignment = System.Windows.VerticalAlignment.Top;
            BorderBrush = new SolidColorBrush(Color.FromRgb(255, 200, 210));
            Margin = new System.Windows.Thickness(0, positionIndex * BoxSpacing, 0, 0);

            GrdContent = new Grid();
            AddChild(GrdContent);
        }
    }
}
