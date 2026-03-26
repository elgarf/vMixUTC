using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace vMixController.PropertiesControls
{
    public partial class MidiMappingControl : UserControl
    {
        public static readonly DependencyProperty LearnFunctionProperty =
            DependencyProperty.Register(nameof(LearnFunction), typeof(Func<Widgets.MidiInterfaceKey>), typeof(MidiMappingControl), new PropertyMetadata(null));

        public Func<Widgets.MidiInterfaceKey> LearnFunction
        {
            get => (Func<Widgets.MidiInterfaceKey>)GetValue(LearnFunctionProperty);
            set => SetValue(LearnFunctionProperty, value);
        }

        public static readonly DependencyProperty MidisProperty =
            DependencyProperty.Register(nameof(Midis), typeof(ObservableCollection<Widgets.MidiInterfaceKey>), typeof(MidiMappingControl), new PropertyMetadata(new ObservableCollection<Widgets.MidiInterfaceKey>()));

        public ObservableCollection<Widgets.MidiInterfaceKey> Midis
        {
            get => (ObservableCollection<Widgets.MidiInterfaceKey>)GetValue(MidisProperty);
            set => SetValue(MidisProperty, value);
        }

        [RelayCommand]
        private void RemovePath(Widgets.MidiInterfaceKey p)
        {
            Midis.Remove(p);
        }

        [RelayCommand]
        private void AddPath()
        {
            Midis.Add(new Widgets.MidiInterfaceKey { A = -1, B = -1, C = "", D = Melanchall.DryWetMidi.Core.MidiEventType.ControlChange });
        }

        [RelayCommand]
        private void LearnMidiKey(Widgets.MidiInterfaceKey p)
        {
            var result = LearnFunction?.Invoke();
            if (result != null)
            {
                p.A = result.A;
                p.B = result.B;
                p.D = result.D;
            }
        }

        public MidiMappingControl()
        {
            InitializeComponent();
        }
    }
}
