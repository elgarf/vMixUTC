using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using vMixController.Classes;
using vMixController.Widgets;

namespace vMixController.PropertiesControls
{
    /// <summary>
    /// Логика взаимодействия для ScriptControl.xaml
    /// </summary>
    public partial class ScriptControl : UserControl, INotifyPropertyChanged
    {
        private int _prevIndex = 0;
        public ScriptControl()
        {
            InitializeComponent();
            Commands.CollectionChanged += Commands_CollectionChanged;
        }

        private void Commands_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RearrangeCommnads();
        }

        private void GenerateCode()
        {
            List<string> code = new List<string>();
            foreach (var item in Commands)
                code.Add(item.ToString());
            if (code.Count > 0)
                TextCode = code.Aggregate((x, y) => x + "\r\n" + y);
            else
                TextCode = "";
        }

        private void RearrangeCommnads()
        {
            var ident = 0;
            foreach (var icmd in Commands)
            {
                icmd.PropertyChanged -= Icmd_PropertyChanged;
                icmd.PropertyChanged += Icmd_PropertyChanged;
                IsInputExist(icmd);
                if (icmd.Action.IsBlock)
                {
                    icmd.Ident = new Thickness(ident, 0, 0, 0);
                    ident += 8;
                    continue;
                }
                if (icmd.Action.Function == NativeFunctions.CONDITIONEND)
                    ident -= 8;

                if (ident < 0) ident = 0;

                icmd.Ident = new Thickness(ident, 0, 0, 0);
                GenerateCode();

            }
        }

