using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace vMixAPI
{
    [Serializable]
    public class InputPosition: InputBase
    {
        [XmlIgnore]
        public override string Type
        {
            get
            {
                return "POS";
            }
        }

        [XmlAttribute("panY")]
        public float PanY { get; set; }
        [XmlAttribute("panX")]
        public float PanX { get; set; }
        [XmlAttribute("zoomX")]
        public float zoomX { get; set; }
        [XmlAttribute("zoomY")]
        public float zoomY { get; set; }
    }
}
