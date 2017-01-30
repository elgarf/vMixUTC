using GalaSoft.MvvmLight;
using System;
using System.Collections;
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
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;
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
        XmlInclude(typeof(vMixControlMultiState))]
    public class vMixControl : DependencyObject, INotifyPropertyChanged, IDisposable
    {


        internal static Dictionary<Type, UserControl> ControlsStore = new Dictionary<Type, UserControl>();
        internal static List<UserControl> ControlsStoreUsage = new List<UserControl>();
        internal static State _internalState;

        static DispatcherTimer _shadowUpdate;

        public vMixControl()
        {
            _shadowUpdate = new DispatcherTimer();
            _shadowUpdate.Interval = TimeSpan.FromSeconds(1);
            _shadowUpdate.Tick += _shadowUpdate_Tick;
            _shadowUpdate.Start();

            _hotkey = GetHotkeys();
        }

        private void _shadowUpdate_Tick(object sender, EventArgs e)
        {
            ShadowUpdate();
        }


        public virtual void ShadowUpdate() { }


        public virtual string Type { get; }


        

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

        internal void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
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
        private Color _color = ViewModel.vMixControlSettingsViewModel.Colors[0].A;

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
        private Color _borderColor = ViewModel.vMixControlSettingsViewModel.Colors[0].B;

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

        private double _width = 128;

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

        [XmlIgnore]
        public virtual vMixAPI.State State
        {
            get { return (vMixAPI.State)GetValue(StateProperty); }
            set { SetValue(StateProperty, value);
                if (_internalState == null && value != null)
                {
                    _internalState = value.Create();
                }
                else if (value == null)
                    _internalState = null;
                else
                    _internalState.Configure(value.Ip, value.Port);
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
                    ((vMixAPI.State)e.OldValue).OnStateUpdated -= (d as vMixControl).VMixControl_Updated;
                if (e.NewValue != null)
                    ((vMixAPI.State)e.NewValue).OnStateUpdated += (d as vMixControl).VMixControl_Updated;
                (d as vMixControl).OnStateUpdated();
            }
        }

        private void VMixControl_Updated(object sender, vMixAPI.StateUpdatedEventArgs e)
        {
            OnStateUpdated();
        }

        internal virtual void OnStateUpdated()
        {

        }

        public virtual void ExecuteHotkey(int index) { }

        public event PropertyChangedEventHandler PropertyChanged;

        protected object GetValueByPath(object obj, string path)
        {
            if (obj == null)
                return null;
            var type = obj.GetType();
            object found = null;
            if (string.IsNullOrWhiteSpace(path))
                return null;
            var items = path.Split('.');
            if (items.Length < 1)
                return null;

            if (items[0].Contains('['))
            {
                var propindex = items[0].Replace("[", ":").Replace("]", "").Split(':');
                var array = type.GetProperty(propindex[0]).GetValue(obj);
                try
                {
                    if (array is List<Input>)
                        found = (array as List<Input>).Where(x => x.Key == propindex[1]).FirstOrDefault();
                    else
                    {
                        var idx = int.Parse(propindex[1]);
                        if (idx >= 0 && idx < (array as IList).Count)
                            found = (array as IList)[idx];
                        else
                            return null;
                    }

                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                var propindex = items[0];
                found = type.GetProperty(propindex).GetValue(obj);
            }
            if (items.Length > 1 && found != null)
                return GetValueByPath(found, items.Skip(1).Aggregate((x, y) => x + "." + y));
            return found;
        }

        protected void SetValueByPath(object obj, string path, object value)
        {
            if (obj == null)
                return;
            var type = obj.GetType();
            object found = null;
            System.Reflection.PropertyInfo found_prop = null;
            if (string.IsNullOrWhiteSpace(path))
                return;
            var items = path.Split('.');
            if (items.Length < 1)
                return;

            if (items[0].Contains('['))
            {
                var propindex = items[0].Replace("[", ":").Replace("]", "").Split(':');
                found_prop = type.GetProperties().Where(x => x.Name == propindex[0]).FirstOrDefault();
                if (found_prop != null)
                {
                    var array = found_prop.GetValue(obj);
                    //try
                    {
                        if (array is List<Input>)
                            found = (array as List<Input>).Where(x => x.Key == propindex[1]).FirstOrDefault();
                        else
                        {
                            var idx = int.Parse(propindex[1]);
                            if (idx >= 0 && idx < (array as IList).Count)
                                found = (array as IList)[idx];
                            else
                                found = null;
                        }

                    }
                    /*catch (Exception)
                    {
                        found = null;
                    }*/
                }
                else
                    found = null;

            }
            else
            {
                var propindex = items[0];
                found_prop = type.GetProperties().Where(x => x.Name == propindex).FirstOrDefault();
                if (found_prop != null)
                    found = found_prop.GetValue(obj);
                else
                    found = null;

            }
            if (items.Length > 1 && found != null)
                SetValueByPath(found, items.Skip(1).Aggregate((x, y) => x + "." + y), value);
            else if (found_prop != null)
                found_prop.SetValue(obj, value);
        }

        protected T GetValueByPath<T>(object obj, string path)
        {
            return (T)GetValueByPath(obj, path);
        }

        protected T GetPropertyControl<T> () where T: UserControl
        {
            if (ControlsStore.ContainsKey(typeof(T)) && !ControlsStoreUsage.Contains(ControlsStore[typeof(T)]))
            {
                ControlsStore[typeof(T)].Tag = null;
                ControlsStoreUsage.Add(ControlsStore[typeof(T)]);
                return (T)ControlsStore[typeof(T)];
            }
            else
            {
                var c = (T)typeof(T).GetConstructor(new Type[0]).Invoke(new object[0]);
                c.Tag = null;
                if (!ControlsStore.ContainsKey(typeof(T)))
                {
                    ControlsStore.Add(typeof(T), c);
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
                return (vMixControl)s.Deserialize(ms);
            }
        }

        public virtual void Update()
        {

        }

        public virtual Hotkey[] GetHotkeys()
        {
            return new Hotkey[] { };
        }

        public virtual UserControl[] GetPropertiesControls()
        {
            return new UserControl[0];
        }

        public virtual void SetProperties(ViewModel.vMixControlSettingsViewModel viewModel)
        {
            Name = viewModel.Name;
            Color = viewModel.Color;
            BorderColor = vMixController.ViewModel.vMixControlSettingsViewModel.Colors.Where(x => x.A == viewModel.Color).FirstOrDefault().B;
            Hotkey = viewModel.Hotkey.ToArray();

            SetProperties(viewModel.WidgetPropertiesControls);

            if (this is IvMixAutoUpdateWidget)
                (this as IvMixAutoUpdateWidget).Period = viewModel.Period;
        }

        public virtual void SetProperties(UserControl[] _controls)
        {
            foreach (var item in _controls)
                if (ControlsStoreUsage.Contains(item))
                    ControlsStoreUsage.Remove(item);
        }

        protected virtual void Dispose(bool managed)
        {

        }

        public virtual void Dispose()
        {
            _shadowUpdate.Stop();
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
