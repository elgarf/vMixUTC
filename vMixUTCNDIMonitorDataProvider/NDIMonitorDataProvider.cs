using GalaSoft.MvvmLight.CommandWpf;
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

namespace vMixUTCNDIMonitorDataProvider
{
    public class NDIMonitorDataProvider : DependencyObject, vMixControllerDataProvider.IvMixDataProvider, IDisposable, INotifyPropertyChanged
    {
        private OnWidgetUI _ui;
        private static Random _random = new Random();
        private int _number;
        //private string _sourcePath;
        /// <summary>
        /// The <see cref="Source" /> property's name.
        /// </summary>
        public const string SourcePropertyName = "Source";

        private NewTek.NDI.Source _source = null;

        /// <summary>
        /// Sets and gets the SourceName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public NewTek.NDI.Source Source
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
                RaisePropertyChanged(SourcePropertyName);
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
        /// The <see cref="Finder" /> property's name.
        /// </summary>
        public const string FinderPropertyName = "Finder";

        private NewTek.NDI.Finder _finder = new NewTek.NDI.Finder(true);

        /// <summary>
        /// Sets and gets the Finder property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public NewTek.NDI.Finder Finder
        {
            get
            {
                return _finder;
            }

            set
            {
                if (_finder == value)
                {
                    return;
                }

                _finder = value;
                RaisePropertyChanged(FinderPropertyName);
            }
        }


        private void RaisePropertyChanged(string sourceNamePropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(sourceNamePropertyName));
        }

        public static readonly DependencyProperty SourceNamesProperty =
            DependencyProperty.Register("SourceNames", typeof(ObservableCollection<String>), typeof(NDIMonitorDataProvider), new PropertyMetadata(new ObservableCollection<String>()));


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
            return new List<object>() { Source.Name, Source.IpAddress, MultiViewLayout, AspectRatio };
        }

        public void SetProperties(List<object> props)
        {
            if (props != null && props.Count > 0)
            {
                Source = new NewTek.NDI.Source((string)props[0], (string)props[1]);//_finder.Sources.Where(x => x.Name == (string)props[0] && x.IpAddress == (string)props[1]).FirstOrDefault();
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
            //_ui.Preview.ConnectedSource = null;
            //_ui.Preview.Disconnect();
            //_ui.Preview.Dispose();
            _finder.Dispose();
        }

        public NDIMonitorDataProvider()
        {
            _number = _random.Next();

            _ui = new OnWidgetUI() { DataContext = this };
            _ui.InitializeComponent();
            /*foreach (var b in ((Grid)_ui.FindName("Multiview8")).Children.OfType<Button>())
                b.Command = PlayInput;*/

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
