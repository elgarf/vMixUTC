using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace vMixController.Controls
{
    public class ReleaseButton : RepeatButton
    {

        DispatcherTimer _timer;
        DateTime _lastClick = DateTime.Now;

        protected override void OnClick()
        {
            Debug.Print("Click");
            Debug.Print("{0}", (DateTime.Now - _lastClick).TotalMilliseconds);
            _lastClick = DateTime.Now;
            
            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Tick += _timer_Tick;
            }
            _timer.Interval = TimeSpan.FromMilliseconds(Interval / 2);
            if (!_timer.IsEnabled)
            {
                OnPress?.Execute(null);
                _timer.Start();
            }
            base.OnClick();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            var dt = DateTime.Now - _lastClick;
            if (dt.TotalMilliseconds > Interval)
            {
                OnRelease?.Execute(null);
                _timer.Stop();
            }
        }



        public ICommand OnPress
        {
            get { return (ICommand)GetValue(OnPressProperty); }
            set { SetValue(OnPressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnPress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnPressProperty =
            DependencyProperty.Register("OnPress", typeof(ICommand), typeof(ReleaseButton), new PropertyMetadata(null));




        public ICommand OnRelease
        {
            get { return (ICommand)GetValue(OnReleaseProperty); }
            set { SetValue(OnReleaseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OnRelease.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnReleaseProperty =
            DependencyProperty.Register("OnRelease", typeof(ICommand), typeof(ReleaseButton), new PropertyMetadata(null));


    }
}
