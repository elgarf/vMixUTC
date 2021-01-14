using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace vMixController.Controls
{
    public class ComboBox : System.Windows.Controls.ComboBox
    {
        public ComboBox()
        {
            this.SetResourceReference(StyleProperty, typeof(ComboBox));
        }

        private bool ignore = false;
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (!ignore)
            {
                base.OnSelectionChanged(e);
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            ignore = true;
            try
            {
                base.OnItemsChanged(e);
            }
            finally
            {
                ignore = false;
            }
        }
    }
}
