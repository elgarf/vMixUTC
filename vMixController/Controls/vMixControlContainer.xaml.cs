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
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Diagnostics;
using vMixControllerSkin;
using System.Threading;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using vMixController.Messages;

namespace vMixController.Controls
{
    /// <summary>
    /// Логика взаимодействия для vMixControlContainer.xaml
    /// </summary>
    public partial class vMixControlContainer : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        static int _prevCount = 0;
        static Queue<vMixControlContainer> _initList = new Queue<vMixControlContainer>();

        static DispatcherTimer _timer = new DispatcherTimer() { };

        static vMixControlContainer()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private static void _timer_Tick(object sender, EventArgs e)
        {
            
            if (_initList.Count > 0)
                _initList.Dequeue().LoadViewFromUri("vMixController;component/Controls/vMixControlContainer.xaml");
            _prevCount = _initList.Count;
            if (_initList.Count == 0)
                _timer.Stop();

            Messenger.Default.Send(new LoadingMessage() { Loading = _initList.Count > 0 });
        }

        public Action<object, SizeChangedEventArgs> OnSizeChanged { get; set; }

        /// <summary>
        /// The <see cref="ParentContainer" /> property's name.
        /// </summary>
        public const string ParentContainerPropertyName = "ParentContainer";

        private vMixControlContainerDummy _parentContainer = null;

        /// <summary>
        /// Sets and gets the ParentContainer property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public vMixControlContainerDummy ParentContainer
        {
            get
            {
                return _parentContainer;
            }

            set
            {
                if (_parentContainer == value)
                {
                    return;
                }

                _parentContainer = value;
                RaisePropertyChanged(ParentContainerPropertyName);
            }
        }

        private void RaisePropertyChanged(string parentContainerPropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(parentContainerPropertyName));
        }

        public vMixControlContainer()
        {
            _initList.Enqueue(this);
            if (!_timer.IsEnabled)
                _timer.Start();
            //this.LoadViewFromUri("vMixController;component/Controls/vMixControlContainerPlaceholder.xaml");
            //InitializeComponent();
            //this.LoadViewFromUri("vMixController;component/Controls/vMixControlContainer.xaml");
        }

        private void VMixControlContainer_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void CC_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnSizeChanged?.Invoke(sender, e);
        }

        private void Caption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CC_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            moveThumb.Visibility = Keyboard.IsKeyDown(Key.LeftAlt) ? Visibility.Visible : Visibility.Collapsed;

        }

        private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var sb = ((Storyboard)Resources["OpacityOn"]);
            sb.Begin(RightButtons);
            //sb.Begin(LockButton);
            RightButtons.IsHitTestVisible = true;
            //LockButton.IsHitTestVisible = true;
        }

        private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var sb = ((Storyboard)Resources["OpacityOff"]);
            if (RightButtons.Opacity == 1)
                sb.BeginTime = TimeSpan.FromSeconds(2);
            else
                sb.BeginTime = TimeSpan.Zero;
            sb.Begin(RightButtons);
            //sb.Begin(LockButton);
            RightButtons.IsHitTestVisible = false;
            //LockButton.IsHitTestVisible = false;
        }
    }

}
