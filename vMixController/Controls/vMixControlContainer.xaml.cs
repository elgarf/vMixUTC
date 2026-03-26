using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using vMixController.Messages;
using vMixController.Classes;

namespace vMixController.Controls
{
    /// <summary>
    /// ������ �������������� ��� vMixControlContainer.xaml
    /// </summary>
    public partial class vMixControlContainer : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        static Queue<vMixControlContainer> _initList = new Queue<vMixControlContainer>();
        static bool? _lastLoadingState = null;
        static IMessenger _messenger;
        static IMessenger Messenger
        {
            get
            {
                if (_messenger != null)
                    return _messenger;

                if (AppServices.IsRegistered<IMessenger>())
                    _messenger = AppServices.GetRequiredService<IMessenger>();
                else
                    _messenger = WeakReferenceMessenger.Default;

                return _messenger;
            }
        }

        static DispatcherTimer _timer = new DispatcherTimer() { };

        static vMixControlContainer()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private static void _timer_Tick(object sender, EventArgs e)
        {

            if (_initList.Count > 0)
            {
                var container = _initList.Dequeue();
                container.InitializeComponent();
                //container.LoadViewFromUri("vMixController;component/Controls/vMixControlContainer.xaml");
                
            }
            if (_initList.Count == 0)
                _timer.Stop();

            var isLoading = _initList.Count > 0;
            if (_lastLoadingState != isLoading)
            {
                _lastLoadingState = isLoading;
                Messenger.Send(new LoadingMessage() { Loading = isLoading });
            }
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
            get => _parentContainer;
            set => SetPropertyValue(ref _parentContainer, value, ParentContainerPropertyName);
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
            if (ParentContainer?.Control != null)
                Messenger.Send(new HoveredWidgetMessage { Widget = ParentContainer.Control });
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
            if (ParentContainer?.Control != null)
                Messenger.Send(new HoveredWidgetMessage { Widget = null });
        }

        private bool SetPropertyValue<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }

}

