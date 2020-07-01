using System;
using System.Collections;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace vMixControllerSkin
{
    /// <summary>
    /// Логика взаимодействия для PropertiesListView.xaml
    /// </summary>
    [ContentProperty("Items")]
    public partial class PropertiesListView : UserControl
    {
        public PropertiesListView()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                var e = new CompositeCollection
                {
                    new TextBox() { Tag = "Test 1" },
                    new TextBox() { Tag = "Test 2" },
                    new TextBox() { Tag = "Test 3" },
                    new TextBox() { Tag = "Test 4" }
                };
                Items = e;
            }
        }



        public IEnumerable Items
        {
            get { return (IEnumerable)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable), typeof(PropertiesListView), new PropertyMetadata(null));


    }
}
