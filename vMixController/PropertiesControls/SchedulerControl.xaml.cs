using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using vMixController.Classes;

namespace vMixController.PropertiesControls
{
    /// <summary>
    /// Логика взаимодействия для PathsControl.xaml
    /// </summary>
    public partial class SchedulerControl : UserControl, INotifyPropertyChanged
    {




        /// <summary>
            /// The <see cref="Events" /> property's name.
            /// </summary>
        public const string EventsPropertyName = "Events";

        private ObservableCollection<Pair<DateTime, string>> _events = new ObservableCollection<Pair<DateTime, string>>();

        /// <summary>
        /// Sets and gets the Events property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Pair<DateTime, string>> Events
        {
            get
            {
                return _events;
            }

            set
            {
                if (_events == value)
                {
                    return;
                }

                _events = value;
                RaisePropertyChanged(EventsPropertyName);
            }
        }

        private RelayCommand<Pair<DateTime, string>> _removePathCommand;

        /// <summary>
        /// Gets the RemoveControlCommand.
        /// </summary>
        public RelayCommand<Pair<DateTime, string>> RemovePathCommand
        {
            get
            {
                return _removePathCommand
                    ?? (_removePathCommand = new RelayCommand<Pair<DateTime, string>>(
                    p =>
                    {
                        Events.Remove(p);
                    }));
            }
        }

        private RelayCommand<Pair<DateTime, string>> _checkM;

        /// <summary>
        /// Gets the CheckM.
        /// </summary>
        public RelayCommand<Pair<DateTime, string>> CheckM
        {
            get
            {
                return _checkM
                    ?? (_checkM = new RelayCommand<Pair<DateTime, string>>(
                    p =>
                    {
                        p.A = p.A.AddMilliseconds((p.A.Millisecond ^ 1) - p.A.Millisecond);
                    }));
            }
        }

        private RelayCommand<Pair<DateTime, string>> _checkT;

        /// <summary>
        /// Gets the CheckM.
        /// </summary>
        public RelayCommand<Pair<DateTime, string>> CheckT
        {
            get
            {
                return _checkT
                    ?? (_checkT = new RelayCommand<Pair<DateTime, string>>(
                    p =>
                    {
                        var n = DateTime.Now;
                        p.A = p.A.AddMilliseconds((p.A.Millisecond ^ 1 << 1) - p.A.Millisecond);
                    }));
            }
        }

        private RelayCommand<Pair<DateTime, string>> _checkW;

        /// <summary>
        /// Gets the CheckM.
        /// </summary>
        public RelayCommand<Pair<DateTime, string>> CheckW
        {
            get
            {
                return _checkW
                    ?? (_checkW = new RelayCommand<Pair<DateTime, string>>(
                    p =>
                    {
                        p.A = p.A.AddMilliseconds((p.A.Millisecond ^ 1 << 2) - p.A.Millisecond);
                    }));
            }
        }

        private RelayCommand<Pair<DateTime, string>> _checkTH;

        /// <summary>
        /// Gets the CheckM.
        /// </summary>
        public RelayCommand<Pair<DateTime, string>> CheckTH
        {
            get
            {
                return _checkTH
                    ?? (_checkTH = new RelayCommand<Pair<DateTime, string>>(
                    p =>
                    {
                        p.A = p.A.AddMilliseconds((p.A.Millisecond ^ 1 << 3) - p.A.Millisecond);
                    }));
            }
        }

        private RelayCommand<Pair<DateTime, string>> _checkF;

        /// <summary>
        /// Gets the CheckM.
        /// </summary>
        public RelayCommand<Pair<DateTime, string>> CheckF
        {
            get
            {
                return _checkF
                    ?? (_checkF = new RelayCommand<Pair<DateTime, string>>(
                    p =>
                    {
                        p.A = p.A.AddMilliseconds((p.A.Millisecond ^ 1 << 4) - p.A.Millisecond);
                    }));
            }
        }

        private RelayCommand<Pair<DateTime, string>> _checkS;

        /// <summary>
        /// Gets the CheckM.
        /// </summary>
        public RelayCommand<Pair<DateTime, string>> CheckS
        {
            get
            {
                return _checkS
                    ?? (_checkS = new RelayCommand<Pair<DateTime, string>>(
                    p =>
                    {
                        p.A = p.A.AddMilliseconds((p.A.Millisecond ^ 1 << 5) - p.A.Millisecond);
                    }));
            }
        }

        private RelayCommand<Pair<DateTime, string>> _checkSU;

        /// <summary>
        /// Gets the CheckM.
        /// </summary>
        public RelayCommand<Pair<DateTime, string>> CheckSU
        {
            get
            {
                return _checkSU
                    ?? (_checkSU = new RelayCommand<Pair<DateTime, string>>(
                    p =>
                    {
                        p.A = p.A.AddMilliseconds((p.A.Millisecond ^ 1 << 6) - p.A.Millisecond);
                    }));
            }
        }

        private RelayCommand _addPathCommand;

        public event PropertyChangedEventHandler PropertyChanged;
        internal void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Gets the AddPathCommand.
        /// </summary>
        public RelayCommand AddPathCommand
        {
            get
            {
                return _addPathCommand
                    ?? (_addPathCommand = new RelayCommand(
                    () =>
                    {
                        var now = DateTime.Now;
                        Events.Add(new Pair<DateTime, string>(now.Subtract(TimeSpan.FromMilliseconds(now.Millisecond)).AddMilliseconds(127), ""));
                    }));
            }
        }

        public SchedulerControl()
        {
            InitializeComponent();
            //DataContext = this;
        }
    }
}
