using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

        public string[] Values
        {
            get
            {
                return new string[0];
            }
        }

        public List<object> GetProperties()
        {
            return new List<object>() { Source.Name, Source.IpAddress };
        }

        public void SetProperties(List<object> props)
        {
            if (props.Count > 0)
                Source = new NewTek.NDI.Source((string)props[0], (string)props[1]);//_finder.Sources.Where(x => x.Name == (string)props[0] && x.IpAddress == (string)props[1]).FirstOrDefault();
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
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
