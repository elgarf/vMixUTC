using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
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

        // --- Поля ---

        // Таймер остается прежним, но с интервалом в 1 секунду - чаще не нужно.
        [NonSerialized]
        private DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromSeconds(1) };

        // Храним отсортированную копию для быстрой обработки
        [NonSerialized]
        private List<ScheduledEvent> _sortedEvents = new List<ScheduledEvent>();

        // Используем HashSet для быстрой проверки сработавших событий (O(1) в среднем)
        [NonSerialized]
        private HashSet<ScheduledEvent> _firedEventsToday = new HashSet<ScheduledEvent>();

        [NonSerialized]
        private DateTime _lastTickDate = DateTime.MinValue;

        // --- Свойства MVVM ---

        /// <summary>
        /// Основная коллекция событий для привязки к UI.
        /// Используем новую, строго типизированную модель.
        /// </summary>
        public ObservableCollection<ScheduledEvent> Events { get; set; } = new ObservableCollection<ScheduledEvent>();

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
                RaisePropertyChanged(nameof(NextEventAt));
            }
        }
        // --- Конструктор и методы ---

        public vMixControlClock()
        {
            _timer.Tick += Timer_Tick;
            // Подписываемся на изменение коллекции, чтобы поддерживать _sortedEvents в актуальном состоянии
            Events.CollectionChanged += (s, e) => UpdateSortedEvents();
            _lastTickDate = DateTime.Now; // Инициализируем дату последнего тика
        }

        private void UpdateSortedEvents()
        {
            _sortedEvents = Events.OrderBy(x => x.TimeOfDay.TimeOfDay).ToList();
            _firedEventsToday.Clear(); // Сброс сработавших событий при любом изменении расписания
            foreach (var ev in _sortedEvents)
            {
                if (ev.Days.HasFlag(ToDaysOfWeek(DateTime.Now.DayOfWeek)) && ev.TimeOfDay.TimeOfDay <= DateTime.Now.TimeOfDay)
                {
                    _firedEventsToday.Add(ev); // Помечаем все события, которые уже должны были сработать сегодня
                    Debug.Print($"Event '{ev.Command}' at {ev.TimeOfDay} marked as fired on update.");
                }
            }
            // После изменения списка событий нужно пересчитать следующее событие
            UpdateNextEventDisplay();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;

            // 1. Проверяем, не наступил ли новый день
            if (now.Date > _lastTickDate.Date)
            {
                _firedEventsToday.Clear();
                Debug.Print("New day detected. Fired events cleared.");
            }
            _lastTickDate = now;

            // 2. Определяем сегодняшний день недели
            var today = ToDaysOfWeek(now.DayOfWeek);

            // 3. Ищем и запускаем события, которые должны были сработать
            foreach (var ev in _sortedEvents)
            {
                // Условия срабатывания:
                // - Событие запланировано на сегодня
                // - Время события уже наступило
                // - Событие еще не срабатывало сегодня
                if (ev.Days.HasFlag(today) && now.TimeOfDay >= ev.TimeOfDay.TimeOfDay && !_firedEventsToday.Contains(ev))
                {
                    Messenger.Default.Send(new HotkeyLinkMessage() { Link = ev.Command, Parameter = ScriptExecutionDispatchRuntime.CreateOutgoingParameter(null) });
                    _firedEventsToday.Add(ev);
                    Debug.Print($"Event '{ev.Command}' at {ev.TimeOfDay} fired.");

                    // После срабатывания события, немедленно обновляем информацию о следующем
                    UpdateNextEventDisplay();
                }
            }
        }

        /// <summary>
        /// Находит следующее запланированное событие и обновляет свойство NextEventAt.
        /// </summary>
        private void UpdateNextEventDisplay()
        {
            var next = FindNextScheduledEvent();
            if (next?.Event != null)
            {
                string dayString = next?.Date.Date == DateTime.Today ? "Today" : next?.Date.ToString("dddd", CultureInfo.InvariantCulture);
                NextEventAt = $"Next Event: <{next?.Event.Command}> at {next?.Event.TimeOfDay:HH\\:mm\\:ss} on {dayString}";
                // Локализация может быть добавлена здесь
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
        /// Ищет следующее по расписанию событие в течение ближайшей недели.
        /// </summary>
        private (ScheduledEvent Event, DateTime Date)? FindNextScheduledEvent()
        {
            if (_sortedEvents.Count == 0) return null;

            var now = DateTime.Now;

            // Ищем событие сегодня, но позже текущего времени
            foreach (var ev in _sortedEvents)
            {
                if (ev.Days.HasFlag(ToDaysOfWeek(now.DayOfWeek)) && ev.TimeOfDay.TimeOfDay > now.TimeOfDay)
                    return (ev, now);
            }

            // Если сегодня больше ничего нет, ищем в последующие 7 дней
            for (int i = 1; i <= 7; i++)
            {
                var nextDay = now.AddDays(i);
                var dayOfWeek = ToDaysOfWeek(nextDay.DayOfWeek);
                foreach (var ev in _sortedEvents)
                {
                    if (ev.Days.HasFlag(dayOfWeek))
                        return (ev, nextDay); // Нашли первое событие на этот день
                }
            }

            return null; // Ничего не найдено в течение недели
        }

        // Вспомогательный метод для конвертации DayOfWeek в наш enum
        private static DaysOfWeek ToDaysOfWeek(DayOfWeek day)
        {
            return (DaysOfWeek)(1 << (((int)day + 6) % 7));
        }


        // --- Переопределенные методы базового класса ---

        public override void Update()
        {
            if (!_timer.IsEnabled)
            {
                UpdateSortedEvents(); // Первоначальная сортировка
                _timer.Start();
            }
            base.Update();
        }

        // Методы GetPropertiesControls и SetProperties потребуют адаптации под новую структуру ScheduledEvent.
        // Это зависит от реализации PropertiesControls.SchedulerControl.
        // Предположим, что он теперь работает с ObservableCollection<ScheduledEvent>.
        public override void BeforePropertiesChanged()
        {
            _timer.Stop();
            base.BeforePropertiesChanged();
        }

        public override void AfterPropertiesChanged()
        {
            base.AfterPropertiesChanged();
            UpdateSortedEvents(); // Обновляем отсортированный список
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
