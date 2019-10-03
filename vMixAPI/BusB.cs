using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace vMixAPI
{
    [Serializable]
    public class BusB: Master
    {
        [XmlIgnore]
        public new string Name { get { return "BusB"; } }
    }
}
