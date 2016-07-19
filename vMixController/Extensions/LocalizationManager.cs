using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace vMixController.Extensions
{
    public static class LocalizationManager
    {
        static Dictionary<string, string> _strings = new Dictionary<string, string>();
        static LocalizationManager()
        {

            var locale = System.Globalization.CultureInfo.CurrentUICulture.Name;
            var filename = Path.Combine("Locales", string.Format("{0}.xml", locale));
            if (File.Exists(filename))
            {
                var doc = new XmlDocument();
                doc.Load(filename);
                foreach (var node in doc.SelectNodes(".//Key").OfType<XmlElement>())
                    _strings.Add(node.GetAttribute("Value"), node.InnerText);
            }

        }
        public static string Get(string key)
        {
            return _strings.ContainsKey(key) ? _strings[key] : key.Split('#').FirstOrDefault();
        }
    }
}
