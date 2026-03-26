using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using vMixAPI;
using vMixController.Classes;
using vMixController.Interfaces;
using vMixController.ViewModel;

namespace vMixController.PropertiesControls
{
    /// <summary>
    /// Код взаимодействия для PathsControl.xaml
    /// </summary>
    public partial class TitleMappingControl : UserControl, ICancellable
    {
        public static readonly DependencyProperty TitlesProperty =
            DependencyProperty.Register(
                nameof(Titles),
                typeof(ObservableCollection<Pair<string, string>>),
                typeof(TitleMappingControl),
                new PropertyMetadata(null));

        public ObservableCollection<Pair<string, string>> Titles
        {
            get { return (ObservableCollection<Pair<string, string>>)GetValue(TitlesProperty); }
            set { SetValue(TitlesProperty, value); }
        }

        public static readonly DependencyProperty IsCancelledPropertyName =
            DependencyProperty.Register(nameof(IsCancelled), typeof(bool), typeof(TitleMappingControl), new PropertyMetadata(false));

        public bool IsCancelled
        {
            get { return (bool)GetValue(IsCancelledPropertyName); }
            set { SetValue(IsCancelledPropertyName, value); }
        }

        public static readonly DependencyProperty AvailableInputsProperty =
            DependencyProperty.Register(nameof(AvailableInputs), typeof(ObservableCollection<Input>), typeof(TitleMappingControl), new PropertyMetadata(new ObservableCollection<Input>()));

        public ObservableCollection<Input> AvailableInputs
        {
            get { return (ObservableCollection<Input>)GetValue(AvailableInputsProperty); }
            set { SetValue(AvailableInputsProperty, value); }
        }

        public static readonly DependencyProperty AvailableModelProperty =
            DependencyProperty.Register(nameof(AvailableModel), typeof(State), typeof(TitleMappingControl), new PropertyMetadata(null));

        public State AvailableModel
        {
            get { return (State)GetValue(AvailableModelProperty); }
            set { SetValue(AvailableModelProperty, value); }
        }

        public static readonly DependencyProperty GlobalVariablesProperty =
            DependencyProperty.Register(nameof(GlobalVariables), typeof(ObservableCollection<Pair<string, string>>), typeof(TitleMappingControl), new PropertyMetadata(new ObservableCollection<Pair<string, string>>()));

        public ObservableCollection<Pair<string, string>> GlobalVariables
        {
            get { return (ObservableCollection<Pair<string, string>>)GetValue(GlobalVariablesProperty); }
            set { SetValue(GlobalVariablesProperty, value); }
        }

        [RelayCommand]
        private void RemovePath(Pair<string, string> p)
        {
            Titles.Remove(p);
        }

        [RelayCommand]
        private void AddPath()
        {
            Titles.Add(new Pair<string, string>() { A = null, B = "" });
        }

        public TitleMappingControl()
        {
            InitializeComponent();
            Loaded += TitleMappingControl_Loaded;
        }

        private void TitleMappingControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshInputsSource();
        }

        private void RefreshInputsSource()
        {
            var widgetSettings = AppServices.IsRegistered<vMixWidgetSettingsViewModel>()
                ? AppServices.GetRequiredService<vMixWidgetSettingsViewModel>()
                : null;
            var model = widgetSettings?.Widget?.State
                ?? widgetSettings?.Model
                ?? (AppServices.IsRegistered<MainViewModel>() ? AppServices.GetRequiredService<MainViewModel>().Model : null);

            AvailableModel = model;
            GlobalVariables = widgetSettings?.GlobalVariables ?? new ObservableCollection<Pair<string, string>>();
            AvailableInputs = new ObservableCollection<Input>(model?.Inputs ?? new System.Collections.Generic.List<Input>());
        }
    }
}
