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
    public class Overlay: DependencyObject, INotifyPropertyChanged
    {
        [XmlAttribute("number")]
        public int Number { get; set; }

        [XmlAttribute("preview")]
        public bool Preview { get; set; }
        [XmlText()]
        public string ActiveInput
        {
            get { return (string)GetValue(ActiveInputProperty); }
            set { SetValue(ActiveInputProperty, value); }
        }

        [XmlIgnore]
        public State ControlledState { get; set; }

        // Using a DependencyProperty as the backing store for ActiveInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveInputProperty =
            DependencyProperty.Register("ActiveInput", typeof(string), typeof(Overlay), new PropertyMetadata("", InternalPropertyChanged));

        private static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (State.IsInitializing) return;
            switch (e.Property.Name)
            {
                case "ActiveInput":
                    if (!string.IsNullOrWhiteSpace((string)e.NewValue))
                        (d as Overlay).ControlledState.OverlayInputIn((string)e.NewValue, ((Overlay)d).Number);
                    else
                        (d as Overlay).ControlledState.OverlayInputOff(((Overlay)d).Number);
                    //StateExtensions.TitleSetText((d as InputText).InputNumber, (string)e.NewValue, (d as InputText).Index);
                    break;
            }
            //throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
