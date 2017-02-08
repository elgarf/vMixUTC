using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using vMixController.Classes;
using vMixController.Extensions;
using vMixController.Widgets;

namespace vMixController.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        vMixControlSettingsView _settings = null;// new vMixControlSettingsView();
        NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The <see cref="Model" /> property's name.
        /// </summary>
        public const string ModelPropertyName = "Model";

        private vMixAPI.State _model = null;

        /// <summary>
        /// Sets and gets the Model property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public vMixAPI.State Model
        {
            get
            {
                return _model;
            }

            set
            {
                if (_model == value)
                {
                    return;
                }

                _model = value;


                _logger.Info("New model setted.");
                /*if (_model != null)
                    Status = "Online";
                else
                    Status = "Offline";*/

                RaisePropertyChanged(ModelPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="WindowSettings" /> property's name.
        /// </summary>
        public const string WindowSettingsPropertyName = "WindowSettings";

        private MainWindowSettings _windowSettings = null;

        /// <summary>
        /// Sets and gets the WindowSettings property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public MainWindowSettings WindowSettings
        {
            get
            {
                return _windowSettings;
            }

            set
            {
                if (_windowSettings == value)
                {
                    return;
                }
                if (_windowSettings != null)
                    _windowSettings.PropertyChanged -= _windowSettings_PropertyChanged;
                _windowSettings = value;


                if (_windowSettings.EnableLog)
                {
                    if (!NLog.LogManager.IsLoggingEnabled())
                        NLog.LogManager.EnableLogging();
                }
                else
                {
                    if (NLog.LogManager.IsLoggingEnabled())
                        NLog.LogManager.DisableLogging();
                }

                _windowSettings.PropertyChanged += _windowSettings_PropertyChanged;
                RaisePropertyChanged(WindowSettingsPropertyName);
            }
        }

        private void _windowSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem((x) =>
            {
                if (e.PropertyName == "IP" || e.PropertyName == "Port")
                    _connectTimer_Tick(null, new EventArgs());
                //vMixAPI.StateFabrique.Configure(WindowSettings.IP, WindowSettings.Port);
            });

        }

        /// <summary>
        /// The <see cref="Status" /> property's name.
        /// </summary>
        public const string StatusPropertyName = "Status";

        private string _status = "Offline";

        /// <summary>
        /// Sets and gets the Status property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                if (_status == value)
                {
                    return;
                }

                _status = value;

                _logger.Info("Status changed to {0}.", value);

                RaisePropertyChanged(StatusPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ControlTemplates" /> property's name.
        /// </summary>
        public const string ControlTemplatesPropertyName = "ControlTemplates";

        private ObservableCollection<Pair<string, vMixControl>> _controlTemplates = new ObservableCollection<Pair<string, vMixControl>>();

        /// <summary>
        /// Sets and gets the ControlTemplates property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Pair<string, vMixControl>> ControlTemplates
        {
            get
            {
                return _controlTemplates;
            }

            set
            {
                if (_controlTemplates == value)
                {
                    return;
                }

                _controlTemplates = value;
                RaisePropertyChanged(ControlTemplatesPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Functions" /> property's name.
        /// </summary>
        public const string FunctionsPropertyName = "Functions";

        private ObservableCollection<vMixFunctionReference> _functions = null;

        /// <summary>
        /// Sets and gets the Functions property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<vMixFunctionReference> Functions
        {
            get
            {
                return _functions;
            }

            set
            {
                if (_functions == value)
                {
                    return;
                }

                _functions = value;
                RaisePropertyChanged(FunctionsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Controls" /> property's name.
        /// </summary>
        public const string ControlsPropertyName = "Controls";

        private ObservableCollection<vMixController.Widgets.vMixControl> _controls = new ObservableCollection<vMixController.Widgets.vMixControl>();

        /// <summary>
        /// Sets and gets the Controls property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<vMixController.Widgets.vMixControl> Controls
        {
            get
            {
                return _controls;
            }

            set
            {
                if (_controls == value)
                {
                    return;
                }

                _controls = value;
                RaisePropertyChanged(ControlsPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="EditorCursor" /> property's name.
        /// </summary>
        public const string CursorPropertyName = "EditorCursor";

        private string _editorCursor = "Arrow";

        /// <summary>
        /// Sets and gets the Cursor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string EditorCursor
        {
            get
            {
                return _editorCursor;
            }

            set
            {
                if (_editorCursor == value)
                {
                    return;
                }

                _editorCursor = value;
                RaisePropertyChanged(CursorPropertyName);
            }
        }

        private RelayCommand<vMixController.Widgets.vMixControl> _switchLockCommand;

        /// <summary>
        /// Gets the SwitchLockCommand.
        /// </summary>
        public RelayCommand<vMixController.Widgets.vMixControl> SwitchLockCommand
        {
            get
            {
                return _switchLockCommand
                    ?? (_switchLockCommand = new RelayCommand<vMixController.Widgets.vMixControl>(
                    p =>
                    {
                        p.Locked = !p.Locked;
                    }));
            }
        }

        private RelayCommand<vMixController.Widgets.vMixControl> _removeControlCommand;

        /// <summary>
        /// Gets the RemoveControlCommand.
        /// </summary>
        public RelayCommand<vMixController.Widgets.vMixControl> RemoveControlCommand
        {
            get
            {
                return _removeControlCommand
                    ?? (_removeControlCommand = new RelayCommand<vMixController.Widgets.vMixControl>(
                    p =>
                    {
                        p.Dispose();
                        Controls.Remove(p);
                    }));
            }
        }

        private RelayCommand<vMixControl> _copyControlCommand;

        /// <summary>
        /// Gets the CopyControlCommand.
        /// </summary>
        public RelayCommand<vMixControl> CopyControlCommand
        {
            get
            {
                return _copyControlCommand
                    ?? (_copyControlCommand = new RelayCommand<vMixControl>(
                    p =>
                    {
                        var copy = p.Copy();
                        copy.Name += " copy";
                        copy.State = Model;
                        copy.Left += 8;
                        copy.Top += 8;
                        _controls.Add(copy);
                        UpdateWithLicense();
                    }));
            }
        }


        private RelayCommand<vMixController.Widgets.vMixControl> _openPropertiesCommand;

        /// <summary>
        /// Gets the OpenPropertiesCommand.
        /// </summary>
        public RelayCommand<vMixController.Widgets.vMixControl> OpenPropertiesCommand
        {
            get
            {
                return _openPropertiesCommand
                    ?? (_openPropertiesCommand = new RelayCommand<vMixController.Widgets.vMixControl>(
                    p =>
                    {
                        _logger.Info("Opening properties for widget {0}.", p.Name);
                        var viewModel = ServiceLocator.Current.GetInstance<vMixController.ViewModel.vMixControlSettingsViewModel>();
                        viewModel.Control = p;
                        viewModel.SetProperties(p);

                        _settings = new vMixControlSettingsView();
                        _settings.Owner = App.Current.MainWindow;
                        var result = _settings.ShowDialog();
                        if (result.HasValue && result.Value)
                            p.SetProperties(viewModel);
                        _logger.Info("Properties updated.");
                        _settings = null;
                    }));
            }
        }

        Action<Point> _createControl;

        private RelayCommand<string> _createControlCommand;

        /// <summary>
        /// Gets the CreateControlCommand.
        /// </summary>
        public RelayCommand<string> CreateControlCommand
        {
            get
            {
                return _createControlCommand
                    ?? (_createControlCommand = new RelayCommand<string>(
                    p =>
                    {
                        EditorCursor = CursorType.Hand.ToString();
                        _createControl = new Action<Point>(x =>
                        {
                            var control = (vMixControl)Assembly.GetAssembly(this.GetType()).CreateInstance("vMixController.Widgets.vMixControl" + p);
                            control.State = Model;
                            control.Top = x.Y;
                            control.Left = x.X;
                            control.AlignByGrid();
                            _controls.Add(control);
                            _logger.Info("New {0} widget added.", control.Type.ToLower());
                            if (!UpdateWithLicense(control))
                                OpenPropertiesCommand.Execute(control);
                        });

                    }));
            }
        }


        private RelayCommand<MouseButtonEventArgs> _mouseButtonUp;

        /// <summary>
        /// Gets the MouseButtonUp.
        /// </summary>
        public RelayCommand<MouseButtonEventArgs> MouseButtonUp
        {
            get
            {
                return _mouseButtonUp
                    ?? (_mouseButtonUp = new RelayCommand<MouseButtonEventArgs>(
                    p =>
                    {
                        if (_createControl != null)
                        {
                            EditorCursor = "Arrow";
                            var pos = p.MouseDevice.GetPosition((IInputElement)p.Source);
                            _createControl(new Point(pos.X / WindowSettings.UIScale, pos.Y / WindowSettings.UIScale));
                            _createControl = null;
                            UpdateWithLicense();
                        }
                    }));
            }
        }

        bool _fromContextMenu = false;
        Point _contextMenuPosition;
        private RelayCommand<ContextMenuEventArgs> _contextMenuOpening;

        /// <summary>
        /// Gets the ContextMenuOpened.
        /// </summary>
        public RelayCommand<ContextMenuEventArgs> ContextMenuOpening
        {
            get
            {
                return _contextMenuOpening
                    ?? (_contextMenuOpening = new RelayCommand<ContextMenuEventArgs>(
                    p =>
                    {
                        _fromContextMenu = true;
                        _contextMenuPosition = Mouse.GetPosition((IInputElement)p.Source);
                    }));
            }
        }

        private RelayCommand<ContextMenuEventArgs> _contextMenuClosing;

        /// <summary>
        /// Gets the ContextMenuClosing.
        /// </summary>
        public RelayCommand<ContextMenuEventArgs> ContextMenuClosing
        {
            get
            {
                return _contextMenuClosing
                    ?? (_contextMenuClosing = new RelayCommand<ContextMenuEventArgs>(
                    p =>
                    {
                        if (_createControl != null && _fromContextMenu)
                        {
                            EditorCursor = "Arrow";
                            var pos = _contextMenuPosition;
                            _createControl(new Point(pos.X / WindowSettings.UIScale, pos.Y / WindowSettings.UIScale));
                            _createControl = null;
                            _fromContextMenu = false;
                            UpdateWithLicense();
                        }
                    }));
            }
        }

        private RelayCommand<Pair<string, vMixControl>> _createControlFromTemplateCommand;

        /// <summary>
        /// Gets the CreateControlFromTemplateCommand.
        /// </summary>
        public RelayCommand<Pair<string, vMixControl>> CreateControlFromTemplateCommand
        {
            get
            {
                return _createControlFromTemplateCommand
                    ?? (_createControlFromTemplateCommand = new RelayCommand<Pair<string, vMixControl>>(
                    p =>
                    {
                        EditorCursor = "Hand";
                        _createControl = x =>
                        {
                            var ctrl = p.B.Copy();
                            ctrl.Left = x.X;
                            ctrl.Top = x.Y;
                            ctrl.IsTemplate = false;
                            ctrl.State = Model;
                            ctrl.AlignByGrid();
                            _logger.Info("Widget \"{0}\" was copied.", p.B.Name);
                            _controls.Add(ctrl);
                            UpdateWithLicense();
                        };

                    }));
            }
        }

        private RelayCommand<Pair<string, vMixControl>> _removeControlTemplateCommand;

        /// <summary>
        /// Gets the RemoveControlTemplateCommand.
        /// </summary>
        public RelayCommand<Pair<string, vMixControl>> RemoveControlTemplateCommand
        {
            get
            {
                return _removeControlTemplateCommand
                    ?? (_removeControlTemplateCommand = new RelayCommand<Pair<string, vMixControl>>(
                    p =>
                    {
                        _controlTemplates.Remove(p);
                    }));
            }
        }

        private RelayCommand<Pair<string, vMixControl>> _editControlTemplateCommand;

        /// <summary>
        /// Gets the EditControlTemplateCommand.
        /// </summary>
        public RelayCommand<Pair<string, vMixControl>> EditControlTemplateCommand
        {
            get
            {
                return _editControlTemplateCommand
                    ?? (_editControlTemplateCommand = new RelayCommand<Pair<string, vMixControl>>(
                    p =>
                    {
                        OpenPropertiesCommand.Execute(p.B);
                    }));
            }
        }

        private RelayCommand _newControllerCommand;

        /// <summary>
        /// Gets the NewControllerCommand.
        /// </summary>
        public RelayCommand NewControllerCommand
        {
            get
            {
                return _newControllerCommand
                    ?? (_newControllerCommand = new RelayCommand(
                    () =>
                    {
                        foreach (var item in _controls)
                            item.Dispose();
                        _controls.Clear();
                    }));
            }
        }

        private RelayCommand _loadControllerCommand;

        /// <summary>
        /// Gets the LoadControllerCommand.
        /// </summary>
        public RelayCommand LoadControllerCommand
        {
            get
            {
                return _loadControllerCommand
                    ?? (_loadControllerCommand = new RelayCommand(
                    () =>
                    {

                        Ookii.Dialogs.Wpf.VistaOpenFileDialog opendlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
                        opendlg.Filter = "vMix Controller|*.vmc";
                        var result = opendlg.ShowDialog(App.Current.MainWindow);
                        if (result.HasValue && result.Value)
                        {
                            /*using (var stream = File.OpenRead(opendlg.FileName))
                            {
                                _logger.Info("Controller loading.");
                                var reader = XmlReader.Create(stream);
                                {
                                    reader.ReadStartElement();
                                    reader.ReadStartElement();
                                    _logger.Info("Loading widgets.");
                                    XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<vMixControl>));
                                    var collection = (ObservableCollection<vMixControl>)s.Deserialize(reader);
                                    foreach (var item in collection)
                                        _controls.Add(item);
                                    reader.ReadEndElement();
                                    reader.ReadStartElement();

                                    _logger.Info("Loading window settings.");
                                    s = new XmlSerializer(typeof(MainWindowSettings));

                                    var state = WindowSettings.State;
                                    var left = WindowSettings.Left;
                                    var top = WindowSettings.Top;
                                    var width = WindowSettings.Width;
                                    var height = WindowSettings.Height;

                                    WindowSettings = (MainWindowSettings)s.Deserialize(reader);

                                    reader.ReadEndElement();
                                    reader.ReadEndElement();

                                    WindowSettings.Height = height;
                                    WindowSettings.Width = width;
                                    WindowSettings.Left = left;
                                    WindowSettings.Top = top;
                                    WindowSettings.State = state;

                                    _logger.Info("Configuring API.");

                                    _connectTimer_Tick(null, new EventArgs());

                                    vMixAPI.State.Configure(WindowSettings.IP, WindowSettings.Port);
                                    UpdatevMixState();
                                }

                            }*/
                            var state = WindowSettings.State;
                            var left = WindowSettings.Left;
                            var top = WindowSettings.Top;
                            var width = WindowSettings.Width;
                            var height = WindowSettings.Height;
                            foreach (var item in _controls)
                                item.Dispose();
                            _controls.Clear();

                            foreach (var item in Utils.LoadController(opendlg.FileName, Functions, out _windowSettings))
                                _controls.Add(item);

                            foreach (var item in _controls)
                                item.Update();

                            RaisePropertyChanged("WindowSettings");
                            _logger.Info("Configuring API.");

                            _connectTimer_Tick(null, new EventArgs());

                            vMixAPI.StateFabrique.Configure(WindowSettings.IP, WindowSettings.Port);
                            UpdatevMixState();
                            UpdateWithLicense();

                        }
                    }));
            }
        }

        private RelayCommand _saveControllerCommand;

        /// <summary>
        /// Gets the SaveControllerCommand.
        /// </summary>
        public RelayCommand SaveControllerCommand
        {
            get
            {
                return _saveControllerCommand
                    ?? (_saveControllerCommand = new RelayCommand(
                    () =>
                    {
                        Ookii.Dialogs.Wpf.VistaSaveFileDialog opendlg = new Ookii.Dialogs.Wpf.VistaSaveFileDialog();
                        opendlg.Filter = "vMix Controller|*.vmc";
                        opendlg.DefaultExt = "vmc";
                        var result = opendlg.ShowDialog(App.Current.MainWindow);
                        if (result.HasValue && result.Value)
                            Utils.SaveController(opendlg.FileName, _controls, _windowSettings);

                    }));
            }
        }

        private RelayCommand _toggleLockCommand;

        /// <summary>
        /// Gets the ToggleLockCommand.
        /// </summary>
        public RelayCommand ToggleLockCommand
        {
            get
            {
                return _toggleLockCommand
                    ?? (_toggleLockCommand = new RelayCommand(
                    () =>
                    {
                        WindowSettings.Locked = !WindowSettings.Locked;
                        foreach (var item in _controls)
                        {
                            item.Locked = WindowSettings.Locked;
                        }
                    }));
            }
        }

        private RelayCommand _updateStateCommand;

        /// <summary>
        /// Gets the UpdateStateCommand.
        /// </summary>
        public RelayCommand UpdateStateCommand
        {
            get
            {
                return _updateStateCommand
                    ?? (_updateStateCommand = new RelayCommand(
                    () =>
                    {
                        UpdatevMixState();
                    }));
            }
        }

        private RelayCommand _exitCommand;

        /// <summary>
        /// Gets the ExitCommand.
        /// </summary>
        public RelayCommand ExitCommand
        {
            get
            {
                return _exitCommand
                    ?? (_exitCommand = new RelayCommand(
                    () =>
                    {
                        App.Current.MainWindow.Close();
                    }));
            }
        }

        private void UpdatevMixState()
        {
            _logger.Info("Updating state.");
            //vMixAPI.StateFabrique.Configure(WindowSettings.IP, WindowSettings.Port);
            //if (Status != "Offline")
            {
                if (Model == null || (Model.Ip != WindowSettings.IP || Model.Port != WindowSettings.Port))
                {
                    Model = null;
                    vMixAPI.StateFabrique.CreateAsync();
                }
                else
                    Model.UpdateAsync();
                /*if (!Model.Update())
                {
                    Model = null;
                    State_OnStateCreated(null, null);
                    Status = "Offline";
                }*/
            }
        }

        private void State_OnStateCreated(object sender, EventArgs e)
        {
            if (Model != null)
                Model.OnStateUpdated -= Model_OnStateUpdated;
            Model = (vMixAPI.State)sender;
            foreach (var item in _controls)
                item.State = Model;
            if (Model != null)
                Model.OnStateUpdated += Model_OnStateUpdated;
            _connectTimer_Tick(null, new EventArgs());
        }

        private void Model_OnStateUpdated(object sender, vMixAPI.StateUpdatedEventArgs e)
        {
            if (!e.Successfully)
            {
                Model = null;
                State_OnStateCreated(null, null);
                Status = "Offline";
            }
            else
            {
                foreach (var item in _controls)
                    item.State = (vMixAPI.State)sender;

                /*_logger.Info("Remapping inputs.");
                Dictionary<int, int> map = new Dictionary<int, int>();
                if (e.OldInputs != null && e.NewInputs != null)
                {
                    for (int i = 0; i < e.OldInputs.Length; i++)
                    {
                        var element = e.NewInputs.Select((x, j) => new { obj = x, idx = j }).Where(x => x.obj == e.OldInputs[i]).FirstOrDefault();
                        if (element != null)
                            map.Add(i, element.idx);
                    }
                    foreach (var item in _controls.OfType<vMixControlTextField>())
                        foreach (var path in item.Paths)
                            if (map.ContainsKey(path.A))
                            {
                                var b = path.B;
                                path.A = map[path.A];
                                path.B = b;
                            }
                    foreach (var item in _controls.OfType<vMixControlButton>())
                        foreach (var cmd in item.Commands)
                            if (map.ContainsKey(cmd.Input))
                                cmd.Input = map[cmd.Input];
                    foreach (var item in _controls)
                        item.Update();
                }*/
            }
        }


        void ProcessHotkey(Key key, Key systemKey, ModifierKeys modifiers)
        {
            foreach (var ctrl in _controls)
            {
                foreach (var item in ctrl.Hotkey.Select((x, i) => new { obj = x, idx = i }))
                {
                    ModifierKeys mod = ModifierKeys.None;
                    if (item.obj.Alt)
                        mod |= ModifierKeys.Alt;
                    if (item.obj.Ctrl)
                        mod |= ModifierKeys.Control;
                    if (item.obj.Shift)
                        mod |= ModifierKeys.Shift;

                    if (item.obj.Active && ((item.obj.Key == key) || (key == Key.System && item.obj.Key == systemKey)) && modifiers == mod)
                        ctrl.ExecuteHotkey(item.idx);
                }
            }
        }

        void ProcessHotkey(string link)
        {
            foreach (var ctrl in _controls)
                foreach (var item in ctrl.Hotkey.Select((x, i) => new { obj = x, idx = i }))
                    if (item.obj.Link == link)
                        ctrl.ExecuteHotkey(item.idx);
        }

        private RelayCommand<KeyEventArgs> _previewKeyUpCommand;

        /// <summary>
        /// Gets the PreviewKeyUpCommand.
        /// </summary>
        public RelayCommand<KeyEventArgs> PreviewKeyUpCommand
        {
            get
            {
                return _previewKeyUpCommand
                    ?? (_previewKeyUpCommand = new RelayCommand<KeyEventArgs>(
                    p =>
                    {
                        ProcessHotkey(p.Key, p.SystemKey, p.KeyboardDevice.Modifiers);
                        /*foreach (var ctrl in _controls)
                        {
                            foreach (var item in ctrl.Hotkey.Select((x, i) => new { obj = x, idx = i }))
                            {
                                ModifierKeys mod = ModifierKeys.None;
                                if (item.obj.Alt)
                                    mod |= ModifierKeys.Alt;
                                if (item.obj.Ctrl)
                                    mod |= ModifierKeys.Control;
                                if (item.obj.Shift)
                                    mod |= ModifierKeys.Shift;

                                if (item.obj.Active && ((item.obj.Key == p.Key) || (p.Key == Key.System && item.obj.Key == p.SystemKey)) && p.KeyboardDevice.Modifiers == mod)
                                    ctrl.ExecuteHotkey(item.idx);
                            }

                        }*/
                    }));
            }
        }

        private RelayCommand _closingCommand;

        /// <summary>
        /// Gets the ClosingCommand.
        /// </summary>
        public RelayCommand ClosingCommand
        {
            get
            {
                return _closingCommand
                    ?? (_closingCommand = new RelayCommand(
                    () =>
                    {
                        _logger.Info("Saving templates.");
                        XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<Pair<string, vMixControl>>));
                        using (var fs = new FileStream(Path.Combine(_documentsPath, "Templates.xml"), FileMode.Create))
                            s.Serialize(fs, _controlTemplates);

                        _logger.Info("Saving window settings.");
                        s = new XmlSerializer(typeof(MainWindowSettings));
                        using (var fs = new FileStream(Path.Combine(_documentsPath, "WindowSettings.xml"), FileMode.Create))
                            s.Serialize(fs, _windowSettings);
                    }));
            }
        }

        DispatcherTimer _connectTimer = new DispatcherTimer();

        string _documentsPath;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            //Default config
            //vMixAPI.State.Configure();

            vMixAPI.StateFabrique.OnStateCreated += State_OnStateCreated;

            //for test only
            //Model = vMixAPI.State.Create(File.ReadAllText("testState.txt"));

            _connectTimer.Interval = TimeSpan.FromSeconds(20);
            _connectTimer.Tick += _connectTimer_Tick;
            _connectTimer.Start();

            _logger.Info("Loading mapped functions.");
            XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<vMixFunctionReference>));
            try
            {
                if (File.Exists("Functions.xml"))
                    using (var fs = new FileStream("Functions.xml", FileMode.Open))
                        Functions = (ObservableCollection<vMixFunctionReference>)s.Deserialize(fs);

                /*Functions[0].IntValues = new List<int> { 1, 2, 3, 4 };
                using (var fs = new FileStream("Functions1.xml", FileMode.Create))
                    s.Serialize(fs, Functions);*/
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while loading mapped functions.");
            }
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _documentsPath = Path.Combine(documents, "vMix UTC");
            if (!Directory.Exists(_documentsPath))
                Directory.CreateDirectory(_documentsPath);

            try
            {
                _logger.Info("Loading templates.");
                s = new XmlSerializer(typeof(ObservableCollection<Pair<string, vMixControl>>));
                if (File.Exists(Path.Combine(_documentsPath, "Templates.xml")))
                    using (var fs = new FileStream(Path.Combine(_documentsPath, "Templates.xml"), FileMode.Open))
                        ControlTemplates = (ObservableCollection<Pair<string, vMixControl>>)s.Deserialize(fs);
            }
            catch (Exception)
            {

            }

            try
            {
                _logger.Info("Loading window settings.");
                s = new XmlSerializer(typeof(MainWindowSettings));
                if (File.Exists(Path.Combine(_documentsPath, "WindowSettings.xml")))
                    using (var fs = new FileStream(Path.Combine(_documentsPath, "WindowSettings.xml"), FileMode.Open))
                        WindowSettings = (MainWindowSettings)s.Deserialize(fs);
                else
                    WindowSettings = new MainWindowSettings();



                var totalwidth = WpfScreenHelper.Screen.AllScreens.Select(x => x.Bounds.Width).Aggregate((x, y) => x + y);
                var totalheight = WpfScreenHelper.Screen.AllScreens.Select(x => x.Bounds.Height).Max();

                if (WindowSettings.Left > totalwidth)
                    WindowSettings.Left = 128;
                if (WindowSettings.Top > totalheight)
                    WindowSettings.Top = 128;
                if (WindowSettings.Width > WpfScreenHelper.Screen.PrimaryScreen.Bounds.Width)
                    WindowSettings.Width = WpfScreenHelper.Screen.PrimaryScreen.Bounds.Width - 32;
                if (WindowSettings.Height > WpfScreenHelper.Screen.PrimaryScreen.Bounds.Height)
                    WindowSettings.Height = WpfScreenHelper.Screen.PrimaryScreen.Bounds.Height - 32;

            }
            catch (Exception)
            {
                WindowSettings = new MainWindowSettings();
            }

            if (Model == null)
                vMixAPI.StateFabrique.CreateAsync();

            MessengerInstance.Register<string>(this, (hk) =>
            {
                /*var modkeys = hk.Split('+');
                ModifierKeys modifiers = ModifierKeys.None;
                Key key = Key.None;
                Key systemKey = Key.None;
                foreach (var item in modkeys)
                {
                    switch (item.ToUpper())
                    {
                        case "SHIFT":
                            modifiers |= ModifierKeys.Shift;
                            break;
                        case "CTRL":
                            modifiers |= ModifierKeys.Control;
                            break;
                        case "ALT":
                            modifiers |= ModifierKeys.Alt;
                            break;
                        default:
                            key = (Key)Enum.Parse(typeof(Key), item.ToUpper());
                            systemKey = key;
                            break;
                    }
                }*/
                ProcessHotkey(hk);
            });


            Singleton<SharedData>.Instance.GetData = (name, property) =>
            {
                var ds = Controls.Where(x => x.Name == name).FirstOrDefault();
                var result = new List<string>();
                if (ds == null)
                    return result;
                var prop = ds.GetType().GetProperty(property);
                if (prop.PropertyType == typeof(string))
                    result.Add((string)prop.GetValue(ds));
                if (typeof(IList<string>).IsAssignableFrom(prop.PropertyType))
                    return (IList<string>)prop.GetValue(ds);
                return result;
            };

            Singleton<SharedData>.Instance.GetDataSources = () =>
            {
                return Controls.Select(x => x.Name).ToList();
            };

            Singleton<SharedData>.Instance.GetDataSourceProps = (name) =>
            {
                var ds = Controls.Where(x => x.Name == name).FirstOrDefault();
                if (ds == null) return new List<string>();
                return ds.GetType().GetProperties().Where(x =>
                {
                    var val = x.GetValue(ds);
                    return x.PropertyType == typeof(string) || typeof(IList<string>).IsAssignableFrom(x.PropertyType);
                }).Select(x => x.Name).ToList();
            };

            Singleton<SharedData>.Instance.GetDataSource = (name) =>
            {
                var ds = Controls.Where(x => x.Name == name).FirstOrDefault();
                return ds;
            };

            /*
            uhttpsharp.HttpServer _server;
            _server = new uhttpsharp.HttpServer(new HttpRequestProvider());
            _server.Use(new TcpListenerAdapter(new System.Net.Sockets.TcpListener(IPAddress.Parse("127.0.0.1"), 11001)));
            _server.Use(new FileHandler());
            _server.Start();*/

        }

        WebClient _client = new vMixAPI.vMixWebClient();

        private void _connectTimer_Tick(object sender, EventArgs e)
        {

            _client.DownloadStringCompleted += _client_DownloadStringCompleted;
            _client.CancelAsync();
            while (_client.IsBusy) Thread.Sleep(10);
            /*try
            {*/
                _client.DownloadStringAsync(new Uri(vMixAPI.StateFabrique.GetUrl(WindowSettings.IP, WindowSettings.Port)));
            /*}
            catch (Exception)
            {

            }*/
        }

        private void _client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            _logger.Info("Checking vMix server.");
            if (e.Error != null)
            {
                _logger.Error(e.Error, "Error while connecting vMix server.");
                Status = "Offline";
                return;
            }
            //Model = null;
            if (Model != null && (Model.Ip == WindowSettings.IP && Model.Port == WindowSettings.Port))
                Status = "Online";
            else
                Status = "Update State";
            //if (Model == null)
            //Model = vMixAPI.State.Create();
        }


        private bool UpdateWithLicense(vMixControl ctrl = null)
        {
            /*var container = App.License.ReadFeature<bool>("Container");
            var externalData = App.License.ReadFeature<bool>("ExternalData");
            var removed = false;
            var limits = false;
            var limit = "";
            if (!container)
            {
                foreach (var ct in _controls.Where(x => x.GetType() == typeof(vMixControlContainer)).ToArray())
                {
                    _controls.Remove(ct);
                    if (ctrl == ct)
                        removed = true;
                    limits = true;
                    limit = LocalizationManager.Get("Container widget is not available");
                }
            }

            if (!externalData)
            {
                foreach (var ct in _controls.Where(x => x.GetType() == typeof(vMixControlExternalData)).ToArray())
                {
                    _controls.Remove(ct);
                    if (ctrl == ct)
                        removed = true;
                    limits = true;
                    limit = LocalizationManager.Get("ExternalData widget is not available");
                }
            }

            var count = App.License.ReadFeature<int>("WidgetCount");
            if (_controls.Count > count)
                limit = LocalizationManager.Get("Widget count is limited by ") + count.ToString();
            while (_controls.Count > count)
            {
                if (ctrl == _controls[_controls.Count - 1])
                    removed = true;
                _controls.RemoveAt(_controls.Count - 1);
                limits = true;
            }

            if (limits)
            {
                Ookii.Dialogs.Wpf.TaskDialog td = new Ookii.Dialogs.Wpf.TaskDialog();
                td.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Ok));
                td.MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Error;
                td.MainInstruction = LocalizationManager.Get("There is license limitation, assigned with this feature!");
                td.WindowTitle = "vMix Universal Title Controller";
                td.Content = limit;
                td.ShowDialog(App.Current.MainWindow);
            }*/
            return false;
        }

        public override void Cleanup()
        {
            // Clean up if needed
            foreach (var item in Controls.OfType<IDisposable>())
            {
                item.Dispose();
            }
            base.Cleanup();
        }
    }
}