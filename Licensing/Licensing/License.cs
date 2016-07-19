using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Licensing
{
    [Serializable]
    public class License
    {
        public string Customer { get; set; }
        public string MachineID { get; set; }
        public List<Feature> Features { get; private set; }

        public License()
        {
            Features = new List<Feature>();
        }

        public void AddFeature(string name, object value)
        {
            Features.Add(new Feature() { Name = name, Value = value });
        }

        public bool CheckFeature(string name)
        {
            var f = Features.Where(x => x.Name == name).FirstOrDefault();
            return f != null;
        }

        public T ReadFeature<T>(string name, T def = default(T))
        {
            if (CheckFeature(name))
            {
                return Features.Where(x => x.Name == name).FirstOrDefault().GetValue<T>();
            }
            return def;
        }

    }
}
