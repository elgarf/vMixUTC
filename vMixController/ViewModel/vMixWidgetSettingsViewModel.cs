using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using vMixController.Classes;
using vMixController.Widgets;
using System.IO;
using vMixController;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Controls;
using vMixController.Extensions;
using System.Xml.Serialization;
using System.Threading;
using vMixController.Classes.Scripting;
using vMixAPI;

namespace vMixController.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// </summary>
    public partial class vMixWidgetSettingsViewModel : ViewModelBase
    {

        public static List<Triple<Color, Color, string>> Colors = new List<Triple<Color, Color, string>>()
        {
            new Triple<Color, Color, string>(Color.FromRgb(41, 48, 56), Color.FromRgb(62, 72, 84), "Gray"),
            new Triple<Color, Color, string>(Color.FromRgb(112, 128, 144), Color.FromRgb(119, 136, 153), "Slate Gray"),
            new Triple<Color, Color, string>(Color.FromRgb(26, 60, 117), Color.FromRgb(24, 72, 140), "Blue"),
            new Triple<Color, Color, string>(Color.FromRgb(0, 135, 255), Color.FromRgb(24, 202, 255), "Aqua"),
            new Triple<Color, Color, string>(Color.FromRgb(127, 255, 212), Color.FromRgb(175, 238, 238), "Aquamarine"),
            new Triple<Color, Color, string>(Color.FromRgb(245, 37, 217), Color.FromRgb(247, 84, 255), "Fuchsia"),//
            new Triple<Color, Color, string>(Color.FromRgb(128, 0, 128), Color.FromRgb(192, 24, 192), "Purple"),
            new Triple<Color, Color, string>(Color.FromRgb(255, 140, 0), Color.FromRgb(255, 210, 24), "Orange"),
            new Triple<Color, Color, string>(Color.FromRgb(0, 100, 0), Color.FromRgb(24, 150, 24), "Green"),
            new Triple<Color, Color, string>(Color.FromRgb(55, 173, 95), Color.FromRgb(80, 200, 120), "Emerald"),//
            new Triple<Color, Color, string>(Color.FromRgb(139, 0, 0), Color.FromRgb(208, 24, 24), "Red"),
            new Triple<Color, Color, string>(Color.FromRgb(255, 215, 0), Color.FromRgb(255, 255, 0), "Yellow")
        };

        [ObservableProperty]
        private Quadriple<double?, double?, double?, double?> windowProperties = new Quadriple<double?, double?, double?, double?>() { A = 512, B = 512, C = 0, D = 0 };

        [ObservableProperty]
        private Visibility periodVisibility = Visibility.Visible;

        [ObservableProperty]
        private vMixAPI.State model = null;

        [ObservableProperty]
        private ObservableCollection<Input> availableInputs = new ObservableCollection<Input>();

        private vMixAPI.State _subscribedModel;

        partial void OnModelChanged(vMixAPI.State value)
        {
            if (!ReferenceEquals(_subscribedModel, value))
            {
                if (_subscribedModel != null)
                    _subscribedModel.OnStateSynced -= Model_OnStateSynced;

                _subscribedModel = value;

                if (_subscribedModel != null)
                    _subscribedModel.OnStateSynced += Model_OnStateSynced;
            }

            RefreshAvailableInputs();
        }

        private void Model_OnStateSynced(object sender, StateSyncedEventArgs e)
        {
            var dispatcher = App.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
                RefreshAvailableInputs();
            else
                dispatcher.BeginInvoke(new System.Action(RefreshAvailableInputs));
        }

        private void RefreshAvailableInputs()
        {
            var source = Model;
            if ((source?.Inputs == null || source.Inputs.Count == 0) && AppServices.IsRegistered<MainViewModel>())
                source = AppServices.GetRequiredService<MainViewModel>().Model;

            AvailableInputs = new ObservableCollection<Input>(source?.Inputs ?? new List<Input>());
        }

        public ObservableCollection<Pair<string, string>> GlobalVariables =>
            AppServices.GetRequiredService<GlobalVariablesViewModel>().Variables;

        public ObservableCollection<vMixFunctionReference> Functions =>
            AppServices.GetRequiredService<MainViewModel>().Functions;

        public ObservableCollection<vMixNewFunctionReference> NewFunctions =>
            AppServices.GetRequiredService<MainViewModel>().NewFunctions;

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private Color color = Colors[0].A;

        [ObservableProperty]
        private ObservableCollection<Hotkey> hotkey = new ObservableCollection<Classes.Hotkey>();

        [ObservableProperty]
        private bool isHotkeysVisible = true;

        [ObservableProperty]
        private int period = 0;

        [ObservableProperty]
        private int zIndex = 0;

        [ObservableProperty]
        private UserControl[] widgetPropertiesControls = null;

        [ObservableProperty]
        private string type = "";

        [ObservableProperty]
        private vMixControl widget = null;

        [RelayCommand]
        private void Ok()
        {
            Messenger.Send(new ValueChangedMessage<bool>(true));
        }

        [RelayCommand]
        private void Cancel()
        {
            Messenger.Send(new ValueChangedMessage<bool>(false));
        }

        [RelayCommand]
        private void SaveTemplate()
        {
            var viewModel = vMixController.Classes.AppServices.GetRequiredService<vMixController.ViewModel.MainViewModel>();
            var obj = viewModel.WidgetTemplates.Select((x, i) => new { obj = x, idx = i }).Where(x => x.obj.A == Name).FirstOrDefault();
            var cpy = Widget.Copy();
            if (cpy != null)
            {
                cpy.IsTemplate = true;
                if (obj != null)
                    viewModel.WidgetTemplates[obj.idx].B = cpy;
                else
                    viewModel.WidgetTemplates.Add(new Pair<string, vMixControl>(Name, cpy));
                Messenger.Send(new ValueChangedMessage<bool>(true));
            }
        }


        public void SetProperties(vMixControl p)
        {
            Model = p?.State ?? (AppServices.IsRegistered<MainViewModel>() ? AppServices.GetRequiredService<MainViewModel>().Model : null);
            RefreshAvailableInputs();


            if (p is IvMixAutoUpdateWidget)
                Period = (p as IvMixAutoUpdateWidget).Period;

            PeriodVisibility = p is IvMixAutoUpdateWidget ? Visibility.Visible : Visibility.Collapsed;
        }

        [RelayCommand]
        private void Closing()
        {
        }

        [RelayCommand]
        private void LearnKey(Hotkey p)
        {
            var wnd = new KeyLearnWindow();
            var result = wnd.ShowDialog();
            if (result ?? true)
            {
                p.Key = wnd.PressedKey;
                wnd.Close();
            }
        }

        /// <summary>
        /// Initializes a new instance of the vMixWidgetSettingsViewModel class.
        /// </summary>
        public vMixWidgetSettingsViewModel()
        {
            List<Triple<Color, Color, string>> list = null;
            XmlSerializer ser = new XmlSerializer(typeof(List<Triple<Color, Color, string>>));

            if (File.Exists("Colours.xml"))
            {
                using (var fs = File.Open("Colours.xml", FileMode.Open))
                    list = (List<Triple<Color, Color, string>>)ser.Deserialize(fs);
            }
            else
            {

                list = new List<Triple<Color, Color, string>>()
                {
                    new Triple<Color, Color, string>(Color.FromRgb(255, 255, 255), Color.FromRgb(255, 255, 255), "White")
                };

                using (var fs = File.Open("Colours.xml", FileMode.Create))
                    ser.Serialize(fs, list);
            }

            foreach (var item in list)
            {
                Colors.Add(item);
            }
        }
    }
}

