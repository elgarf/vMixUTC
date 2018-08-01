using GalaSoft.MvvmLight.CommandWpf;
using System;
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
    /// Логика взаимодействия для ListControl.xaml
    /// </summary>
    public partial class ListControl : UserControl, INotifyPropertyChanged
    {
        public ListControl()
        {
            InitializeComponent();
        }



        /// <summary>
        /// The <see cref="Items" /> property's name.
        /// </summary>
        public const string ItemsPropertyName = "Items";

        private ObservableCollection<DummyStringProperty> _items = new ObservableCollection<DummyStringProperty>();

        /// <summary>
        /// Sets and gets the Items property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<DummyStringProperty> Items
        {
            get
            {
                return _items;
            }

            set
            {
                if (_items == value)
                {
                    return;
                }

                _items = value;
                RaisePropertyChanged(ItemsPropertyName);
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
                        Items.Add(new DummyStringProperty() { Value = "" });
                    }));
            }
        }

        private RelayCommand<DummyStringProperty> _removeItemCommand;

        /// <summary>
        /// Gets the RemoveItemCommand.
        /// </summary>
        public RelayCommand<DummyStringProperty> RemoveItemCommand
        {
            get
            {
                return _removeItemCommand
                    ?? (_removeItemCommand = new RelayCommand<DummyStringProperty>(
                    p =>
                    {
                        Items.Remove(p);
                    }));
            }
        }

        private RelayCommand _saveItemsListCommand;

        /// <summary>
        /// Gets the SaveItemsListCOmmand.
        /// </summary>
        public RelayCommand SaveItemsListCommand
        {
            get
            {
                return _saveItemsListCommand
                    ?? (_saveItemsListCommand = new RelayCommand(
                    () =>
                    {
                        Ookii.Dialogs.Wpf.VistaSaveFileDialog opendlg = new Ookii.Dialogs.Wpf.VistaSaveFileDialog();
                        opendlg.Filter = "Text Files|*.txt";
                        opendlg.DefaultExt = "txt";
                        var result = opendlg.ShowDialog(App.Current.Windows.OfType<vMixWidgetSettingsView>().FirstOrDefault());
                        if (result.HasValue && result.Value)
                            File.WriteAllLines(opendlg.FileName, Items.Select(x => x.Value).ToArray());
                    }));
            }
        }

        private RelayCommand _loadItemsListCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Gets the LoadItemsListCommand.
        /// </summary>
        public RelayCommand LoadItemsListCommand
        {
            get
            {
                return _loadItemsListCommand
                    ?? (_loadItemsListCommand = new RelayCommand(
                    () =>
                    {
                        Ookii.Dialogs.Wpf.VistaOpenFileDialog opendlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
                        opendlg.Filter = "Text Files|*.txt";
                        opendlg.DefaultExt = "txt";
                        var result = opendlg.ShowDialog(App.Current.Windows.OfType<vMixWidgetSettingsView>().FirstOrDefault());
                        if (result.HasValue && result.Value)
                        {
                            Items.Clear();
                            foreach (var item in File.ReadAllLines(opendlg.FileName)/*.OrderBy(x => x)*/)
                            {
                                Items.Add(new DummyStringProperty() { Value = item });
                            }
                        }
                    }));
            }
        }

    }
}