        private void Icmd_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "InputKey")
            {
                var s = (sender as vMixControlButtonCommand);
                IsInputExist(s);
            }
        }

        private void IsInputExist(vMixControlButtonCommand s)
        {
            var key = s.InputKey;
            var l = (ViewModel.ViewModelLocator)TryFindResource("Locator");
            var check = !l.WidgetSettings.Model?.Inputs.Select(x => x.Key).Contains(key);
            s.NoInputAssigned = check ?? true;
        }


        /// <summary>
        /// The <see cref="TextCode" /> property's name.
        /// </summary>
        public const string TextCodePropertyName = "TextCode";

        private string _textCode = "";

        /// <summary>
        /// Sets and gets the TextCode property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string TextCode
        {
            get
            {
                return _textCode;
            }

            set
            {
                if (_textCode == value)
                {
                    return;
                }

                _textCode = value;

                RaisePropertyChanged(TextCodePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Commands" /> property's name.
        /// </summary>
        public const string CommandsPropertyName = "Commands";

        private ObservableCollection<vMixControlButtonCommand> _commands = new ObservableCollection<vMixControlButtonCommand>();

        /// <summary>
        /// Sets and gets the Commands property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<vMixControlButtonCommand> Commands
        {
            get
            {
                return _commands;
            }

            set
            {
                if (_commands == value)
                {
                    return;
                }

                _commands = value;
                RaisePropertyChanged(CommandsPropertyName);
            }
        }


        private RelayCommand<vMixControlButtonCommand> _removeCommandCommand;

        /// <summary>
        /// Gets the RemoveCommandCommand.
        /// </summary>
        public RelayCommand<vMixControlButtonCommand> RemoveCommandCommand
        {
            get
            {
                return _removeCommandCommand
                    ?? (_removeCommandCommand = new RelayCommand<vMixControlButtonCommand>(
                    p =>
                    {
                        Commands.Remove(p);
                        RearrangeCommnads();
                        //CollectionViewSource.GetDefaultView(script.ItemsSource)?.Refresh();
                    }));
            }
        }

        private RelayCommand _addCommandCommand;

        /// <summary>
        /// Gets the AddCommandCommand.
        /// </summary>
        public RelayCommand AddCommandCommand
        {
            get
            {
                return _addCommandCommand
                    ?? (_addCommandCommand = new RelayCommand(
                    () =>
                    {
                        var cmd = new vMixControlButtonCommand() { Action = new Classes.vMixFunctionReference() };
                        for (int i = 0; i < 10; i++)
                            cmd.AdditionalParameters.Add(new One<string>() { A = "" });
                        Commands.Add(cmd);
                        RearrangeCommnads();
                        //CollectionViewSource.GetDefaultView(script.ItemsSource)?.Refresh();
                    }));
            }
        }


        private RelayCommand _exportScriptCommand;

        /// <summary>
        /// Gets the ExportScriptCommand.
        /// </summary>
        public RelayCommand ExportScriptCommand
        {
            get
            {
                return _exportScriptCommand
                    ?? (_exportScriptCommand = new RelayCommand(
                    () =>
                    {
                        Ookii.Dialogs.Wpf.VistaSaveFileDialog opendlg = new Ookii.Dialogs.Wpf.VistaSaveFileDialog
                        {
                            Filter = "UTC Script File|*.usf",
                            DefaultExt = "usf"
                        };
                        var result = opendlg.ShowDialog(App.Current.Windows.OfType<vMixWidgetSettingsView>().FirstOrDefault());
                        if (result.HasValue && result.Value)
                        {
                            XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<vMixControlButtonCommand>));
                            using (var fs = new FileStream(opendlg.FileName, FileMode.Create))
                                s.Serialize(fs, Commands);
                        }

                    }));
            }
        }

        private RelayCommand _importScriptCommand;

        /// <summary>
        /// Gets the ImportScriptCommand.
        /// </summary>
        public RelayCommand ImportScriptCommand
        {
            get
            {
                return _importScriptCommand
                    ?? (_importScriptCommand = new RelayCommand(
                    () =>
                    {
                        Ookii.Dialogs.Wpf.VistaOpenFileDialog opendlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
                        {
                            Filter = "UTC Script File|*.usf",
                            DefaultExt = "usf"
                        };
                        var result = opendlg.ShowDialog(App.Current.Windows.OfType<vMixWidgetSettingsView>().FirstOrDefault());
                        if (result.HasValue && result.Value)
                        {
                            try
                            {
                                XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<vMixControlButtonCommand>));
                                using (var fs = new FileStream(opendlg.FileName, FileMode.Open))
                                {
                                    var temp = (ObservableCollection<vMixControlButtonCommand>)s.Deserialize(fs);
                                    Commands.Clear();
                                    foreach (var item in temp)
                                    {
                                        Commands.Add(item);
                                    }
                                }
                                RearrangeCommnads();
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }));
            }
        }

        private RelayCommand _clearScriptCommand;

        /// <summary>
        /// Gets the ClearScriptCommand.
        /// </summary>
        public RelayCommand ClearScriptCommand
        {
            get
            {
                return _clearScriptCommand
                    ?? (_clearScriptCommand = new RelayCommand(
                    () =>
                    {
                        Commands.Clear();
                    }));
            }
        }

        private RelayCommand<vMixControlButtonCommand> _moveCommandUpCommand;

        /// <summary>
        /// Gets the MoveCommandUpCommand.
        /// </summary>
        public RelayCommand<vMixControlButtonCommand> MoveCommandUpCommand
        {
            get
            {
                return _moveCommandUpCommand
                    ?? (_moveCommandUpCommand = new RelayCommand<vMixControlButtonCommand>(
                    p =>
                    {
                        var idx = Commands.IndexOf(p);
                        Commands.Move(idx, idx - 1 >= 0 ? idx - 1 : idx);
                        CollectionViewSource.GetDefaultView(script.ItemsSource)?.Refresh();
                        RearrangeCommnads();
                    }));
            }
        }

        private RelayCommand<vMixControlButtonCommand> _moveCommandDownCommand;

        public event PropertyChangedEventHandler PropertyChanged;
        internal void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        /// <summary>
        /// Gets the MoveCommandDownCommand.
        /// </summary>
        public RelayCommand<vMixControlButtonCommand> MoveCommandDownCommand
        {
            get
            {
                return _moveCommandDownCommand
                    ?? (_moveCommandDownCommand = new RelayCommand<vMixControlButtonCommand>(
                    p =>
                    {
                        var idx = Commands.IndexOf(p);
                        Commands.Move(idx, idx + 1 < Commands.Count ? idx + 1 : idx);
                        RearrangeCommnads();
                    }));
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as TabControl).SelectedIndex == 1)
            {
                GenerateCode();
            }
        }

        private void BindableAvalonEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TextCode))
            {
                var code = TextCode.Split('\r', '\n').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                Commands.Clear();
                foreach (var line in code)
                    Commands.Add(vMixControlButtonCommand.FromString(line));
                //_prevIndex = 0;
            }
        }

        private void BindableAvalonEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            //_prevIndex++;
        }
    }
}
