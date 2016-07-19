using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Licensing
{
    [Serializable]
    public class Feature
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public T GetValue<T>()
        {
            return (T)Value;
        }
    }
}
