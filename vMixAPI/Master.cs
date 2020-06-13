using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace vMixAPI
{
    [Serializable]
    public class Master
    {
        [XmlIgnore]
        public virtual string Name { get { return "Master"; } }
        [XmlAttribute("headphonesVolume")]
        public double HeadphonesVolume { get; set; }
        [XmlAttribute("volume")]
        public double Volume { get; set; }
        [XmlAttribute("muted")]
        public bool Muted { get; set; }
        [XmlAttribute("meterF1")]
        public double MeterF1 { get; set; }
        [XmlAttribute("meterF2")]
        public double MeterF2 { get; set; }

        public Master()
        {

        }

    }
}
