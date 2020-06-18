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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WeatherExternalDataProvider
{
    /// <summary>
    /// Логика взаимодействия для OnWidgetUI.xaml
    /// </summary>
    public partial class OnWidgetUI : UserControl
    {

        public WeatherProvider Provider
        {
            get { return (WeatherProvider)GetValue(ProviderProperty); }
            set { SetValue(ProviderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Provider.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register("Provider", typeof(WeatherProvider), typeof(OnWidgetUI), new PropertyMetadata(null));

        public OnWidgetUI()
        {
            InitializeComponent();
        }
    }
}
