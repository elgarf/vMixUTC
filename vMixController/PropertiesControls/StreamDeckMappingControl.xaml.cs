using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace vMixController.PropertiesControls
{
    /// <summary>
    /// Код взаимодействия для PathsControl.xaml
    /// </summary>
    public partial class StreamDeckMappingControl : UserControl
    {
        public static readonly DependencyProperty LearnFunctionProperty =
            DependencyProperty.Register(
                nameof(LearnFunction),
                typeof(Func<Widgets.StreamDeckKey>),
                typeof(StreamDeckMappingControl),
                new PropertyMetadata(null));

        public Func<Widgets.StreamDeckKey> LearnFunction
        {
            get => (Func<Widgets.StreamDeckKey>)GetValue(LearnFunctionProperty);
            set => SetValue(LearnFunctionProperty, value);
        }

        public static readonly DependencyProperty KeysProperty =
            DependencyProperty.Register(
                nameof(Keys),
                typeof(ObservableCollection<Widgets.StreamDeckKey>),
                typeof(StreamDeckMappingControl),
                new PropertyMetadata(new ObservableCollection<Widgets.StreamDeckKey>()));

        public ObservableCollection<Widgets.StreamDeckKey> Keys
        {
            get => (ObservableCollection<Widgets.StreamDeckKey>)GetValue(KeysProperty);
            set => SetValue(KeysProperty, value);
        }

        [RelayCommand]
        private void RemovePath(Widgets.StreamDeckKey p)
        {
            Keys.Remove(p);
        }

        [RelayCommand]
        private void AddPath()
        {
            Keys.Add(new Widgets.StreamDeckKey() { A = "" });
        }

        [RelayCommand]
        private void LearnStreamDeckKey(Widgets.StreamDeckKey p)
        {
            var result = LearnFunction?.Invoke();
            if (result != null)
            {
                p.A = result.A;
            }
        }

        public StreamDeckMappingControl()
        {
            InitializeComponent();
        }
    }
}
