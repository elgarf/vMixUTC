using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using vMixController.Classes;
using vMixController.ViewModel;
using System.Windows.Controls;
using vMixController.Extensions;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlClock: vMixControl
    {
        public override string Type => "Clock";
        public override int MaxCount => 1;

        private DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Background);

        /// <summary>
        /// The <see cref="Events" /> property's name.
        /// </summary>
        public const string EventsPropertyName = "Events";

        private ObservableCollection<Pair<DateTime, string>> _events = new ObservableCollection<Pair<DateTime, string>>();

        /// <summary>
        /// Sets and gets the Events property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Pair<DateTime, string>> Events
        {
            get
            {
                return _events;
            }

            set
            {
                if (_events == value)
                {
                    return;
                }

                _events = value;
                RaisePropertyChanged(EventsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="NextEventAt" /> property's name.
        /// </summary>
        public const string NextEvetnAtPropertyName = "NextEventAt";

        private string _nextEventAt = "";

        /// <summary>
        /// Sets and gets the NextEvetnAt property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string NextEventAt
        {
            get
            {
                return _nextEventAt;
            }

            set
            {
                if (_nextEventAt == value)
                {
                    return;
                }

                _nextEventAt = value;
                RaisePropertyChanged(NextEvetnAtPropertyName);
            }
        }

        public vMixControlClock()
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            foreach (var item in _events)
            {
                if (item.A.Hour == now.Hour && item.A.Minute == now.Minute && item.A.Second == now.Second)
                    Messenger.Default.Send(item.B);
            }
            NextEventAt = GetNextEvent(now);
        }

        private string GetNextEvent(DateTime now)
        {

            foreach (var item in _events)
                if (item.A.Hour + item.A.Minute / 60.0 + item.A.Second / 3600.0 > now.Hour + now.Minute / 60.0 + now.Second / 3600.0)
                    return String.Format(@"{2} <{1}> {3} {0:HH\:mm\:ss}", item.A, item.B, LocalizationManager.Get("Next Event"), LocalizationManager.Get("At"));

            return LocalizationManager.Get("No Events Scheduled");
        }

        public override UserControl[] GetPropertiesControls()
        {
            _timer.Stop();
            var ctrl = GetPropertyControl<PropertiesControls.SchedulerControl>();
            ctrl.Events.Clear();
            foreach (var item in _events)
            {
                ctrl.Events.Add(item);
            }
            return base.GetPropertiesControls().Union(new UserControl[] { ctrl }).ToArray();
        }

        public override void SetProperties(UserControl[] _controls)
        {
            base.SetProperties(_controls);
            var ctrl = _controls.OfType<PropertiesControls.SchedulerControl>().First();
            _events.Clear();
            foreach (var item in ctrl.Events.OrderBy(x=>x.A.Ticks))
            {
                _events.Add(item);
            }
            _timer.Start();
        }

        public override void Dispose()
        {
            _timer.Stop();
            base.Dispose();
        }
    }
}
