﻿using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using vMixController.Classes;
using System.Windows.Controls;
using vMixController.Extensions;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Diagnostics;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlClock : vMixControl
    {
        public override string Type => "Clock";
        public override int MaxCount => 1;

        //private string _lastExecuted = null;

        [NonSerialized]
        private DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Render);
        /*[NonSerialized]
        private Triple<DateTime, string, bool> _currentEvent;
        [NonSerialized]
        private Triple<DateTime, string, bool> _prevEvent;*/
        [NonSerialized]
        Queue<Pair<DateTime, string>> _eventQueue = new Queue<Pair<DateTime, string>>();


        /// <summary>
        /// The <see cref="Events" /> property's name.
        /// </summary>
        public const string EventsPropertyName = "Events";

        private ObservableCollection<Pair<DateTime, string>> _events = new ObservableCollection<Pair<DateTime, string>>();

        private List<Pair<DateTime, string>> _firedEvents = new List<Pair<DateTime, string>>();

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
            _timer.Interval = TimeSpan.FromSeconds(0.5);
            _timer.Tick += Timer_Tick;
        }

        private bool ExecuteToday(DateTime date, DateTime now)
        {
            switch (now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return (date.Millisecond & 1) == 1;
                case DayOfWeek.Tuesday:
                    return (date.Millisecond & (1 << 1)) == 1 << 1;
                case DayOfWeek.Wednesday:
                    return (date.Millisecond & (1 << 2)) == 1 << 2;
                case DayOfWeek.Thursday:
                    return (date.Millisecond & (1 << 3)) == 1 << 3;
                case DayOfWeek.Friday:
                    return (date.Millisecond & (1 << 4)) == 1 << 4;
                case DayOfWeek.Saturday:
                    return (date.Millisecond & (1 << 5)) == 1 << 5;
                case DayOfWeek.Sunday:
                    return (date.Millisecond & (1 << 6)) == 1 << 6;
            }
            return false;
        }

        private bool IsEventFired(Pair<DateTime, string> e)
        {
            //limit fired events to 32
            while (_firedEvents.Count > 32)
                _firedEvents.RemoveAt(0);

            foreach (var item in _firedEvents)
            {
                if (item.A == e.A && item.B == e.B)
                    return true;
            }
            return false;
        }

        private string GetDayOfWeek(DateTime date)
        {

            int d = (int)date.DayOfWeek - 1;
            if (d < 0)
                d = 6;
            return new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday", "Never" }[d];
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            if (_eventQueue.Count > 0 && _eventQueue.Peek().A.Hour == now.Hour && _eventQueue.Peek().A.Minute == now.Minute && _eventQueue.Peek().A.Second <= now.Second && ExecuteToday(_eventQueue.Peek().A, DateTime.Now) && !IsEventFired(_eventQueue.Peek()))
            {
                var ev = _eventQueue.Dequeue();
                Messenger.Default.Send(new Pair<string, object>(ev.B, null));
                _firedEvents.Add(ev);
                Debug.Print("{0} is fired", ev.B);
            }

            var day = GetNextEvent(now);
            DateTime lastDate = now;
            if (day == null)
            {
                var date = DateTime.Now.Date;
                for (int i = 1; i < 7 && day == null; i++)
                    day = GetNextEvent(lastDate = date.AddDays(i));
            }
            if (day == null)
                NextEventAt = LocalizationManager.Get("No new events scheduled");
            else
            {
                NextEventAt = String.Format(@"{2} <{1}> {3} {0:HH\:mm\:ss}, {4}", day.A, day.B, LocalizationManager.Get("Next Event"), LocalizationManager.Get("At"), GetDayOfWeek(lastDate) == GetDayOfWeek(now) ? "Today" : GetDayOfWeek(lastDate));
                if (_eventQueue.Count > 0)
                {
                    var item = _eventQueue.Peek();
                    if (item.A != day.A || item.B != day.B)
                        _eventQueue.Enqueue(day);
                }
                else
                {
                    _eventQueue.Enqueue(day);
                }
            }

        }

        private Pair<DateTime, string> GetNextEvent(DateTime now)
        {
            foreach (var item in _events.OrderBy(x => new TimeSpan(0, x.A.Hour, x.A.Minute, x.A.Second, 0)))
            {
                var day = GetDayOfWeek(item.A);
                if (item.A.Hour + item.A.Minute / 60.0 + item.A.Second / 3600.0 > now.Hour + now.Minute / 60.0 + now.Second / 3600.0 && (ExecuteToday(item.A, now)))
                    return item;//String.Format(@"{2} <{1}> {3} {0:HH\:mm\:ss}, {4}", item.A, item.B, LocalizationManager.Get("Next Event"), LocalizationManager.Get("At"), day);
            }
            return null;
        }

        public override void Update()
        {
            if (!_timer.IsEnabled)
                _timer.Start();
            base.Update();
        }

        public override UserControl[] GetPropertiesControls()
        {
            _timer.Stop();

            var clockSchedule = GetPropertyControl<PropertiesControls.SchedulerControl>();
            clockSchedule.Events.Clear();
            foreach (var item in _events)
            {
                clockSchedule.Events.Add(item);
            }
            return base.GetPropertiesControls().Union(new UserControl[] { clockSchedule }).ToArray();
        }

        public override void SetProperties(UserControl[] _controls)
        {
            base.SetProperties(_controls);
            var ctrl = _controls.OfType<PropertiesControls.SchedulerControl>().First();
            _events.Clear();
            foreach (var item in ctrl.Events.OrderBy(x => new TimeSpan(0, x.A.Hour, x.A.Minute, x.A.Second, 0)))
            {
                _events.Add(item);
            }
            _timer.Start();
        }

        protected override void Dispose(bool managed)
        {
            if (_disposed) return;

            if (managed)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
                base.Dispose(managed);
                GC.SuppressFinalize(this);
            }
        }
    }
}
