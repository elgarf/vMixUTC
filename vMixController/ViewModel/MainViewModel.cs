using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using vMixController.Classes;
using vMixController.Widgets;

namespace vMixController.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase, IDisposable
    {
        vMixControlSettingsView _settings = null;// new vMixControlSettingsView();
        NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The <see cref="Model" /> property's name.
        /// </summary>
        public const string ModelPropertyName = "Model";



        object _moveSource = null;
        Point _clickPoint;
        Point _relativeClickPoint;
        Thickness _rawSelectorPosition = new Thickness();
        bool _isHotkeysEnabled = true;

        /// <summary>
        /// The <see cref="IsFiltersRegistered" /> property's name.
        /// </summary>
        public const string IsFiltersRegisteredPropertyName = "IsFiltersRegistered";

        private bool _isFiltersRegistered = false;

        /// <summary>
        /// Sets and gets the IsFiltersRegistered property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsFiltersRegistered
        {
            get
            {
                return _isFiltersRegistered;
            }

            set
            {
                if (_isFiltersRegistered == value)
                {
                    return;
                }

                _isFiltersRegistered = value;
                RaisePropertyChanged(IsFiltersRegisteredPropertyName);
            }
        }

        /*Selector*/
        /// <summary>
        /// The <see cref="SelectorPosition" /> property's name.
        /// </summary>
        public const string SelectorPositionPropertyName = "SelectorPosition";

        private Thickness _selectorPosition = new Thickness(0);

        /// <summary>
        /// Sets and gets the SelectorPosition property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Thickness SelectorPosition
        {
            get
            {
                return _selectorPosition;
            }

            set
            {
                if (_selectorPosition == value)
                {
                    return;
                }

                _selectorPosition = value;
                RaisePropertyChanged(SelectorPositionPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="SelectorWidth" /> property's name.
        /// </summary>
        public const string SelectorWidthPropertyName = "SelectorWidth";

        private double _selectorWidth = 0;

        /// <summary>
        /// Sets and gets the SelectorWidth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double SelectorWidth
        {
            get
            {
                return _selectorWidth;
            }

            set
            {
                if (_selectorWidth == value)
                {
                    return;
                }

                _selectorWidth = value;
                RaisePropertyChanged(SelectorWidthPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectorHeight" /> property's name.
        /// </summary>
        public const string SelectorHeightPropertyName = "SelectorHeight";

        private double _selectorHeight = 0;

        /// <summary>
        /// Sets and gets the SelectorHeight property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double SelectorHeight
        {
            get
            {
                return _selectorHeight;
            }

            set
            {
                if (_selectorHeight == value)
                {
                    return;
                }

                _selectorHeight = value;
                RaisePropertyChanged(SelectorHeightPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="SelectorEnabled" /> property's name.
        /// </summary>
        public const string SelectorEnabledPropertyName = "SelectorEnabled";

        private bool _selectorEnabled = false;

        /// <summary>
        /// Sets and gets the SelectorEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool SelectorEnabled
        {
            get
            {
                return _selectorEnabled;
            }

            set
            {
                if (_selectorEnabled == value)
                {
                    return;
                }

                _selectorEnabled = value;
                RaisePropertyChanged(SelectorEnabledPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsUrlValid" /> property's name.
        /// </summary>
        public const string IsUrlValidPropertyName = "IsUrlValid";

        private bool _isUrlValid = true;

        /// <summary>
        /// Sets and gets the IsUrlValid property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUrlValid
        {
            get
            {
                return _isUrlValid;
            }

            set
            {
                if (_isUrlValid == value)
                {
                    return;
                }

                _isUrlValid = value;
                RaisePropertyChanged(IsUrlValidPropertyName);
            }
        }

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


        #region Gets the build date and time (by reading the COFF header)

        // http://msdn.microsoft.com/en-us/library/ms680313
#pragma warning disable CS0649
        struct _IMAGE_FILE_HEADER
        {
            public ushort Machinev;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        };
#pragma warning restore CS0649

        static DateTime GetBuildDateTime(Assembly assembly)
        {
            var path = assembly.Location;
            if (File.Exists(path))
            {
                var buffer = new byte[Math.Max(Marshal.SizeOf(typeof(_IMAGE_FILE_HEADER)), 4)];
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Position = 0x3C;
                    fileStream.Read(buffer, 0, 4);
                    fileStream.Position = BitConverter.ToUInt32(buffer, 0); // COFF header offset
                    fileStream.Read(buffer, 0, 4); // "PE\0\0"
                    fileStream.Read(buffer, 0, buffer.Length);
                }
                var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    var coffHeader = (_IMAGE_FILE_HEADER)Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(), typeof(_IMAGE_FILE_HEADER));

                    return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1) + new TimeSpan(coffHeader.TimeDateStamp * TimeSpan.TicksPerSecond));
                }
                finally
                {
                    pinnedBuffer.Free();
                }
            }
            return new DateTime();
        }

        #endregion

        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string _title = "vMix Universal Title Controller";

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {

                return string.Format(CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat, _title + " ({0:d})", GetBuildDateTime(Assembly.GetExecutingAssembly()));
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
                        try
                        {
                            var copy = p.Copy();
                            copy.Name += " copy";
                            copy.State = Model;
                            copy.Left += 8;
                            copy.Top += 8;
                            copy.Update();
                            _controls.Add(copy);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Error while copying widget.");
                        }
                    }));
            }
        }


        private RelayCommand<vMixControl> _scaleUpCommand;

        /// <summary>
        /// Gets the ScaleUpCommand.
        /// </summary>
        public RelayCommand<vMixControl> ScaleUpCommand
        {
            get
            {
                return _scaleUpCommand
                    ?? (_scaleUpCommand = new RelayCommand<vMixControl>(
                    p =>
                    {
                        p.Scale += 0.25f;
                    }));
            }
        }


        private RelayCommand<vMixControl> _scaleDownCommand;

        /// <summary>
        /// Gets the ScaleDownCommand.
        /// </summary>
        public RelayCommand<vMixControl> ScaleDownCommand
        {
            get
            {
                return _scaleDownCommand
                    ?? (_scaleDownCommand = new RelayCommand<vMixControl>(
                    p =>
                    {
                        if (p.Scale - 0.25f >= 1.0f)
                            p.Scale -= 0.25f;

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
                        _isHotkeysEnabled = false;

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

                        _isHotkeysEnabled = true;
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

                            var count = _controls.Where(y => y.GetType() == control.GetType()).Count();

                            if (control.MaxCount == -1 || control.MaxCount > count)
                            {
                                control.State = Model;
                                control.Top = x.Y;
                                control.Left = x.X;
                                control.AlignByGrid();
                                control.Update();
                                _controls.Add(control);
                                _logger.Info("New {0} widget added.", control.Type.ToLower());

                                OpenPropertiesCommand.Execute(control);

                            }
                            else
                                control.Dispose();
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
                        }
                        if ((p.OriginalSource is ListView))
                            _isHotkeysEnabled = true;

                    }));
            }
        }

        private RelayCommand<MouseButtonEventArgs> _mouseButtonDown;

        /// <summary>
        /// Gets the MouseButtonDown.
        /// </summary>
        public RelayCommand<MouseButtonEventArgs> MouseButtonDown
        {
            get
            {
                return _mouseButtonDown
                    ?? (_mouseButtonDown = new RelayCommand<MouseButtonEventArgs>(
                    p =>
                    {
                        if (WindowSettings.Locked)
                            return;
                        var pos = p.MouseDevice.GetPosition((IInputElement)p.Source);
                        _relativeClickPoint = pos;
                        SelectorEnabled = true;
                        _rawSelectorPosition = new Thickness(pos.X, pos.Y, 0, 0);
                        SelectorPosition = new Thickness(pos.X, pos.Y, 0, 0);
                        SelectorWidth = 0;
                        SelectorHeight = 0;

                    }));
            }
        }


        private RelayCommand<MouseEventArgs> _mouseMove;

        /// <summary>
        /// Gets the MouseButtonDown.
        /// </summary>
        public RelayCommand<MouseEventArgs> MouseMove
        {
            get
            {
                return _mouseMove
                    ?? (_mouseMove = new RelayCommand<MouseEventArgs>(
                    p =>
                    {

                        _moveSource = p.Source;

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
                            ctrl.Update();
                            _logger.Info("Widget \"{0}\" was copied.", p.B.Name);
                            _controls.Add(ctrl);
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

                            IsUrlValid = vMixAPI.StateFabrique.IsUrlValid(vMixAPI.StateFabrique.GetUrl(WindowSettings.IP, WindowSettings.Port));

                            UpdatevMixState();

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
            {
                if (Model == null || (Model.Ip != WindowSettings.IP || Model.Port != WindowSettings.Port))
                {
                    Model = null;
                    vMixAPI.StateFabrique.Configure(WindowSettings.IP, WindowSettings.Port);
                    vMixAPI.StateFabrique.CreateAsync();
                }
                else
                {
                    Model.Configure(WindowSettings.IP, WindowSettings.Port);
                    Model.UpdateAsync();
                }
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
            }
        }


        bool ProcessHotkey(Key key, Key systemKey, ModifierKeys modifiers)
        {
            var result = false;
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
                    {
                        ctrl.ExecuteHotkey(item.idx);
                        result = true;
                    }
                }
            }
            return result;
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
                        if (!_isHotkeysEnabled)
                            return;
                        p.Handled = ProcessHotkey(p.Key, p.SystemKey, p.KeyboardDevice.Modifiers);

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

            vMixAPI.StateFabrique.OnStateCreated += State_OnStateCreated;

            _connectTimer.Interval = TimeSpan.FromSeconds(20);
            _connectTimer.Tick += _connectTimer_Tick;
            _connectTimer.Start();

            _logger.Info("Loading mapped functions.");
            XmlSerializer s = new XmlSerializer(typeof(ObservableCollection<vMixFunctionReference>));
            try
            {
                if (File.Exists("Functions.xml"))
                    using (var fs = new FileStream("Functions.xml", FileMode.Open))
                        _functions = (ObservableCollection<vMixFunctionReference>)s.Deserialize(fs);
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
                        _controlTemplates = (ObservableCollection<Pair<string, vMixControl>>)s.Deserialize(fs);
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

                if (_windowSettings.Left > totalwidth)
                    _windowSettings.Left = 128;
                if (_windowSettings.Top > totalheight)
                    _windowSettings.Top = 128;
                if (_windowSettings.Width > WpfScreenHelper.Screen.PrimaryScreen.Bounds.Width)
                    _windowSettings.Width = WpfScreenHelper.Screen.PrimaryScreen.Bounds.Width - 32;
                if (_windowSettings.Height > WpfScreenHelper.Screen.PrimaryScreen.Bounds.Height)
                    _windowSettings.Height = WpfScreenHelper.Screen.PrimaryScreen.Bounds.Height - 32;

            }
            catch (Exception)
            {
                _windowSettings = new MainWindowSettings();
            }

            if (Model == null)
                vMixAPI.StateFabrique.CreateAsync();

            MessengerInstance.Register<string>(this, (hk) =>
            {
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

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<Triple<vMixControl, double, double>>(this, (t) =>
            {
                foreach (var item in _controls.Where(x => x.Selected && x != t.A))
                {
                    item.Left = Math.Round(item.Left + t.B);
                    item.Top = Math.Round(item.Top + t.C);
                    item.AlignByGrid();
                }
            });

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<Pair<string, bool>>(this, (t) =>
            {
                switch (t.A)
                {
                    case "Hotkeys":
                        _isHotkeysEnabled = t.B;
                        //Debug.WriteLine(t.B);
                        break;
                    default:
                        break;
                }
            });

            if (!IsInDesignMode)
            {
                var globalEvents = Gma.System.MouseKeyHook.Hook.GlobalEvents();

                globalEvents.MouseMove += MainViewModel_MouseMove;
                globalEvents.MouseUp += MainViewModel_MouseUp;
            }

            IsUrlValid = vMixAPI.StateFabrique.IsUrlValid(vMixAPI.StateFabrique.GetUrl(WindowSettings.IP, WindowSettings.Port));


            Accord.Video.DirectShow.FilterInfoCollection filters = new Accord.Video.DirectShow.FilterInfoCollection(new Guid("{083863F1-70DE-11D0-BD40-00A0C911CE86}"));
            foreach (var item in filters)
                if (item.Name.Contains("NewTek NDI Source"))
                {
                    IsFiltersRegistered = true;
                    return;
                }

            if (Properties.Settings.Default.NDIFiltersRegistered) return;

            Ookii.Dialogs.Wpf.TaskDialog dialog = new Ookii.Dialogs.Wpf.TaskDialog();
            dialog.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Yes));
            dialog.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.No));
            dialog.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Cancel));
            dialog.Buttons[0].ElevationRequired = true;
            dialog.WindowTitle = Extensions.LocalizationManager.Get("Register NDI filters");
            dialog.MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Warning;
            dialog.MainInstruction = Extensions.LocalizationManager.Get("NDI filters not recognized in your system. Register them?");
            var dialogresult = dialog.ShowDialog(Application.Current.MainWindow);
            switch (dialogresult.ButtonType)
            {

                case Ookii.Dialogs.Wpf.ButtonType.Yes:
                    Process p = new Process();
                    p.StartInfo = new ProcessStartInfo(Path.Combine(Directory.GetCurrentDirectory(), "RegisterFilters.cmd")) { CreateNoWindow = true, Verb = "runas", Arguments = Directory.GetCurrentDirectory()  };
                    p.Start();
                    break;
                case Ookii.Dialogs.Wpf.ButtonType.Cancel:
                    Properties.Settings.Default.NDIFiltersRegistered = true;
                    break;
            }
            Properties.Settings.Default.Save();

        }

        private void MainViewModel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (SelectorWidth != 0 && SelectorHeight != 0)
            {

                var sr = new Rect(SelectorPosition.Left, SelectorPosition.Top, SelectorWidth, SelectorHeight);
                foreach (var item in _controls)
                {
                    var ir = new Rect(item.Left, item.Top, item.Width, double.IsNaN(item.Height) || double.IsInfinity(item.Height) ? 0 : item.Height + item.CaptionHeight);
                    item.Selected = (item.Selected || sr.Contains(ir)) && !item.Locked;
                }
                SelectorWidth = 0;
                SelectorHeight = 0;
                SelectorEnabled = false;
                return;
            }
            SelectorEnabled = false;
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                foreach (var item in _controls)
                {
                    item.Selected = false;
                }

        }

        private void MainViewModel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!SelectorEnabled)
            {
                _clickPoint = new Point(e.X, e.Y);
                return;
            }


            var pos = new Point(e.X, e.Y) - _clickPoint + _relativeClickPoint;//Mouse.PrimaryDevice.GetPosition((IInputElement)_moveSource);
            var pos1 = Mouse.PrimaryDevice.GetPosition(null);
            var w = -(_rawSelectorPosition.Left - pos.X);
            var h = -(_rawSelectorPosition.Top - pos.Y);

            SelectorPosition = new Thickness(w < 0 ? pos.X : _rawSelectorPosition.Left, h < 0 ? pos.Y : _rawSelectorPosition.Top, 0, 0);
            SelectorWidth = Math.Abs(w);
            SelectorHeight = Math.Abs(h);
        }

        WebClient _client = new vMixAPI.vMixWebClient();

        private void _connectTimer_Tick(object sender, EventArgs e)
        {
            IsUrlValid = vMixAPI.StateFabrique.IsUrlValid(vMixAPI.StateFabrique.GetUrl(WindowSettings.IP, WindowSettings.Port));
            if (!IsUrlValid)
                return;

            _client.DownloadStringCompleted += _client_DownloadStringCompleted;
            _client.CancelAsync();
            while (_client.IsBusy) Thread.Sleep(100);
            _client.DownloadStringAsync(new Uri(vMixAPI.StateFabrique.GetUrl(WindowSettings.IP, WindowSettings.Port)));
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
            if (Model != null && (Model.Ip == WindowSettings.IP && Model.Port == WindowSettings.Port))
                Status = "Online";
            else
                Status = "Update State";
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

        protected virtual void Dispose(bool managed)
        {
            if (_client != null)
                _client.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            //throw new NotImplementedException();
        }

        private RelayCommand _registerNDIFilters;

        /// <summary>
        /// Gets the RegisterNDIFilters.
        /// </summary>
        public RelayCommand RegisterNDIFilters
        {
            get
            {
                return _registerNDIFilters
                    ?? (_registerNDIFilters = new RelayCommand(
                    () =>
                    {
                        Process p = new Process();
                        p.StartInfo = new ProcessStartInfo(Path.Combine(Directory.GetCurrentDirectory(), "RegisterFilters.cmd")) { CreateNoWindow = true, Verb = "runas", Arguments = Directory.GetCurrentDirectory() };
                        try
                        {
                            p.Start();
                        }
                        catch (Exception) { }
                        _isFiltersRegistered = true;
                    },
                    () => !IsFiltersRegistered));
            }
        }


        private RelayCommand<System.Windows.Input.KeyEventArgs> _textBoxPreviewKeyUp;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand<System.Windows.Input.KeyEventArgs> TextBoxPreviewKeyUp
        {
            get
            {
                return _textBoxPreviewKeyUp
                    ?? (_textBoxPreviewKeyUp = new RelayCommand<System.Windows.Input.KeyEventArgs>(
                    p =>
                    {
                        if (p.Key == System.Windows.Input.Key.Return)
                        {

                            DependencyObject parent = ((FrameworkElement)p.Source).Parent;
                            while (parent is FrameworkElement && ((FrameworkElement)parent).Parent != null)
                                parent = ((FrameworkElement)parent).Parent;
                            while (parent is FrameworkElement && VisualTreeHelper.GetParent(parent) != null)
                                parent = VisualTreeHelper.GetParent(parent);
                            Keyboard.ClearFocus();
                            FocusManager.SetFocusedElement(parent, (IInputElement)parent);
                            //MoveFocus
                            ((FrameworkElement)parent).MoveFocus(new TraversalRequest(FocusNavigationDirection.Last) { });



                            _isHotkeysEnabled = true;
                        }
                    }));
            }
        }

        private RelayCommand<RoutedEventArgs> _textBoxGotFocus;

        /// <summary>
        /// Gets the GotFocus.
        /// </summary>
        public RelayCommand<RoutedEventArgs> TextBoxGotFocus
        {
            get
            {
                return _textBoxGotFocus
                    ?? (_textBoxGotFocus = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        _isHotkeysEnabled = false;
                       // GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Pair<string, bool>() { A = "Hotkeys", B = false });
                    }));
            }
        }

        private RelayCommand<RoutedEventArgs> _textBoxLostFocus;

        /// <summary>
        /// Gets the LostFocus.
        /// </summary>
        public RelayCommand<RoutedEventArgs> TextBoxLostFocus
        {
            get
            {
                return _textBoxLostFocus
                    ?? (_textBoxLostFocus = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        _isHotkeysEnabled = true;
                        //GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Pair<string, bool>() { A = "Hotkeys", B = true });
                    }));
            }
        }

    }
}