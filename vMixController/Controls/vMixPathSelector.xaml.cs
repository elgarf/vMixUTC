using System;
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

namespace vMixController.Controls
{
    /// <summary>
    /// Логика взаимодействия для vMixPathSelector.xaml
    /// </summary>
    public partial class vMixPathSelector : UserControl
    {


        public vMixAPI.State Model
        {
            get { return (vMixAPI.State)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Model.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(vMixAPI.State), typeof(vMixPathSelector), new PropertyMetadata(null, InternalPropertyChanged));


        public int InputIndex
        {
            get { return (int)GetValue(InputIndexProperty); }
            set { SetValue(InputIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputIndexProperty =
            DependencyProperty.Register("InputIndex", typeof(int), typeof(vMixPathSelector), new PropertyMetadata(0, InternalPropertyChanged));


        public string InputKey
        {
            get { return (string)GetValue(InputKeyProperty); }
            set { SetValue(InputKeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputKeyProperty =
            DependencyProperty.Register("InputKey", typeof(string), typeof(vMixPathSelector), new PropertyMetadata(null, InternalPropertyChanged));


        public string TitleName
        {
            get { return (string)GetValue(TitleNameProperty); }
            set { SetValue(TitleNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleNameProperty =
            DependencyProperty.Register("TitleName", typeof(string), typeof(vMixPathSelector), new PropertyMetadata(""));



        private static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var self = (vMixPathSelector)d;
                /*if (e.Property.Name == "InputIndex")
                    self.InputKey = self.Model.Inputs[(int)e.NewValue].Key;*/
                if (e.Property.Name == "InputKey")
                {
                    if (self.Model == null)
                        return;
                    self.InputIndex = self.Model.Inputs.Select((x, i) => new { obj = x, idx = i }).Where(x => x.obj.Key == (string)e.NewValue).FirstOrDefault()?.idx ?? -2;
                }
            }
            catch (Exception)
            {

            }
            //if (e.Property.Name == "0") ;
            /*if (e.Property != SelectedPathProperty)
                d.SetValue(SelectedPathProperty, string.Format("Inputs[{0}].Elements[{1}]", (int)d.GetValue(InputIndexProperty), (int)d.GetValue(TitleIndexProperty)));
            (d as vMixPathSelector).RaisePropertyChanged(e.Property.Name);*/
            //throw new NotImplementedException();
        }


        public vMixPathSelector()
        {
            InitializeComponent();
            //this.DataContext = this;
        }
    }
}
