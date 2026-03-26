using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using vMixController.Classes;

namespace vMixController.PropertiesControls
{
    /// <summary>
    /// Код взаимодействия для PathsControl.xaml
    /// </summary>
    public partial class SchedulerControl : UserControl
    {
        public static readonly DependencyProperty EventsProperty =
            DependencyProperty.Register(
                nameof(Events),
                typeof(ObservableCollection<ScheduledEvent>),
                typeof(SchedulerControl),
                new PropertyMetadata(new ObservableCollection<ScheduledEvent>()));

        public ObservableCollection<ScheduledEvent> Events
        {
            get => (ObservableCollection<ScheduledEvent>)GetValue(EventsProperty);
            set => SetValue(EventsProperty, value);
        }

        [RelayCommand]
        private void RemovePath(ScheduledEvent p)
        {
            Events.Remove(p);
        }

        [RelayCommand]
        private void CheckM(ScheduledEvent p)
        {
            p.Days = p.Days ^ DaysOfWeek.Monday;
        }

        [RelayCommand]
        private void CheckT(ScheduledEvent p)
        {
            p.Days = p.Days ^ DaysOfWeek.Tuesday;
        }

        [RelayCommand]
        private void CheckW(ScheduledEvent p)
        {
            p.Days = p.Days ^ DaysOfWeek.Wednesday;
        }

        [RelayCommand]
        private void CheckTH(ScheduledEvent p)
        {
            p.Days = p.Days ^ DaysOfWeek.Thursday;
        }

        [RelayCommand]
        private void CheckF(ScheduledEvent p)
        {
            p.Days = p.Days ^ DaysOfWeek.Friday;
        }

        [RelayCommand]
        private void CheckS(ScheduledEvent p)
        {
            p.Days = p.Days ^ DaysOfWeek.Saturday;
        }

        [RelayCommand]
        private void CheckSU(ScheduledEvent p)
        {
            p.Days = p.Days ^ DaysOfWeek.Sunday;
        }

        [RelayCommand]
        private void AddPath()
        {
            var now = DateTime.Now;
            Events.Add(new ScheduledEvent() { TimeOfDay = now, Days = DaysOfWeek.Everyday });
        }

        public SchedulerControl()
        {
            InitializeComponent();
        }
    }
}
