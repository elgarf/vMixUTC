using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace vMixAPI
{
    public class SampleInput: DependencyObject, INotifyPropertyChanged
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlAttribute("number")]
        public int Number { get; set; }

        [XmlAttribute("title")]
        public virtual string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        [XmlElement(typeof(InputText), ElementName = "text"),
    XmlElement(typeof(InputOverlay), ElementName = "overlay"),
    XmlElement(typeof(InputPosition), ElementName = "position"),
    XmlElement(typeof(InputImage), ElementName = "image")]
        public ObservableCollection<InputBase> Elements { get; set; }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Input), new PropertyMetadata(""));

        public event PropertyChangedEventHandler PropertyChanged;

        public void PropertyChange(object sender, string property)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(property));
        }
    }
}
