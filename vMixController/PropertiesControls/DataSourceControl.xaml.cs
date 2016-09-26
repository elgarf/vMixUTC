using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для DataSourceControl.xaml
    /// </summary>
    public partial class DataSourceControl : UserControl
    {

        public ObservableCollection<string> Sources
        {
            get { return (ObservableCollection<string>)GetValue(SourcesProperty); }
            set { SetValue(SourcesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Sources.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourcesProperty =
            DependencyProperty.Register("Sources", typeof(ObservableCollection<string>), typeof(DataSourceControl), new PropertyMetadata(null));

        public ObservableCollection<string> Properties
        {
            get { return (ObservableCollection<string>)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Sources.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(ObservableCollection<string>), typeof(DataSourceControl), new PropertyMetadata(null));



        public string DataSource
        {
            get { return (string)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(string), typeof(DataSourceControl), new PropertyMetadata(""));




        public string DataProperty
        {
            get { return (string)GetValue(DataPropertyProperty); }
            set { SetValue(DataPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataPropertyProperty =
            DependencyProperty.Register("DataProperty", typeof(string), typeof(DataSourceControl), new PropertyMetadata(""));




        public bool Active
        {
            get { return (bool)GetValue(ActiveProperty); }
            set { SetValue(ActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Active.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveProperty =
            DependencyProperty.Register("Active", typeof(bool), typeof(DataSourceControl), new PropertyMetadata(false));



        public void Update()
        {
            Sources = new ObservableCollection<string>(Singleton<SharedData>.Instance.GetDataSources());
        }

        public DataSourceControl()
        {
            InitializeComponent();
            Sources = new ObservableCollection<string>(Singleton<SharedData>.Instance.GetDataSources());
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties = new ObservableCollection<string>(Singleton<SharedData>.Instance.GetDataSourceProps(DataSource));
        }
    }
}
