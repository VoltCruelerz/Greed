using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Greed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Debug.WriteLine("Main Window");
            InitializeComponent();
            Debug.WriteLine("Load Done");
        }

        private void Activate_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Activate_Click()");
        }

        private void Deactivate_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Deactivate_Click()");
        }
    }
}
