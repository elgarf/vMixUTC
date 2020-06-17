using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace vMixAPI
{
    [Serializable]
    public class Master : DependencyObject
    {
        [XmlIgnore]
        public virtual string Name { get { return "Master"; } }
        [XmlAttribute("headphonesVolume")]
        public double HeadphonesVolume { get; set; }
        [XmlAttribute("volume")]
        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Volume.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(double), typeof(Master), new PropertyMetadata(0.0));

        [XmlAttribute("muted")]
        public bool Muted
        {
            get { return (bool)GetValue(MutedProperty); }
            set { SetValue(MutedProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Muted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MutedProperty =
            DependencyProperty.Register("Muted", typeof(bool), typeof(Master), new PropertyMetadata(false));

        [XmlAttribute("meterF1")]
        public double MeterF1 { get; set; }
        [XmlAttribute("meterF2")]
        public double MeterF2 { get; set; }

        public Master()
        {

        }

    }
}
