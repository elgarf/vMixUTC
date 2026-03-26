using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace vMixController.PropertiesControls
{
    public partial class FilePathControl : UserControl
    {
        public FilePathControl()
        {
            InitializeComponent();
        }

        public bool FileNotFound
        {
            get { return (bool)GetValue(FileNotFoundProperty); }
            set { SetValue(FileNotFoundProperty, value); }
        }

        public static readonly DependencyProperty FileNotFoundProperty =
            DependencyProperty.Register(nameof(FileNotFound), typeof(bool), typeof(FilePathControl), new PropertyMetadata(false));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(FilePathControl), new PropertyMetadata("", PropChanged));

        private static void PropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FilePathControl).FileNotFound = !File.Exists((string)e.NewValue);
        }

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(nameof(Filter), typeof(string), typeof(FilePathControl), new PropertyMetadata("External Data|*.dll"));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(FilePathControl), new PropertyMetadata("File Path"));

        [RelayCommand]
        private void SelectFilePath()
        {
            var openDlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
            {
                Filter = Filter
            };

            var result = openDlg.ShowDialog(App.Current.Windows.OfType<vMixWidgetSettingsView>().FirstOrDefault());
            if (result.HasValue && result.Value)
                Value = openDlg.FileName;
        }
    }
}
