using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
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

namespace vMixController.PropertiesControls
{
    /// <summary>
    /// Логика взаимодействия для FilePathControl.xaml
    /// </summary>
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

        // Using a DependencyProperty as the backing store for FileNotFound.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileNotFoundProperty =
            DependencyProperty.Register("FileNotFound", typeof(bool), typeof(FilePathControl), new PropertyMetadata(false));



        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(FilePathControl), new PropertyMetadata("", propchanged));

        private static void propchanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FilePathControl).FileNotFound = (!File.Exists((string)e.NewValue));
        }

        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Filter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(FilePathControl), new PropertyMetadata("External Data|*.dll"));



        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(FilePathControl), new PropertyMetadata("File Path"));



        private RelayCommand _selectFilePathCommand;

        /// <summary>
        /// Gets the SelectFilePathCommand.
        /// </summary>
        public RelayCommand SelectFilePathCommand
        {
            get
            {
                return _selectFilePathCommand
                    ?? (_selectFilePathCommand = new RelayCommand(
                    () =>
                    {
                        Ookii.Dialogs.Wpf.VistaOpenFileDialog opendlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
                        opendlg.Filter = Filter;
                        var result = opendlg.ShowDialog(App.Current.Windows.OfType<vMixWidgetSettingsView>().FirstOrDefault());
                        if (result.HasValue && result.Value)
                            Value = opendlg.FileName;
                    }));
            }
        }

    }
}
