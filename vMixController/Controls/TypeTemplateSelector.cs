using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace vMixController.Controls
{
    public class TypeTemplateSelector: DataTemplateSelector
    {


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            try
            {
                return (DataTemplate)element.FindResource(item.GetType().Name);
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
                return (DataTemplate)element.FindResource(item.GetType().BaseType.Name);
            }
        }
    }
}
