using System.Windows.Controls;
using vMixControllerSkin;

namespace UTCExcelDataProvider
{
    /// <summary>
    /// Логика взаимодействия для OnWidgetUI.xaml
    /// </summary>
    public partial class OnWidgetUI : UserControl
    {
        public OnWidgetUI()
        {
            //InitializeComponent();
            this.LoadViewFromUri("/ExcelDataProvider;component/OnWidgetUI.xaml");
        }
    }
}
