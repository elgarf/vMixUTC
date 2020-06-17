using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace vMixAPI
{
    [Serializable]
    public class Mix: DependencyObject
    {
        [XmlAttribute("number")]
        public int Number { get; set; }


        [XmlElement("preview")]
        public int Preview
        {
            get { return (int)GetValue(PreviewProperty); }
            set { SetValue(PreviewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Preview.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreviewProperty =
            DependencyProperty.Register("Preview", typeof(int), typeof(Mix), new PropertyMetadata(0));


        [XmlElement("active")]
        public int Active
        {
            get { return (int)GetValue(ActiveProperty); }
            set { SetValue(ActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Active.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveProperty =
            DependencyProperty.Register("Active", typeof(int), typeof(Mix), new PropertyMetadata(0));


    }
}
