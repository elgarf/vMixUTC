using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace vMixController.Controls
{
    public class vMixControlContainerDummy: UserControl
    {
        protected override void OnInitialized(EventArgs e)
        {
            
            base.OnInitialized(e);
            var CC = new vMixControlContainer();
            CC.ParentContainer = this;
            Content = CC;
            PreviewMouseDown += VMixControlContainerDummy_PreviewMouseDown;
            //CC.OnSizeChanged = CC_SizeChanged;
            this.SizeChanged += CC_SizeChanged;
        }

        private void VMixControlContainerDummy_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Control.Selected = !Control.Selected && !Control.Locked;
                e.Handled = true;
            }
        }



        public Color BorderColorProxy
        {
            get { return (Color)GetValue(BorderColorProxyProperty); }
            set { SetValue(BorderColorProxyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderColorProxy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderColorProxyProperty =
            DependencyProperty.Register("BorderColorProxy", typeof(Color), typeof(vMixControlContainerDummy), new PropertyMetadata(null));



        public vMixController.Widgets.vMixControl Control
        {
            get { return (vMixController.Widgets.vMixControl)GetValue(ControlProperty); }
            set { SetValue(ControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Control.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ControlProperty =
            DependencyProperty.Register("Control", typeof(vMixController.Widgets.vMixControl), typeof(vMixControlContainerDummy), new PropertyMetadata(null));

        public RelayCommand<Widgets.vMixControl> CloseCommand
        {
            get { return (RelayCommand<Widgets.vMixControl>)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CloseCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(RelayCommand<Widgets.vMixControl>), typeof(vMixControlContainerDummy), new PropertyMetadata(null));



        public RelayCommand<Widgets.vMixControl> SettingsCommand
        {
            get { return (RelayCommand<Widgets.vMixControl>)GetValue(SettingsCommandProperty); }
            set { SetValue(SettingsCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SettingsCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingsCommandProperty =
            DependencyProperty.Register("SettingsCommand", typeof(RelayCommand<Widgets.vMixControl>), typeof(vMixControlContainerDummy), new PropertyMetadata(null));




        public RelayCommand<Widgets.vMixControl> CopyCommand
        {
            get { return (RelayCommand<Widgets.vMixControl>)GetValue(CopyCommandProperty); }
            set { SetValue(CopyCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CopyCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CopyCommandProperty =
            DependencyProperty.Register("CopyCommand", typeof(RelayCommand<Widgets.vMixControl>), typeof(vMixControlContainerDummy), new PropertyMetadata(null));


        public RelayCommand<Widgets.vMixControl> ScaleUpCommand
        {
            get { return (RelayCommand<Widgets.vMixControl>)GetValue(ScaleUpCommandProperty); }
            set { SetValue(ScaleUpCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CopyCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleUpCommandProperty =
            DependencyProperty.Register("ScaleUpCommand", typeof(RelayCommand<Widgets.vMixControl>), typeof(vMixControlContainerDummy), new PropertyMetadata(null));


        public RelayCommand<Widgets.vMixControl> ScaleDownCommand
        {
            get { return (RelayCommand<Widgets.vMixControl>)GetValue(ScaleDownCommandProperty); }
            set { SetValue(ScaleDownCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CopyCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleDownCommandProperty =
            DependencyProperty.Register("ScaleDownCommand", typeof(RelayCommand<Widgets.vMixControl>), typeof(vMixControlContainerDummy), new PropertyMetadata(null));

        public object MainContent
        {
            get { return (object)GetValue(MainContentProperty); }
            set { SetValue(MainContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MainContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.Register("MainContent", typeof(object), typeof(vMixControlContainerDummy), new PropertyMetadata(null));



        public object CaptionContent
        {
            get { return (object)GetValue(CaptionContentProperty); }
            set { SetValue(CaptionContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CaptionContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionContentProperty =
            DependencyProperty.Register("CaptionContent", typeof(object), typeof(vMixControlContainerDummy), new PropertyMetadata(null));

        private void CC_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*var Caption = (((this.GetVisualChild(0) as Border).Child as Grid).Children[0] as StackPanel).Children[0] as Grid;
            var ContentControl = (((this.GetVisualChild(0) as Border).Child as Grid).Children[0] as StackPanel).Children[1] as ContentControl;*/
            if (!double.IsNaN(((vMixControlContainer)Content).ContentControl.ActualHeight) && ((vMixControlContainer)Content).ContentControl.ActualHeight != 0)
                Control.Height = ((vMixControlContainer)Content).ContentControl.ActualHeight;
            if (!double.IsNaN(((vMixControlContainer)Content).Caption.ActualHeight) && ((vMixControlContainer)Content).Caption.ActualHeight != 0)
                Control.CaptionHeight = ((vMixControlContainer)Content).Caption.ActualHeight;
        }


    }

    public class LockButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? (char)0xF33E : (char)0xF340;
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
