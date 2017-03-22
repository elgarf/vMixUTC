using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace vMixController.Classes
{
    public class MenuStyleSelector: StyleSelector
    {


        public Style MenuItem { get; set; }
        public Style Separator { get; set; }



        public override Style SelectStyle(object item, DependencyObject container)
        {
            return item is MenuItem ? MenuItem : Separator;
        }
    }
}
