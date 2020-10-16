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
    public class InputImage: InputBase
    {
        [XmlIgnore]
        public override string Type
        {
            get
            {
                return "IMG";
            }
        }

        //public string Text { get; set; }
        public override int ID
        {
            get
            {
                return base.ID + Image.GetHashCode();
            }
        }

        [XmlText()]
        public string Image
        {
            get { return (string)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value);

                if (!State.IsInitializing)
                    ControlledState.TitleSetImage(InputKey, Environment.ExpandEnvironmentVariables(value), Index);

                RaisePropertyChanged("Image"); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(string), typeof(InputImage), new PropertyMetadata("", InternalPropertyChanged));

        private static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (State.IsInitializing) return;
            switch (e.Property.Name)
            {
                case "Image":
                    //StateExtensions.InputSelectIndex((d as InputText).InputNumber, );
                    (d as InputImage).ControlledState.TitleSetImage((d as InputImage).InputKey, Environment.ExpandEnvironmentVariables((string)e.NewValue), (d as InputImage).Index);
                    break;
            }
        }
    }
}
