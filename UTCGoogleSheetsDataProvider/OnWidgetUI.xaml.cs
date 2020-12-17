using System.Windows.Controls;
using vMixControllerSkin;

namespace UTCGoogleSheetsDataProvider
{
    /// <summary>
    /// Логика взаимодействия для OnWidgetUI.xaml
    /// </summary>
    public partial class OnWidgetUI : UserControl
    {
        public OnWidgetUI()
        {
            //InitializeComponent();
            this.LoadViewFromUri("/GoogleSheetsDataProvider;component/OnWidgetUI.xaml");
        }
    }
}
