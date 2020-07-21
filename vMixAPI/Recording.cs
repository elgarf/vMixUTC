using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace vMixAPI
{
    [Serializable]
    public class Recording
    {
        [XmlText(DataType = "boolean")]
        public bool Active { get; set; }
        [XmlAttribute("duration")]
        public int Duration { get; set; }
        [XmlAttribute("filename1")]
        public string FileName1 { get; set; }
        [XmlAttribute("filename2")]
        public string FileName2 { get; set; }
        [XmlAttribute("filename3")]
        public string FileName3 { get; set; }
    }
}
