using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;
using vMixController.Messages;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    //Button: Toggler
    //TextField: Set text to input text
    //Score: Count score
    //Timer: Timer
    [Serializable]
    [XmlInclude(typeof(vMixControlTextField)),
    XmlInclude(typeof(vMixControlButton)),
        XmlInclude(typeof(vMixControlScore)),
        XmlInclude(typeof(vMixControlList)),
        XmlInclude(typeof(vMixControlLabel)),
        XmlInclude(typeof(vMixControlVariableViewer)),
        XmlInclude(typeof(vMixControlExternalData)),
        XmlInclude(typeof(vMixControlTimer)),
        XmlInclude(typeof(vMixControlContainer)),
        XmlInclude(typeof(vMixControlMultiState)),
        XmlInclude(typeof(vMixControlMidiInterface)),
        XmlInclude(typeof(vMixControlStreamDeck)),
        XmlInclude(typeof(vMixControlClock)),
        XmlInclude(typeof(vMixControlRegion)),
        XmlInclude(typeof(vMixControlVolume)),
        XmlInclude(typeof(vMixControlSlider)),
        XmlInclude(typeof(vMixControlTBar)),
        XmlInclude(typeof(vMixControlPlaylist)),
        XmlInclude(typeof(vMixControlNewButton))]
    public partial class vMixControl : DependencyObject, INotifyPropertyChanged, IDisposable
    {

        protected NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        internal static List<UserControl> ControlsStoreUsage = new List<UserControl>();
        internal static State _internalState;
        internal static Regex _regexInt = new Regex(@"^\d+$");
        private static readonly ConcurrentDictionary<Tuple<Type, string>, PropertyInfo> _propertyCache =
            new ConcurrentDictionary<Tuple<Type, string>, PropertyInfo>();
        [NonSerialized]
        private IMessenger _messenger;

        protected IMessenger Messenger
        {
            get
            {
                if (_messenger != null)
                    return _messenger;

                if (AppServices.IsRegistered<IMessenger>())
                    _messenger = AppServices.GetRequiredService<IMessenger>();
                else
                    _messenger = WeakReferenceMessenger.Default;

                return _messenger;
            }
        }

        protected static DispatcherTimer _shadowUpdate;

        public static TimeSpan ShadowUpdatePollTime
        {
            get { return _shadowUpdate.Interval; }
            set
            {
                _shadowUpdate.Interval = value;
                _shadowUpdate.Stop();
                _shadowUpdate.Start();
            }
        }

        static vMixControl()
        {
            _shadowUpdate = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };
            _shadowUpdate.Start();
        }

        public vMixControl()
        {
            if (AppServices.IsRegistered<vMixWidgetSettingsViewModel>())
            {
                WindowProperties = AppServices.GetRequiredService<vMixWidgetSettingsViewModel>().WindowProperties;
            }
        }

        private Guid _widgetId = Guid.NewGuid();

        /// <summary>
        /// Stable widget identity used for incremental undo/redo patching.
        /// </summary>
        public Guid WidgetId
        {
            get => _widgetId;
            set => SetPropertyValue(ref _widgetId, value, nameof(WidgetId));
        }

        public virtual string Type { get; }
        public virtual bool IsResizeableVertical { get { return false; } }

        public virtual int MaxCount => -1;

        private Quadriple<double?, double?, double?, double?> _windowProperties = new Quadriple<double?, double?, double?, double?>();

        /// <summary>
        /// Sets and gets the WindowProperties property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Quadriple<double?, double?, double?, double?> WindowProperties
        {
            get => _windowProperties;
            set => SetPropertyValue(ref _windowProperties, value, nameof(WindowProperties));
        }

        private bool _isPasswordLockable = true;

        /// <summary>
        /// Sets and gets the IsPasswordLockable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPasswordLockable
        {
            get => _isPasswordLockable;
            set => SetPropertyValue(ref _isPasswordLockable, value, nameof(IsPasswordLockable));
        }

        private bool _isPasswordLocked = false;

        /// <summary>
        /// Sets and gets the IsPasswordLocked property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPasswordLocked
        {
            get => _isPasswordLocked;
            set => SetPropertyValue(ref _isPasswordLocked, value, nameof(IsPasswordLocked));
        }

        private bool _locked = false;

        /// <summary>
        /// Sets and gets the Locked property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public virtual bool Locked
        {
            get => _locked;
            set => SetPropertyValue(ref _locked, value, nameof(Locked));
        }

        private bool _isCaptionVisible = true;

        /// <summary>
        /// Sets and gets the IsCaptionVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsCaptionVisible
        {
            get => _isCaptionVisible;
            set => SetPropertyValue(ref _isCaptionVisible, value, nameof(IsCaptionVisible));
        }

        private bool _isCaptionOn = true;

        /// <summary>
        /// Sets and gets the IsCaptionOff property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public virtual bool IsCaptionOn
        {
            get => _isCaptionOn;
            set => SetPropertyValue(ref _isCaptionOn, value, nameof(IsCaptionOn));
        }

        [NonSerialized]
        private bool _isFocused = false;

        /// <summary>
        /// Sets and gets the IsFocused property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool IsFocused
        {
            get => _isFocused;
            set => SetPropertyValue(ref _isFocused, value, nameof(IsFocused));
        }

        internal void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        protected bool SetPropertyValue<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool SetPropertyValue<T>(ref T field, T value, string propertyName, Action<T> onChanged)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            onChanged?.Invoke(value);
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool SetDoublePropertyValue(ref double field, double value, string propertyName)
        {
            var isSame =
                (double.IsNaN(field) && double.IsNaN(value)) ||
                field.Equals(value);
            if (isSame)
                return false;

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        private string _name = "";

        /// <summary>
        /// Sets and gets the Name property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetPropertyValue(ref _name, value, nameof(Name));
        }


        [NonSerialized]
        private Color _color = ViewModel.vMixWidgetSettingsViewModel.Colors[0].A;

        /// <summary>
        /// Sets and gets the Color property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Color Color
        {
            get => _color;
            set => SetPropertyValue(ref _color, value, nameof(Color));
        }

        [NonSerialized]
        private Color _borderColor = ViewModel.vMixWidgetSettingsViewModel.Colors[0].B;

        /// <summary>
        /// Sets and gets the BorderColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Color BorderColor
        {
            get => _borderColor;
            set => SetPropertyValue(ref _borderColor, value, nameof(BorderColor));
        }

        private double _top = 0;

        /// <summary>
        /// Sets and gets the Top property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Top
        {
            get => _top;
            set => SetDoublePropertyValue(ref _top, value, nameof(Top));
        }

        private double _left = 0;

        /// <summary>
        /// Sets and gets the Left property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Left
        {
            get => _left;
            set => SetDoublePropertyValue(ref _left, value, nameof(Left));
        }

        protected double _width = 128;

        /// <summary>
        /// Sets and gets the Width property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public virtual double Width
        {
            get => _width;
            set => SetDoublePropertyValue(ref _width, value, nameof(Width));
        }

        private double _height = double.NaN;

        /// <summary>
        /// Sets and gets the Height property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        //[XmlIgnore]
        public double Height
        {
            get => _height;
            set => SetDoublePropertyValue(ref _height, value, nameof(Height));
        }

        private int _ZIndex = 0;

        /// <summary>
        /// Sets and gets the ZIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int ZIndex
        {
            get => _ZIndex;
            set => SetPropertyValue(ref _ZIndex, value, nameof(ZIndex));
        }

        private bool _selected = false;

        /// <summary>
        /// Sets and gets the Selected property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Selected
        {
            get => _selected;
            set => SetPropertyValue(ref _selected, value, nameof(Selected));
        }

        [NonSerialized]
        private bool _isGhosted = false;

        /// <summary>
        /// Sets and gets the IsGhosted property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool IsGhosted
        {
            get => _isGhosted;
            set => SetPropertyValue(ref _isGhosted, value, nameof(IsGhosted));
        }

        private double _captionHeight = 0;

        /// <summary>
        /// Sets and gets the CaptionHeight property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        //[XmlIgnore]
        public double CaptionHeight
        {
            get => _captionHeight;
            set => SetDoublePropertyValue(ref _captionHeight, value, nameof(CaptionHeight));
        }

        private Hotkey[] _hotkey = null;

        /// <summary>
        /// Sets and gets the Hotkey property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Hotkey[] Hotkey
        {
            get => _hotkey;
            set => SetPropertyValue(ref _hotkey, value, nameof(Hotkey));
        }

        private bool _isTemplate = false;

        /// <summary>
        /// Sets and gets the IsTemplate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsTemplate
        {
            get => _isTemplate;
            set => SetPropertyValue(ref _isTemplate, value, nameof(IsTemplate));
        }

        private float _scale = 1.0f;

        /// <summary>
        /// Sets and gets the Scale property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float Scale
        {
            get => _scale;
            set => SetPropertyValue(ref _scale, value, nameof(Scale));
        }

        [XmlIgnore]
        public virtual vMixAPI.State State
        {
            get { return (vMixAPI.State)GetValue(StateProperty); }
            set
            {
                SetValue(StateProperty, value);
                if (_internalState == null && value != null)
                {
                    _internalState = value.Create();
                    _internalState?.Configure(value.Ip, value.Port, value.Login, value.Password);
                }
                else if (value == null)
                    _internalState = null;
                else
                {
                    _internalState?.Configure(value.Ip, value.Port, value.Login, value.Password);
                    //_internalState?.UpdateAsync();
                }
            }
        }


        ObservableCollection<Triple<string, string, string>> _info = new ObservableCollection<Triple<string, string, string>>();

        [XmlIgnore]
        public ObservableCollection<Triple<string, string, string>> Info
        {
            get => _info;
            set => SetPropertyValue(ref _info, value, nameof(Info));
        }

        private int _page = 0;

        /// <summary>
        /// Sets and gets the Page property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Page
        {
            get => _page;
            set => SetPropertyValue(ref _page, value, nameof(Page));
        }

        // Using a DependencyProperty as the backing store for State.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(nameof(State), typeof(vMixAPI.State), typeof(vMixControl), new PropertyMetadata(null, InternalPropertyChanged));

        [XmlIgnore]
        public bool IsVisualReady
        {
            get => _isVisualReady;
            set => SetPropertyValue(ref _isVisualReady, value, nameof(IsVisualReady));
        }
        private bool _isVisualReady = true;

        private static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == nameof(State))
            {
                if (e.OldValue != null)
                    ((vMixAPI.State)e.OldValue).OnStateSynced -= (d as vMixControl).VMixControl_Updated;
                if (e.NewValue != null)
                    ((vMixAPI.State)e.NewValue).OnStateSynced += (d as vMixControl).VMixControl_Updated;
                (d as vMixControl).OnStateSynced();
            }
        }

        public bool Intersect(vMixControl c)
        {
            return new Rect(Left, Top, Width, Height).Contains(new Rect(c.Left, c.Top, c.Width, c.Height));

        }

        private void VMixControl_Updated(object sender, vMixAPI.StateSyncedEventArgs e)
        {
            OnStateSynced();
        }

        internal virtual void OnStateSynced()
        {

        }

        public virtual void ExecuteHotkey(int index, object parameter)
        {
            if (parameter == null)
                ExecuteHotkey(index);
        }

        public virtual void ExecuteHotkey(int index)
        {

        }

        protected void RunOnUiThread(Action action, DispatcherPriority priority = DispatcherPriority.Background)
        {
            if (action == null)
                return;

            if (Dispatcher == null || Dispatcher.CheckAccess())
            {
                action();
                return;
            }

            Dispatcher.BeginInvoke(action, priority);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected static PropertyInfo GetPropertyOrNull(Type type, string name)
        {
            if (type == null || string.IsNullOrEmpty(name))
                return null;

            var cacheKey = Tuple.Create(type, name);
            return _propertyCache.GetOrAdd(cacheKey, key =>
                key.Item1.GetProperty(key.Item2, BindingFlags.Instance | BindingFlags.Public));
        }

        private string[] GetValueAndPropertyInfo(object obj, string path, out PropertyInfo found_prop, out object found)
        {
            found_prop = null;
            found = null;
            if (obj == null)
                return null;
            var type = obj.GetType();
            if (string.IsNullOrWhiteSpace(path))
                return null;
            var items = new List<string>();//path.Split('.');

            int intoArray = 0;
            int start = 0;
            for (int i = 0; i < path.Length; i++)
                switch (path[i])
                {
                    case '[':
                        intoArray++;
                        break;
                    case ']':
                        intoArray--;
                        break;
                    case '.':
                        if (intoArray == 0)
                        {
                            items.Add(path.Substring(start, i - start));
                            start = i + 1;
                        }
                        break;
                }
            items.Add(path.Substring(start, path.Length - start));

            if (items.Count < 1)
                return null;

            //If path goes to array
            if (items[0].Contains('['))
            {
                //Split path to array and index
                var propindex = items[0].Replace("[", ":").Replace("]", "").Split(':');
                found_prop = GetPropertyOrNull(type, propindex[0]);
                var array = found_prop?.GetValue(obj);
                if (array != null)
                {
                    //If it's array of Inputs just look to the right key
                    if (array is List<Input>)
                    {
                        //Look into global variables for key
                        string inputKey = Utils.FindInputKeyByVariable(propindex[1], Dispatcher);

                        found = Dispatcher.Invoke(() => (array as List<Input>).Where(x => x.Key == inputKey || x.Title == inputKey).FirstOrDefault() ?? (_regexInt.IsMatch(inputKey) ? (array as List<Input>).Where(x => x.Number == Convert.ToInt32(inputKey)).FirstOrDefault() : null));
                    }
                    else
                    {

                        //If it's just index
                        if (!propindex[1].Contains("/") && _regexInt.IsMatch(propindex[1]))
                        {

                            var idx = -1;
                            if (int.TryParse(propindex[1], out idx))
                            {
                                if (idx >= 0 && idx < (array as IList).Count)
                                    found = (array as IList)[idx];
                                else
                                    return null;
                            }
                            else
                                return null;
                        }
                        //If it has Type/Property/Value descriptor
                        else
                        {
                            var parts = propindex[1].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length < 3)
                            {
                                if (parts.Length > 0)
                                {
                                    //try find property by name
                                    foreach (var item in array as IList)
                                    {
                                        var prop = item.GetType().GetProperties()?.Where(x => x.Name == "Name" && x.PropertyType == typeof(string))?.SingleOrDefault();
                                        if (prop == null)
                                            return null;
                                        if ((string)prop.GetValue(item) == parts[0])
                                        {
                                            found = item;
                                            break;
                                        }
                                    }
                                }
                                else
                                    return null;
                            }
                            else
                            {
                                var tpof = Assembly.GetAssembly(typeof(Input)).DefinedTypes.Where(x => x.Name.Contains(parts[0])).First().UnderlyingSystemType;
                                var idx = int.Parse(parts[2]);
                                if (idx >= 0 && idx < (array as IList).Count)
                                {
                                    foreach (var item in array as IList)
                                    {
                                        if (tpof.IsInstanceOfType(item))
                                        {
                                            var p = GetPropertyOrNull(tpof, parts[1]);
                                            if (p != null && p.PropertyType == typeof(int))
                                                if ((int)p.GetValue(item) == idx)
                                                {
                                                    found = item;
                                                    break;
                                                }
                                        }
                                    }
                                }
                                else
                                    return null;
                            }
                        }
                    }

                }
                else
                {
                    return null;
                }
            }
            else
            {
                var propindex = items[0];

                found_prop = GetPropertyOrNull(type, propindex);
                var propinfo = found_prop;

                if (Thread.CurrentThread.ThreadState != System.Threading.ThreadState.Stopped &&
                    Thread.CurrentThread.ThreadState != System.Threading.ThreadState.AbortRequested &&
                    Thread.CurrentThread.ThreadState != System.Threading.ThreadState.StopRequested &&
                    Thread.CurrentThread.ThreadState != System.Threading.ThreadState.SuspendRequested)
                    if (Dispatcher.CheckAccess())
                        found = propinfo?.GetValue(obj);
                    else
                        found = Dispatcher.Invoke<object>(() =>
                        {
                            //TODO: CHECK
                            if (Thread.CurrentThread.ThreadState != System.Threading.ThreadState.Stopped) return propinfo?.GetValue(obj); else return null;
                        });
                else
                    found = null;
            }

            return items.ToArray();
        }

        protected object GetValueByPath(object obj, string path)
        {
            var items = GetValueAndPropertyInfo(obj, path, out PropertyInfo found_prop, out object found);
            if (items != null && items.Length > 1 && found != null)
                return GetValueByPath(found, items.Skip(1).Aggregate((x, y) => x + "." + y));
            return found;
        }

        protected void SetValueByPath(object obj, string path, object value)
        {
            var items = GetValueAndPropertyInfo(obj, path, out PropertyInfo found_prop, out object found);

            if (items != null && items.Length > 1 && found != null)
                SetValueByPath(found, items.Skip(1).Aggregate((x, y) => x + "." + y), value);
            else if (found_prop != null && found_prop.PropertyType == value.GetType())
                Dispatcher.Invoke(() => found_prop.SetValue(obj, value));
        }

        protected T GetValueByPath<T>(object obj, string path)
        {
            return (T)GetValueByPath(obj, path);
        }

        public vMixControl Copy()
        {
            if (MaxCount != -1) return null;
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer s = new XmlSerializer(typeof(vMixControl));
                s.Serialize(ms, this);
                ms.Seek(0, SeekOrigin.Begin);
                var ctrl = (vMixControl)s.Deserialize(ms);
                ctrl.WidgetId = Guid.NewGuid();
                //ctrl.Update();
                return ctrl;
            }
        }

        public int GetHotkeyNumber(string name)
        {
            if (_hotkey == null)
                _hotkey = GetHotkeys();
            for (int i = 0; i < _hotkey.Length; i++)
                if (_hotkey[i].Name == name)
                    return i;
            return -1;
        }

        public virtual void Update()
        {
            if (_hotkey == null)
                _hotkey = GetHotkeys();
            else
            {
                //UpdateHotkeys
                var hk = _hotkey;
                var hotkeys = GetHotkeys();
                if (hk.Length != hotkeys.Length)
                {
                    _hotkey = hotkeys;
                    for (int i = 0; i < _hotkey.Length; i++)
                        for (int j = 0; j < hk.Length; j++)
                            if (_hotkey[i].Name == hk[j].Name)
                                _hotkey[i] = hk[j];
                }
            }
            UpdateHotkeys();
        }

        public virtual Hotkey[] GetHotkeys()
        {
            return Array.Empty<Hotkey>();
        }

        public virtual void BeforePropertiesChanged()
        {
            //return Array.Empty<UserControl>();
        }

        /*public virtual void SetProperties(ViewModel.vMixWidgetSettingsViewModel viewModel)
        {
            //Name = viewModel.Name;
            //Color = viewModel.Color;

            //Hotkey = viewModel.Hotkey.ToArray();
            //ZIndex = viewModel.ZIndex;

            //WindowProperties = viewModel.WindowProperties;
            AfterPropertiesChanged();
            //SetProperties(viewModel.WidgetPropertiesControls);

            /*if (this is IvMixAutoUpdateWidget)
                (this as IvMixAutoUpdateWidget).Period = viewModel.Period;*/
        //}

        private void UpdateHotkeys()
        {
            if (_info == null)
                Info = new ObservableCollection<Triple<string, string, string>>();
            Info.Clear();
            if (Hotkey.Length != 0)
            {
                var active = Hotkey.Where(x => x.Active).ToArray();
                if (active.Length != 0)
                {
                    foreach (var item in active.Select(x => new Triple<string, string, string>()
                    {
                        A = string.IsNullOrWhiteSpace(x.Name) ? "N/A" : x.Name,
                        B = x.Link,
                        C = (x.Alt ? "Alt + " : "") +
                        (x.Ctrl ? "Ctrl + " : "") +
                        (x.Shift ? "Shift + " : "") +
                        x.Key.ToString()
                    }))
                        Info.Add(item);
                }
                else
                    Info.Add(new Triple<string, string, string>("N/A", "N/A", "N/A"));
            }
            else
                Info.Add(new Triple<string, string, string>("N/A", "N/A", "N/A"));
        }

        public virtual void AfterPropertiesChanged()
        {
            var color = vMixController.ViewModel.vMixWidgetSettingsViewModel.Colors.Where(x => x.A.A == this.Color.A && x.A.R == this.Color.R && x.A.G == this.Color.G && x.A.B == this.Color.B).FirstOrDefault();
            BorderColor = color?.B ?? this.Color;
            UpdateHotkeys();
        }

        [RelayCommand]
        private void PreviewKeyUp(System.Windows.Input.KeyEventArgs p)
        {
            if (p == null)
                return;

            if (p.Key == System.Windows.Input.Key.Return)
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
                if (parent != null)
                {
                    FocusManager.SetFocusedElement(parent, (IInputElement)parent);
                    //MoveFocus
                    ((FrameworkElement)parent).MoveFocus(new TraversalRequest(FocusNavigationDirection.Last) { });
                }
                else
                {
                    FocusManager.SetFocusedElement(Application.Current.MainWindow, Application.Current.MainWindow);
                    //MoveFocus
                    Application.Current.MainWindow.MoveFocus(new TraversalRequest(FocusNavigationDirection.Last) { });
                }

                Messenger.Send(new HotkeysEnabledMessage() { IsEnabled = true });
                p.Handled = true;
            }
        }

        [RelayCommand]
        private void GotFocus(RoutedEventArgs p)
        {
            Messenger.Send(new HotkeysEnabledMessage() { IsEnabled = false });
        }

        [RelayCommand]
        private void LostFocus(RoutedEventArgs p)
        {
            Messenger.Send(new HotkeysEnabledMessage() { IsEnabled = true });
        }

        protected bool _disposed = false;

        protected virtual void Dispose(bool managed)
        {
            if (_disposed) return;

            if (managed)
            {
                //_shadowUpdate.Tick -= ShadowUpdate_Tick;
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public virtual void Dispose()
        {
            BindingOperations.ClearAllBindings(this);
            Dispose(true);
            //throw new NotImplementedException();
        }
    }
}


