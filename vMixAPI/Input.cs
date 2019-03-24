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
    [Serializable]
    public class Input : DependencyObject, INotifyPropertyChanged
    {
        [XmlIgnore]
        public int ID
        {
            get
            {

                var inputsCfg = 0;
                foreach (var item in Elements)
                {
                    inputsCfg += item.ID % (65536 * 15);
                }

                return ((Title + Type + Duration.ToString()).GetHashCode() + inputsCfg);
            }
        }
        State _controlledState;
        [XmlIgnore]
        public State ControlledState { get { return _controlledState; } set {

                foreach (var item in Elements)
                    item.ControlledState = value;
                /*foreach (var item in Overlays)
                    item.ControlledState = value;*/
                _controlledState = value;

            } }

        [XmlAttribute("key")]
        public string Key { get; set; }
        [XmlAttribute("number")]
        public int Number { get; set; }
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlText()]
        public string InnerTitle { get; set; }

        [XmlAttribute("state")]
        public string State { get; set; }

        [XmlAttribute("position")]
        ///Position can be not actual, call State.Update() before read
        public int Position
        {
            get { return (int)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); RaisePropertyChanged("Position"); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(int), typeof(Input), new PropertyMetadata(0, InternalPropertyChanged));

        private static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (vMixAPI.State.IsInitializing)
                return;
            switch (e.Property.Name)
            {
                case "Position":
                    ((Input)d).ControlledState.InputSetPosition(((Input)d).Number, (int)e.NewValue);
                    break;
                case "SelectedIndex":
                    ((Input)d).ControlledState.InputSelectIndex(((Input)d).Number.ToString(), (int)e.NewValue);
                    break;
                case "Loop":
                    if ((bool)e.NewValue)
                        ((Input)d).ControlledState.InputLoopOn(((Input)d).Number);
                    else
                        ((Input)d).ControlledState.InputLoopOff(((Input)d).Number);
                    break;
                case "Muted":
                    if (e.NewValue != e.OldValue)
                        ((Input)d).ControlledState.Audio(((Input)d).Number);
                    break;
            }
        }

        [XmlAttribute("duration")]
        public int Duration { get; set; }

        [XmlAttribute("selectedIndex")]
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); RaisePropertyChanged("SelectedIndex"); }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(Input), new PropertyMetadata(0, InternalPropertyChanged));



        [XmlAttribute("muted")]
        public bool Muted
        {
            get { return (bool)GetValue(MutedProperty); }
            set { SetValue(MutedProperty, value); RaisePropertyChanged("Muted"); }
        }

        // Using a DependencyProperty as the backing store for Muted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MutedProperty =
            DependencyProperty.Register("Muted", typeof(bool), typeof(Input), new PropertyMetadata(false, InternalPropertyChanged));



        [XmlAttribute("loop")]
        public bool Loop
        {
            get { return (bool)GetValue(LoopProperty); }
            set { SetValue(LoopProperty, value); RaisePropertyChanged("Loop"); }
        }

        // Using a DependencyProperty as the backing store for Loop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoopProperty =
            DependencyProperty.Register("Loop", typeof(bool), typeof(Input), new PropertyMetadata(false, InternalPropertyChanged));



        [XmlAttribute("meterF1")]
        public float MeterF1
        {
            get { return (float)GetValue(MeterF1Property); }
            set { SetValue(MeterF1Property, value); }
        }

        // Using a DependencyProperty as the backing store for MeterF1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MeterF1Property =
            DependencyProperty.Register("MeterF1", typeof(float), typeof(Input), new PropertyMetadata(default(float)));

        [XmlAttribute("meterF2")]
        public float MeterF2
        {
            get { return (float)GetValue(MeterF2Property); }
            set { SetValue(MeterF2Property, value); }
        }

        // Using a DependencyProperty as the backing store for MeterF1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MeterF2Property =
            DependencyProperty.Register("MeterF2", typeof(float), typeof(Input), new PropertyMetadata(default(float)));



        [XmlAttribute("audiobusses")]
        public string Audiobusses
        {
            get { return (string)GetValue(AudiobussesProperty); }
            set { SetValue(AudiobussesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Audiobusses.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AudiobussesProperty =
            DependencyProperty.Register("Audiobusses", typeof(string), typeof(Input), new PropertyMetadata("M"));





        [XmlElement(typeof(InputText), ElementName = "text"),
            XmlElement(typeof(InputOverlay), ElementName = "overlay"),
            XmlElement(typeof(InputPosition), ElementName = "position"),
            XmlElement(typeof(InputImage), ElementName = "image")]
        public ObservableCollection<InputBase> Elements { get; set; }

        public Input()
        {
            Elements = new ObservableCollection<InputBase>();
            Elements.CollectionChanged += Elements_CollectionChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void Elements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems.OfType<InputBase>())
            {
                item.InputNumber = this.Number;
                item.InputKey = this.Key;
                item.ControlledState = this.ControlledState;
            }
        }

    }
}
