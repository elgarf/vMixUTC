using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Melanchall.DryWetMidi.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;
using vMixController.Classes.Scripting;
using vMixController.Classes.vMixController.Classes;
using vMixController.Extensions;
using vMixControllerSkin.Localization;
using vMixController.Messages;
using vMixController.Widgets;
using vMixControllerSkin;

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
        private const int MaxUndoSteps = 10;

        public class ByteArrayDelta
        {
            public int PrefixLength { get; set; }
            public int RemovedLength { get; set; }
            public byte[] InsertedData { get; set; }
        }

        public class UndoSnapshot
        {
            public bool IsFullSnapshot { get; set; }
            public byte[] WidgetsData { get; set; }
            public byte[] WindowSettingsData { get; set; }
            public ByteArrayDelta WidgetsDelta { get; set; }
            public ByteArrayDelta WindowSettingsDelta { get; set; }
        }

        public class UndoEntry
        {
            public UndoSnapshot Snapshot { get; set; }
            public string Reason { get; set; }
        }

        bool _isPressed = false;
        //LowLevelInput.Hooks.LowLevelMouseHook mouseHook = new LowLevelInput.Hooks.LowLevelMouseHook(true);
        vMixWidgetSettingsView _settings = new vMixWidgetSettingsView();
        NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ScriptExecutionLoopGuard _scriptExecutionLoopGuard = new ScriptExecutionLoopGuard(100, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        private DateTime _lastScriptLoopGuardWarningUtc = DateTime.MinValue;
        private static readonly TimeSpan ScriptLoopGuardWarningThrottle = TimeSpan.FromSeconds(2);

        Point _clickPoint;
        Point _relativeClickPoint;
        Thickness _rawSelectorPosition = new Thickness();
        bool _skipClick = false;


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
                RaisePropertyChanged(nameof(IsHotkeysEnabled));
            }
        }


        private bool _isLoading = false;

        /// <summary>
        /// Sets and gets the IsLoading property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }

            set
            {
                if (_isLoading == value)
                {
                    return;
                }


                _isLoading = value;

                if (!_isLoading)
                    foreach (var item in Widgets)
                        item.IsVisualReady = true;

                RaisePropertyChanged(nameof(IsLoading));
            }
        }

        private string _controllerPath = Directory.GetCurrentDirectory();

        /// <summary>
        /// Sets and gets the ControllerPath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ControllerPath
        {
            get
            {
                return _controllerPath;
            }

            set
            {
                if (_controllerPath == value)
                {
                    return;
                }

                _controllerPath = value;
                RaisePropertyChanged(nameof(ControllerPath));
            }
        }

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
                RaisePropertyChanged(nameof(IsFiltersRegistered));
            }
        }

        /*Selector*/
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
                RaisePropertyChanged(nameof(SelectorPosition));
            }
        }


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
                RaisePropertyChanged(nameof(SelectorWidth));
            }
        }

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
                RaisePropertyChanged(nameof(SelectorHeight));
            }
        }


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
                RaisePropertyChanged(nameof(SelectorEnabled));
            }
        }


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
                RaisePropertyChanged(nameof(IsUrlValid));
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


                _logger.Debug("New model setted.");
                /*if (_model != null)
                    Status = "Online";
                else
                    Status = "Offline";*/

                RaisePropertyChanged(nameof(Model));
            }
        }

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
                        NLog.LogManager.ResumeLogging();
                }
                else
                {
                    if (NLog.LogManager.IsLoggingEnabled())
                        NLog.LogManager.SuspendLogging();
                }

                _windowSettings.PropertyChanged += WindowSettings_PropertyChanged;
                RaisePropertyChanged(nameof(WindowSettings));
            }
        }

        private void WindowSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _ = Task.Run(() =>
            {
                if (e.PropertyName == "IP" || e.PropertyName == "Port" || e.PropertyName == "HttpLogin" || e.PropertyName == "HttpPassword")
                    CheckvMixConnection(null, new EventArgs());
            });

            if (e.PropertyName == nameof(Classes.MainWindowSettings.AutoSync) ||
                e.PropertyName == nameof(Classes.MainWindowSettings.IP))
                UpdateTcpSubscriber();
        }

        private void UpdateTcpSubscriber()
        {
            if (WindowSettings?.AutoSync == true && !string.IsNullOrWhiteSpace(WindowSettings.IP))
            {
                _tcpSubscriber.ActsReceived -= OnVmixActsReceived;
                _tcpSubscriber.ActsReceived += OnVmixActsReceived;
                _tcpSubscriber.Start(WindowSettings.IP);
            }
            else
            {
                _tcpSubscriber.ActsReceived -= OnVmixActsReceived;
                _tcpSubscriber.Stop();
            }
        }

        private void OnVmixActsReceived(object sender, EventArgs e)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(SyncTovMixState));
        }

        private void LocalizationManager_CultureChanged(object sender, EventArgs e)
        {
            LocalizeVirtualInputs(Model);
        }

        private Status _status = Classes.Status.Offline;

        /// <summary>
        /// Sets and gets the Status property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Status Status
        {
            get
            {
                return _status;
            }

            set
            {
                XmlDocumentMessenger.Sync = value == Status.Online || value == Status.InputsChanged;

                if (_status == value)
                {
                    return;
                }

                _status = value;

                _logger.Debug("Status changed to {0}.", value);

                RaisePropertyChanged(nameof(Status));
            }
        }


        private Status _pollingstatus = Classes.Status.Offline;

        /// <summary>
        /// Sets and gets the Status property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Status PollingStatus
        {
            get
            {
                return _pollingstatus;
            }

            set
            {
                if (_pollingstatus == value)
                {
                    return;
                }

                _pollingstatus = value;

                RaisePropertyChanged(nameof(PollingStatus));
            }
        }

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
                RaisePropertyChanged(nameof(WidgetTemplates));
            }
        }

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
                RaisePropertyChanged(nameof(ExternalDataProviders));
            }
        }

        private ObservableCollection<vMixNewFunctionReference> _newFunctions = null;

        public ObservableCollection<vMixNewFunctionReference> NewFunctions
        {
            get
            {
                return _newFunctions;
            }

            set
            {
                if (_newFunctions == value)
                {
                    return;
                }

                _newFunctions = value;
                RaisePropertyChanged(nameof(NewFunctions));
            }
        }

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
                RaisePropertyChanged(nameof(Functions));
            }
        }

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
                RaisePropertyChanged(nameof(Widgets));
            }
        }

        private readonly List<UndoEntry> _undoStack = new List<UndoEntry>();
        private UndoSnapshot _undoState = null;

        /// <summary>
        /// Sets and gets the UndoState property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public UndoSnapshot UndoState
        {
            get
            {
                return _undoState;
            }

            set
            {
                if (_undoState == value)
                {
                    return;
                }

                _undoState = value;
                RaisePropertyChanged(nameof(UndoState));
            }
        }

        private string _undoReason = "";

        /// <summary>
        /// Sets and gets the UndoReason property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string UndoReason
        {
            get
            {
                return _undoReason;
            }

            set
            {
                if (_undoReason == value)
                {
                    return;
                }

                _undoReason = value;
                RaisePropertyChanged(nameof(UndoReason));
            }
        }

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
                RaisePropertyChanged(nameof(EditorCursor));
            }
        }

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
                RaisePropertyChanged(nameof(LIVE));
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

        public static DateTime GetBuildDateTime(Assembly assembly)
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

                    return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1) + new TimeSpan(coffHeader.TimeDateStamp * TimeSpan.TicksPerSecond)).ToUniversalTime();
                }
                finally
                {
                    pinnedBuffer.Free();
                }
            }
            return new DateTime();
        }

        #endregion


        private string _title = string.Format(CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat, "vMix Universal Title Controller ({0:d})", GetBuildDateTime(Assembly.GetExecutingAssembly()));

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {

                return _title;
            }

            private set
            {
                _title = value;
                RaisePropertyChanged(nameof(Title));
            }
        }

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

                RaisePropertyChanged(nameof(IsGhosted));
            }
        }

        private string _updateLink = null;

        /// <summary>
        /// Sets and gets the UpdateLink property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string UpdateLink
        {
            get
            {
                return _updateLink;
            }

            set
            {
                if (_updateLink == value)
                {
                    return;
                }

                _updateLink = value;
                RaisePropertyChanged(nameof(UpdateLink));
            }
        }

        private DateTime _availableVersion = GetBuildDateTime(Assembly.GetExecutingAssembly());

        /// <summary>
        /// Sets and gets the AvailableVersion property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DateTime AvailableVersion
        {
            get
            {
                return _availableVersion;
            }

            set
            {
                if (_availableVersion == value)
                {
                    return;
                }

                _availableVersion = value;
                RaisePropertyChanged(nameof(AvailableVersion));
            }
        }

        private int _selectedTab = 1;

        /// <summary>
        /// Sets and gets the SelectedTab property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int SelectedTab
        {
            get
            {
                return _selectedTab;
            }

            set
            {
                if (_selectedTab == value)
                {
                    return;
                }

                if (value == 1)
                {
                    foreach (var w in _widgets.OfType<vMixControlTextField>())
                    {
                        w.Update();
                    }
                }

                _selectedTab = value;
                RaisePropertyChanged(nameof(SelectedTab));
            }
        }

        private int _pageIndex = 0;

        /// <summary>
        /// Sets and gets the PageIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int PageIndex
        {
            get
            {
                return _pageIndex;
            }

            set
            {
                if (_pageIndex == value)
                {
                    return;
                }

                _pageIndex = value;
                RaisePropertyChanged(nameof(PageIndex));
            }
        }

        private void InsertWidgetByZIndex(vMixControl widget)
        {
            widget.IsGhosted = widget.ZIndex >= 0 && IsGhosted;
            if (widget.ZIndex < 0)
                _widgets.Insert(0, widget);
            else
                _widgets.Add(widget);
            RaisePropertyChanged(nameof(Widgets));
        }

        private static void EnsureWidgetIdentity(IEnumerable<vMixControl> widgets)
        {
            if (widgets == null)
                return;

            var used = new HashSet<Guid>();
            foreach (var widget in widgets)
            {
                if (widget == null)
                    continue;

                if (widget.WidgetId == Guid.Empty || !used.Add(widget.WidgetId))
                {
                    Guid next;
                    do
                    {
                        next = Guid.NewGuid();
                    }
                    while (!used.Add(next));

                    widget.WidgetId = next;
                }
            }
        }


        private void SaveUndo(string reason = "")
        {
            try
            {
                EnsureWidgetIdentity(Widgets);
                var currentWidgetsData = SerializeToBytes(Widgets);
                var currentWindowSettingsData = SerializeToBytes(WindowSettings);
                var snapshot = BuildUndoSnapshot(currentWidgetsData, currentWindowSettingsData);

                _undoStack.Add(new UndoEntry
                {
                    Reason = reason,
                    Snapshot = snapshot
                });

                if (_undoStack.Count > MaxUndoSteps)
                    _undoStack.RemoveAt(0);

                UpdateUndoPreview();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error when saving undo");
            }
        }

        private UndoSnapshot BuildUndoSnapshot(byte[] currentWidgetsData, byte[] currentWindowSettingsData)
        {
            if (_undoStack.Count == 0)
            {
                return new UndoSnapshot
                {
                    IsFullSnapshot = true,
                    WidgetsData = currentWidgetsData,
                    WindowSettingsData = currentWindowSettingsData
                };
            }

            var previousState = ResolveUndoState(_undoStack.Count - 1);
            var widgetsDelta = CreateDelta(previousState.WidgetsData, currentWidgetsData);
            var settingsDelta = CreateDelta(previousState.WindowSettingsData, currentWindowSettingsData);

            var widgetsDeltaSize = EstimateDeltaSize(widgetsDelta);
            var settingsDeltaSize = EstimateDeltaSize(settingsDelta);
            var fullSize = (currentWidgetsData?.Length ?? 0) + (currentWindowSettingsData?.Length ?? 0);

            // If delta is not actually smaller, keep full snapshot for this step.
            if (widgetsDeltaSize + settingsDeltaSize >= fullSize)
            {
                return new UndoSnapshot
                {
                    IsFullSnapshot = true,
                    WidgetsData = currentWidgetsData,
                    WindowSettingsData = currentWindowSettingsData
                };
            }

            return new UndoSnapshot
            {
                IsFullSnapshot = false,
                WidgetsDelta = widgetsDelta,
                WindowSettingsDelta = settingsDelta
            };
        }

        private static int EstimateDeltaSize(ByteArrayDelta delta)
        {
            if (delta == null)
                return 0;

            return sizeof(int) * 2 + (delta.InsertedData?.Length ?? 0);
        }

        private static byte[] SerializeToBytes<T>(T value)
        {
            if (value == null)
                return null;

            using (var stream = new MemoryStream())
            {
                var serializer = Utils.GetXmlSerializer(typeof(T));
                serializer.Serialize(stream, value);
                return stream.ToArray();
            }
        }

        private static T DeserializeFromBytes<T>(byte[] data)
        {
            if (data == null || data.Length == 0)
                return default(T);

            using (var stream = new MemoryStream(data))
            {
                var serializer = Utils.GetXmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stream);
            }
        }

        private static ByteArrayDelta CreateDelta(byte[] source, byte[] target)
        {
            source = source ?? Array.Empty<byte>();
            target = target ?? Array.Empty<byte>();

            var maxPrefix = Math.Min(source.Length, target.Length);
            var prefix = 0;
            while (prefix < maxPrefix && source[prefix] == target[prefix])
                prefix++;

            var sourceTail = source.Length - 1;
            var targetTail = target.Length - 1;
            var suffix = 0;
            while (sourceTail - suffix >= prefix &&
                   targetTail - suffix >= prefix &&
                   source[sourceTail - suffix] == target[targetTail - suffix])
            {
                suffix++;
            }

            var removedLength = source.Length - prefix - suffix;
            var insertedLength = target.Length - prefix - suffix;
            var insertedData = insertedLength > 0
                ? target.Skip(prefix).Take(insertedLength).ToArray()
                : Array.Empty<byte>();

            return new ByteArrayDelta
            {
                PrefixLength = prefix,
                RemovedLength = removedLength,
                InsertedData = insertedData
            };
        }

        private static byte[] ApplyDelta(byte[] source, ByteArrayDelta delta)
        {
            source = source ?? Array.Empty<byte>();
            if (delta == null)
                return source.ToArray();

            var prefixLength = Math.Max(0, Math.Min(delta.PrefixLength, source.Length));
            var removedLength = Math.Max(0, Math.Min(delta.RemovedLength, source.Length - prefixLength));
            var suffixStart = prefixLength + removedLength;
            var suffixLength = source.Length - suffixStart;
            var inserted = delta.InsertedData ?? Array.Empty<byte>();

            var result = new byte[prefixLength + inserted.Length + suffixLength];
            Buffer.BlockCopy(source, 0, result, 0, prefixLength);
            if (inserted.Length > 0)
                Buffer.BlockCopy(inserted, 0, result, prefixLength, inserted.Length);
            if (suffixLength > 0)
                Buffer.BlockCopy(source, suffixStart, result, prefixLength + inserted.Length, suffixLength);

            return result;
        }

        private UndoSnapshot ResolveUndoState(int index)
        {
            if (index < 0 || index >= _undoStack.Count)
                return new UndoSnapshot
                {
                    IsFullSnapshot = true,
                    WidgetsData = Array.Empty<byte>(),
                    WindowSettingsData = Array.Empty<byte>()
                };

            byte[] widgets = Array.Empty<byte>();
            byte[] settings = Array.Empty<byte>();
            for (var i = 0; i <= index; i++)
            {
                var step = _undoStack[i].Snapshot;
                if (step.IsFullSnapshot)
                {
                    widgets = step.WidgetsData ?? Array.Empty<byte>();
                    settings = step.WindowSettingsData ?? Array.Empty<byte>();
                }
                else
                {
                    widgets = ApplyDelta(widgets, step.WidgetsDelta);
                    settings = ApplyDelta(settings, step.WindowSettingsDelta);
                }
            }

            return new UndoSnapshot
            {
                IsFullSnapshot = true,
                WidgetsData = widgets,
                WindowSettingsData = settings
            };
        }

        private void LoadUndo()
        {
            using (PerfMetrics.Measure("undo.load"))
            {
                try
                {
                    if (_undoStack.Count > 0)
                    {
                        var state = ResolveUndoState(_undoStack.Count - 1);

                        var live = LIVE;

                        LIVE = true;

                        ApplyUndoWidgets(state.WidgetsData);

                        var restoredWindowSettings = DeserializeFromBytes<MainWindowSettings>(state.WindowSettingsData);
                        if (restoredWindowSettings != null)
                            WindowSettings = restoredWindowSettings;

                        CheckvMixConnection(null, new EventArgs());

                        vMixAPI.StateFabrique.Configure(WindowSettings.IP, WindowSettings.Port, WindowSettings.HttpLogin, WindowSettings.HttpPassword);

                        IsUrlValid = vMixAPI.StateFabrique.IsUrlValid(WindowSettings.IP, WindowSettings.Port);

                        SyncTovMixState();

                        LIVE = live;

                        _undoStack.RemoveAt(_undoStack.Count - 1);
                        UpdateUndoPreview();
                    }
                }
                catch (Exception e)
                {
                    PerfMetrics.Error("undo.load");
                    _logger.Error(e, "Error when applying undo");
                }
            }
        }

        private void ApplyUndoWidgets(byte[] widgetsData)
        {
            var restoredWidgets = DeserializeFromBytes<ObservableCollection<vMixControl>>(widgetsData)
                ?? new ObservableCollection<vMixControl>();

            EnsureWidgetIdentity(restoredWidgets);
            EnsureWidgetIdentity(_widgets);

            var currentById = _widgets.ToDictionary(x => x.WidgetId);
            var targetById = restoredWidgets.ToDictionary(x => x.WidgetId);

            var currentSerialized = _widgets.ToDictionary(x => x.WidgetId, SerializeToBytes);
            var targetSerialized = restoredWidgets.ToDictionary(x => x.WidgetId, SerializeToBytes);

            foreach (var id in currentById.Keys.Except(targetById.Keys).ToArray())
            {
                var removed = currentById[id];
                _widgets.Remove(removed);
                removed.Dispose();
                currentById.Remove(id);
            }

            for (int targetIndex = 0; targetIndex < restoredWidgets.Count; targetIndex++)
            {
                var target = restoredWidgets[targetIndex];
                if (currentById.TryGetValue(target.WidgetId, out var existing))
                {
                    var isSame = currentSerialized.TryGetValue(target.WidgetId, out var currentBytes)
                        && targetSerialized.TryGetValue(target.WidgetId, out var targetBytes)
                        && currentBytes.SequenceEqual(targetBytes);

                    if (!isSame)
                    {
                        var existingIndex = _widgets.IndexOf(existing);
                        existing.Dispose();
                        target.State = Model;
                        if (target is vMixControlTextField textField)
                            textField.IsLive = LIVE;
                        target.Update();
                        if (existingIndex >= 0)
                            _widgets[existingIndex] = target;
                        else
                            _widgets.Insert(Math.Min(targetIndex, _widgets.Count), target);
                        currentById[target.WidgetId] = target;
                    }

                    var actualIndex = _widgets.IndexOf(currentById[target.WidgetId]);
                    if (actualIndex >= 0 && actualIndex != targetIndex)
                        _widgets.Move(actualIndex, targetIndex);
                }
                else
                {
                    target.State = Model;
                    if (target is vMixControlTextField textField)
                        textField.IsLive = LIVE;
                    target.Update();
                    _widgets.Insert(Math.Min(targetIndex, _widgets.Count), target);
                    currentById[target.WidgetId] = target;
                }
            }

            RaisePropertyChanged(nameof(Widgets));
        }

        private void UpdateUndoPreview()
        {
            if (_undoStack.Count == 0)
            {
                UndoState = null;
                UndoReason = "";
                return;
            }

            var top = _undoStack[_undoStack.Count - 1];
            UndoState = top.Snapshot;
            UndoReason = top.Reason;
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
                        SaveUndo(string.Format(LocalizationManager.Instance["Undo.LockChanged"], p.Type, p.Name));
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
                        SaveUndo(string.Format(LocalizationManager.Instance["Undo.WidgetRemoved"], p.Type, p.Name));

                        int processed = 0;
                        foreach (var widget in Widgets.Where(x => x.Selected).ToArray())
                        {
                            Widgets.Remove(widget);
                            widget.Dispose();
                            processed++;
                        }
                        if (processed == 0)
                        {
                            p.Dispose();
                            Widgets.Remove(p);
                        }
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
                            vMixControl copy = null;
                            int processed = 0;
                            bool undoSaved = false;
                            //Process selection
                            foreach (var widget in Widgets.Where(x => x.Selected).ToArray())
                            {
                                copy = widget.Copy();
                                if (copy == null) continue;
                                if (!undoSaved)
                                {
                                    SaveUndo(LocalizationManager.Instance["Undo.WidgetCopied"]);
                                    undoSaved = true;
                                }
                                copy.Name = Utils.GetNextCopyName(copy.Name);
                                copy.State = Model;
                                copy.Left += 16;
                                copy.Top += 16;
                                copy.Update();
                                if (copy.ZIndex < 0)
                                    InsertWidgetByZIndex(copy);
                                else
                                {
                                    copy.ZIndex++;
                                    _widgets.Add(copy);
                                }
                                widget.Selected = false;
                                processed++;
                            }
                            if (processed == 0)
                            {
                                copy = p.Copy();
                                if (copy == null) return;
                                if (!undoSaved)
                                    SaveUndo(LocalizationManager.Instance["Undo.WidgetCopied"]);
                                copy.Name = Utils.GetNextCopyName(copy.Name);
                                copy.State = Model;
                                copy.Left += 16;
                                copy.Top += 16;
                                copy.Update();
                                if (copy.ZIndex < 0)
                                    InsertWidgetByZIndex(copy);
                                else
                                {
                                    copy.ZIndex++;
                                    _widgets.Add(copy);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Error while copying widget.");
                        }
                    }));
            }
        }


        private RelayCommand<ControlIntParameter> _moveWidgetCommand;

        /// <summary>
        /// Gets the MoveWidgetCommand.
        /// </summary>
        public RelayCommand<ControlIntParameter> MoveWidgetCommand
        {
            get
            {
                return _moveWidgetCommand
                    ?? (_moveWidgetCommand = new RelayCommand<ControlIntParameter>(
                    p =>
                    {
                        SaveUndo(LocalizationManager.Instance["Undo.WidgetPageChanged"]);
                        //Process selection
                        foreach (var widget in Widgets.Where(x => x.Selected))
                        {
                            widget.Page = p.B;
                            widget.Selected = false;
                        }
                        p.A.Page = p.B;
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
                        SaveUndo(LocalizationManager.Instance["Undo.WidgetCaptionShowHidden"]);
                        //Process selection
                        foreach (var widget in Widgets.Where(x => x.Selected))
                        {
                            widget.IsCaptionOn = !p.IsCaptionOn;
                        }
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
                        //SaveUndo("Widget scale increased");
                        //Process selection
                        foreach (var widget in Widgets.Where(x => x.Selected))
                        {
                            widget.Scale += 0.25f;
                        }
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
                        //SaveUndo("Widget scale decreased");
                        //Process selection
                        foreach (var widget in Widgets.Where(x => x.Selected))
                        {
                            if (widget.Scale - 0.25f >= 1.0f)
                                widget.Scale -= 0.25f;
                        }
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

                        _logger.Debug("Opening properties for widget {0}.", p.Name);
                        var viewModel = ServiceLocator.Current.GetInstance<vMixController.ViewModel.vMixWidgetSettingsViewModel>();
                        viewModel.Widget = p;
                        viewModel.SetProperties(p);


                        if (_settings == null)
                            _settings = new vMixWidgetSettingsView
                            {
                                Owner = App.Current.MainWindow
                            };
                        p.BeforePropertiesChanged();
                        var result = _settings.ShowDialog();
                        if (result.HasValue && result.Value)
                        {
                            SaveUndo(string.Format(LocalizationManager.Instance["Undo.WidgetPropertiesChanged"], p.Type, p.Name));
                            _settings.SaveConnectedWidgetProperties();
                        }
                        p.AfterPropertiesChanged();

                        //Analyze script loops
                        ScriptLoopAnalyzer.RefreshPotentialLoopWarnings(Widgets);

                        _logger.Debug("Properties updated.");
                        _settings = null;

                        IsHotkeysEnabled = true;

                        /*App.Current.MainWindow.Focus();
                        FocusManager.SetFocusedElement(null, null);*/
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
                        EditorCursor = CursorType.Cross.ToString();
                        _createWidget = new Action<Point>(x =>
                        {

                            var widget = (vMixControl)Assembly.GetAssembly(this.GetType()).CreateInstance("vMixController.Widgets.vMixControl" + p);

                            var count = _widgets.Where(y => y.GetType() == widget.GetType()).Count();

                            if (widget.MaxCount == -1 || widget.MaxCount > count)
                            {
                                widget.State = Model;
                                widget.Top = x.Y;
                                widget.Left = x.X;
                                widget.Page = PageIndex;
                                widget.AlignByGrid();
                                if (widget is vMixControlTextField)
                                    ((vMixControlTextField)widget).IsLive = LIVE;
                                widget.Update();


                                SaveUndo(string.Format(LocalizationManager.Instance["Undo.WidgetCreated"], widget.Type));
                                var undoStackDepthBeforeProperties = _undoStack.Count;

                                InsertWidgetByZIndex(widget);
                                if (widget.ZIndex >= 0)
                                    widget.ZIndex = Widgets.Count;

                                _logger.Debug("New {0} widget added.", widget.Type.ToLower());

                                OpenPropertiesCommand.Execute(widget);

                                while (_undoStack.Count > undoStackDepthBeforeProperties)
                                    _undoStack.RemoveAt(_undoStack.Count - 1);
                                UpdateUndoPreview();



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
                        var mw = App.Current?.MainWindow as vMixController.MainWindow;
                        if (mw?.LayoutGrid?.IsMouseCaptured == true)
                            mw.LayoutGrid.ReleaseMouseCapture();

                        if (SelectorWidth != 0 && SelectorHeight != 0)
                        {


                            var m = ((MatrixTransform)mw.CanvasContent.RenderTransform).Matrix;
                            m.Invert();

                            var tl = m.Transform(new Point(SelectorPosition.Left, SelectorPosition.Top));
                            var br = m.Transform(new Point(SelectorPosition.Left + SelectorWidth, SelectorPosition.Top + SelectorHeight));
                            var sr = new Rect(tl.X, tl.Y, br.X - tl.X, br.Y - tl.Y);

                            foreach (var item in _widgets)
                            {
                                var ir = new Rect(item.Left, item.Top, item.Width, double.IsNaN(item.Height) || double.IsInfinity(item.Height) ? 0 : item.Height + item.CaptionHeight);
                                item.Selected = (item.Selected || sr.Contains(ir)) && !item.Locked && item.Page == PageIndex;
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


                        if (_skipClick)
                        {
                            _skipClick = !_skipClick;
                            return;
                        }

                        if (_createWidget != null)
                        {
                            EditorCursor = "Arrow";
                            var pos = mw?.LayoutGrid != null
                            ? mw.ToCanvasContentPoint(Mouse.GetPosition(mw.LayoutGrid), WindowSettings.UseInfiniteCanvas)
                            : p.MouseDevice.GetPosition((IInputElement)p.Source);
                            _createWidget(new Point(pos.X / WindowSettings.UIScale, pos.Y / WindowSettings.UIScale));
                            _createWidget = null;
                        }

                        if (p.OriginalSource is ListView || p.OriginalSource is Grid)
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

                        var mw = App.Current?.MainWindow as vMixController.MainWindow;
                        var pos = mw?.LayoutGrid != null
                            ? (mw.ToCanvasContentPoint(Mouse.GetPosition(mw.LayoutGrid), WindowSettings.UseInfiniteCanvas))
                            : Mouse.GetPosition((IInputElement)p.Source);
                        if (mw?.LayoutGrid != null && !mw.LayoutGrid.IsMouseCaptured)
                            mw.LayoutGrid.CaptureMouse();

                        if (WindowSettings.UseInfiniteCanvas)
                            pos = ((MatrixTransform)mw.CanvasContent.RenderTransform).Matrix.Transform(pos);

                        _clickPoint = pos;
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
                        //MouseEventArgs
                        //_moveSource = p.Source;
                        var mw = App.Current?.MainWindow as vMixController.MainWindow;
                        var ipos = mw?.LayoutGrid != null
                            ? mw.ToCanvasContentPoint(Mouse.GetPosition(mw.LayoutGrid), WindowSettings.UseInfiniteCanvas)
                            : new Point(p.GetPosition(App.Current.MainWindow).X, p.GetPosition(App.Current.MainWindow).Y);

                        if (WindowSettings.UseInfiniteCanvas)
                            ipos = ((MatrixTransform)mw.CanvasContent.RenderTransform).Matrix.Transform(ipos);

                        if (!SelectorEnabled)
                        {

                            _clickPoint = new Point(ipos.X, ipos.Y);
                            return;
                        }


                        var pos = new Point(ipos.X, ipos.Y) - _clickPoint + _relativeClickPoint;//Mouse.PrimaryDevice.GetPosition((IInputElement)_moveSource);
                        var w = -(_rawSelectorPosition.Left - pos.X);
                        var h = -(_rawSelectorPosition.Top - pos.Y);

                        SelectorPosition = new Thickness(w < 0 ? pos.X : _rawSelectorPosition.Left, h < 0 ? pos.Y : _rawSelectorPosition.Top, 0, 0);
                        SelectorWidth = Math.Abs(w);
                        SelectorHeight = Math.Abs(h);
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

                        var mw = App.Current?.MainWindow as MainWindow;
                        if (mw?.LayoutGrid != null)
                        {
                            var pos = Mouse.GetPosition(mw.LayoutGrid);
                            _contextMenuPosition = mw.ToCanvasContentPoint(pos, WindowSettings.UseInfiniteCanvas);
                        }
                        else
                        {
                            _contextMenuPosition = Mouse.GetPosition((IInputElement)p.Source);
                        }
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
                            SaveUndo(string.Format(LocalizationManager.Instance["Undo.WidgetCreatedFromTemplate"], p.A));
                            var count = _widgets.Where(y => y.GetType() == p.B.GetType()).Count();
                            if (p.B.MaxCount == -1 || count < p.B.MaxCount)
                            {
                                var ctrl = p.B.Copy();
                                ctrl.Left = x.X;
                                ctrl.Top = x.Y;
                                ctrl.IsTemplate = false;
                                ctrl.State = Model;
                                ctrl.Page = PageIndex;
                                ctrl.AlignByGrid();
                                ctrl.Update();
                                _logger.Debug("Widget \"{0}\" was copied.", p.B.Name);
                                InsertWidgetByZIndex(ctrl);
                                ctrl.ZIndex = Widgets.Count;
                            }
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
                        SaveUndo(LocalizationManager.Instance["Undo.ControllerCreated"]);

                        foreach (var item in _widgets)
                            item.Dispose();

                        WindowSettings.Password = null;
                        WindowSettings.UserName = null;
                        WindowSettings.Locked = false;

                        ((ViewModelLocator)App.Current.FindResource("Locator"))?.GlobalSettings?.Variables.Clear();

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
                            Filter = LocalizationManager.Instance["Dialog.FileFilter.VmixController"]
                        };
                        var result = opendlg.ShowDialog(App.Current.MainWindow);
                        if (result.HasValue && result.Value)
                        {
                            SaveUndo(LocalizationManager.Instance["Undo.ControllerLoaded"]);
                            LoadControllerFromFile(opendlg.FileName);

                            //Analyze script loops
                            ScriptLoopAnalyzer.RefreshPotentialLoopWarnings(Widgets);

                            WindowSettings.AddRecentFile(opendlg.FileName);
                        }
                    }));
            }
        }

        private RelayCommand<string> _loadRecentControllerCommand;

        /// <summary>
        /// Gets the LoadControllerCommand.
        /// </summary>
        public RelayCommand<string> LoadRecentControllerCommand
        {
            get
            {
                return _loadRecentControllerCommand
                    ?? (_loadRecentControllerCommand = new RelayCommand<string>(
                    (path) =>
                    {
                        if (File.Exists(path))
                        {
                            SaveUndo(LocalizationManager.Instance["Undo.ControllerLoaded"]);
                            LoadControllerFromFile(path);

                            //Analyze script loops
                            ScriptLoopAnalyzer.RefreshPotentialLoopWarnings(Widgets);

                            WindowSettings.AddRecentFile(path);
                        }
                    }));
            }
        }

        private RelayCommand _appendControllerCommand;

        /// <summary>
        /// Gets the AppendControllerCommand.
        /// </summary>
        public RelayCommand AppendControllerCommand
        {
            get
            {
                return _appendControllerCommand
                    ?? (_appendControllerCommand = new RelayCommand(
                    () =>
                    {
                        Ookii.Dialogs.Wpf.VistaOpenFileDialog opendlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
                        {
                            Filter = "vMix Controller|*.vmc"
                        };
                        var result = opendlg.ShowDialog(App.Current.MainWindow);
                        if (result.HasValue && result.Value)
                        {
                            SaveUndo(LocalizationManager.Instance["Undo.ControllerLoaded"]);

                            MainWindowSettings ws;
                            var w = Utils.LoadController(opendlg.FileName, Functions, out ws);
                            EditorCursor = "Hand";
                            _createWidget = x =>
                            {
                                var mintop = double.MaxValue;
                                var minleft = double.MaxValue;
                                foreach (var wgt in w)
                                {
                                    if (wgt.Top < mintop) mintop = wgt.Top;
                                    if (wgt.Left < minleft) minleft = wgt.Left;
                                }
                                foreach (var wgt in w)
                                {
                                    wgt.Left += x.X - minleft;
                                    wgt.Top += x.Y - mintop;
                                    wgt.Selected = true;
                                    wgt.Page = PageIndex;
                                    wgt.AlignByGrid();
                                    _widgets.Add(wgt);
                                }

                                //Analyze script loops
                                ScriptLoopAnalyzer.RefreshPotentialLoopWarnings(Widgets);

                            };
                            _skipClick = true;
                        }
                    }));
            }
        }

        private void LoadControllerFromFile(string opendlg)
        {
            try
            {
                ControllerPath = opendlg;

                //IsWidgetsVisualLoading = true;

                foreach (var item in _widgets)
                    item.Dispose();
                _widgets.Clear();

                LIVE = true;

                var ol = _windowSettings.OpenLastAtStart;
                var recent = (ObservableCollection<string>)_windowSettings.RecentFiles.Copy();

                foreach (var item in Utils.LoadController(opendlg, Functions, out _windowSettings))
                    _widgets.Add(item);


                _windowSettings.OpenLastAtStart = ol;
                _windowSettings.RecentFiles = recent;

                NormalizeMainWindowPlacement(WindowSettings);

                foreach (var item in _widgets)
                {
                    item.IsVisualReady = false;
                    item.Update();
                }

                RaisePropertyChanged(nameof(WindowSettings));
                _logger.Debug("Configuring API.");

                CheckvMixConnection(null, new EventArgs());

                vMixAPI.StateFabrique.Configure(WindowSettings.IP, WindowSettings.Port, WindowSettings.HttpLogin, WindowSettings.HttpPassword);

                IsUrlValid = vMixAPI.StateFabrique.IsUrlValid(WindowSettings.IP, WindowSettings.Port);

                SyncTovMixState();
                ScriptLoopAnalyzer.RefreshPotentialLoopWarnings(Widgets);

                //IsWidgetsVisualLoading = false;

            }
            catch (Exception e)
            {
                //IsWidgetsVisualLoading = false;
                _logger.Error(e, "Error while loading controller");
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
                        {
                            Utils.SaveController(opendlg.FileName, _widgets, _windowSettings);
                            WindowSettings.AddRecentFile(opendlg.FileName);
                        }

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


                                TextInputWindow twi = new TextInputWindow()
                                {
                                    Mode = InputTextWindowMode.NewPassword,
                                    Title = LocalizationManager.Instance["Dialog.PasswordLock.Title"],
                                    InputTitle = LocalizationManager.Instance["Dialog.PasswordLock.InputTitle"],
                                };

                                /*Ookii.Dialogs.Wpf.CredentialDialog cred = new Ookii.Dialogs.Wpf.CredentialDialog
                                {
                                    ShowSaveCheckBox = false,
                                    ShowUIForSavedCredentials = false,
                                    Target = "vMixUTC",
                                    MainInstruction = "Enter password to lock controller"
                                };*/
                                if (twi.ShowDialog() ?? false)
                                {
                                    if (twi.Password == twi.PasswordConfirmation)
                                    {
                                        WindowSettings.Password = ToMD5Hash(twi.Password);
                                        WindowSettings.UserName = ToMD5Hash(twi.Text);
                                    }
                                    else
                                    {
                                        Ookii.Dialogs.Wpf.TaskDialog td = new Ookii.Dialogs.Wpf.TaskDialog();
                                        td.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Ok));
                                        td.MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Error;
                                        td.MainInstruction = LocalizationManager.Instance["Dialog.Error.PasswordMismatch"];
                                        td.ShowDialog();
                                    }
                                }
                            }
                            SelectedTab = 1;
                            PageIndex = 0;
                        }
                        else
                        if (!string.IsNullOrWhiteSpace(WindowSettings.UserName) || !string.IsNullOrWhiteSpace(WindowSettings.Password))
                        {
                            TextInputWindow twi = new TextInputWindow()
                            {
                                Mode = InputTextWindowMode.Password,
                                Title = LocalizationManager.Instance["Dialog.PasswordUnlock.Title"],
                                InputTitle = LocalizationManager.Instance["Dialog.PasswordUnlock.InputTitle"]
                            };
                            /*Ookii.Dialogs.Wpf.CredentialDialog cred = new Ookii.Dialogs.Wpf.CredentialDialog
                            {
                                ShowSaveCheckBox = false,
                                ShowUIForSavedCredentials = false,
                                MainInstruction = "Enter password to unlock controller",
                                Target = "UTC",
                                WindowTitle = "Universal Title Controller"
                            };*/
                            if (twi.ShowDialog() ?? false)
                            {

                                if (ToMD5Hash(twi.Password) == WindowSettings.Password && ToMD5Hash(twi.Text) == WindowSettings.UserName)
                                {
                                    WindowSettings.Password = null;
                                    WindowSettings.UserName = null;
                                }
                                else
                                {
                                    Ookii.Dialogs.Wpf.TaskDialog td = new Ookii.Dialogs.Wpf.TaskDialog();
                                    td.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Ok));
                                    td.MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Error;
                                    td.MainInstruction = LocalizationManager.Instance["Dialog.Error.IncorrectPassword"];
                                    td.ShowDialog();
                                    return;
                                }
                            }
                            else return;
                        }
                        //SaveUndo(!WindowSettings.Locked ? "Controller widgets locked" : "Controller widgets unlocked");
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
                    ?? (_switchPasswordLockableCommand = new RelayCommand<vMixController.Widgets.vMixControl>(
                    (p) =>
                    {
                        //SaveUndo(string.Format("Widget {1}[{0}] password lockability changed", p.Type, p.Name));
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

        private RelayCommand _stopAllScriptsCommand;

        public RelayCommand StopAllScriptsCommand
        {
            get
            {
                return _stopAllScriptsCommand
                    ?? (_stopAllScriptsCommand = new RelayCommand(
                    () =>
                    {
                        foreach (var widget in Widgets)
                        {
                            if (widget is vMixControlButton btn)
                                btn.ExecuteHotkey(btn.GetHotkeyNumber("Reset"));
                            if (widget is vMixControlNewButton nbtn)
                                nbtn.ExecuteHotkey(nbtn.GetHotkeyNumber("Reset"));
                        }
                    }));
            }
        }

        private void SyncTovMixState()
        {
            using (PerfMetrics.Measure("sync.to-vmix-state"))
            {
                _logger.Debug("Syncing to vMix state.");
                {
                    if (Model == null || (Model.Ip != WindowSettings.IP || Model.Port != WindowSettings.Port))
                    {
                        Model = null;
                        vMixAPI.StateFabrique.Configure(WindowSettings.IP, WindowSettings.Port, WindowSettings.HttpLogin, WindowSettings.HttpPassword);
                        vMixAPI.StateFabrique.CreateAsync();
                    }
                    else
                    {
                        Model.Configure(WindowSettings.IP, WindowSettings.Port, WindowSettings.HttpLogin, WindowSettings.HttpPassword);
                        Model.UpdateAsync();
                    }
                }
            }
        }

        private void LinkGlobalVariables(vMixAPI.State state)
        {
            var globalSettings = ((ViewModelLocator)App.Current.FindResource("Locator"))?.GlobalSettings;

            if (state == null || state.Inputs.Where(x => x.Key == "vmix-utc-internal-gv").FirstOrDefault() != null) return;
            var input = new vMixAPI.Input() { Number = -99, Title = LocalizationManager.Instance["MainViewModel.GlobalVariables.InputTitle"], Key = "vmix-utc-internal-gv" };
            int index = 0;
            foreach (var v in globalSettings.Variables)
            {
                var t = new InputText() { Text = v.B, Index = index++ };
                t.Name = v.A + ".Value";
                Binding b = new Binding("Text");
                b.Source = t;
                b.Mode = BindingMode.TwoWay;
                BindingOperations.SetBinding(v, Pair<string, string>.BProperty, b);

                input.Elements.Add(t);
            }
            state.Inputs.Insert(0, input);
        }

        private void LocalizeVirtualInputs(vMixAPI.State state)
        {
            if (state?.Inputs == null) return;

            var active = state.Inputs.FirstOrDefault(x => x.Key == "-1");
            if (active != null)
                active.Title = LocalizationManager.Instance["MainViewModel.VirtualInputs.Active"];

            var preview = state.Inputs.FirstOrDefault(x => x.Key == "0");
            if (preview != null)
                preview.Title = LocalizationManager.Instance["MainViewModel.VirtualInputs.Preview"];
        }

        private void State_OnStateCreated(object sender, EventArgs e)
        {
            if (Model != null)
                Model.OnStateSynced -= Model_OnStateUpdated;
            Model = (vMixAPI.State)sender;
            LinkGlobalVariables(Model);
            LocalizeVirtualInputs(Model);
            foreach (var item in _widgets)
                item.State = Model;
            if (Model != null)
                Model.OnStateSynced += Model_OnStateUpdated;
            CheckvMixConnection(null, new EventArgs());
        }

        private void Model_OnStateUpdated(object sender, vMixAPI.StateSyncedEventArgs e)
        {
            if (!e.Successfully)
            {
                Model = null;
                State_OnStateCreated(null, null);
                Status = Status.Offline;
            }
            else
            {

                var instate = (vMixAPI.State)sender;

                LinkGlobalVariables(instate);
                LocalizeVirtualInputs(instate);

                foreach (var item in _widgets)
                    item.State = (vMixAPI.State)sender;
            }
            CheckvMixConnection(null, new EventArgs());
        }


        bool ProcessHotkey(Key key, Key systemKey, ModifierKeys modifiers, bool onPress = true)
        {
            if (Keyboard.FocusedElement is System.Windows.Controls.Primitives.TextBoxBase ||
                Keyboard.FocusedElement is System.Windows.Controls.PasswordBox)
                return false;

            FocusManager.SetFocusedElement(App.Current.MainWindow, (IInputElement)App.Current.MainWindow);

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

                    if (item.obj.Active && ((item.obj.Key == key) || (key == Key.System && item.obj.Key == systemKey)) && modifiers == mod && item.obj.OnPress == onPress)
                    {
                        ctrl.ExecuteHotkey(item.idx);
                        result = true;
                    }
                }
            }

            return result;
        }
        void ProcessHotkey(string link, object parameter = null)
        {
            if (string.IsNullOrWhiteSpace(link))
                return;

            var normalizedLink = link.Trim();
            ScriptExecutionChainToken incomingChain = null;
            object payload = parameter;
            if (parameter is ScriptExecutionDispatchPayload wrapped)
            {
                incomingChain = wrapped.ChainToken;
                payload = wrapped.Payload;
            }

            var chain = _scriptExecutionLoopGuard.CreateOrContinueChain(incomingChain, normalizedLink, DateTime.UtcNow);
            var previousChain = ScriptExecutionDispatchRuntime.Push(chain);
            var entered = false;
            try
            {
                if (WindowSettings != null && !WindowSettings.EnableLoopGuard)
                {
                    foreach (var ctrl in _widgets)
                        for (int i = 0; i < ctrl.Hotkey.Length; i++)
                        {
                            if (ctrl.Hotkey[i].Link == link && ctrl.Hotkey[i].Active)
                                if (ctrl is vMixControlButton || ctrl is vMixControlNewButton)
                                    ctrl.ExecuteHotkey(i, new ScriptExecutionDispatchPayload { ChainToken = chain, Payload = payload });
                                else if (payload != null)
                                    ctrl.ExecuteHotkey(i, payload);
                                else
                                    ctrl.ExecuteHotkey(i);
                        }

                    return;
                }

                var reentrantCycleResult = _scriptExecutionLoopGuard.TryEnterLink(chain, normalizedLink);
                if (reentrantCycleResult != null && !reentrantCycleResult.IsAllowed)
                {
                    HandleScriptLoopGuard(reentrantCycleResult);
                    return;
                }

                entered = true;

                var guardResult = _scriptExecutionLoopGuard.CheckAndRegister(chain, normalizedLink, DateTime.UtcNow);
                if (!guardResult.IsAllowed)
                {
                    HandleScriptLoopGuard(guardResult);
                    return;
                }

                foreach (var ctrl in _widgets)
                    for (int i = 0; i < ctrl.Hotkey.Length; i++)
                    {
                        if (ctrl.Hotkey[i].Link == link && ctrl.Hotkey[i].Active)
                            if (ctrl is vMixControlButton || ctrl is vMixControlNewButton)
                                ctrl.ExecuteHotkey(i, new ScriptExecutionDispatchPayload { ChainToken = chain, Payload = payload });
                            else if (payload != null)
                                ctrl.ExecuteHotkey(i, payload);
                            else
                                ctrl.ExecuteHotkey(i);
                    }
            }
            finally
            {
                if (entered)
                    _scriptExecutionLoopGuard.ExitLink(chain, normalizedLink);

                ScriptExecutionDispatchRuntime.Pop(previousChain);
            }
        }

        private void HandleScriptLoopGuard(ScriptExecutionLoopGuardResult result)
        {
            _logger.Warn("Script execution loop guard blocked exec link chain. Chain='{0}', Root='{1}', Current='{2}', Hops={3}, HitsInWindow={4}, RetryAfterMs={5}.",
                result.Chain?.Id.ToString("N"),
                result.Chain?.RootLink,
                result.CurrentLink,
                result.Chain?.HopCount ?? 0,
                result.HitsInWindow,
                (int)Math.Max(0, result.RetryAfter.TotalMilliseconds));

            if (!result.JustTriggered)
                return;

            var nowUtc = DateTime.UtcNow;
            if ((nowUtc - _lastScriptLoopGuardWarningUtc) < ScriptLoopGuardWarningThrottle)
                return;

            _lastScriptLoopGuardWarningUtc = nowUtc;

            Action showWarning = () =>
            {
                try
                {
                    var title = LocalizationManager.Instance["Dialog.Warning.PossibleScriptError.Title"];
                    
                    var content = string.Format(LocalizationManager.Instance["Dialog.Warning.PossibleScriptError.Loop"], result.Chain?.RootLink, result.CurrentLink, result.Chain?.HopCount ?? 0, result.HitsInWindow);
                    var dialog = new Ookii.Dialogs.Wpf.TaskDialog
                    {
                        WindowTitle = string.IsNullOrWhiteSpace(title) ? "Warning" : title,
                        MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Warning,
                        Content = content
                    };
                    dialog.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Ok));
                    dialog.ShowDialog();
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Failed to show script loop guard warning dialog.");
                }
            };

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
                dispatcher.BeginInvoke((Action)showWarning);
            else
                showWarning();
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
                        //Debug.Print("Key Up");
                        _isPressed = false;
                        if (!IsHotkeysEnabled)
                            return;
                        p.Handled = ProcessHotkey(p.Key, p.SystemKey, p.KeyboardDevice.Modifiers, false);
                        //p.Handled = true;

                    }));
            }
        }

        private RelayCommand<KeyEventArgs> _previewKeyDownCommand;

        /// <summary>
        /// Gets the PreviewKeyDownCommand.
        /// </summary>
        public RelayCommand<KeyEventArgs> PreviewKeyDownCommand
        {
            get
            {
                return _previewKeyDownCommand
                    ?? (_previewKeyDownCommand = new RelayCommand<KeyEventArgs>(
                    p =>
                    {
                        //Debug.Print("Key Down");
                        if (_createWidget != null && p.Key == Key.Escape)
                        {
                            _createWidget = null;
                            EditorCursor = Cursors.Arrow.ToString();
                        }


                        if (!IsHotkeysEnabled || _isPressed)
                            return;
                        _isPressed = true;
                        p.Handled = ProcessHotkey(p.Key, p.SystemKey, p.KeyboardDevice.Modifiers, true);
                        //p.Handled = true;
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
                        XmlSerializer s = Utils.GetXmlSerializer(typeof(ObservableCollection<Pair<string, vMixControl>>));
                        using (var fs = new FileStream(Path.Combine(_documentsPath, "Templates.xml"), FileMode.Create))
                            s.Serialize(fs, _widgetTemplates);

                        _logger.Info("Saving window settings.");
                        s = Utils.GetXmlSerializer(typeof(MainWindowSettings));
                        using (var fs = new FileStream(Path.Combine(_documentsPath, "WindowSettings.xml"), FileMode.Create))
                            s.Serialize(fs, _windowSettings);


                        _logger.Info("Saving last controller");
                        using (var fs = new FileStream(Path.Combine(_documentsPath, "Last.vmc"), FileMode.Create))
                            Utils.SaveController(fs, Widgets, WindowSettings);

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


        private RelayCommand _openLogFolder;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand OpenLogFolder
        {
            get
            {
                return _openLogFolder
                    ?? (_openLogFolder = new RelayCommand(
                    () =>
                    {
                        try
                        {
                            Process.Start(Path.Combine(_documentsPath, "logs"));
                        }
                        catch (Exception) { }
                    }));
            }
        }

        private RelayCommand _resetScalingCommand;

        /// <summary>
        /// Gets the ResetScalingCommand.
        /// </summary>
        public RelayCommand ResetScalingCommand
        {
            get
            {
                return _resetScalingCommand
                    ?? (_resetScalingCommand = new RelayCommand(
                    () =>
                    {
                        if (WindowSettings != null)
                            WindowSettings.UIScale = 1;
                    }));
            }
        }

        DispatcherTimer _connectTimer = new DispatcherTimer();
        DispatcherTimer _metricsTimer = new DispatcherTimer();
        Classes.VmixTcpSubscriber _tcpSubscriber = new Classes.VmixTcpSubscriber();

        string _documentsPath;

        List<vMixControl> _intersections = new List<vMixControl>();
        private readonly HashSet<vMixControl> _movedWidgetsBuffer = new HashSet<vMixControl>();
        private readonly List<vMixControlRegion> _selectedStickyRegionsBuffer = new List<vMixControlRegion>();


        private static Rect ToRect(System.Drawing.Rectangle bounds)
        {
            return new Rect(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
        }

        private static double IntersectionArea(Rect a, Rect b)
        {
            var intersection = Rect.Intersect(a, b);
            if (intersection.IsEmpty || intersection.Width <= 0 || intersection.Height <= 0)
                return 0;

            return intersection.Width * intersection.Height;
        }

        private static void NormalizeMainWindowPlacement(MainWindowSettings settings)
        {
            if (settings == null)
                return;

            var screens = WpfScreenHelper.Screen.AllScreens?.ToArray() ?? Array.Empty<WpfScreenHelper.Screen>();
            if (screens.Length == 0)
                return;

            var primary = WpfScreenHelper.Screen.PrimaryScreen ?? screens[0];
            var primaryWork = primary.WorkingArea;

            var minWidth = 400.0;
            var minHeight = 400.0;
            var maxWidth = Math.Max(minWidth, screens.Max(s => s.WorkingArea.Width) - 32);
            var maxHeight = Math.Max(minHeight, screens.Max(s => s.WorkingArea.Height) - 32);

            settings.Width = Math.Max(minWidth, Math.Min(settings.Width, maxWidth));
            settings.Height = Math.Max(minHeight, Math.Min(settings.Height, maxHeight));

            var windowRect = new Rect(settings.Left, settings.Top, settings.Width, settings.Height);
            var workingAreas = screens.Select(x => x.WorkingArea).ToArray();
            var windowArea = Math.Max(1.0, windowRect.Width * windowRect.Height);
            var visibleArea = workingAreas.Sum(area => IntersectionArea(windowRect, area));
            var visibleRatio = visibleArea / windowArea;

            var titleHeight = Math.Max(24.0, SystemParameters.CaptionHeight + 8.0);
            var titleRect = new Rect(windowRect.Left, windowRect.Top, windowRect.Width, titleHeight);
            var isTitleVisible = workingAreas.Any(area => area.IntersectsWith(titleRect));

            if (visibleRatio < 0.2 || !isTitleVisible)
            {
                settings.Left = primaryWork.Left + 64;
                settings.Top = primaryWork.Top + 64;

                settings.Left = Math.Max(primaryWork.Left, Math.Min(settings.Left, primaryWork.Right - settings.Width));
                settings.Top = Math.Max(primaryWork.Top, Math.Min(settings.Top, primaryWork.Bottom - settings.Height));
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            LocalizationManager.Instance.CultureChanged += LocalizationManager_CultureChanged;

            _logger.Info(Environment.Version);
            _logger.Info(Environment.OSVersion);
            ThreadPool.GetAvailableThreads(out int t1, out int t2);
            _logger.Info("Worker Threads:Completion Pool Threads - {0}:{1}", t1, t2);
            //Fix changing current directory on opening .vmc from external folder
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            XmlDocumentMessenger.Start();

            vMixAPI.StateFabrique.OnStateCreated += State_OnStateCreated;

            _connectTimer.Interval = TimeSpan.FromSeconds(20);
            _connectTimer.Tick += CheckvMixConnection;
            _connectTimer.Start();

            _metricsTimer.Interval = TimeSpan.FromSeconds(30);
            _metricsTimer.Tick += (sender, args) =>
            {
                var snapshot = PerfMetrics.SnapshotAndReset();
                if (!string.IsNullOrWhiteSpace(snapshot))
                    _logger.Info(snapshot);
            };
            _metricsTimer.Start();

            //For loading NCalc before adding buttons to avoid throttle at button click
            var _expression = new NCalc.SafeExpression("1+1");
            _expression.TryEvaluate(out _, out _);

            _logger.Info("Loading mapped functions.");
            XmlSerializer s = Utils.GetXmlSerializer(typeof(ObservableCollection<vMixFunctionReference>));
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
            s = Utils.GetXmlSerializer(typeof(ObservableCollection<vMixNewFunctionReference>));
            try
            {
                if (File.Exists("NewFunctions.xml"))
                    using (var fs = new FileStream("NewFunctions.xml", FileMode.Open))
                        _newFunctions = (ObservableCollection<vMixNewFunctionReference>)s.Deserialize(fs);
                _newFunctions = new ObservableCollection<vMixNewFunctionReference>(_newFunctions.OrderBy(x => x.Function).GroupBy(x => x.Category == "Native" ? "_" : x.Category).OrderBy(x => x.Key).SelectMany(x => x.ToArray()).ToArray());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while loading new mapped functions.");
            }

            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _documentsPath = Path.Combine(documents, "vMix UTC");
            if (!Directory.Exists(_documentsPath))
                Directory.CreateDirectory(_documentsPath);

            try
            {
                _logger.Info("Loading templates.");
                s = Utils.GetXmlSerializer(typeof(ObservableCollection<Pair<string, vMixControl>>));
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
                    B = new vMixControlExternalData()
                    {
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
                catch (Exception e)
                {
                    _logger.Error(e, "Failed loading data provider. {0}");
                }

            }


            try
            {
                _logger.Info("Loading window settings.");
                s = Utils.GetXmlSerializer(typeof(MainWindowSettings));
                if (File.Exists(Path.Combine(_documentsPath, "WindowSettings.xml")))
                    using (var fs = new FileStream(Path.Combine(_documentsPath, "WindowSettings.xml"), FileMode.Open))
                        WindowSettings = (MainWindowSettings)s.Deserialize(fs);
                else
                    WindowSettings = new MainWindowSettings();

                NormalizeMainWindowPlacement(WindowSettings);
                RaisePropertyChanged(nameof(WindowSettings));

            }
            catch (Exception)
            {
                _windowSettings = new MainWindowSettings();
            }

            if (_windowSettings.Pages.Count != 7)
            {
                _windowSettings.Pages.Clear();
                _windowSettings.Pages.Add(Utils.MAINPAGENAME);
                _windowSettings.Pages.Add(Utils.DATAPAGENAME);
                _windowSettings.Pages.Add(Utils.PAGE1PAGENAME);
                _windowSettings.Pages.Add(Utils.PAGE2PAGENAME);
                _windowSettings.Pages.Add(Utils.PAGE3PAGENAME);
                _windowSettings.Pages.Add(Utils.PAGE4PAGENAME);
                _windowSettings.Pages.Add(Utils.PAGE5PAGENAME);
            }

            if (Model == null)
            {
                vMixAPI.StateFabrique.Configure(_windowSettings.IP, _windowSettings.Port, _windowSettings.HttpLogin, _windowSettings.HttpPassword);
                vMixAPI.StateFabrique.CreateAsync();
            }

            MessengerInstance.Register<HotkeyLinkMessage>(this, (hk) =>
            {
                ProcessHotkey(hk.Link, hk.Parameter);
            });

            MessengerInstance.Register<string>(this, (hk) =>
            {
                ProcessHotkey(hk, null);
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


            Messenger.Default.Register<LIVEToggleMessage>(this, (msg) =>
            {
                switch (msg.State)
                {
                    case 0: LIVE = false; break;
                    case 1: LIVE = true; break;
                    case 2: LIVE = !LIVE; break;
                }
            });

            Messenger.Default.Register<LoadingMessage>(this, (msg) =>
            {
                IsLoading = msg.Loading;
            });


            Messenger.Default.Register<WidgetMoveDeltaMessage>(this, (t) =>
            {
                _movedWidgetsBuffer.Clear();
                foreach (var widget in _widgets)
                    if (widget.Selected && widget != t.Widget)
                        _movedWidgetsBuffer.Add(widget);
                foreach (var widget in _intersections)
                    _movedWidgetsBuffer.Add(widget);

                if (_movedWidgetsBuffer.Count == 0)
                    return;

                foreach (var item in _movedWidgetsBuffer)
                {
                    item.Left = Math.Round(item.Left + t.DeltaX);
                    item.Top = Math.Round(item.Top + t.DeltaY);
                }
            });

            Messenger.Default.Register<WidgetMoveStateMessage>(this, (t) =>
            {
                _selectedStickyRegionsBuffer.Clear();
                foreach (var widget in _widgets)
                    if (widget.Selected && widget is vMixControlRegion selectedRegion && selectedRegion.Sticky)
                        _selectedStickyRegionsBuffer.Add(selectedRegion);

                if (!(t.Widget is vMixControlRegion reg) || (_selectedStickyRegionsBuffer.Count == 0 && !reg.Sticky))
                    return;
                _selectedStickyRegionsBuffer.Add(reg);


                if (!t.IsStarted)
                {
                    _intersections.Clear();
                }
                else
                {
                    _intersections.Clear();
                    var selectedSet = new HashSet<vMixControl>(_selectedStickyRegionsBuffer.Cast<vMixControl>());
                    var intersectionsSet = new HashSet<vMixControl>();
                    foreach (var rgn in _selectedStickyRegionsBuffer)
                    {
                        foreach (var item in _widgets)
                            if (rgn.Intersect(item) && item.Page == rgn.Page && !selectedSet.Contains(item) && intersectionsSet.Add(item))
                                _intersections.Add(item);
                    }
                }

            });

            Messenger.Default.Register<HotkeysEnabledMessage>(this, (t) =>
            {
                IsHotkeysEnabled = t.IsEnabled;
            });

            Messenger.Default.Register<SyncStateRequestMessage>(this, (t) =>
            {
                if (t.Force)
                    SyncTovMixState();
            });

            Messenger.Default.Register<PageNavigationMessage>(this, (t) =>
            {
                switch (t.Mode)
                {
                    case PageNavigationMode.Next:
                        PageIndex = (PageIndex + 1) % 7;
                        break;
                    case PageNavigationMode.Previous:
                        PageIndex = Math.Max(0, PageIndex - 1);
                        break;
                    case PageNavigationMode.SetIndex:
                        PageIndex = t.PageIndex;
                        break;
                }
            });

            Messenger.Default.Register<WidgetEditMessage>(this, (msg) =>
            {
                if (msg?.Widget == null || msg.Widget.Locked || !msg.IsStarted)
                    return;

                switch (msg.Action)
                {
                    case WidgetEditAction.Move:
                        SaveUndo(LocalizationManager.Instance["Undo.WidgetMoved"]);
                        break;
                    case WidgetEditAction.Resize:
                        SaveUndo(LocalizationManager.Instance["Undo.WidgetResized"]);
                        break;
                }
            });
            if (!IsInDesignMode)
            {
                //var globalEvents = Gma.System.MouseKeyHook.Hook.AppEvents();
                //globalEvents.MouseMove += MainViewModel_MouseMove;
                //globalEvents.MouseUp += MainViewModel_MouseUp;
            }

            IsUrlValid = vMixAPI.StateFabrique.IsUrlValid(WindowSettings.IP, WindowSettings.Port);

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
            else if (WindowSettings.OpenLastAtStart)
            {
                var last = Path.Combine(_documentsPath, "Last.vmc");
                if (File.Exists(last))
                    try
                    {
                        _logger.Info("Trying to load {0}.", last);
                        LoadControllerFromFile(last);

                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Error loading controller. {0}");
                    }
            }
            _ = CheckUpdate();

        }

        private const string css = "p { margin-left: 3em; }";

        private async Task CheckUpdate()
        {
            //https://forums.vmix.com/posts/t6468--FREE--Universal-Title-Controller
            ///html/body/form/div/div[3]/div/table[3]/tbody/tr[2]/td[2]/div/div/div/span[6]/strong/span
            ///html/body/form/div/div[3]/div/table[3]/tbody/tr[2]/td[2]/div/div/div/div[1]

            _logger.Info("Checking for updates");
            try
            {
                Octokit.GitHubClient client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("vMixUTC"));
                IReadOnlyList<Octokit.Release> _releases = await client.Repository.Release.GetAll("elgarf", "vMixUTC");
                foreach (var release in _releases)
                {
                    var version = DateTime.Parse(release.Name, new CultureInfo("RU-ru"));
                    var build = GetBuildDateTime(Assembly.GetExecutingAssembly());

                    if (build < version)
                    {
                        Title += " [Update Available]";
                        UpdateLink = release.HtmlUrl;
                        AvailableVersion = version;
                        return;
                    }

                    Debug.WriteLine(release.Name);

                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while checking updates. {0}");
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
                    item.Selected = (item.Selected || sr.Contains(ir)) && !item.Locked && item.Page == PageIndex;
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
            var ipos = new Point(e.X, e.Y);
            if (!SelectorEnabled)
            {

                _clickPoint = new Point(ipos.X, ipos.Y);
                return;
            }


            var pos = new Point(ipos.X, ipos.Y) - _clickPoint + _relativeClickPoint;//Mouse.PrimaryDevice.GetPosition((IInputElement)_moveSource);
            var w = -(_rawSelectorPosition.Left - pos.X);
            var h = -(_rawSelectorPosition.Top - pos.Y);

            SelectorPosition = new Thickness(w < 0 ? pos.X : _rawSelectorPosition.Left, h < 0 ? pos.Y : _rawSelectorPosition.Top, 0, 0);
            SelectorWidth = Math.Abs(w);
            SelectorHeight = Math.Abs(h);
        }


        private void CheckvMixConnection(object sender, EventArgs e)
        {
            if (IsInDesignMode) return;
            IsUrlValid = vMixAPI.StateFabrique.IsUrlValid(WindowSettings.IP, WindowSettings.Port);
            if (!IsUrlValid)
            {
                Status = Status.Offline;
                return;
            }

            var url = new Uri(vMixAPI.StateFabrique.GetUrl(WindowSettings.IP, WindowSettings.Port));
            vMixAPI.APIRequestManagerV2.GetApiResponseAsync(url.ToString(), new WeakAction((response, exception) =>
            {
                var dispatcher = Application.Current?.Dispatcher;
                Action updateAction = () =>
                {
                    _logger.Debug("Checking vMix server.");
                    if (exception != null)
                    {
                        _logger.Error(exception, "Error while connecting vMix server.");
                        Status = Status.Offline;
                        return;
                    }
                    if (Model != null && (Model.Ip == WindowSettings.IP && Model.Port == WindowSettings.Port))
                    {
                        var oldInputs = Model.Inputs.Select(x => x.Key).Skip(3);
                        var newInputs = Regex.Matches(response, @"<input[^>]*key=""([^""]+)""").OfType<Match>().Select(x => x.Groups[1].Value);
                        if (oldInputs.Count() != newInputs.Count() || oldInputs.Intersect(newInputs).Count() != oldInputs.Count())
                            Status = Status.InputsChanged;
                        else
                            Status = Status.Online;
                    }
                    else
                        Status = Status.Sync;

                };

                if (dispatcher == null || dispatcher.CheckAccess())
                    updateAction();
                else
                    dispatcher.BeginInvoke(updateAction);
            }), vMixAPI.StateFabrique.GetCredentials(WindowSettings.HttpLogin, WindowSettings.HttpPassword));
        }

        public override void Cleanup()
        {
            // Clean up if needed
            foreach (var item in Widgets.OfType<IDisposable>())
            {
                item.Dispose();
            }
            foreach (var item in WidgetTemplates.OfType<IDisposable>())
            {
                item.Dispose();
            }
            base.Cleanup();
        }

        protected virtual void Dispose(bool managed)
        {
            if (!managed)
                return;

            _connectTimer.Stop();
            _connectTimer.Tick -= CheckvMixConnection;
            _metricsTimer.Stop();
            _tcpSubscriber.ActsReceived -= OnVmixActsReceived;
            _tcpSubscriber.Dispose();
            LocalizationManager.Instance.CultureChanged -= LocalizationManager_CultureChanged;
            vMixAPI.StateFabrique.OnStateCreated -= State_OnStateCreated;
        }

        public void Dispose()
        {
            XmlDocumentMessenger.StopAsync().GetAwaiter().GetResult();
            Dispose(true);
            //throw new NotImplementedException();
        }


        private RelayCommand _downloadUpdateCommand;

        /// <summary>
        /// Gets the DownloadUpdateCommand.
        /// </summary>
        public RelayCommand DownloadUpdateCommand
        {
            get
            {
                return _downloadUpdateCommand
                    ?? (_downloadUpdateCommand = new RelayCommand(
                    () =>
                    {
                        Process.Start(new ProcessStartInfo(UpdateLink));
                    }));
            }
        }

        private RelayCommand _viewChangelogCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand ViewChangelogCommand
        {
            get
            {
                return _viewChangelogCommand
                    ?? (_viewChangelogCommand = new RelayCommand(
                    () =>
                    {
                        Process.Start(Path.Combine(_documentsPath, "Changelog.html"));
                    }));
            }
        }

        private RelayCommand<object> _previewMouseUp;

        /// <summary>
        /// Gets the PreviewMouseUp.
        /// </summary>
        public RelayCommand<object> PreviewMouseUp
        {
            get
            {
                return _previewMouseUp
                    ?? (_previewMouseUp = new RelayCommand<object>(
                    p =>
                    {
                        //mouseHook.UninstallHook();
                        //Gma.System.MouseKeyHook.Hook.GlobalEvents().MouseMove -= MainViewModel_MouseMove;
                        //Gma.System.MouseKeyHook.Hook.GlobalEvents().MouseUp -= MainViewModel_MouseUp;
                    }));
            }
        }

        private RelayCommand<object> _previewMouseDown;

        /// <summary>
        /// Gets the PreviewMouseDown.
        /// </summary>
        public RelayCommand<object> PreviewMouseDown
        {
            get
            {
                return _previewMouseDown
                    ?? (_previewMouseDown = new RelayCommand<object>(
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

                            if (parent == null)
                                FocusManager.SetFocusedElement(App.Current.MainWindow, (IInputElement)App.Current.MainWindow);
                            else
                            {
                                FocusManager.SetFocusedElement(parent, (IInputElement)parent);
                                //MoveFocus
                                ((FrameworkElement)parent).MoveFocus(new TraversalRequest(FocusNavigationDirection.Last) { });
                            }



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
                            WindowTitle = LocalizationManager.Instance["Dialog.About.Title"],

                            MainInstruction = LocalizationManager.Instance["Dialog.About.MainInstruction"],
                            MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Information,
                            ExpandedInformation = LocalizationManager.Instance["Dialog.About.ExpandedInformation"],
                            ExpandFooterArea = true,
                            Footer = Title,
                            ButtonStyle = Ookii.Dialogs.Wpf.TaskDialogButtonStyle.CommandLinks
                        };

                        var forumbtn = new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Custom) { Text = LocalizationManager.Instance["Dialog.About.Button.Forum"] };
                        var githubbtn = new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Custom) { Text = LocalizationManager.Instance["Dialog.About.Button.GitHub"] };
                        var redditbtn = new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Custom) { Text = LocalizationManager.Instance["Dialog.About.Button.Reddit"] };
                        var donatebtn = new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Custom) { Text = LocalizationManager.Instance["Dialog.About.Button.Donate"] };
                        td.Buttons.Add(forumbtn);
                        td.Buttons.Add(githubbtn);
                        td.Buttons.Add(redditbtn);
                        td.Buttons.Add(donatebtn);
                        td.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Close) { Default = true, Text = LocalizationManager.Instance["Dialog.About.Button.Close"] });

                        var btn = td.ShowDialog();
                        if (btn == forumbtn)
                            Process.Start(new ProcessStartInfo("https://forums.vmix.com/default.aspx?g=posts&t=6468"));
                        else if (btn == donatebtn)
                            Process.Start(new ProcessStartInfo("https://coindrop.to/elgarf"));
                        else if (btn == githubbtn)
                            Process.Start(new ProcessStartInfo("https://github.com/elgarf/vMixUTC"));
                        else if (btn == redditbtn)
                            Process.Start(new ProcessStartInfo("https://www.reddit.com/r/vMixUTC/"));

                    }));
            }

        }

        private RelayCommand _undoCommand;

        /// <summary>
        /// Gets the UndoCommand.
        /// </summary>
        public RelayCommand UndoCommand
        {
            get
            {
                return _undoCommand
                    ?? (_undoCommand = new RelayCommand(
                    () =>
                    {
                        LoadUndo();
                    }));
            }
        }


        private RelayCommand<int> _editPageNameCommand;

        /// <summary>
        /// Gets the EditPagenameCommand.
        /// </summary>
        public RelayCommand<int> EditPageNameCommand
        {
            get
            {
                return _editPageNameCommand
                    ?? (_editPageNameCommand = new RelayCommand<int>(
                    p =>
                    {
                        //WindowSettings.Pages[p] = "PAGE 2";
                        TextInputWindow ti = new TextInputWindow() { Title = LocalizationManager.Instance["Dialog.PageName.Title"], InputTitle = LocalizationManager.Instance["Dialog.PageName.InputTitle"], Text = WindowSettings.Pages[p], Mode = InputTextWindowMode.Default };
                        IsHotkeysEnabled = false;
                        if (ti.ShowDialog() ?? false)
                        {
                            WindowSettings.Pages[p] = ti.Text;
                            WindowSettings.UpdatePages();
                        }
                        IsHotkeysEnabled = true;
                    }));
            }
        }

        private RelayCommand _duplicateSelectedCommand;

        /// <summary>
        /// Gets the DublicateSelectedCommand.
        /// </summary>
        public RelayCommand DuplicateSelectedCommand
        {
            get
            {
                return _duplicateSelectedCommand
                    ?? (_duplicateSelectedCommand = new RelayCommand(
                    () =>
                    {

                        List<vMixControl> dups = new List<vMixControl>();
                        foreach (var item in _widgets.Where(x => x.Selected))
                        {

                            var copy = item.Copy();
                            if (copy != null)
                            {
                                dups.Add(copy);
                                copy.Left += 16;
                                copy.Top += 16;
                                if (copy.ZIndex > 0)
                                    copy.ZIndex++;
                                copy.State = Model;
                                item.Selected = false;
                                copy.Name = Utils.GetNextCopyName(copy.Name);
                            }
                        }
                        if (dups.Count > 0)
                        {
                            SaveUndo(LocalizationManager.Instance["Undo.WidgetsDuplicated"]);
                            foreach (var item in dups)
                                _widgets.Add(item);
                        }
                    }));
            }
        }
    }
}
