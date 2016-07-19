using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace vMixAPI
{
    [Serializable]
    public class Transition
    {
        [XmlAttribute("number")]
        public int Number { get; set; }
        [XmlAttribute("duration")]
        public int Duration { get; set; }
        [XmlAttribute("effect")]
        public TransitionEffect Effect { get; set; }
    }
}
