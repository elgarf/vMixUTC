using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace vMixController.Controls
{
    public class TypeTemplateSelector: DataTemplateSelector
    {


        public int Page { get; set; } = 0;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            //Debug.Print(Page.ToString());
            FrameworkElement element = container as FrameworkElement;
            //if (((int?)item.GetType().GetProperty("Page")?.GetValue(item) ?? 0) == Page)
            //{
                try
                {
                    return (DataTemplate)element.FindResource(item.GetType().Name);
                }
                catch (ResourceReferenceKeyNotFoundException)
                {
                    return (DataTemplate)element.FindResource(item.GetType().BaseType.Name);
                }
            //}
            //else
            //   return (DataTemplate)element.FindResource("Dummy");
        }
    }
}
