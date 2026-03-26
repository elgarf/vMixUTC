using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewTek;
using NewTek.NDI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace UTCNDIMonitorDataProvider
{
    public partial class NDIMonitorDataProvider : DependencyObject, vMixControllerDataProvider.IvMixDataProvider, IDisposable, INotifyPropertyChanged
    {
        public object PreviewKeyUp { get; set; }
        public object GotFocus { get; set; }
        public object LostFocus { get; set; }

        private OnWidgetUI _ui;
        private static Random _random = new Random();
        private static Finder _finder;
        private static OMT.Finder _finderOMT;
        private static int _instances;
        private static bool _initialized = false;
        private bool _eventsSubscribed;

        private static event EventHandler OnReset;

        //private string _sourcePath;
        /// <summary>
        /// The <see cref="Source" /> property's name.
        /// </summary>
        public const string SourcePropertyName = "Source";

        private string _source = null;

        /// <summary>
        /// Sets and gets the SourceName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Source
        {
            get => _source;
            set => SetPropertyValue(ref _source, value, SourcePropertyName, v => NDISource = new NewTek.NDI.Source(v));
        }

        /// <summary>
        /// The <see cref="NDISource" /> property's name.
        /// </summary>
        public const string NDISourcePropertyName = "NDISource";

        private NewTek.NDI.Source _NDISource = null;

        /// <summary>
        /// Sets and gets the NDISource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public NewTek.NDI.Source NDISource
        {
            get => _NDISource;
            set => SetPropertyValue(ref _NDISource, value, NDISourcePropertyName);
        }


        /// <summary>
        /// The <see cref="IsAudioEnabled" /> property's name.
        /// </summary>
        public const string IsAudioEnabledPropertyName = "IsAudioEnabled";

        private bool _isAudioEnabled = false;

        /// <summary>
        /// Sets and gets the AudioEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsAudioEnabled
        {
            get => _isAudioEnabled;
            set => SetPropertyValue(ref _isAudioEnabled, value, IsAudioEnabledPropertyName);
        }

        /// <summary>
        /// The <see cref="IsLowBandwidth" /> property's name.
        /// </summary>
        public const string IsLowBandwidthPropertyName = "IsLowBandwidth";

        private bool _isLowBandwidth = true;

        /// <summary>
        /// Sets and gets the IsLowBandwidth property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsLowBandwidth
        {
            get => _isLowBandwidth;
            set => SetPropertyValue(ref _isLowBandwidth, value, IsLowBandwidthPropertyName);
        }

        /// <summary>
        /// The <see cref="MultiViewLayout" /> property's name.
        /// </summary>
        public const string MultiViewLayoutPropertyName = "MultiViewLayout";

        private byte _multiViewLayout = 0;

        /// <summary>
        /// Sets and gets the MultiViewLayout property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public byte MultiViewLayout
        {
            get => _multiViewLayout;
            set => SetPropertyValue(ref _multiViewLayout, value, MultiViewLayoutPropertyName);
        }

        /// <summary>
        /// The <see cref="AspectRatio" /> property's name.
        /// </summary>
        public const string AspectRatioPropertyName = "AspectRatio";

        private byte _aspectRatio = 0;

        /// <summary>
        /// Sets and gets the AspectRatio property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public byte AspectRatio
        {
            get => _aspectRatio;
            set => SetPropertyValue(ref _aspectRatio, value, AspectRatioPropertyName);
        }


        /// <summary>
        /// The <see cref="Sources" /> property's name.
        /// </summary>
        public const string SourcesPropertyName = "Sources";

        private ObservableCollection<string> _sources = new ObservableCollection<string>();

        /// <summary>
        /// Sets and gets the Sources property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> Sources
        {
            get => _sources;
            set => SetPropertyValue(ref _sources, value, SourcesPropertyName);
        }

        private void RaisePropertyChanged(string sourceNamePropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(sourceNamePropertyName));
        }

        //public static readonly DependencyProperty SourceNamesProperty =
        //    DependencyProperty.Register("SourceNames", typeof(ObservableCollection<string>), typeof(NDIMonitorDataProvider), new PropertyMetadata(new ObservableCollection<String>()));


        public System.Windows.UIElement CustomUI
        {
            get
            {
                return _ui;
            }
        }

        public bool IsProvidingCustomProperties
        {
            get
            {
                return false;
            }
        }

        public int Period
        {
            get;
            set;
        }


        string[] _values = new string[] {
            "@[cmd]",
            "@[cmd]",
            "@[cmd]",
            "@[cmd]",
            "@[cmd]",
            "@[cmd]",
            "@[cmd]",
            "@[cmd]",
            "@[cmd]",
            "@[cmd]",
            "@[cmd]",
            "@[cmd]",
            "@[cmd]",
            "@[cmd]"
        };
        public string[] Values
        {
            get
            {
                var vals = _values.ToArray();
                for (int i = 0; i < _values.Length; i++)
                {
                    _values[i] = "@[cmd]";
                }
                return vals;
            }
        }

        public List<object> GetProperties()
        {
            return new List<object>() { NDISource?.Name, null, MultiViewLayout, AspectRatio, IsAudioEnabled, IsLowBandwidth };
        }

        public void SetProperties(List<object> props)
        {
            if (props != null && props.Count > 0)
            {
                /*foreach (var src in Finder.Sources)
                    if (src.Name == (string)props[0])
                        Source = src;*/
                Source = (string)props[0];

                //NDISource = new NewTek.NDI.Source((string)props[0]);//_finder.Sources.Where(x => x.Name == (string)props[0] && x.IpAddress == (string)props[1]).FirstOrDefault();
                //8in, 14in, legacy
                if (props.Count >= 3)
                    MultiViewLayout = (byte)props[2];
                //wide, normal, anamorphic
                if (props.Count >= 4)
                    AspectRatio = (byte)props[3];
                //audio enabled
                if (props.Count >= 5)
                    IsAudioEnabled = (bool)props[4];
                //low bandwidth
                if (props.Count >= 6)
                    IsLowBandwidth = (bool)props[5];
            }


        }

        public void ShowProperties(System.Windows.Window owner)
        {
            return;
        }

        ~NDIMonitorDataProvider()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ui.Preview.ConnectedSource = null;
                _ui.Preview.Disconnect();
                _ui.Preview.Dispose();
                OnReset -= NDIMonitorDataProvider_OnReset;
                UnsubscribeFinderEvents();
                _instances--;
                if (_instances <= 0)
                {
                    _finder?.Dispose();
                    _finder = null;
                    _finderOMT?.Dispose();
                    _finderOMT = null;

                    NDIlib.destroy();
                    _initialized = false;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public NDIMonitorDataProvider()
        {
            _instances++;
            OnReset += NDIMonitorDataProvider_OnReset;
            _ui = new OnWidgetUI() { DataContext = this };
            //_ui.InitializeComponent();

            if (_finder == null)
            {
                _finder = new Finder(true);
                _finderOMT = new OMT.Finder();
            }
            else
            {
                RefreshSources();
            }
            SubscribeFinderEvents();
            RefreshSources();


            // Not required, but "correct". (see the SDK documentation)
            if (!_initialized)
                if (!NDIlib.initialize())
                {
                    // Cannot run NDI. Most likely because the CPU is not sufficient (see SDK documentation).
                    // you can check this directly with a call to NDIlib.is_supported_CPU()
                    if (!NDIlib.is_supported_CPU())
                    {
                        MessageBox.Show("CPU unsupported");
                    }
                    else
                    {
                        // not sure why, but it's not going to run
                        MessageBox.Show("Cannot run NDI");
                    }

                    // we can't go on
                }
                else
                    _initialized = true;
            /*foreach (var b in ((Grid)_ui.FindName("Multiview8")).Children.OfType<Button>())
                b.Command = PlayInput;*/

        }

        private void NDIMonitorDataProvider_OnReset(object sender, EventArgs e)
        {
            _ui.Preview.Disconnect();
            SubscribeFinderEvents();

            RaisePropertyChanged(nameof(Source));
        }

        private void Sources_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshSources();
            //foreach (var item in _finder.Sources)
            //    Sources.Add(item.Name);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [RelayCommand]
        private void PlayInput(string p)
        {
            _values[Convert.ToInt32(p) - 1] = "@[cmd]Function=QuickPlay&Input={0}";
        }
        [RelayCommand]
        private void MultiViewChange(string p)
        {
            MultiViewLayout = Convert.ToByte(p);
        }
        [RelayCommand]
        private void Reset()
        {
            if (_finder != null)
            {
                UnsubscribeFinderEvents();
                _finder.Dispose();
                _finder = null;
                _finderOMT.Dispose();
                _finderOMT = null;
                _finder = new Finder(true);
                _finderOMT = new OMT.Finder();
                SubscribeFinderEvents();
                RefreshSources();
                OnReset?.Invoke(this, new EventArgs());
            }
        }

        
        

        private void SubscribeFinderEvents()
        {
            if (_eventsSubscribed || _finder == null || _finderOMT == null)
            {
                return;
            }

            _finder.Sources.CollectionChanged += Sources_CollectionChanged;
            _finderOMT.Sources.CollectionChanged += Sources_CollectionChanged;
            _eventsSubscribed = true;
        }

        private void UnsubscribeFinderEvents()
        {
            if (!_eventsSubscribed || _finder == null || _finderOMT == null)
            {
                return;
            }

            _finder.Sources.CollectionChanged -= Sources_CollectionChanged;
            _finderOMT.Sources.CollectionChanged -= Sources_CollectionChanged;
            _eventsSubscribed = false;
        }

        private void RefreshSources()
        {
            if (_finder == null || _finderOMT == null)
            {
                Sources = new ObservableCollection<string>();
                return;
            }

            var updated = _finder.Sources.Select(x => x.Name)
                .Union(_finderOMT.Sources.Select(x => "OMT: " + x))
                .ToArray();

            if (_sources.SequenceEqual(updated))
            {
                return;
            }

            Sources = new ObservableCollection<string>(updated);
        }

        private bool SetPropertyValue<T>(ref T field, T value, string propertyName, Action<T> onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            onChanged?.Invoke(value);
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}


