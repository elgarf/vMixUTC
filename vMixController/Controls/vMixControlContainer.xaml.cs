using GalaSoft.MvvmLight.CommandWpf;
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
using System.Globalization;

namespace vMixController.Controls
{
    /// <summary>
    /// Логика взаимодействия для vMixControlContainer.xaml
    /// </summary>
    public partial class vMixControlContainer : UserControl
    {
        public vMixControlContainer()
        {
            InitializeComponent();
        }

        public vMixController.Widgets.vMixControl Control
        {
            get { return (vMixController.Widgets.vMixControl)GetValue(ControlProperty); }
            set { SetValue(ControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Control.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ControlProperty =
            DependencyProperty.Register("Control", typeof(vMixController.Widgets.vMixControl), typeof(vMixControlContainer), new PropertyMetadata(null));

        public RelayCommand<Widgets.vMixControl> CloseCommand
        {
            get { return (RelayCommand<Widgets.vMixControl>)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CloseCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(RelayCommand<Widgets.vMixControl>), typeof(vMixControlContainer), new PropertyMetadata(null));



        public RelayCommand<Widgets.vMixControl> SettingsCommand
        {
            get { return (RelayCommand<Widgets.vMixControl>)GetValue(SettingsCommandProperty); }
            set { SetValue(SettingsCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SettingsCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingsCommandProperty =
            DependencyProperty.Register("SettingsCommand", typeof(RelayCommand<Widgets.vMixControl>), typeof(vMixControlContainer), new PropertyMetadata(null));




        public RelayCommand<Widgets.vMixControl> CopyCommand
        {
            get { return (RelayCommand<Widgets.vMixControl>)GetValue(CopyCommandProperty); }
            set { SetValue(CopyCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CopyCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CopyCommandProperty =
            DependencyProperty.Register("CopyCommand", typeof(RelayCommand<Widgets.vMixControl>), typeof(vMixControlContainer), new PropertyMetadata(null));



        public object MainContent
        {
            get { return (object)GetValue(MainContentProperty); }
            set { SetValue(MainContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MainContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.Register("MainContent", typeof(object), typeof(vMixControlContainer), new PropertyMetadata(null));



        public object CaptionContent
        {
            get { return (object)GetValue(CaptionContentProperty); }
            set { SetValue(CaptionContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CaptionContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionContentProperty =
            DependencyProperty.Register("CaptionContent", typeof(object), typeof(vMixControlContainer), new PropertyMetadata(null));

        private void CC_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!double.IsNaN(ContentControl.ActualHeight) && ContentControl.ActualHeight != 0)
                Control.Height = ContentControl.ActualHeight;
            if (!double.IsNaN(Caption.ActualHeight) && Caption.ActualHeight != 0)
                Control.CaptionHeight = Caption.ActualHeight;
        }
    }

    public class LockButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? (char)0xE1F6 : (char)0xE1F7;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LockToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
