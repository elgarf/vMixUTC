using GalaSoft.MvvmLight.CommandWpf;
using NewTek;
using NewTek.NDI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class NDIMonitorDataProvider : DependencyObject, vMixControllerDataProvider.IvMixDataProvider, IDisposable, INotifyPropertyChanged
    {
        public object PreviewKeyUp { get; set; }
        public object GotFocus { get; set; }
        public object LostFocus { get; set; }

        private OnWidgetUI _ui;
        private static Random _random = new Random();
        private static Finder _finder;
        private static int _instances;
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
            get
            {
                return _source;
            }

            set
            {
                if (_source == value)
                {
                    return;
                }

                _source = value;
                NDISource = new NewTek.NDI.Source(value);
                RaisePropertyChanged(SourcePropertyName);
            }
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
            get
            {
                return _NDISource;
            }

            set
            {
                if (_NDISource == value)
                {
                    return;
                }

                _NDISource = value;
                RaisePropertyChanged(NDISourcePropertyName);
            }
        }


        /// <summary>
        /// The <see cref="AudioEnabled" /> property's name.
        /// </summary>
        public const string AudioEnabledPropertyName = "AudioEnabled";

        private bool _audioEnabled = false;

        /// <summary>
        /// Sets and gets the AudioEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool AudioEnabled
        {
            get
            {
                return _audioEnabled;
            }

            set
            {
                if (_audioEnabled == value)
                {
                    return;
                }

                _ui.Preview.IsAudioEnabled = value;

                _audioEnabled = value;
                RaisePropertyChanged(AudioEnabledPropertyName);
            }
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
            get
            {
                return _multiViewLayout;
            }

            set
            {
                if (_multiViewLayout == value)
                {
                    return;
                }

                _multiViewLayout = value;
                RaisePropertyChanged(MultiViewLayoutPropertyName);
            }
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
            get
            {
                return _aspectRatio;
            }

            set
            {
                if (_aspectRatio == value)
                {
                    return;
                }

                _aspectRatio = value;
                RaisePropertyChanged(AspectRatioPropertyName);
            }
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
            get
            {
                return _sources;
            }

            set
            {
                if (_sources == value)
                {
                    return;
                }

                _sources = value;
                RaisePropertyChanged(SourcesPropertyName);
            }
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
            return new List<object>() { NDISource?.Name, null, MultiViewLayout, AspectRatio, AudioEnabled };
        }

        public void SetProperties(List<object> props)
        {
            if (props != null && props.Count > 0)
            {
                /*foreach (var src in Finder.Sources)
                    if (src.Name == (string)props[0])
                        Source = src;*/
                Source = (string)props[0];

                if (props.Count >= 5)
                    AudioEnabled = (bool)props[4];
                //NDISource = new NewTek.NDI.Source((string)props[0]);//_finder.Sources.Where(x => x.Name == (string)props[0] && x.IpAddress == (string)props[1]).FirstOrDefault();
                //8in, 14in, legacy
                if (props.Count >= 3)
                    MultiViewLayout = (byte)props[2];
                //wide, normal, anamorphic
                if (props.Count >= 4)
                    AspectRatio = (byte)props[3];
            }


        }

        public void ShowProperties(System.Windows.Window owner)
        {
            return;
        }

        public void Dispose()
        {
            _ui.Preview.ConnectedSource = null;
            _ui.Preview.Disconnect();
            _ui.Preview.Dispose();
            
            _instances--;
            if (_instances <= 0)
            {
                _finder.Dispose();
                _finder = null;
            }
        }

        public NDIMonitorDataProvider()
        {
            _instances++;

            _ui = new OnWidgetUI() { DataContext = this };
            _ui.InitializeComponent();

            if (_finder == null)
                _finder = new Finder(true);
            else
                Sources = new ObservableCollection<string>(_finder.Sources.Select(x => x.Name).ToArray());
            _finder.Sources.CollectionChanged += Sources_CollectionChanged;

            // Not required, but "correct". (see the SDK documentation)
            if (!NDIlib.initialize())
            {
                // Cannot run NDI. Most likely because the CPU is not sufficient (see SDK documentation).
                // you can check this directly with a call to NDIlib.is_supported_CPU()
                if (!NDIlib.is_supported_CPU())
                {
                    MessageBox.Show("CPU unsupported.");
                }
                else
                {
                    // not sure why, but it's not going to run
                    MessageBox.Show("Cannot run NDI.");
                }

                // we can't go on
            }
            /*foreach (var b in ((Grid)_ui.FindName("Multiview8")).Children.OfType<Button>())
                b.Command = PlayInput;*/

        }

        private void Sources_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Sources = new ObservableCollection<string>(_finder.Sources.Select(x => x.Name).ToArray());
            //foreach (var item in _finder.Sources)
            //    Sources.Add(item.Name);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private RelayCommand<string> _playInput;

        /// <summary>
        /// Gets the PlayInput.
        /// </summary>
        public RelayCommand<string> PlayInput
        {
            get
            {
                return _playInput
                    ?? (_playInput = new RelayCommand<string>(
                    p =>
                    {
                        _values[Convert.ToInt32(p) - 1] = "@[cmd]Function=QuickPlay&Input={0}";
                    }));
            }
        }

        private RelayCommand<string> _multiViewChange;

        /// <summary>
        /// Gets the MultiViewChange.
        /// </summary>
        public RelayCommand<string> MultiViewChange
        {
            get
            {
                return _multiViewChange
                    ?? (_multiViewChange = new RelayCommand<string>(
                    p =>
                    {
                        MultiViewLayout = Convert.ToByte(p);
                    }));
            }
        }
    }
}
