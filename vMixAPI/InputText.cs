using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace vMixAPI
{
    [Serializable]
    public class InputText: InputBase
    {
        [XmlIgnore]
        public override string Type
        {
            get
            {
                return "TXT";
            }
        }
        [XmlAttribute("index")]
        public int Index { get; set; }

        //public string Text { get; set; }
        public override int ID
        {
            get
            {
                return base.ID + Text.GetHashCode();
            }
        }

        [XmlText()]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); RaisePropertyChanged("Text"); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(InputText), new PropertyMetadata("", InternalPropertyChanged));

        private static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (State.IsInitializing) return;
            switch (e.Property.Name)
            {
                case "Text":
                    //StateExtensions.InputSelectIndex((d as InputText).InputNumber, );
                   (d as InputText).ControlledState.TitleSetText((d as InputText).InputKey, (string)e.NewValue, (d as InputText).Index);
                    break;
            }
        }
    }
}
