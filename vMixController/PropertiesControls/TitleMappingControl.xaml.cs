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
    public partial class TitleMappingControl : UserControl, INotifyPropertyChanged
    {

        /// <summary>
        /// The <see cref="Titles" /> property's name.
        /// </summary>
        public const string TitlesPropertyName = "Titles";

        private ObservableCollection<Pair<string, string>> _myProperty = new ObservableCollection<Pair<string, string>>();

        /// <summary>
        /// Sets and gets the Titles property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Pair<string, string>> Titles
        {
            get
            {
                return _myProperty;
            }

            set
            {
                if (_myProperty == value)
                {
                    return;
                }

                _myProperty = value;
                RaisePropertyChanged(TitlesPropertyName);
            }
        }
        private RelayCommand<Pair<string, string>> _removePathCommand;

        /// <summary>
        /// Gets the RemoveControlCommand.
        /// </summary>
        public RelayCommand<Pair<string, string>> RemovePathCommand
        {
            get
            {
                return _removePathCommand
                    ?? (_removePathCommand = new RelayCommand<Pair<string, string>>(
                    p =>
                    {
                        Titles.Remove(p);
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
                        Titles.Add(new Pair<string, string>() { A = -1, B = "" });
                    }));
            }
        }

        public TitleMappingControl()
        {
            InitializeComponent();
            //DataContext = this;
        }
    }
}
