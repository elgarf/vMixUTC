using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vMixController.Classes;

namespace vMixController.ViewModel
{
    public class GlobalVariablesViewModel: ViewModelBase
    {
        /// <summary>
        /// The <see cref="Variables" /> property's name.
        /// </summary>
        public const string VariablesPropertyName = "Variables";

        public static ObservableCollection<Pair<string, string>> _variables = new ObservableCollection<Pair<string, string>>();

        /// <summary>
        /// Sets and gets the Variables property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Pair<string, string>> Variables
        {
            get
            {
                return _variables;
            }

            set
            {
                if (_variables == value)
                {
                    return;
                }

                _variables = value;
                RaisePropertyChanged(VariablesPropertyName);
            }
        }

        public GlobalVariablesViewModel()
        {
            /*Variables.Add(new Pair<string, string>("test", "test"));
            Variables.Add(new Pair<string, string>("test1", "test5"));
            Variables.Add(new Pair<string, string>("test2", "test6"));
            Variables.Add(new Pair<string, string>("test3", "test7"));
            Variables.Add(new Pair<string, string>("test4", "test8"));*/
        }

        private RelayCommand<Pair<string, string>> _removeItemCommand;

        /// <summary>
        /// Gets the RemoveItemCommand.
        /// </summary>
        public RelayCommand<Pair<string, string>> RemoveItemCommand
        {
            get
            {
                return _removeItemCommand
                    ?? (_removeItemCommand = new RelayCommand<Pair<string, string>>(
                    p =>
                    {
                        Variables.Remove(p);
                    }));
            }
        }

        private RelayCommand _addItemCommand;

        /// <summary>
        /// Gets the AddItemCommand.
        /// </summary>
        public RelayCommand AddItemCommand
        {
            get
            {
                return _addItemCommand
                    ?? (_addItemCommand = new RelayCommand(
                    () =>
                    {
                        Variables.Add(new Pair<string, string>("", ""));
                    }));
            }
        }


        private RelayCommand _okCommand;

        /// <summary>
        /// Gets the OkCommand.
        /// </summary>
        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand
                    ?? (_okCommand = new RelayCommand(
                    () =>
                    {
                        MessengerInstance.Send(true);
                    }));
            }
        }
    }
}
