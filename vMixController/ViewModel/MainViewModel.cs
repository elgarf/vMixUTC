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
using System.Text;

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
        //LowLevelInput.Hooks.LowLevelMouseHook mouseHook = new LowLevelInput.Hooks.LowLevelMouseHook(true);
        vMixWidgetSettingsView _settings = null;// new vMixWidgetSettingsView();
        NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The <see cref="Model" /> property's name.
        /// </summary>
        public const string ModelPropertyName = "Model";

        Point _clickPoint;
        Point _relativeClickPoint;
        Thickness _rawSelectorPosition = new Thickness();


        /// <summary>
            /// The <see cref="IsHotkeysEnabled" /> property's name.
            /// </summary>
        public const string IsHotkeysEnabledPropertyName = "IsHotkeysEnabled";

        private bool _isHotkeysEnabled = true;

        /// <summary>
        /// Sets and gets the IsHotkeysEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsHotkeysEnabled
        {
            get
            {
                return _isHotkeysEnabled;
            }

            set
            {
                if (_isHotkeysEnabled == value)
                {
                    return;
                }

                _isHotkeysEnabled = value;
                RaisePropertyChanged(IsHotkeysEnabledPropertyName);
            }
        }

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
                    _windowSettings.PropertyChanged -= WindowSettings_PropertyChanged;
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

                _windowSettings.PropertyChanged += WindowSettings_PropertyChanged;
                RaisePropertyChanged(WindowSettingsPropertyName);
            }
        }

        private void WindowSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem((x) =>
            {
                if (e.PropertyName == "IP" || e.PropertyName == "Port")
                    ConnectTimer_Tick(null, new EventArgs());
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
        /// The <see cref="ExecLinks" /> property's name.
        /// </summary>
        public const string ExecLinksPropertyName = "ExecLinks";

        private ObservableCollection<Triple<vMixControl, vMixControl, string>> _execLinks  = new ObservableCollection<Triple<vMixControl, vMixControl, string>>();

        /// <summary>
        /// Sets and gets the ExecLinks property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Triple<vMixControl, vMixControl, string>> ExecLinks
        {
            get
            {
                return _execLinks;
            }

            set
            {
                if (_execLinks == value)
                {
                    return;
                }

                _execLinks = value;
                RaisePropertyChanged(ExecLinksPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="WidgetTemplates" /> property's name.
        /// </summary>
        public const string WidgetTemplatesPropertyName = "WidgetTemplates";

        private ObservableCollection<Pair<string, vMixControl>> _widgetTemplates = new ObservableCollection<Pair<string, vMixControl>>();

        /// <summary>
        /// Sets and gets the WidgetTemplates property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Pair<string, vMixControl>> WidgetTemplates
        {
            get
            {
                return _widgetTemplates;
            }

            set
            {
                if (_widgetTemplates == value)
                {
                    return;
                }

                _widgetTemplates = value;
                RaisePropertyChanged(WidgetTemplatesPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ExternalDataProviders" /> property's name.
        /// </summary>
        public const string ExternalDataProvidersPropertyName = "ExternalDataProviders";

        private ObservableCollection<Pair<string, vMixControl>> _externalDataProviders = new ObservableCollection<Pair<string, vMixControl>>();

        /// <summary>
        /// Sets and gets the ExternalDataProviders property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Pair<string, vMixControl>> ExternalDataProviders
        {
            get
            {
                return _externalDataProviders;
            }

            set
            {
                if (_externalDataProviders == value)
                {
                    return;
                }

                _externalDataProviders = value;
                RaisePropertyChanged(ExternalDataProvidersPropertyName);
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
        /// The <see cref="Widgets" /> property's name.
        /// </summary>
        public const string WidgetsPropertyName = "Widgets";

        private ObservableCollection<vMixController.Widgets.vMixControl> _widgets = new ObservableCollection<vMixController.Widgets.vMixControl>();

        /// <summary>
        /// Sets and gets the Widgetss property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<vMixController.Widgets.vMixControl> Widgets
        {
            get
            {
                return _widgets;
            }

            set
            {
                if (_widgets == value)
                {
                    return;
                }

                _widgets = value;
                RaisePropertyChanged(WidgetsPropertyName);
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

        /// <summary>
        /// The <see cref="LIVE" /> property's name.
        /// </summary>
        public const string LIVEPropertyName = "LIVE";

        private bool _LIVE = true;

        /// <summary>
        /// Sets and gets the LIVE property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool LIVE
        {
            get
            {
                return _LIVE;
            }

            set
            {
                if (_LIVE == value)
                {
                    return;
                }

                foreach (var item in _widgets)
                    if (item is vMixControlTextField)
                    {
                        ((vMixControlTextField)item).IsLive = value;
                        //item.Update();
                    }

                _LIVE = value;
                RaisePropertyChanged(LIVEPropertyName);
            }
        }


        #region Gets the build date and time (by reading the COFF header)

        // http://msdn.microsoft.com/en-us/library/ms680313
#pragma warning disable CS0649
        struct IMAGE_FILE_HEADER
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
                var buffer = new byte[Math.Max(Marshal.SizeOf(typeof(IMAGE_FILE_HEADER)), 4)];
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
                    var coffHeader = (IMAGE_FILE_HEADER)Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(), typeof(IMAGE_FILE_HEADER));

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

        /// <summary>
        /// The <see cref="IsGhosted" /> property's name.
        /// </summary>
        public const string IsGhostedPropertyName = "IsGhosted";

        private bool _isGhosted = false;

        /// <summary>
        /// Sets and gets the IsGhosted property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsGhosted
        {
            get
            {
                return _isGhosted;
            }

            set
            {
                if (_isGhosted == value)
                {
                    return;
                }

                _isGhosted = value;

                foreach (var item in _widgets)
                {
                    if (item.ZIndex >= 0)
                        item.IsGhosted = _isGhosted;
                }

                RaisePropertyChanged(IsGhostedPropertyName);
            }
        }

        private void InsertWidgetByZIndex(vMixControl widget)
        {
            widget.IsGhosted = widget.ZIndex >= 0 && IsGhosted;
            //var first = _widgets.Select((item, index) => new { itm = item, idx = index }).OrderBy(i => i.itm.ZIndex).FirstOrDefault();
            //if (first != null)
            //{
            if (widget.ZIndex < 0)
                _widgets.Insert(0, widget);
            else
                _widgets.Add(widget);
            //}
            //else
            //    _widgets.Add(widget);
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
                        p.IsPasswordLocked = p.IsPasswordLockable && p.Locked && (!string.IsNullOrWhiteSpace(WindowSettings.UserName) || !string.IsNullOrWhiteSpace(WindowSettings.Password));
                    }));
            }
        }

        private RelayCommand<vMixControl> _removeWidgetCommand;

        /// <summary>
        /// Gets the RemoveWidgetCommand.
        /// </summary>
        public RelayCommand<vMixControl> RemoveWidgetCommand
        {
            get
            {
                return _removeWidgetCommand
                    ?? (_removeWidgetCommand = new RelayCommand<vMixController.Widgets.vMixControl>(
                    p =>
                    {
                        p.Dispose();
                        Widgets.Remove(p);
                    }));
            }
        }

        private RelayCommand<vMixControl> _copyWidgetCommand;

        /// <summary>
        /// Gets the CopyWidgetCommand.
        /// </summary>
        public RelayCommand<vMixControl> CopyWidgetCommand
        {
            get
            {
                return _copyWidgetCommand
                    ?? (_copyWidgetCommand = new RelayCommand<vMixControl>(
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
                            if (copy.ZIndex < 0)
                                InsertWidgetByZIndex(copy);
                            else
                                _widgets.Add(copy);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Error while copying widget.");
                        }
                    }));
            }
        }


        private RelayCommand<vMixControl> _toggleCaptionCommand;

        /// <summary>
        /// Gets the ToggleCaptionCommand.
        /// </summary>
        public RelayCommand<vMixControl> ToggleCaptionCommand
        {
            get
            {
                return _toggleCaptionCommand
                    ?? (_toggleCaptionCommand = new RelayCommand<vMixControl>(
                    p =>
                    {
                        p.IsCaptionOn = !p.IsCaptionOn;
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
                        IsHotkeysEnabled = false;

                        _logger.Info("Opening properties for widget {0}.", p.Name);
                        var viewModel = ServiceLocator.Current.GetInstance<vMixController.ViewModel.vMixWidgetSettingsViewModel>();
                        viewModel.Widget = p;
                        viewModel.SetProperties(p);

                        _settings = new vMixWidgetSettingsView
                        {
                            Owner = App.Current.MainWindow
                        };
                        var result = _settings.ShowDialog();
                        if (result.HasValue && result.Value)
                            p.SetProperties(viewModel);
                        _logger.Info("Properties updated.");
                        _settings = null;

                        IsHotkeysEnabled = true;
                    }));
            }
        }

        Action<Point> _createWidget;

        private RelayCommand<string> _createWidgetCommand;

        /// <summary>
        /// Gets the CreateWidgetCommand.
        /// </summary>
        public RelayCommand<string> CreateWidgetCommand
        {
            get
            {
                return _createWidgetCommand
                    ?? (_createWidgetCommand = new RelayCommand<string>(
                    p =>
                    {
                        EditorCursor = CursorType.Hand.ToString();
                        _createWidget = new Action<Point>(x =>
                        {
                            var widget = (vMixControl)Assembly.GetAssembly(this.GetType()).CreateInstance("vMixController.Widgets.vMixControl" + p);

                            var count = _widgets.Where(y => y.GetType() == widget.GetType()).Count();

                            if (widget.MaxCount == -1 || widget.MaxCount > count)
                            {
                                widget.State = Model;
                                widget.Top = x.Y;
                                widget.Left = x.X;
                                widget.AlignByGrid();
                                if (widget is vMixControlTextField)
                                    ((vMixControlTextField)widget).IsLive = LIVE;
                                widget.Update();
                                InsertWidgetByZIndex(widget);
                                _logger.Info("New {0} widget added.", widget.Type.ToLower());

                                OpenPropertiesCommand.Execute(widget);

                            }
                            else
                                widget.Dispose();
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
                        if (_createWidget != null)
                        {
                            EditorCursor = "Arrow";
                            var pos = p.MouseDevice.GetPosition((IInputElement)p.Source);
                            _createWidget(new Point(pos.X / WindowSettings.UIScale, pos.Y / WindowSettings.UIScale));
                            _createWidget = null;
                        }
                        if ((p.OriginalSource is ListView))
                            IsHotkeysEnabled = true;

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

                        /*Gma.System.MouseKeyHook.Hook.GlobalEvents().MouseMove += MainViewModel_MouseMove;
                        Gma.System.MouseKeyHook.Hook.GlobalEvents().MouseUp += MainViewModel_MouseUp;*/

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
                        //_moveSource = p.Source;

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
                        if (_createWidget != null && _fromContextMenu)
                        {
                            EditorCursor = "Arrow";
                            var pos = _contextMenuPosition;
                            _createWidget(new Point(pos.X / WindowSettings.UIScale, pos.Y / WindowSettings.UIScale));
                            _createWidget = null;
                            _fromContextMenu = false;
                        }
                    }));
            }
        }

        private RelayCommand<Pair<string, vMixControl>> _createWidgetFromTemplateCommand;

        /// <summary>
        /// Gets the CreateWidgetFromTemplateCommand.
        /// </summary>
        public RelayCommand<Pair<string, vMixControl>> CreateWidgetFromTemplateCommand
        {
            get
            {
                return _createWidgetFromTemplateCommand
                    ?? (_createWidgetFromTemplateCommand = new RelayCommand<Pair<string, vMixControl>>(
                    p =>
                    {
                        EditorCursor = "Hand";
                        _createWidget = x =>
                        {
                            var ctrl = p.B.Copy();
                            ctrl.Left = x.X;
                            ctrl.Top = x.Y;
                            ctrl.IsTemplate = false;
                            ctrl.State = Model;
                            ctrl.AlignByGrid();
                            ctrl.Update();
                            _logger.Info("Widget \"{0}\" was copied.", p.B.Name);
                            InsertWidgetByZIndex(ctrl);
                            //_widgets.Add(ctrl);
                        };

                    }));
            }
        }

        private RelayCommand<Pair<string, vMixControl>> _removeWidgetTemplateCommand;

        /// <summary>
        /// Gets the RemoveWidgetTemplateCommand.
        /// </summary>
        public RelayCommand<Pair<string, vMixControl>> RemoveWidgetTemplateCommand
        {
            get
            {
                return _removeWidgetTemplateCommand
                    ?? (_removeWidgetTemplateCommand = new RelayCommand<Pair<string, vMixControl>>(
                    p =>
                    {
                        _widgetTemplates.Remove(p);
                    }));
            }
        }

        private RelayCommand<Pair<string, vMixControl>> _editWidgetTemplateCommand;

        /// <summary>
        /// Gets the EditWidgetTemplateCommand.
        /// </summary>
        public RelayCommand<Pair<string, vMixControl>> EditWidgetTemplateCommand
        {
            get
            {
                return _editWidgetTemplateCommand
                    ?? (_editWidgetTemplateCommand = new RelayCommand<Pair<string, vMixControl>>(
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
                        foreach (var item in _widgets)
                            item.Dispose();

                        WindowSettings.Password = null;
                        WindowSettings.UserName = null;
                        WindowSettings.Locked = false;

                        _widgets.Clear();
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
                        Ookii.Dialogs.Wpf.VistaOpenFileDialog opendlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
                        {
                            Filter = "vMix Controller|*.vmc"
                        };
                        var result = opendlg.ShowDialog(App.Current.MainWindow);
                        if (result.HasValue && result.Value)
                        {
                            LoadControllerFromFile(opendlg.FileName);
                        }
                    }));
            }
        }

        private void LoadControllerFromFile(string opendlg)
        {
            foreach (var item in _widgets)
                item.Dispose();
            _widgets.Clear();

            LIVE = true;

            foreach (var item in Utils.LoadController(opendlg, Functions, out _windowSettings))
                _widgets.Add(item);

            foreach (var item in _widgets)
                item.Update();

            RaisePropertyChanged(nameof(WindowSettings));
            _logger.Info("Configuring API.");

            ConnectTimer_Tick(null, new EventArgs());

            vMixAPI.StateFabrique.Configure(WindowSettings.IP, WindowSettings.Port);

            IsUrlValid = vMixAPI.StateFabrique.IsUrlValid(vMixAPI.StateFabrique.GetUrl(WindowSettings.IP, WindowSettings.Port));

            SyncTovMixState();
            UpdateExecLinks();
        }


        private void UpdateExecLinks()
        {
            ExecLinks.Clear();
            foreach (var item in _widgets)
            {
                var active = item.Hotkey.Where(x => !string.IsNullOrWhiteSpace(x.Link)).Select(x => x.Link).ToArray();
                foreach (var item1 in _widgets)
                {
                    switch (item1)
                    {
                        case vMixControlButton b:
                            foreach (var cmd in b.Commands)
                            {
                                if (cmd.Action.Function == "ExecLink" && active.Contains(cmd.StringParameter))
                                    ExecLinks.Add(new Triple<vMixControl, vMixControl, string>() { A = item, B = item1, C = cmd.StringParameter });
                            }
                            break;
                        case vMixControlTimer t:
                            foreach (var l in t.Links.Where(x=>active.Contains(x)))
                                ExecLinks.Add(new Triple<vMixControl, vMixControl, string>() { A = item, B = item1, C = l });
                            break;
                        case vMixControlMidiInterface m:
                            foreach (var l in m.Midis.Where(x => active.Contains(x.C)))
                                ExecLinks.Add(new Triple<vMixControl, vMixControl, string>() { A = item, B = item1, C = l.C });
                            break;
                        case vMixControlClock c:
                            foreach (var l in c.Events.Where(x => active.Contains(x.B)))
                                ExecLinks.Add(new Triple<vMixControl, vMixControl, string>() { A = item, B = item1, C = l.B });
                            break;
                    }
                }
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
                        Ookii.Dialogs.Wpf.VistaSaveFileDialog opendlg = new Ookii.Dialogs.Wpf.VistaSaveFileDialog
                        {
                            Filter = "vMix Controller|*.vmc",
                            DefaultExt = "vmc"
                        };
                        var result = opendlg.ShowDialog(App.Current.MainWindow);
                        if (result.HasValue && result.Value)
                            Utils.SaveController(opendlg.FileName, _widgets, _windowSettings);

                    }));
            }
        }

        private string ToMD5Hash(string str)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                return Convert.ToBase64String(md5.ComputeHash(Encoding.ASCII.GetBytes(str)));
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
                        if (!WindowSettings.Locked)
                        {
                            WindowSettings.Password = null;
                            WindowSettings.UserName = null;
                            if (Keyboard.IsKeyDown(Key.LeftShift))
                            {
                                Ookii.Dialogs.Wpf.CredentialDialog cred = new Ookii.Dialogs.Wpf.CredentialDialog
                                {
                                    ShowSaveCheckBox = false,
                                    ShowUIForSavedCredentials = false,
                                    Target = "vMixUTC",
                                    MainInstruction = "Enter password to lock controller"
                                };
                                if (cred.ShowDialog())
                                {
                                    WindowSettings.Password = ToMD5Hash(cred.Password);
                                    WindowSettings.UserName = ToMD5Hash(cred.UserName);
                                }
                            }
                        }
                        else
                        if (!string.IsNullOrWhiteSpace(WindowSettings.UserName) || !string.IsNullOrWhiteSpace(WindowSettings.Password))
                        {
                            Ookii.Dialogs.Wpf.CredentialDialog cred = new Ookii.Dialogs.Wpf.CredentialDialog
                            {
                                ShowSaveCheckBox = false,
                                ShowUIForSavedCredentials = false,
                                MainInstruction = "Enter password to unlock controller",
                                Target = "UTC",
                                WindowTitle = "Universal Title Controller"
                            };
                            if (cred.ShowDialog())
                            {

                                if (ToMD5Hash(cred.Password) == WindowSettings.Password && ToMD5Hash(cred.UserName) == WindowSettings.UserName)
                                {
                                    WindowSettings.Password = null;
                                    WindowSettings.UserName = null;
                                }
                                else
                                {
                                    Ookii.Dialogs.Wpf.TaskDialog td = new Ookii.Dialogs.Wpf.TaskDialog();
                                    td.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Ok));
                                    td.MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Error;
                                    td.MainInstruction = "Incorrect password!";
                                    td.ShowDialog();
                                    return;
                                }
                            }
                            else return;
                        }
                        WindowSettings.Locked = !WindowSettings.Locked;
                        foreach (var item in _widgets)
                        {
                            item.Locked = WindowSettings.Locked;
                            item.IsPasswordLocked = item.IsPasswordLockable && (!string.IsNullOrWhiteSpace(WindowSettings.UserName) || !string.IsNullOrWhiteSpace(WindowSettings.Password));
                        }
                    }));
            }
        }

        private RelayCommand<vMixController.Widgets.vMixControl> _switchPasswordLockableCommand;

        /// <summary>
        /// Gets the SwitchPasswordLockableCommand.
        /// </summary>
        public RelayCommand<vMixController.Widgets.vMixControl> SwitchPasswordLockableCommand
        {
            get
            {
                return _switchPasswordLockableCommand
                    ?? ( _switchPasswordLockableCommand = new RelayCommand<vMixController.Widgets.vMixControl>(
                    (p) =>
                    {
                        p.IsPasswordLockable = !p.IsPasswordLockable;
                        //p.IsPasswordLocked = (p.Locked && p.IsPasswordLockable && (!string.IsNullOrWhiteSpace(WindowSettings.UserName) || !string.IsNullOrWhiteSpace(WindowSettings.Password)));
                    }));
            }
        }

        private RelayCommand _syncStateCommand;

        /// <summary>
        /// Gets the UpdateStateCommand.
        /// </summary>
        public RelayCommand SyncStateCommand
        {
            get
            {
                return _syncStateCommand
                    ?? (_syncStateCommand = new RelayCommand(
                    () =>
                    {
                        SyncTovMixState();
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

        private void SyncTovMixState()
        {
            _logger.Info("Syncing to vMix state.");
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
                Model.OnStateSynced -= Model_OnStateUpdated;
            Model = (vMixAPI.State)sender;
            foreach (var item in _widgets)
                item.State = Model;
            if (Model != null)
                Model.OnStateSynced += Model_OnStateUpdated;
            ConnectTimer_Tick(null, new EventArgs());
        }

        private void Model_OnStateUpdated(object sender, vMixAPI.StateSyncedEventArgs e)
        {
            if (!e.Successfully)
            {
                Model = null;
                State_OnStateCreated(null, null);
                Status = "Offline";
            }
            else
            {
                foreach (var item in _widgets)
                    item.State = (vMixAPI.State)sender;
            }
        }


        bool ProcessHotkey(Key key, Key systemKey, ModifierKeys modifiers)
        {
            var result = false;
            foreach (var ctrl in _widgets)
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
            foreach (var ctrl in _widgets)
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
                        if (!IsHotkeysEnabled)
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
                            s.Serialize(fs, _widgetTemplates);

                        _logger.Info("Saving window settings.");
                        s = new XmlSerializer(typeof(MainWindowSettings));
                        using (var fs = new FileStream(Path.Combine(_documentsPath, "WindowSettings.xml"), FileMode.Create))
                            s.Serialize(fs, _windowSettings);

                        //Dispose external data providers
                        foreach (var item in ExternalDataProviders)
                        {
                            item.B.Dispose();
                        }
                        foreach (var item in _widgetTemplates)
                        {
                            item.B.Dispose();
                        }
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
            //Fix changing current directory on opening .vmc from external folder
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            vMixAPI.StateFabrique.OnStateCreated += State_OnStateCreated;

            _connectTimer.Interval = TimeSpan.FromSeconds(20);
            _connectTimer.Tick += ConnectTimer_Tick;
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
                        _widgetTemplates = (ObservableCollection<Pair<string, vMixControl>>)s.Deserialize(fs);
            }
            catch (Exception)
            {

            }

            _logger.Info("Searching for data providers.");
            var files = Directory.EnumerateFiles(Path.Combine(Directory.GetCurrentDirectory(), "DataProviders"), "*DataProvider.dll");
            if (files.Count() > 0)
                ExternalDataProviders.Add(new Pair<string, vMixControl>()
                {
                    A = "Default",
                    B = new vMixControlExternalData() {
                        Color = ViewModel.vMixWidgetSettingsViewModel.Colors[1].A,
                        BorderColor = ViewModel.vMixWidgetSettingsViewModel.Colors[1].B
                    },
                });
            foreach (var item in files)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(item);
                    var attr = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
                    var control = new vMixControlExternalData()
                    {
                        Name = attr.Description,
                        DataProviderPath = item,
                        IsTemplate = true,
                        IsLive = true,
                        Color = ViewModel.vMixWidgetSettingsViewModel.Colors[0].A,
                        BorderColor = ViewModel.vMixWidgetSettingsViewModel.Colors[0].B
                    };
                    control.Update();
                    ExternalDataProviders.Add(new Pair<string, vMixControl>() { A = control.Name, B = control });
                    
                }
                catch (Exception e) {
                    _logger.Error(e, "Failed loading data provider. {0}");
                }

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
                var ds = Widgets.Where(x => x.Name == name).FirstOrDefault();
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
                return Widgets.Select(x => x.Name).ToList();
            };

            Singleton<SharedData>.Instance.GetDataSourceProps = (name) =>
            {
                var ds = Widgets.Where(x => x.Name == name).FirstOrDefault();
                if (ds == null) return new List<string>();
                return ds.GetType().GetProperties().Where(x =>
                {
                    var val = x.GetValue(ds);
                    return x.PropertyType == typeof(string) || typeof(IList<string>).IsAssignableFrom(x.PropertyType);
                }).Select(x => x.Name).ToList();
            };

            Singleton<SharedData>.Instance.GetDataSource = (name) =>
            {
                var ds = Widgets.Where(x => x.Name == name).FirstOrDefault();
                return ds;
            };


            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<LIVEToggleMessage>(this, (msg) =>
            {
                switch (msg.State)
                {
                    case 0: LIVE = false; break;
                    case 1: LIVE = true; break;
                    case 2: LIVE = !LIVE; break;
                }
            });

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<Triple<vMixControl, double, double>>(this, (t) =>
            {
                foreach (var item in _widgets.Where(x => x.Selected && x != t.A))
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
                        IsHotkeysEnabled = t.B;
                        //Debug.WriteLine(t.B);
                        break;
                    default:
                        break;
                }
            });

            if (!IsInDesignMode)
            {   
                var globalEvents = Gma.System.MouseKeyHook.Hook.AppEvents();
                globalEvents.MouseMove += MainViewModel_MouseMove;
                globalEvents.MouseUp += MainViewModel_MouseUp;
            }

            IsUrlValid = vMixAPI.StateFabrique.IsUrlValid(vMixAPI.StateFabrique.GetUrl(WindowSettings.IP, WindowSettings.Port));

            Properties.Settings.Default.Save();

            if (!string.IsNullOrWhiteSpace((string)App.Current.Resources["CommandLine"]))
                try
                {
                    _logger.Info("Trying to load {0}.", (string)App.Current.Resources["CommandLine"]);
                    LoadControllerFromFile((string)App.Current.Resources["CommandLine"]);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error loading controller. {0}");
                }

        }

        private void MainViewModel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (SelectorWidth != 0 && SelectorHeight != 0)
            {

                var sr = new Rect(SelectorPosition.Left, SelectorPosition.Top, SelectorWidth, SelectorHeight);
                foreach (var item in _widgets)
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
                foreach (var item in _widgets)
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
            var w = -(_rawSelectorPosition.Left - pos.X);
            var h = -(_rawSelectorPosition.Top - pos.Y);

            SelectorPosition = new Thickness(w < 0 ? pos.X : _rawSelectorPosition.Left, h < 0 ? pos.Y : _rawSelectorPosition.Top, 0, 0);
            SelectorWidth = Math.Abs(w);
            SelectorHeight = Math.Abs(h);
        }

        WebClient _client = new vMixAPI.vMixWebClient();

        private void ConnectTimer_Tick(object sender, EventArgs e)
        {
            if (IsInDesignMode) return;

            IsUrlValid = vMixAPI.StateFabrique.IsUrlValid(vMixAPI.StateFabrique.GetUrl(WindowSettings.IP, WindowSettings.Port));
            if (!IsUrlValid)
                return;

            _client.DownloadStringCompleted += Client_DownloadStringCompleted;
            _client.CancelAsync();
            while (_client.IsBusy) Thread.Sleep(100);
            _client.DownloadStringAsync(new Uri(vMixAPI.StateFabrique.GetUrl(WindowSettings.IP, WindowSettings.Port)));
        }

        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
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
                Status = "Sync";
        }

        public override void Cleanup()
        {
            // Clean up if needed
            foreach (var item in Widgets.OfType<IDisposable>())
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

        private RelayCommand<MouseEventArgs> _previewMouseUp;

        /// <summary>
        /// Gets the PreviewMouseUp.
        /// </summary>
        public RelayCommand<MouseEventArgs> PreviewMouseUp
        {
            get
            {
                return _previewMouseUp
                    ?? (_previewMouseUp = new RelayCommand<MouseEventArgs>(
                    p =>
                    {
                        //mouseHook.UninstallHook();
                        //Gma.System.MouseKeyHook.Hook.GlobalEvents().MouseMove -= MainViewModel_MouseMove;
                        //Gma.System.MouseKeyHook.Hook.GlobalEvents().MouseUp -= MainViewModel_MouseUp;
                    }));
            }
        }

        private RelayCommand<MouseEventArgs> _previewMouseDown;

        /// <summary>
        /// Gets the PreviewMouseDown.
        /// </summary>
        public RelayCommand<MouseEventArgs> PreviewMouseDown
        {
            get
            {
                return _previewMouseDown
                    ?? (_previewMouseDown = new RelayCommand<MouseEventArgs>(
                    p =>
                    {
                       //mouseHook.InstallHook();
                    }));
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
                        if (p.Key == System.Windows.Input.Key.Return || p.Key == Key.Escape)
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



                            IsHotkeysEnabled = true;
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
                        IsHotkeysEnabled = false;
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
                        IsHotkeysEnabled = true;
                        //GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Pair<string, bool>() { A = "Hotkeys", B = true });
                    }));
            }
        }

        private RelayCommand _aboutCommand;

        /// <summary>
        /// Gets the AboutCommand.
        /// </summary>
        public RelayCommand AboutCommand
        {
            get
            {
                return _aboutCommand
                    ?? (_aboutCommand = new RelayCommand(
                    () =>
                    {
                        Ookii.Dialogs.Wpf.TaskDialog td = new Ookii.Dialogs.Wpf.TaskDialog
                        {
                            WindowTitle = "About",

                            MainInstruction = "One controller to rule them all.",
                            MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Information,
                            Footer = Title
                        };

                        var forumbtn = new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Custom) { Text = "vMix Forum" };
                        var donatebtn = new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Custom) { Text = "Donate" };
                        td.Buttons.Add(forumbtn);
                        td.Buttons.Add(donatebtn);
                        td.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Close) { Default = true });

                        var btn = td.ShowDialog();
                        if (btn == forumbtn)
                            Process.Start(new ProcessStartInfo("https://forums.vmix.com/default.aspx?g=posts&t=6468"));
                        else if (btn == donatebtn)
                            Process.Start(new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WT9QZ2XH97HMN&lc=US&item_name=vMix%20Universal%20Title%20Controller&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donate_SM%2egif%3aNonHosted"));


                    }));
            }
        }
    }
}