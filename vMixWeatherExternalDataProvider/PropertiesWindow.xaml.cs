using System.Windows;

namespace WeatherExternalDataProvider
{
    /// <summary>
    /// Description for PropertiesWindow.
    /// </summary>
    public partial class PropertiesWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the PropertiesWindow class.
        /// </summary>
        public PropertiesWindow()
        {
            InitializeComponent();
        }



        public WeatherProvider Provider
        {
            get { return (WeatherProvider)GetValue(ProviderProperty); }
            set { SetValue(ProviderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Provider.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register("Provider", typeof(WeatherProvider), typeof(PropertiesWindow), new PropertyMetadata(null));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}