using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace vMixAPI
{
    [Serializable]
    public class BusE: Master
    {
        [XmlIgnore]
        public new string Name { get { return "BusE"; } }
    }
}
