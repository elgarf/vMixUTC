using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для IntControl.xaml
    /// </summary>
    public partial class BoolControl : UserControl
    {
        public BoolControl()
        {
            InitializeComponent();
        }





        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(BoolControl), new PropertyMetadata(""));





        public bool Value
        {
            get { return (bool)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(bool), typeof(BoolControl), new PropertyMetadata(false));



        public List<Pair<bool, string>> Values
        {
            get { return (List<Pair<bool, string>>)GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Values.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register("Values", typeof(List<Pair<bool, string>>), typeof(BoolControl), new PropertyMetadata(null));



        public string Help
        {
            get { return (string)GetValue(HelpProperty); }
            set { SetValue(HelpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Help.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HelpProperty =
            DependencyProperty.Register("Help", typeof(string), typeof(BoolControl), new PropertyMetadata(""));




        public bool Grouped
        {
            get { return (bool)GetValue(GroupedProperty); }
            set { SetValue(GroupedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Grouped.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupedProperty =
            DependencyProperty.Register("Grouped", typeof(bool), typeof(BoolControl), new PropertyMetadata(false));

    }
}
