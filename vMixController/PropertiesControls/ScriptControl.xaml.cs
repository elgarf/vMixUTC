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
using vMixController.Widgets;

namespace vMixController.PropertiesControls
{
    /// <summary>
    /// Логика взаимодействия для ScriptControl.xaml
    /// </summary>
    public partial class ScriptControl : UserControl, INotifyPropertyChanged
    {
        public ScriptControl()
        {
            InitializeComponent();
        }



        /// <summary>
        /// The <see cref="Commands" /> property's name.
        /// </summary>
        public const string CommandsPropertyName = "Commands";

        private ObservableCollection<vMixControlButtonCommand> _commands = new ObservableCollection<vMixControlButtonCommand>();

        /// <summary>
        /// Sets and gets the Commands property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<vMixControlButtonCommand> Commands
        {
            get
            {
                return _commands;
            }

            set
            {
                if (_commands == value)
                {
                    return;
                }

                _commands = value;
                RaisePropertyChanged(CommandsPropertyName);
            }
        }


        private RelayCommand<vMixControlButtonCommand> _removeCommandCommand;

        /// <summary>
        /// Gets the RemoveCommandCommand.
        /// </summary>
        public RelayCommand<vMixControlButtonCommand> RemoveCommandCommand
        {
            get
            {
                return _removeCommandCommand
                    ?? (_removeCommandCommand = new RelayCommand<vMixControlButtonCommand>(
                    p =>
                    {
                        Commands.Remove(p);
                    }));
            }
        }

        private RelayCommand _addCommandCommand;

        /// <summary>
        /// Gets the AddCommandCommand.
        /// </summary>
        public RelayCommand AddCommandCommand
        {
            get
            {
                return _addCommandCommand
                    ?? (_addCommandCommand = new RelayCommand(
                    () =>
                    {
                        Commands.Add(new vMixControlButtonCommand());
                    }));
            }
        }

        private RelayCommand<vMixControlButtonCommand> _moveCommandUpCommand;

        /// <summary>
        /// Gets the MoveCommandUpCommand.
        /// </summary>
        public RelayCommand<vMixControlButtonCommand> MoveCommandUpCommand
        {
            get
            {
                return _moveCommandUpCommand
                    ?? (_moveCommandUpCommand = new RelayCommand<vMixControlButtonCommand>(
                    p =>
                    {
                        var idx = Commands.IndexOf(p);
                        Commands.Move(idx, idx - 1 >= 0 ? idx - 1 : idx);
                    }));
            }
        }

        private RelayCommand<vMixControlButtonCommand> _moveCommandDownCommand;

        public event PropertyChangedEventHandler PropertyChanged;
        internal void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        /// <summary>
        /// Gets the MoveCommandDownCommand.
        /// </summary>
        public RelayCommand<vMixControlButtonCommand> MoveCommandDownCommand
        {
            get
            {
                return _moveCommandDownCommand
                    ?? (_moveCommandDownCommand = new RelayCommand<vMixControlButtonCommand>(
                    p =>
                    {
                        var idx = Commands.IndexOf(p);
                        Commands.Move(idx, idx + 1 < Commands.Count ? idx + 1 : idx);
                    }));
            }
        }
    }
}
