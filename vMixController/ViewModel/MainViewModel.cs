using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
using vMixController.Extensions;
using vMixControllerSkin.Localization;
using vMixController.Messages;
using vMixController.Widgets;
using vMixControllerSkin;

namespace vMixController.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// </summary>
    public partial class MainViewModel : ViewModelBase, IDisposable
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
        vMixWidgetSettingsView _settings;
        NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IStateFactory _stateFactory = AppServices.IsRegistered<IStateFactory>()
            ? AppServices.GetRequiredService<IStateFactory>()
            : StateFabriqueAdapter.Instance;
        private readonly IStateSyncService _stateSyncService = AppServices.IsRegistered<IStateSyncService>()
            ? AppServices.GetRequiredService<IStateSyncService>()
            : StateFabriqueAdapter.Instance;
        public GlobalVariablesViewModel GlobalSettings => AppServices.GetRequiredService<GlobalVariablesViewModel>();

        private readonly ScriptExecutionLoopGuard _scriptExecutionLoopGuard = new ScriptExecutionLoopGuard(100, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        private DateTime _lastScriptLoopGuardWarningUtc = DateTime.MinValue;
        private static readonly TimeSpan ScriptLoopGuardWarningThrottle = TimeSpan.FromSeconds(2);

        Point _clickPoint;
        Point _relativeClickPoint;
        Thickness _rawSelectorPosition = new Thickness();
        bool _skipClick = false;


        [ObservableProperty]
        private bool _isHotkeysEnabled = true;


        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _controllerPath = Directory.GetCurrentDirectory();

        partial void OnControllerPathChanged(string value)
        {
            Utils.PortableControllerPath = value;
        }

        [ObservableProperty]
        private bool _isFiltersRegistered = false;

        /*Selector*/
        [ObservableProperty]
        private Thickness _selectorPosition = new Thickness(0);


        [ObservableProperty]
        private double _selectorWidth = 0;

        [ObservableProperty]
        private double _selectorHeight = 0;


        [ObservableProperty]
        private bool _selectorEnabled = false;


        [ObservableProperty]
        private bool _isUrlValid = true;

        [ObservableProperty]
        private vMixAPI.State _model = null;

        [ObservableProperty]
        private MainWindowSettings _windowSettings = null;

        partial void OnIsLoadingChanged(bool value)
        {
            if (!value)
                foreach (var item in Widgets)
                    item.IsVisualReady = true;
        }

        partial void OnModelChanged(vMixAPI.State value)
        {
            _logger.Debug("New model setted.");
        }

        partial void OnWindowSettingsChanging(MainWindowSettings oldValue, MainWindowSettings newValue)
        {
            if (oldValue != null)
                oldValue.PropertyChanged -= WindowSettings_PropertyChanged;
        }

        partial void OnWindowSettingsChanged(MainWindowSettings value)
        {
            if (value == null)
                return;

            if (value.EnableLog)
            {
                if (!NLog.LogManager.IsLoggingEnabled())
                    NLog.LogManager.ResumeLogging();
            }
            else
            {
                if (NLog.LogManager.IsLoggingEnabled())
                    NLog.LogManager.SuspendLogging();
            }

            value.PropertyChanged += WindowSettings_PropertyChanged;
            UpdateXmlDocumentMessengerSettings();
        }

        private void WindowSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _ = Task.Run(() =>
            {
                if (e.PropertyName == "IP" || e.PropertyName == "Port" || e.PropertyName == "HttpLogin" || e.PropertyName == "HttpPassword")
                {
                    UpdateXmlDocumentMessengerSettings();
                    CheckvMixConnection(null, new EventArgs());
                }
            });

        }

        private void UpdateXmlDocumentMessengerSettings()
        {
            if (WindowSettings == null)
                return;

            var options = BuildConnectionOptions();
            _stateFactory.SetConnectionOptions(options);
            XmlDocumentMessenger.Url = _stateFactory.GetUrl(options.Ip, options.Port);
            XmlDocumentMessenger.Credentials = _stateFactory.GetCredentials(options.Login, options.Password);
        }

        private ConnectionOptions BuildConnectionOptions()
        {
            if (WindowSettings == null)
                return new ConnectionOptions();

            return new ConnectionOptions(
                WindowSettings.IP,
                WindowSettings.Port,
                WindowSettings.HttpLogin,
                WindowSettings.HttpPassword);
        }

        private void LocalizationManager_CultureChanged(object sender, EventArgs e)
        {
            LocalizeVirtualInputs(Model);
        }

        [ObservableProperty]
        private Status _status = Classes.Status.Offline;

        partial void OnStatusChanged(Status value)
        {
            XmlDocumentMessenger.Sync = value == Status.Online || value == Status.InputsChanged;
            _logger.Debug("Status changed to {0}.", value);
        }


        [ObservableProperty]
        private Status _pollingstatus = Classes.Status.Offline;

        [ObservableProperty]
        private ObservableCollection<Pair<string, vMixControl>> _widgetTemplates = new ObservableCollection<Pair<string, vMixControl>>();

        [ObservableProperty]
        private ObservableCollection<Pair<string, vMixControl>> _externalDataProviders = new ObservableCollection<Pair<string, vMixControl>>();

        [ObservableProperty]
        private ObservableCollection<vMixNewFunctionReference> _newFunctions = null;

        [ObservableProperty]
        private ObservableCollection<vMixFunctionReference> _functions = null;

        [ObservableProperty]
        private ObservableCollection<vMixController.Widgets.vMixControl> _widgets = new ObservableCollection<vMixController.Widgets.vMixControl>();

        private readonly List<UndoEntry> _undoStack = new List<UndoEntry>();
        [ObservableProperty]
        private UndoSnapshot _undoState = null;

        [ObservableProperty]
        private string _undoReason = "";

        [ObservableProperty]
        private string _editorCursor = "Arrow";

        [ObservableProperty]
        private bool _LIVE = true;

        partial void OnLIVEChanged(bool value)
        {
            foreach (var item in Widgets)
                if (item is vMixControlTextField)
                {
                    ((vMixControlTextField)item).IsLive = value;
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

                    return DateTimeOffset.FromUnixTimeSeconds(coffHeader.TimeDateStamp).UtcDateTime;
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
                OnPropertyChanged(nameof(Title));
            }
        }

        [ObservableProperty]
        private bool _isGhosted = false;

        partial void OnIsGhostedChanged(bool value)
        {
            foreach (var item in Widgets)
            {
                if (item.ZIndex >= 0)
                    item.IsGhosted = value;
            }
        }

        [ObservableProperty]
        private string _updateLink = null;

        [ObservableProperty]
        private DateTime _availableVersion = GetBuildDateTime(Assembly.GetExecutingAssembly());

        [ObservableProperty]
        private int _selectedTab = 1;

        partial void OnSelectedTabChanged(int value)
        {
            if (value == 1)
            {
                foreach (var w in Widgets.OfType<vMixControlTextField>())
                {
                    w.Update();
                }
            }
        }

        [ObservableProperty]
        private int _pageIndex = 0;

        private void InsertWidgetByZIndex(vMixControl widget)
        {
            widget.IsGhosted = widget.ZIndex >= 0 && IsGhosted;
            if (widget.ZIndex < 0)
                Widgets.Insert(0, widget);
            else
                Widgets.Add(widget);
            OnPropertyChanged(nameof(Widgets));
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

        private List<vMixControl> GetSelectedWidgetsSnapshot()
        {
            return Widgets.Where(x => x.Selected).ToList();
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

                        _stateFactory.SetConnectionOptions(BuildConnectionOptions());

                        var options = BuildConnectionOptions();
                        IsUrlValid = _stateFactory.IsUrlValid(options.Ip, options.Port);

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
            EnsureWidgetIdentity(Widgets);

            var currentById = Widgets.ToDictionary(x => x.WidgetId);
            var targetById = restoredWidgets.ToDictionary(x => x.WidgetId);

            var currentSerialized = Widgets.ToDictionary(x => x.WidgetId, SerializeToBytes);
            var targetSerialized = restoredWidgets.ToDictionary(x => x.WidgetId, SerializeToBytes);

            foreach (var id in currentById.Keys.Except(targetById.Keys).ToArray())
            {
                var removed = currentById[id];
                Widgets.Remove(removed);
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
                        var existingIndex = Widgets.IndexOf(existing);
                        existing.Dispose();
                        target.State = Model;
                        if (target is vMixControlTextField textField)
                            textField.IsLive = LIVE;
                        target.Update();
                        if (existingIndex >= 0)
                            Widgets[existingIndex] = target;
                        else
                            Widgets.Insert(Math.Min(targetIndex, Widgets.Count), target);
                        currentById[target.WidgetId] = target;
                    }

                    var actualIndex = Widgets.IndexOf(currentById[target.WidgetId]);
                    if (actualIndex >= 0 && actualIndex != targetIndex)
                        Widgets.Move(actualIndex, targetIndex);
                }
                else
                {
                    target.State = Model;
                    if (target is vMixControlTextField textField)
                        textField.IsLive = LIVE;
                    target.Update();
                    Widgets.Insert(Math.Min(targetIndex, Widgets.Count), target);
                    currentById[target.WidgetId] = target;
                }
            }

            OnPropertyChanged(nameof(Widgets));
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

        [RelayCommand]
        private void SwitchLock(vMixController.Widgets.vMixControl p)
        {
            SaveUndo(string.Format(LocalizationManager.Instance["Undo.LockChanged"], p.Type, p.Name));
            p.Locked = !p.Locked;
            p.IsPasswordLocked = p.IsPasswordLockable && p.Locked && (!string.IsNullOrWhiteSpace(WindowSettings.UserName) || !string.IsNullOrWhiteSpace(WindowSettings.Password));
        }

        [RelayCommand]
        private void RemoveWidget(vMixControl p)
        {
            SaveUndo(string.Format(LocalizationManager.Instance["Undo.WidgetRemoved"], p.Type, p.Name));

            int processed = 0;
            var selectedWidgets = GetSelectedWidgetsSnapshot();
            foreach (var widget in selectedWidgets)
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
        }

        [RelayCommand]
        private void CopyWidget(vMixControl p)
        {
            try
            {
                vMixControl copy = null;
                int processed = 0;
                bool undoSaved = false;
                var selectedWidgets = GetSelectedWidgetsSnapshot();
                foreach (var widget in selectedWidgets)
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
                        Widgets.Add(copy);
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
                        Widgets.Add(copy);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while copying widget.");
            }
        }

        [RelayCommand]
        private void MoveWidget(ControlIntParameter p)
        {
            SaveUndo(LocalizationManager.Instance["Undo.WidgetPageChanged"]);
            var selectedWidgets = GetSelectedWidgetsSnapshot();
            foreach (var widget in selectedWidgets)
            {
                widget.Page = p.B;
                widget.Selected = false;
            }
            p.A.Page = p.B;
        }

        [RelayCommand]
        private void ToggleCaption(vMixControl p)
        {
            SaveUndo(LocalizationManager.Instance["Undo.WidgetCaptionShowHidden"]);
            var selectedWidgets = GetSelectedWidgetsSnapshot();
            foreach (var widget in selectedWidgets)
            {
                widget.IsCaptionOn = !p.IsCaptionOn;
            }
            p.IsCaptionOn = !p.IsCaptionOn;
        }

        [RelayCommand]
        private void ScaleUp(vMixControl p)
        {
            var selectedWidgets = GetSelectedWidgetsSnapshot();
            foreach (var widget in selectedWidgets)
            {
                widget.Scale += 0.25f;
            }
            p.Scale += 0.25f;
        }

        [RelayCommand]
        private void ScaleDown(vMixControl p)
        {
            var selectedWidgets = GetSelectedWidgetsSnapshot();
            foreach (var widget in selectedWidgets)
            {
                if (widget.Scale - 0.25f >= 1.0f)
                    widget.Scale -= 0.25f;
            }
            if (p.Scale - 0.25f >= 1.0f)
                p.Scale -= 0.25f;
        }

        [RelayCommand]
        private void OpenProperties(vMixController.Widgets.vMixControl p)
        {
            IsHotkeysEnabled = false;

            _logger.Debug("Opening properties for widget {0}.", p.Name);
            var viewModel = vMixController.Classes.AppServices.GetRequiredService<vMixController.ViewModel.vMixWidgetSettingsViewModel>();
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

            ScriptLoopAnalyzer.RefreshPotentialLoopWarnings(Widgets);

            _logger.Debug("Properties updated.");
            _settings = null;
            IsHotkeysEnabled = true;
        }

        Action<Point> _createWidget;

        [RelayCommand]
        private void CreateWidget(string p)
        {
            EditorCursor = CursorType.Cross.ToString();
            _createWidget = new Action<Point>(x =>
            {
                var widget = (vMixControl)Assembly.GetAssembly(this.GetType()).CreateInstance("vMixController.Widgets.vMixControl" + p);

                var count = Widgets.Where(y => y.GetType() == widget.GetType()).Count();

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
                {
                    widget.Dispose();
                }
            });
        }


        [RelayCommand]
        private void MouseButtonUp(MouseButtonEventArgs p)
        {
            if (p == null)
                return;

            var mw = App.Current?.MainWindow as vMixController.MainWindow;
            if (mw?.LayoutGrid?.IsMouseCaptured == true)
                mw.LayoutGrid.ReleaseMouseCapture();

            if (SelectorWidth != 0 && SelectorHeight != 0)
            {
                if (!(mw?.CanvasContent?.RenderTransform is MatrixTransform transform))
                    return;

                var m = transform.Matrix;
                m.Invert();

                var tl = m.Transform(new Point(SelectorPosition.Left, SelectorPosition.Top));
                var br = m.Transform(new Point(SelectorPosition.Left + SelectorWidth, SelectorPosition.Top + SelectorHeight));
                var sr = new Rect(tl.X, tl.Y, br.X - tl.X, br.Y - tl.Y);

                foreach (var item in Widgets)
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
            {
                foreach (var item in Widgets)
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
                    : (p.Source is IInputElement src ? p.MouseDevice.GetPosition(src) : new Point(0, 0));
                _createWidget(new Point(pos.X / WindowSettings.UIScale, pos.Y / WindowSettings.UIScale));
                _createWidget = null;
            }

            if (p.OriginalSource is ListView || p.OriginalSource is Grid)
                IsHotkeysEnabled = true;
        }

        [RelayCommand]
        private void MouseButtonDown(MouseButtonEventArgs p)
        {
            if (p == null)
                return;

            if (WindowSettings.Locked)
                return;

            var mw = App.Current?.MainWindow as vMixController.MainWindow;
            var pos = mw?.LayoutGrid != null
                ? mw.ToCanvasContentPoint(Mouse.GetPosition(mw.LayoutGrid), WindowSettings.UseInfiniteCanvas)
                : (p.Source is IInputElement src ? Mouse.GetPosition(src) : new Point(0, 0));
            if (mw?.LayoutGrid != null && !mw.LayoutGrid.IsMouseCaptured)
                mw.LayoutGrid.CaptureMouse();

            if (WindowSettings.UseInfiniteCanvas && mw?.CanvasContent?.RenderTransform is MatrixTransform transform)
                pos = transform.Matrix.Transform(pos);
            else if (WindowSettings.UseInfiniteCanvas && mw?.CanvasContent?.RenderTransform == null)
                return;

            _clickPoint = pos;
            _relativeClickPoint = pos;
            SelectorEnabled = true;

            _rawSelectorPosition = new Thickness(pos.X, pos.Y, 0, 0);
            SelectorPosition = new Thickness(pos.X, pos.Y, 0, 0);
            SelectorWidth = 0;
            SelectorHeight = 0;
        }


        [RelayCommand]
        private void MouseMove(MouseEventArgs p)
        {
            if (p == null)
                return;

            var mw = App.Current?.MainWindow as vMixController.MainWindow;
            var ipos = mw?.LayoutGrid != null
                ? mw.ToCanvasContentPoint(Mouse.GetPosition(mw.LayoutGrid), WindowSettings.UseInfiniteCanvas)
                : (App.Current?.MainWindow != null ? new Point(p.GetPosition(App.Current.MainWindow).X, p.GetPosition(App.Current.MainWindow).Y) : new Point(0, 0));

            if (WindowSettings.UseInfiniteCanvas && mw?.CanvasContent?.RenderTransform is MatrixTransform transform)
                ipos = transform.Matrix.Transform(ipos);

            if (!SelectorEnabled)
            {
                _clickPoint = new Point(ipos.X, ipos.Y);
                return;
            }

            var pos = new Point(ipos.X, ipos.Y) - _clickPoint + _relativeClickPoint;
            var w = -(_rawSelectorPosition.Left - pos.X);
            var h = -(_rawSelectorPosition.Top - pos.Y);

            SelectorPosition = new Thickness(w < 0 ? pos.X : _rawSelectorPosition.Left, h < 0 ? pos.Y : _rawSelectorPosition.Top, 0, 0);
            SelectorWidth = Math.Abs(w);
            SelectorHeight = Math.Abs(h);
        }

        bool _fromContextMenu = false;
        Point _contextMenuPosition;
        [RelayCommand]
        private void ContextMenuOpening(ContextMenuEventArgs p)
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
        }

        [RelayCommand]
        private void ContextMenuClosing(ContextMenuEventArgs p)
        {
            if (_createWidget != null && _fromContextMenu)
            {
                EditorCursor = "Arrow";
                var pos = _contextMenuPosition;
                _createWidget(new Point(pos.X / WindowSettings.UIScale, pos.Y / WindowSettings.UIScale));
                _createWidget = null;
                _fromContextMenu = false;
            }
        }

        [RelayCommand]
        private void CreateWidgetFromTemplate(Pair<string, vMixControl> p)
        {
            EditorCursor = "Hand";
            _createWidget = x =>
            {
                SaveUndo(string.Format(LocalizationManager.Instance["Undo.WidgetCreatedFromTemplate"], p.A));
                var count = Widgets.Where(y => y.GetType() == p.B.GetType()).Count();
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
            };
        }

        [RelayCommand]
        private void RemoveWidgetTemplate(Pair<string, vMixControl> p)
        {
            WidgetTemplates.Remove(p);
        }

        [RelayCommand]
        private void EditWidgetTemplate(Pair<string, vMixControl> p)
        {
            OpenPropertiesCommand.Execute(p.B);
        }

        [RelayCommand]
        private void NewController()
        {
            SaveUndo(LocalizationManager.Instance["Undo.ControllerCreated"]);

            foreach (var item in Widgets)
                item.Dispose();

            WindowSettings.Password = null;
            WindowSettings.UserName = null;
            WindowSettings.Locked = false;

            if (AppServices.IsRegistered<GlobalVariablesViewModel>())
            {
                AppServices.GetRequiredService<GlobalVariablesViewModel>().Variables.Clear();
            }

            Widgets.Clear();
        }

        [RelayCommand]
        private void LoadController()
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
        }

        [RelayCommand]
        private void LoadRecentController(string path)
        {
            if (File.Exists(path))
            {
                SaveUndo(LocalizationManager.Instance["Undo.ControllerLoaded"]);
                LoadControllerFromFile(path);

                //Analyze script loops
                ScriptLoopAnalyzer.RefreshPotentialLoopWarnings(Widgets);

                WindowSettings.AddRecentFile(path);
            }
        }

        [RelayCommand]
        private void AppendController()
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
                        Widgets.Add(wgt);
                    }

                    //Analyze script loops
                    ScriptLoopAnalyzer.RefreshPotentialLoopWarnings(Widgets);

                };
                _skipClick = true;
            }
        }

        private void LoadControllerFromFile(string opendlg)
        {
            try
            {
                ControllerPath = opendlg;

                //IsWidgetsVisualLoading = true;

                foreach (var item in Widgets)
                    item.Dispose();
                Widgets.Clear();

                LIVE = true;

                var ol = WindowSettings.OpenLastAtStart;
                var recent = (ObservableCollection<string>)WindowSettings.RecentFiles.Copy();

                MainWindowSettings loadedWindowSettings;
                foreach (var item in Utils.LoadController(opendlg, Functions, out loadedWindowSettings))
                    Widgets.Add(item);
                WindowSettings = loadedWindowSettings;


                WindowSettings.OpenLastAtStart = ol;
                WindowSettings.RecentFiles = recent;

                NormalizeMainWindowPlacement(WindowSettings);

                foreach (var item in Widgets)
                {
                    item.IsVisualReady = false;
                    item.Update();
                }

                OnPropertyChanged(nameof(WindowSettings));
                _logger.Debug("Configuring API.");

                CheckvMixConnection(null, new EventArgs());

                _stateFactory.SetConnectionOptions(BuildConnectionOptions());

                var options = BuildConnectionOptions();
                IsUrlValid = _stateFactory.IsUrlValid(options.Ip, options.Port);

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

        [RelayCommand]
        private void SaveController()
        {
            Ookii.Dialogs.Wpf.VistaSaveFileDialog opendlg = new Ookii.Dialogs.Wpf.VistaSaveFileDialog
            {
                Filter = "vMix Controller|*.vmc",
                DefaultExt = "vmc"
            };
            var result = opendlg.ShowDialog(App.Current.MainWindow);
            if (result.HasValue && result.Value)
            {
                Utils.SaveController(opendlg.FileName, Widgets, WindowSettings);
                WindowSettings.AddRecentFile(opendlg.FileName);
            }
        }

        private string ToMD5Hash(string str)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                return Convert.ToBase64String(md5.ComputeHash(Encoding.ASCII.GetBytes(str)));
        }

        [RelayCommand]
        private void ToggleLock()
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
            else if (!string.IsNullOrWhiteSpace(WindowSettings.UserName) || !string.IsNullOrWhiteSpace(WindowSettings.Password))
            {
                TextInputWindow twi = new TextInputWindow()
                {
                    Mode = InputTextWindowMode.Password,
                    Title = LocalizationManager.Instance["Dialog.PasswordUnlock.Title"],
                    InputTitle = LocalizationManager.Instance["Dialog.PasswordUnlock.InputTitle"]
                };

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
                else
                {
                    return;
                }
            }

            WindowSettings.Locked = !WindowSettings.Locked;
            foreach (var item in Widgets)
            {
                item.Locked = WindowSettings.Locked;
                item.IsPasswordLocked = item.IsPasswordLockable && (!string.IsNullOrWhiteSpace(WindowSettings.UserName) || !string.IsNullOrWhiteSpace(WindowSettings.Password));
            }
        }

        [RelayCommand]
        private void SwitchPasswordLockable(vMixController.Widgets.vMixControl p)
        {
            p.IsPasswordLockable = !p.IsPasswordLockable;
        }

        [RelayCommand]
        private void SyncState()
        {
            SyncTovMixState();
        }

        [RelayCommand]
        private void Exit()
        {
            App.Current.MainWindow.Close();
        }

        [RelayCommand]
        private void StopAllScripts()
        {
            foreach (var widget in Widgets)
            {
                if (widget is vMixControlButton btn)
                    btn.ExecuteHotkey(btn.GetHotkeyNumber("Reset"));
                if (widget is vMixControlNewButton nbtn)
                    nbtn.ExecuteHotkey(nbtn.GetHotkeyNumber("Reset"));
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
                        _stateFactory.SetConnectionOptions(BuildConnectionOptions());
                        _stateSyncService.CreateAsync();
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
            var globalSettings = AppServices.IsRegistered<GlobalVariablesViewModel>()
                ? AppServices.GetRequiredService<GlobalVariablesViewModel>()
                : null;

            if (state == null || globalSettings == null || state.Inputs.Where(x => x.Key == "vmix-utc-internal-gv").FirstOrDefault() != null) return;
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
            foreach (var item in Widgets)
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

                foreach (var item in Widgets)
                    item.State = (vMixAPI.State)sender;
            }
            CheckvMixConnection(null, new EventArgs());
        }


        bool ProcessHotkey(Key key, Key systemKey, ModifierKeys modifiers, bool onPress = true)
        {
            FocusManager.SetFocusedElement(App.Current.MainWindow, (IInputElement)App.Current.MainWindow);

            var result = false;
            foreach (var ctrl in Widgets)
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
                    foreach (var ctrl in Widgets)
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

                foreach (var ctrl in Widgets)
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


        [RelayCommand]
        private void PreviewKeyUp(KeyEventArgs p)
        {
            _isPressed = false;
            if (p == null)
                return;
            if (!IsHotkeysEnabled)
                return;
            p.Handled = ProcessHotkey(p.Key, p.SystemKey, p.KeyboardDevice.Modifiers, false);
        }

        [RelayCommand]
        private void PreviewKeyDown(KeyEventArgs p)
        {
            if (p == null)
                return;

            if (_createWidget != null && p.Key == Key.Escape)
            {
                _createWidget = null;
                EditorCursor = Cursors.Arrow.ToString();
            }

            if (!IsHotkeysEnabled || _isPressed)
                return;
            _isPressed = true;
            p.Handled = ProcessHotkey(p.Key, p.SystemKey, p.KeyboardDevice.Modifiers, true);
        }

        [RelayCommand]
        private void Closing()
        {
            _logger.Info("Saving templates.");
            XmlSerializer s = Utils.GetXmlSerializer(typeof(ObservableCollection<Pair<string, vMixControl>>));
            using (var fs = new FileStream(Path.Combine(_documentsPath, "Templates.xml"), FileMode.Create))
                s.Serialize(fs, WidgetTemplates);

            _logger.Info("Saving window settings.");
            s = Utils.GetXmlSerializer(typeof(MainWindowSettings));
            using (var fs = new FileStream(Path.Combine(_documentsPath, "WindowSettings.xml"), FileMode.Create))
                s.Serialize(fs, WindowSettings);

            _logger.Info("Saving last controller");
            using (var fs = new FileStream(Path.Combine(_documentsPath, "Last.vmc"), FileMode.Create))
                Utils.SaveController(fs, Widgets, WindowSettings);

            //Dispose external data providers
            foreach (var item in ExternalDataProviders)
            {
                item.B.Dispose();
            }
            foreach (var item in WidgetTemplates)
            {
                item.B.Dispose();
            }
        }


        [RelayCommand]
        private void OpenLogFolder()
        {
            try
            {
                Process.Start(Path.Combine(_documentsPath, "logs"));
            }
            catch (Exception) { }
        }

        [RelayCommand]
        private void ResetScaling()
        {
            if (WindowSettings != null)
                WindowSettings.UIScale = 1;
        }

        DispatcherTimer _connectTimer = new DispatcherTimer();
        DispatcherTimer _metricsTimer = new DispatcherTimer();

        string _documentsPath;

        private static string ResolveWritableDataDirectory()
        {
            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vMix UTC"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "vMix UTC"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data")
            };

            foreach (var candidate in candidates)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(candidate))
                        continue;
                    Directory.CreateDirectory(candidate);
                    return candidate;
                }
                catch
                {
                    // try next candidate
                }
            }

            return AppDomain.CurrentDomain.BaseDirectory;
        }

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
            Utils.PortableControllerPath = ControllerPath;

            XmlDocumentMessenger.Start();
            _stateSyncService.Start();

            _stateSyncService.OnStateCreated += State_OnStateCreated;

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
            var _expression = new NCalc.Expression("1+1");
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

            _documentsPath = ResolveWritableDataDirectory();

            try
            {
                _logger.Info("Loading templates.");
                s = Utils.GetXmlSerializer(typeof(ObservableCollection<Pair<string, vMixControl>>));
                if (File.Exists(Path.Combine(_documentsPath, "Templates.xml")))
                    using (var fs = new FileStream(Path.Combine(_documentsPath, "Templates.xml"), FileMode.Open))
                        WidgetTemplates = (ObservableCollection<Pair<string, vMixControl>>)s.Deserialize(fs);
            }
            catch (Exception)
            {

            }

            _logger.Info("Searching for data providers.");
            var providersDirectory = Path.Combine(Directory.GetCurrentDirectory(), "DataProviders");
            var files = Directory.Exists(providersDirectory)
                ? Directory.EnumerateFiles(providersDirectory, "*DataProvider.dll").ToArray()
                : Array.Empty<string>();

            if (!Directory.Exists(providersDirectory))
            {
                _logger.Warn("Data providers directory not found: {0}", providersDirectory);
            }

            if (files.Length > 0)
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
                OnPropertyChanged(nameof(WindowSettings));

            }
            catch (Exception)
            {
                WindowSettings = new MainWindowSettings();
            }

            if (WindowSettings.Pages.Count != 7)
            {
                WindowSettings.Pages.Clear();
                WindowSettings.Pages.Add(Utils.MAINPAGENAME);
                WindowSettings.Pages.Add(Utils.DATAPAGENAME);
                WindowSettings.Pages.Add(Utils.PAGE1PAGENAME);
                WindowSettings.Pages.Add(Utils.PAGE2PAGENAME);
                WindowSettings.Pages.Add(Utils.PAGE3PAGENAME);
                WindowSettings.Pages.Add(Utils.PAGE4PAGENAME);
                WindowSettings.Pages.Add(Utils.PAGE5PAGENAME);
            }

            if (Model == null)
            {
                _stateFactory.SetConnectionOptions(BuildConnectionOptions());
                _stateSyncService.CreateAsync();
            }

            Messenger.Register<HotkeyLinkMessage>(this, (r, hk) =>
            {
                ProcessHotkey(hk.Link, hk.Parameter);
            });

            Messenger.Register<string>(this, (r, hk) =>
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


            Messenger.Register<LIVEToggleMessage>(this, (r, msg) =>
            {
                switch (msg.State)
                {
                    case 0: LIVE = false; break;
                    case 1: LIVE = true; break;
                    case 2: LIVE = !LIVE; break;
                }
            });

            Messenger.Register<LoadingMessage>(this, (r, msg) =>
            {
                IsLoading = msg.Loading;
            });


            Messenger.Register<WidgetMoveDeltaMessage>(this, (r, t) =>
            {
                _movedWidgetsBuffer.Clear();
                foreach (var widget in Widgets)
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

            Messenger.Register<WidgetMoveStateMessage>(this, (r, t) =>
            {
                _selectedStickyRegionsBuffer.Clear();
                foreach (var widget in Widgets)
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
                        foreach (var item in Widgets)
                            if (rgn.Intersect(item) && item.Page == rgn.Page && !selectedSet.Contains(item) && intersectionsSet.Add(item))
                                _intersections.Add(item);
                    }
                }

            });

            Messenger.Register<HotkeysEnabledMessage>(this, (r, t) =>
            {
                IsHotkeysEnabled = t.IsEnabled;
            });

            Messenger.Register<SyncStateRequestMessage>(this, (r, t) =>
            {
                if (t.Force)
                    SyncTovMixState();
            });

            Messenger.Register<PageNavigationMessage>(this, (r, t) =>
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

            Messenger.Register<WidgetEditMessage>(this, (r, msg) =>
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

            var options = BuildConnectionOptions();
            IsUrlValid = _stateFactory.IsUrlValid(options.Ip, options.Port);

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
                foreach (var item in Widgets)
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
                foreach (var item in Widgets)
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
            var options = BuildConnectionOptions();
            IsUrlValid = _stateFactory.IsUrlValid(options.Ip, options.Port);
            if (!IsUrlValid)
            {
                Status = Status.Offline;
                return;
            }

            var url = new Uri(_stateFactory.GetUrl(options.Ip, options.Port));
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
            }), _stateFactory.GetCredentials(options.Login, options.Password));
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
            LocalizationManager.Instance.CultureChanged -= LocalizationManager_CultureChanged;
            _stateSyncService.OnStateCreated -= State_OnStateCreated;
            _stateSyncService.Stop();
        }

        public void Dispose()
        {
            XmlDocumentMessenger.StopAsync().GetAwaiter().GetResult();
            Dispose(true);
            //throw new NotImplementedException();
        }


        [RelayCommand]
        private void DownloadUpdate()
        {
            Process.Start(new ProcessStartInfo(UpdateLink));
        }

        [RelayCommand]
        private void ViewChangelog()
        {
            Process.Start(Path.Combine(_documentsPath, "Changelog.html"));
        }

        [RelayCommand]
        private void PreviewMouseUp(object p)
        {
            //mouseHook.UninstallHook();
            //Gma.System.MouseKeyHook.Hook.GlobalEvents().MouseMove -= MainViewModel_MouseMove;
            //Gma.System.MouseKeyHook.Hook.GlobalEvents().MouseUp -= MainViewModel_MouseUp;
        }

        [RelayCommand]
        private void PreviewMouseDown(object p)
        {
            //mouseHook.InstallHook();
        }

        [RelayCommand]
        private void TextBoxPreviewKeyUp(System.Windows.Input.KeyEventArgs p)
        {
            if (p == null)
                return;

            if (p.Key == System.Windows.Input.Key.Return || p.Key == Key.Escape)
            {
                var sourceElement = p.Source as FrameworkElement;
                if (sourceElement == null)
                    return;

                DependencyObject parent = sourceElement.Parent;
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
        }

        [RelayCommand]
        private void TextBoxGotFocus(RoutedEventArgs p)
        {
            IsHotkeysEnabled = false;
            // WeakReferenceMessenger.Default.Send(new Pair<string, bool>() { A = "Hotkeys", B = false });
        }

        [RelayCommand]
        private void TextBoxLostFocus(RoutedEventArgs p)
        {
            IsHotkeysEnabled = true;
            //WeakReferenceMessenger.Default.Send(new Pair<string, bool>() { A = "Hotkeys", B = true });
        }

        [RelayCommand]
        private void About()
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
                Process.Start(new ProcessStartInfo("https://forums.vmix.com/default.aspx?g=posts&t=6468") { UseShellExecute = true });
            else if (btn == donatebtn)
                Process.Start(new ProcessStartInfo("https://coindrop.to/elgarf") { UseShellExecute = true });
            else if (btn == githubbtn)
                Process.Start(new ProcessStartInfo("https://github.com/elgarf/vMixUTC") { UseShellExecute = true });
            else if (btn == redditbtn)
                Process.Start(new ProcessStartInfo("https://www.reddit.com/r/vMixUTC/") { UseShellExecute = true });
        }

        [RelayCommand]
        private void Undo()
        {
            LoadUndo();
        }

        [RelayCommand]
        private void EditPageName(int p)
        {
            TextInputWindow ti = new TextInputWindow() { Title = LocalizationManager.Instance["Dialog.PageName.Title"], InputTitle = LocalizationManager.Instance["Dialog.PageName.InputTitle"], Text = WindowSettings.Pages[p], Mode = InputTextWindowMode.Default };
            IsHotkeysEnabled = false;
            if (ti.ShowDialog() ?? false)
            {
                WindowSettings.Pages[p] = ti.Text;
                WindowSettings.UpdatePages();
            }
            IsHotkeysEnabled = true;
        }

        [RelayCommand]
        private void DuplicateSelected()
        {
            List<vMixControl> dups = new List<vMixControl>();
            var selectedWidgets = GetSelectedWidgetsSnapshot();
            foreach (var item in selectedWidgets)
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
                    Widgets.Add(item);
            }
        }
    }
}








