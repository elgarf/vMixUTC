using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XmlDataProviderNs
{
    public class CacheStats
    {
        public XmlDocument Document { get; set; }
        public int LastId { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
