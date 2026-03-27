using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;
using vMixController.Classes.Scripting;
using vMixController.Converters;
using vMixController.Interfaces;
using vMixController.ViewModel;
using vMixController.Widgets;

namespace vMixController.PropertiesControls
{
    /// <summary>
    /// ������ �������������� ��� ScriptControl.xaml
    /// </summary>
    public partial class ScriptControl : UserControl, ICancellable
    {
        private State _inputsState;

        public ScriptControl()
        {
            InitializeComponent();
            Loaded += ScriptControl_Loaded;
            Unloaded += ScriptControl_Unloaded;

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                Commands = new ObservableCollection<vMixControlButtonCommand>();
                Commands.CollectionChanged += OnCommandsChanged;
                Commands.Add(new vMixControlButtonCommand());
            }


        }

        public static readonly DependencyProperty AvailableInputsProperty =
            DependencyProperty.Register(nameof(AvailableInputs), typeof(ObservableCollection<Input>), typeof(ScriptControl), new PropertyMetadata(new ObservableCollection<Input>()));

        public ObservableCollection<Input> AvailableInputs
        {
            get { return (ObservableCollection<Input>)GetValue(AvailableInputsProperty); }
            set { SetValue(AvailableInputsProperty, value); }
        }

        private void ScriptControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshInputsSource();
        }

        private void ScriptControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_inputsState != null)
                _inputsState.OnStateSynced -= InputsState_OnStateSynced;
            _inputsState = null;
        }

        private void InputsState_OnStateSynced(object sender, StateSyncedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(RefreshInputsSource));
        }

        private void RefreshInputsSource()
        {
            var widgetSettings = AppServices.IsRegistered<vMixWidgetSettingsViewModel>()
                ? AppServices.GetRequiredService<vMixWidgetSettingsViewModel>()
                : null;
            var model = widgetSettings?.Widget?.State
                ?? widgetSettings?.Model
                ?? (AppServices.IsRegistered<MainViewModel>() ? AppServices.GetRequiredService<MainViewModel>().Model : null);

            if (!ReferenceEquals(_inputsState, model))
            {
                if (_inputsState != null)
                    _inputsState.OnStateSynced -= InputsState_OnStateSynced;
                _inputsState = model;
                if (_inputsState != null)
                    _inputsState.OnStateSynced += InputsState_OnStateSynced;
            }

            var merged = new ObservableCollection<Input>(model?.Inputs ?? new List<Input>());
            var vars = widgetSettings?.GlobalVariables;
            if (vars != null)
            {
                var converted = VariableListToInputListConverter.Instance.Convert(vars, typeof(List<SampleInput>), null, System.Globalization.CultureInfo.InvariantCulture) as IEnumerable<SampleInput>;
                if (converted != null)
                {
                    foreach (var item in converted)
                        merged.Add(new Input
                        {
                            Key = item.Key,
                            Title = item.Title,
                            Number = item.Number,
                            Elements = item.Elements
                        });
                }
            }
            AvailableInputs = merged;
        }

        private void OnCommandsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (vMixControlButtonCommand cmd in e.NewItems)
                {
                    cmd.PropertyChanged += OnCommandPropertyChanged;
                    foreach (One<string> par in cmd.AdditionalParameters)
                        par.PropertyChanged += OnAdditionalParameterChanged;
                }
            if (e.OldItems != null)
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    foreach (vMixControlButtonCommand cmd in e.OldItems)
                    {
                        cmd.PropertyChanged -= OnCommandPropertyChanged;
                        foreach (One<string> par in cmd.AdditionalParameters)
                            par.PropertyChanged -= OnAdditionalParameterChanged;
                    }
            RearrangeCommnads();
            GenerateCode();
        }

        private void OnAdditionalParameterChanged(object sender, PropertyChangedEventArgs e)
        {
            GenerateCode();
        }

        private void OnCommandPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GenerateCode();
        }

        private void GenerateCode()
        {
            if (Commands.Count > 0)
                TextCode = Commands.Select(x => x.ToString()).Aggregate((x, y) => x + "\r\n" + y);
            else
                TextCode = "";

            Code.Document.Text = TextCode;
        }

        private void RearrangeCommnads()
        {
            var ident = 0;
            foreach (var icmd in Commands)
            {
                if (icmd == null) continue;
                icmd.PropertyChanged -= Icmd_PropertyChanged;
                icmd.PropertyChanged += Icmd_PropertyChanged;
                IsInputExist(icmd);
                if ((icmd?.Action.IsBlock).GetValueOrDefault(false))
                {
                    icmd.Ident = new Thickness(ident, 0, 0, 0);
                    ident += 8;
                    continue;
                }
                if (icmd?.Action.Function == NativeFunctions.CONDITIONEND)
                    ident -= 8;

                if (ident < 0) ident = 0;
                icmd.Ident = new Thickness(ident, 0, 0, 0);

            }
        }

        private void Icmd_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(vMixControlButtonCommand.InputKey))
            {
                var s = (sender as vMixControlButtonCommand);
                IsInputExist(s);
            }
        }

        private void IsInputExist(vMixControlButtonCommand s)
        {
            var key = Utils.FindInputKeyByVariable(s.InputKey, Dispatcher);
            IEnumerable<vMixAPI.Input> inputs = AvailableInputs;
            var check = inputs?.Where(x =>
            {
                int number;
                return x.Key == key || x.Title == key || (int.TryParse(key, out number) && x.Number == number);
            }).Count() == 0;
            s.NoInputAssigned = check;
        }


        private void ShowMovedItem(int moveTo)
        {
            script.UpdateLayout();
            var item = script.ItemContainerGenerator.ContainerFromIndex(moveTo) as ListViewItem;
            if (item != null)
            {

                var border = item.Template.FindName("border", item);
                if (border != null)
                    ((Storyboard)FindResource("Blink")).Begin((FrameworkElement)border);
                item.BringIntoView();
            }
        }

        public static readonly DependencyProperty TextCodePropertyName =
             DependencyProperty.Register(nameof(TextCode), typeof(string), typeof(ScriptControl), new PropertyMetadata(""));

        public string TextCode
        {
            get { return (string)GetValue(TextCodePropertyName); }
            set { SetValue(TextCodePropertyName, value); }
        }

        public static readonly DependencyProperty IsCancelledPropertyName =
             DependencyProperty.Register(nameof(IsCancelled), typeof(bool), typeof(ScriptControl), new PropertyMetadata(false));

        public bool IsCancelled
        {
            get { return (bool)GetValue(IsCancelledPropertyName); }
            set { SetValue(IsCancelledPropertyName, value); }
        }

        public static readonly DependencyProperty LogProperty =
            DependencyProperty.Register(
                nameof(Log),
                typeof(string),
                typeof(ScriptControl),
                new PropertyMetadata(""));

        public string Log
        {
            get { return (string)GetValue(LogProperty); }
            set { SetValue(LogProperty, value); }
        }

        public static readonly DependencyProperty CommandsProperty =
            DependencyProperty.Register(
                nameof(Commands),
                typeof(ObservableCollection<vMixControlButtonCommand>),
                typeof(ScriptControl),
                new PropertyMetadata(null, CommandsChangedCallback));

        private static void CommandsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (ScriptControl)d;
            var ov = (ObservableCollection<vMixControlButtonCommand>)e.OldValue;
            var nv = (ObservableCollection<vMixControlButtonCommand>)e.NewValue;
            ctrl.GenerateCode();
            if (ov != null)
                ov.CollectionChanged -= ((ScriptControl)d).OnCommandsChanged;
            if (nv != null)
                nv.CollectionChanged += ((ScriptControl)d).OnCommandsChanged;
        }

        public ObservableCollection<vMixControlButtonCommand> Commands
        {
            get { return (ObservableCollection<vMixControlButtonCommand>)GetValue(CommandsProperty); }
            set { SetValue(CommandsProperty, value); }
        }

        [RelayCommand]
        private void RemoveCommand(vMixControlButtonCommand p)
        {
            Commands.Remove(p);
            RearrangeCommnads();
        }

        [RelayCommand]
        private void DuplicateCommand(vMixControlButtonCommand p)
        {
            var idx = Commands.IndexOf(p);
            var copy = (vMixControlButtonCommand)p.Clone();
            Commands.Insert(idx, copy);
            CollectionViewSource.GetDefaultView(script.ItemsSource)?.Refresh();
            RearrangeCommnads();
            ShowMovedItem(idx + 1);
        }

        [RelayCommand]
        private void AddCommand()
        {
            var cmd = new vMixControlButtonCommand() { Action = new vMixFunctionReference() };
            for (int i = 0; i < 10; i++)
                cmd.AdditionalParameters.Add(new One<string>() { A = "" });
            Commands.Add(cmd);
            var index = Math.Max(Commands.Count - 2, 0);
            RearrangeCommnads();
            bottomMarker.BringIntoView();
        }

        [RelayCommand]
        private void ExportScript()
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
        }

        [RelayCommand]
        private void ImportScript()
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
                catch (Exception)
                {
                }
            }
        }

        [RelayCommand]
        private void ClearScript()
        {
            Commands.Clear();
        }

        [RelayCommand]
        private void MoveCommandUp(vMixControlButtonCommand p)
        {
            var idx = Commands.IndexOf(p);
            var moveTo = idx - 1 >= 0 ? idx - 1 : idx;
            Commands.Move(idx, moveTo);
            CollectionViewSource.GetDefaultView(script.ItemsSource)?.Refresh();
            RearrangeCommnads();
            ShowMovedItem(moveTo);
        }

        [RelayCommand]
        private void MoveCommandDown(vMixControlButtonCommand p)
        {
            var idx = Commands.IndexOf(p);
            var moveTo = idx + 1 < Commands.Count ? idx + 1 : idx;
            Commands.Move(idx, moveTo);
            RearrangeCommnads();
            ShowMovedItem(moveTo);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as TabControl).SelectedIndex == 1)
            {
                Code.Select(0, 0);
            }
        }

        private void BindableAvalonEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            TextCode = Code.Document.Text;
            if (!string.IsNullOrWhiteSpace(TextCode))
            {
                var code = TextCode.Split('\r', '\n').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                Commands.Clear();
                foreach (var line in code)
                    Commands.Add(vMixControlButtonCommand.FromString(line));
            }
        }

        private void BindableAvalonEditor_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void Func_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Keyboard.ClearFocus();
                var parent = ((ComboBox)sender).Parent;

                while (parent is FrameworkElement && ((FrameworkElement)parent).Parent != null && !(parent is Grid))
                    parent = ((FrameworkElement)parent).Parent;
                while (parent is FrameworkElement && VisualTreeHelper.GetParent(parent) != null && !(parent is Grid))
                    parent = VisualTreeHelper.GetParent(parent);

                FocusManager.SetFocusedElement(parent, (IInputElement)parent);
                ((FrameworkElement)parent).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var value = (int?)(sender as MenuItem).DataContext.GetType().GetProperty("Index")?.GetValue((sender as MenuItem).DataContext);
            if (value.HasValue)
                ((sender as MenuItem).Tag as vMixControlButtonCommand).Parameter = value.Value.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.Tag = (sender as Button).Tag;
            (sender as Button).ContextMenu.DataContext = (sender as Button).DataContext;
            if ((sender as Button).ContextMenu.HasItems)
                (sender as Button).ContextMenu.IsOpen = true;
        }

        private void Button_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!(sender as Button).ContextMenu.HasItems)
            {
                (sender as Button).ContextMenu.IsOpen = false;
                e.Handled = true;
            }
        }
    }
}
