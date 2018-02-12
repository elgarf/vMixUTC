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
using vMixUTCNDIMonitorDataProvider.Extensions;

namespace vMixUTCNDIMonitorDataProvider
{
    /// <summary>
    /// Логика взаимодействия для OnWidgetUI.xaml
    /// </summary>
    public partial class OnWidgetUI : UserControl
    {
        public OnWidgetUI()
        {
            InitializeComponent();
        }
        public void UpdatePreview(BitmapSource src)
        {
            //Preview.Source = src;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //(DataContext as NDIMonitorDataProvider).UpdateFindList();
        }

        private void Preview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
            //var img = (sender as Image);
            //img.Height = SizeObserver.GetObservedWidth(img);
        }
    }


}
