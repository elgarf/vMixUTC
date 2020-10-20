using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace vMixController
{
    /// <summary>
    /// Логика взаимодействия для KeyLearnWindow.xaml
    /// </summary>
    public partial class KeyLearnWindow : Window
    {
        public Key PressedKey { get; set; }

        public KeyLearnWindow()
        {
            InitializeComponent();
            Topmost = Application.Current?.MainWindow?.Topmost ?? false;
            Activate();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            PressedKey = e.Key;
            DialogResult = true;
        }
    }
}
