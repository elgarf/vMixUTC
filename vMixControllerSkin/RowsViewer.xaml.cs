using System;
using System.Collections;
using System.Collections.Generic;
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

namespace vMixControllerSkin
{
    /// <summary>
    /// Логика взаимодействия для RowsViewer.xaml
    /// </summary>
    public partial class RowsViewer : Window
    {
        public RowsViewer()
        {
            //InitializeComponent();
            this.LoadViewFromUri("vMixControllerSkin;component/RowsViewer.xaml");
            DataContext = this;
            Topmost = Application.Current?.MainWindow?.Topmost ?? false;
        }

        public void Bind(object caller, string path)
        {
            var b = new Binding(path);
            b.Source = caller;
            BindingOperations.SetBinding(this, RowsViewer.RowsProperty, b);
            this.ShowDialog();
        }

        public IList Rows
        {
            get { return (IList)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Rows.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(IList), typeof(RowsViewer), new PropertyMetadata(null));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
