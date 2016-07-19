using System.Windows;
using Licensing.KeyGenerator.ViewModel;

namespace Licensing.KeyGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) =>
            {
                ViewModelLocator.Cleanup();
                Properties.Settings.Default.Save();
            };
        }
    }
}