using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;
using vMixController.Extensions;
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
        XmlInclude(typeof(vMixControlExternalData)),
        XmlInclude(typeof(vMixControlTimer)),
        XmlInclude(typeof(vMixControlContainer)),
        XmlInclude(typeof(vMixControlMultiState)),
        XmlInclude(typeof(vMixControlMidiInterface)),
        XmlInclude(typeof(vMixControlClock)),
        XmlInclude(typeof(vMixControlRegion)),
        XmlInclude(typeof(vMixControlVolume)),
        XmlInclude(typeof(vMixControlSlider)),
        XmlInclude(typeof(vMixControlTBar))]
    public class vMixControl : DependencyObject, INotifyPropertyChanged, IDisposable
    {

        protected NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        internal static Dictionary<string, UserControl> ControlsStore = new Dictionary<string, UserControl>();
        internal static List<UserControl> ControlsStoreUsage = new List<UserControl>();
        internal static State _internalState;
        internal static Regex _regexInt = new Regex(@"^\d+$");

        protected static DispatcherTimer _shadowUpdate;

        public static TimeSpan ShadowUpdatePollTime
        {
            get { return _shadowUpdate.Interval; }
            set {
                _shadowUpdate.Interval = value;
                _shadowUpdate.Stop();
                _shadowUpdate.Start();
            }
        }


        public vMixControl()
        {
            if (_shadowUpdate == null)
            {
                _shadowUpdate = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(1000)
                };
                _shadowUpdate.Start();
            }

            WindowProperties = ((ViewModelLocator)Application.Current.FindResource("Locator")).WidgetSettings.WindowProperties;
        }


        public virtual string Type { get; }
        public virtual bool IsResizeableVertical { get { return false; } }

        public virtual int MaxCount => -1;


        /// <summary>
        /// The <see cref="WindowProperties" /> property's name.
        /// </summary>
        public const string WindowPropertiesPropertyName = "WindowProperties";

        private Quadriple<double?, double?, double?, double?> _windowProperties = new Quadriple<double?, double?, double?, double?>();

        /// <summary>
        /// Sets and gets the WindowProperties property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Quadriple<double?, double?, double?, double?> WindowProperties
        {
            get
            {
                return _windowProperties;
            }

            set
            {
                if (_windowProperties == value)
                {
                    return;
                }

                _windowProperties = value;
                RaisePropertyChanged(WindowPropertiesPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsPasswordLockable" /> property's name.
        /// </summary>
        public const string IsPasswordLockablePropertyName = "IsPasswordLockable";

        private bool _isPasswordLockable = true;

        /// <summary>
        /// Sets and gets the IsPasswordLockable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPasswordLockable
        {
            get
            {
                return _isPasswordLockable;
            }

            set
            {
                if (_isPasswordLockable == value)
                {
                    return;
                }

                _isPasswordLockable = value;
                RaisePropertyChanged(IsPasswordLockablePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsPasswordLocked" /> property's name.
        /// </summary>
        public const string IsPasswordLockedPropertyName = "IsPasswordLocked";

        private bool _isPasswordLocked = false;

        /// <summary>
        /// Sets and gets the IsPasswordLocked property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPasswordLocked
        {
            get
            {
                return _isPasswordLocked;
            }

            set
            {
                if (_isPasswordLocked == value)
                {
                    return;
                }

                _isPasswordLocked = value;
                RaisePropertyChanged(IsPasswordLockedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Locked" /> property's name.
        /// </summary>
        public const string LockedPropertyName = "Locked";

        private bool _locked = false;

        /// <summary>
        /// Sets and gets the Locked property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Locked
        {
            get
            {
                return _locked;
            }

            set
            {
                if (_locked == value)
                {
                    return;
                }

                _locked = value;
                RaisePropertyChanged(LockedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsCaptionVisible" /> property's name.
        /// </summary>
        public const string IsCaptionVisiblePropertyName = "IsCaptionVisible";

        private bool _isCaptionVisible = true;

        /// <summary>
        /// Sets and gets the IsCaptionVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsCaptionVisible
        {
            get
            {
                return _isCaptionVisible;
            }

            set
            {
                if (_isCaptionVisible == value)
                {
                    return;
                }

                _isCaptionVisible = value;
                RaisePropertyChanged(IsCaptionVisiblePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsCaptionOn" /> property's name.
        /// </summary>
        public const string IsCaptionOffPropertyName = "IsCaptionOn";

        private bool _isCaptionOn = true;

        /// <summary>
        /// Sets and gets the IsCaptionOff property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public virtual bool IsCaptionOn
        {
            get
            {
                return _isCaptionOn;
            }

            set
            {
                if (_isCaptionOn == value)
                {
                    return;
                }

                _isCaptionOn = value;
                RaisePropertyChanged(IsCaptionOffPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsFocused" /> property's name.
        /// </summary>
        public const string IsFocusedPropertyName = "IsFocused";

        [NonSerialized]
        private bool _isFocused = false;

        /// <summary>
        /// Sets and gets the IsFocused property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool IsFocused
        {
            get
            {
                return _isFocused;
            }

            set
            {
                if (_isFocused == value)
                {
                    return;
                }

                _isFocused = value;
                RaisePropertyChanged(IsFocusedPropertyName);
            }
        }

        internal void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// The <see cref="Name" /> property's name.
        /// </summary>
        public const string NamePropertyName = "Name";

        private string _name = "";

        /// <summary>
        /// Sets and gets the Name property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                RaisePropertyChanged(NamePropertyName);
            }
        }


        /// <summary>
        /// The <see cref="Color" /> property's name.
        /// </summary>
        public const string ColorPropertyName = "Color";
        [NonSerialized]
        private Color _color = ViewModel.vMixWidgetSettingsViewModel.Colors[0].A;

        /// <summary>
        /// Sets and gets the Color property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Color Color
        {
            get
            {
                return _color;
            }

            set
            {
                if (_color == value)
                {
                    return;
                }

                _color = value;
                RaisePropertyChanged(ColorPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="BorderColor" /> property's name.
        /// </summary>
        public const string BorderColorPropertyName = "BorderColor";
        [NonSerialized]
        private Color _borderColor = ViewModel.vMixWidgetSettingsViewModel.Colors[0].B;

        /// <summary>
        /// Sets and gets the BorderColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Color BorderColor
        {
            get
            {
                return _borderColor;
            }

            set
            {
                if (_borderColor == value)
                {
                    return;
                }

                _borderColor = value;
                RaisePropertyChanged(BorderColorPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Top" /> property's name.
        /// </summary>
        public const string TopPropertyName = "Top";

        private double _top = 0;

        /// <summary>
        /// Sets and gets the Top property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Top
        {
            get
            {
                return _top;
            }

            set
            {
                if (_top == value)
                {
                    return;
                }

                _top = value;
                RaisePropertyChanged(TopPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Left" /> property's name.
        /// </summary>
        public const string LeftPropertyName = "Left";

        private double _left = 0;

        /// <summary>
        /// Sets and gets the Left property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Left
        {
            get
            {
                return _left;
            }

            set
            {
                if (_left == value)
                {
                    return;
                }

                _left = value;
                RaisePropertyChanged(LeftPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Width" /> property's name.
        /// </summary>
        public const string WidthPropertyName = "Width";

        protected double _width = 128;

        /// <summary>
        /// Sets and gets the Width property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public virtual double Width
        {
            get
            {
                return _width;
            }

            set
            {
                if (_width == value)
                {
                    return;
                }

                _width = value;
                RaisePropertyChanged(WidthPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Height" /> property's name.
        /// </summary>
        public const string HeightPropertyName = "Height";

        private double _height = double.NaN;

        /// <summary>
        /// Sets and gets the Height property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        //[XmlIgnore]
        public double Height
        {
            get
            {
                return _height;
            }

            set
            {
                if (_height == value)
                {
                    return;
                }

                _height = value;
                RaisePropertyChanged(HeightPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="ZIndex" /> property's name.
        /// </summary>
        public const string ZIndexPropertyName = "ZIndex";

        private int _ZIndex = 0;

        /// <summary>
        /// Sets and gets the ZIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int ZIndex
        {
            get
            {
                return _ZIndex;
            }

            set
            {
                if (_ZIndex == value)
                {
                    return;
                }

                _ZIndex = value;
                RaisePropertyChanged(ZIndexPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Selected" /> property's name.
        /// </summary>
        public const string SelectedPropertyName = "Selected";

        private bool _selected = false;

        /// <summary>
        /// Sets and gets the Selected property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Selected
        {
            get
            {
                return _selected;
            }

            set
            {
                if (_selected == value)
                {
                    return;
                }

                _selected = value;
                RaisePropertyChanged(SelectedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsGhosted" /> property's name.
        /// </summary>
        public const string IsGhostedPropertyName = "IsGhosted";

        [NonSerialized]
        private bool _isGhosted = false;

        /// <summary>
        /// Sets and gets the IsGhosted property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
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
                RaisePropertyChanged(IsGhostedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CaptionHeight" /> property's name.
        /// </summary>
        public const string CaptionHeightPropertyName = "CaptionHeight";

        private double _captionHeight = 0;

        /// <summary>
        /// Sets and gets the CaptionHeight property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        //[XmlIgnore]
        public double CaptionHeight
        {
            get
            {
                return _captionHeight;
            }

            set
            {
                if (_captionHeight == value)
                {
                    return;
                }

                _captionHeight = value;
                RaisePropertyChanged(CaptionHeightPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Hotkey" /> property's name.
        /// </summary>
        public const string HotkeyPropertyName = "Hotkey";

        private Hotkey[] _hotkey = null;

        /// <summary>
        /// Sets and gets the Hotkey property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Hotkey[] Hotkey
        {
            get
            {
                //if (_hotkey == null) return new ObservableCollection<Classes.Hotkey>(GetHotkeys());                
                return _hotkey;
            }

            set
            {
                if (_hotkey == value)
                {
                    return;
                }

                _hotkey = value;
                RaisePropertyChanged(HotkeyPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsTemplate" /> property's name.
        /// </summary>
        public const string IsTemplatePropertyName = "IsTemplate";

        private bool _isTemplate = false;

        /// <summary>
        /// Sets and gets the IsTemplate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsTemplate
        {
            get
            {
                return _isTemplate;
            }

            set
            {
                if (_isTemplate == value)
                {
                    return;
                }

                _isTemplate = value;
                RaisePropertyChanged(IsTemplatePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Scale" /> property's name.
        /// </summary>
        public const string ScalePropertyName = "Scale";

        private float _scale = 1.0f;

        /// <summary>
        /// Sets and gets the Scale property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public float Scale
        {
            get
            {
                return _scale;
            }

            set
            {
                if (_scale == value)
                {
                    return;
                }

                _scale = value;
                RaisePropertyChanged(ScalePropertyName);
            }
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
                    _internalState?.Configure(value.Ip, value.Port);
                }
                else if (value == null)
                    _internalState = null;
                else
                {
                    _internalState?.Configure(value.Ip, value.Port);
                    //_internalState?.UpdateAsync();
                }
            }
        }


        ObservableCollection<Triple<string, string, string>> _info = new ObservableCollection<Triple<string, string, string>>();

        [XmlIgnore]
        public ObservableCollection<Triple<string, string, string>> Info
        {
            get
            {
                return _info;
            }
            set
            {
                _info = value;
                RaisePropertyChanged("Info");
            }
        }


        /// <summary>
        /// The <see cref="Page" /> property's name.
        /// </summary>
        public const string PagePropertyName = "Page";

        private int _page = 0;

        /// <summary>
        /// Sets and gets the Page property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Page
        {
            get
            {
                return _page;
            }

            set
            {
                if (_page == value)
                {
                    return;
                }

                _page = value;
                RaisePropertyChanged(PagePropertyName);
            }
        }

        // Using a DependencyProperty as the backing store for State.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(vMixAPI.State), typeof(vMixControl), new PropertyMetadata(null, InternalPropertyChanged));

        private static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "State")
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

        public virtual void ExecuteHotkey(int index, object parameter) {
            if (parameter == null)
                ExecuteHotkey(index);
        }

        public virtual void ExecuteHotkey(int index) {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected static PropertyInfo GetPropertyOrNull(Type type, string name)
        {
            return type.GetProperties().Where(x => x.Name == name).FirstOrDefault();
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

        protected static T GetPropertyControl<T>(string key = "") where T : UserControl
        {
            if (ControlsStore.ContainsKey(typeof(T).FullName + key ?? ""))
            {
                ControlsStore[typeof(T).FullName + key].Tag = key;
                ControlsStoreUsage.Add(ControlsStore[typeof(T).FullName + key ?? ""]);
                return (T)ControlsStore[typeof(T).FullName + key ?? ""];
            }
            else
            {
                var c = (T)typeof(T).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
                c.Tag = key;
                if (!ControlsStore.ContainsKey(typeof(T).FullName + key ?? ""))
                {
                    ControlsStore.Add(typeof(T).FullName + key ?? "", c);
                    ControlsStoreUsage.Add(c);
                }
                else
                    ControlsStoreUsage.Add(c);
                return c;
            }
        }

        public vMixControl Copy()
        {

            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer s = new XmlSerializer(typeof(vMixControl));
                s.Serialize(ms, this);
                ms.Seek(0, SeekOrigin.Begin);
                var ctrl = (vMixControl)s.Deserialize(ms);
                //ctrl.Update();
                return ctrl;
            }
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

        public virtual UserControl[] GetPropertiesControls()
        {
            return Array.Empty<UserControl>();
        }

        public virtual void SetProperties(ViewModel.vMixWidgetSettingsViewModel viewModel)
        {
            Name = viewModel.Name;
            Color = viewModel.Color;
            BorderColor = vMixController.ViewModel.vMixWidgetSettingsViewModel.Colors.Where(x => x.A == viewModel.Color).FirstOrDefault().B;
            Hotkey = viewModel.Hotkey.ToArray();

            WindowProperties = viewModel.WindowProperties;

            SetProperties(viewModel.WidgetPropertiesControls);

            /*if (this is IvMixAutoUpdateWidget)
                (this as IvMixAutoUpdateWidget).Period = viewModel.Period;*/
            UpdateHotkeys();

            GC.Collect(1);
        }

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

        public virtual void SetProperties(UserControl[] _controls)
        {
            foreach (var item in _controls)
                if (ControlsStoreUsage.Contains(item))
                    ControlsStoreUsage.Remove(item);
        }

        [NonSerialized]
        private RelayCommand<System.Windows.Input.KeyEventArgs> _previewKeyUp;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand<System.Windows.Input.KeyEventArgs> PreviewKeyUp
        {
            get
            {
                return _previewKeyUp
                    ?? (_previewKeyUp = new RelayCommand<System.Windows.Input.KeyEventArgs>(
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

                            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Pair<string, bool>() { A = "Hotkeys", B = true });
                            p.Handled = true;
                        }
                    }));
            }
        }
        [NonSerialized]
        private RelayCommand<RoutedEventArgs> _gotFocus;

        /// <summary>
        /// Gets the GotFocus.
        /// </summary>
        public RelayCommand<RoutedEventArgs> GotFocus
        {
            get
            {
                return _gotFocus
                    ?? (_gotFocus = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Pair<string, bool>() { A = "Hotkeys", B = false });
                    }));
            }
        }
        [NonSerialized]
        private RelayCommand<RoutedEventArgs> _lostFocus;

        /// <summary>
        /// Gets the LostFocus.
        /// </summary>
        public RelayCommand<RoutedEventArgs> LostFocus
        {
            get
            {
                return _lostFocus
                    ?? (_lostFocus = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Pair<string, bool>() { A = "Hotkeys", B = true });
                    }));
            }
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
