using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using vMixController.Classes;

namespace vMixController.PropertiesControls
{
    public partial class ListControl : UserControl
    {
        public ListControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<DummyStringProperty>), typeof(ListControl), new PropertyMetadata(null));

        public ObservableCollection<DummyStringProperty> Items
        {
            get { return (ObservableCollection<DummyStringProperty>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        [RelayCommand]
        private void AddItem()
        {
            Items.Add(new DummyStringProperty { Value = "" });
        }

        [RelayCommand]
        private void RemoveItem(DummyStringProperty p)
        {
            Items.Remove(p);
        }

        [RelayCommand]
        private void SaveItemsList()
        {
            var saveDlg = new Ookii.Dialogs.Wpf.VistaSaveFileDialog
            {
                Filter = "Text Files|*.txt",
                DefaultExt = "txt"
            };

            var result = saveDlg.ShowDialog(App.Current.Windows.OfType<vMixWidgetSettingsView>().FirstOrDefault());
            if (result.HasValue && result.Value)
                File.WriteAllLines(saveDlg.FileName, Items.Select(x => x.Value).ToArray());
        }

        [RelayCommand]
        private void LoadItemsList()
        {
            var openDlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                Filter = "Text Files|*.txt",
                DefaultExt = "txt"
            };

            var result = openDlg.ShowDialog(App.Current.Windows.OfType<vMixWidgetSettingsView>().FirstOrDefault());
            if (result.HasValue && result.Value)
            {
                Items.Clear();
                foreach (var item in File.ReadAllLines(openDlg.FileName))
                    Items.Add(new DummyStringProperty { Value = item });
            }
        }
    }
}
