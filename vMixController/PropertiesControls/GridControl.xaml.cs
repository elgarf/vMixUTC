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

namespace vMixController.PropertiesControls
{
    /// <summary>
    /// Логика взаимодействия для GridControl.xaml
    /// </summary>
    public partial class GridControl : UserControl
    {
        public GridControl()
        {
            InitializeComponent();
            DataContext = this;
        }


        public UIElementCollection Children { get { return InnerGrid.Children; } }

        int _columns = 0;
        public int Columns
        {
            get
            {
                return _columns;
            }
            set
            {
                if (_columns != value)
                {
                    _columns = value;
                    InnerGrid.ColumnDefinitions.Clear();
                    for (int i = 0; i < _columns; i++)
                        InnerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }
            }
        }

    }
}
