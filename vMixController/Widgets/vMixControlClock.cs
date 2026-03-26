using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using vMixController.Classes;
using vMixController.Classes.Scripting;
using vMixController.Extensions;
using vMixController.Messages;

namespace vMixController.Widgets
{

    [Serializable]
    public class vMixControlClock : vMixControl
    {
        public override string Type => "Clock";
        public override int MaxCount => 1;

        // --- ���� ---

        // ������ �������� �������, �� � ���������� � 1 ������� - ���� �� �����.
        [NonSerialized]
        private DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromSeconds(1) };

        // ������ ��������������� ����� ��� ������� ���������
        [NonSerialized]
        private List<ScheduledEvent> _sortedEvents = new List<ScheduledEvent>();

        // ���������� HashSet ��� ������� �������� ����������� ������� (O(1) � �������)
        [NonSerialized]
        private HashSet<ScheduledEvent> _firedEventsToday = new HashSet<ScheduledEvent>();

        [NonSerialized]
        private DateTime _lastTickDate = DateTime.MinValue;

        // --- �������� MVVM ---

        /// <summary>
        /// �������� ��������� ������� ��� �������� � UI.
        /// ���������� �����, ������ �������������� ������.
        /// </summary>
        public ObservableCollection<ScheduledEvent> Events { get; set; } = new ObservableCollection<ScheduledEvent>();

        private string _nextEventAt = "";

        /// <summary>
        /// Sets and gets the NextEvetnAt property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string NextEventAt
        {
            get => _nextEventAt;
            set => SetPropertyValue(ref _nextEventAt, value, nameof(NextEventAt));
        }


        // --- ����������� � ������ ---

        public vMixControlClock()
        {
            _timer.Tick += Timer_Tick;
            // ������������� �� ��������� ���������, ����� ������������ _sortedEvents � ���������� ���������
            Events.CollectionChanged += (s, e) => UpdateSortedEvents();
        }

        private void UpdateSortedEvents()
        {
            _sortedEvents = Events.OrderBy(x => x.TimeOfDay).ToList();
            // ����� ��������� ������ ������� ����� ����������� ��������� �������
            UpdateNextEventDisplay();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;

            // 1. ���������, �� �������� �� ����� ����
            if (now.Date > _lastTickDate.Date)
            {
                _firedEventsToday.Clear();
                Debug.Print("New day detected. Fired events cleared.");
            }
            _lastTickDate = now;

            // 2. ���������� ����������� ���� ������
            var today = ToDaysOfWeek(now.DayOfWeek);

            // 3. ���� � ��������� �������, ������� ������ ���� ���������
            foreach (var ev in _sortedEvents)
            {
                // ������� ������������:
                // - ������� ������������� �� �������
                // - ����� ������� ��� ���������
                // - ������� ��� �� ����������� �������
                if (ev.Days.HasFlag(today) && now >= ev.TimeOfDay && !_firedEventsToday.Contains(ev))
                {
                    Messenger.Send(new HotkeyLinkMessage() { Link = ev.Command, Parameter = ScriptExecutionDispatchRuntime.CreateOutgoingParameter(null) });
                    _firedEventsToday.Add(ev);
                    Debug.Print($"Event '{ev.Command}' at {ev.TimeOfDay} fired.");

                    // ����� ������������ �������, ���������� ��������� ���������� � ���������
                    UpdateNextEventDisplay();
                }
            }
        }

        /// <summary>
        /// ������� ��������� ��������������� ������� � ��������� �������� NextEventAt.
        /// </summary>
        private void UpdateNextEventDisplay()
        {
            var next = FindNextScheduledEvent();
            if (next?.Event != null)
            {
                string dayString = next?.Date.Date == DateTime.Today ? "Today" : next?.Date.ToString("dddd", CultureInfo.InvariantCulture);
                NextEventAt = $"Next Event: <{next?.Event.Command}> at {next?.Event.TimeOfDay:HH\\:mm\\:ss} on {dayString}";
                // ����������� ����� ���� ��������� �����
                // NextEventAt = string.Format("{0}: <{1}> {2} {3:hh\\:mm\\:ss} {4} {5}", 
                //      LocalizationManager.Get("Next Event"), next.Event.Command, LocalizationManager.Get("at"), 
                //      next.Event.TimeOfDay, LocalizationManager.Get("on"), dayString);
            }
            else
            {
                NextEventAt = "No new events scheduled";
                // NextEventAt = LocalizationManager.Get("No new events scheduled");
            }
        }

        /// <summary>
        /// ���� ��������� �� ���������� ������� � ������� ��������� ������.
        /// </summary>
        private (ScheduledEvent Event, DateTime Date)? FindNextScheduledEvent()
        {
            if (_sortedEvents.Count == 0) return null;

            var now = DateTime.Now;

            // ���� ������� �������, �� ����� �������� �������
            foreach (var ev in _sortedEvents)
            {
                if (ev.Days.HasFlag(ToDaysOfWeek(now.DayOfWeek)) && ev.TimeOfDay > now)
                    return (ev, now);
            }

            // ���� ������� ������ ������ ���, ���� � ����������� 7 ����
            for (int i = 1; i <= 7; i++)
            {
                var nextDay = now.AddDays(i);
                var dayOfWeek = ToDaysOfWeek(nextDay.DayOfWeek);
                foreach (var ev in _sortedEvents)
                {
                    if (ev.Days.HasFlag(dayOfWeek))
                        return (ev, nextDay); // ����� ������ ������� �� ���� ����
                }
            }

            return null; // ������ �� ������� � ������� ������
        }

        // ��������������� ����� ��� ����������� DayOfWeek � ��� enum
        private static DaysOfWeek ToDaysOfWeek(DayOfWeek day)
        {
            return (DaysOfWeek)(1 << (((int)day + 6) % 7));
        }


        // --- ���������������� ������ �������� ������ ---

        public override void Update()
        {
            if (!_timer.IsEnabled)
            {
                UpdateSortedEvents(); // �������������� ����������
                _timer.Start();
            }
            base.Update();
        }

        // ������ GetPropertiesControls � SetProperties ��������� ��������� ��� ����� ��������� ScheduledEvent.
        // ��� ������� �� ���������� PropertiesControls.SchedulerControl.
        // �����������, ��� �� ������ �������� � ObservableCollection<ScheduledEvent>.
        public override void BeforePropertiesChanged()
        {
            _timer.Stop();
            base.BeforePropertiesChanged();
        }

        public override void AfterPropertiesChanged()
        {
            base.AfterPropertiesChanged();
            UpdateSortedEvents(); // ��������� ��������������� ������
            _timer.Start();
        }

        protected override void Dispose(bool managed)
        {
            if (_disposed) return;

            if (managed)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
            }
            base.Dispose(managed);
        }
    }
}


