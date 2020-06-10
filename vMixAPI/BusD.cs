using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace vMixAPI
{
    [Serializable]
    public class BusD: Master
    {
        [XmlIgnore]
        public new string Name { get { return "BusD"; } }
    }
}
