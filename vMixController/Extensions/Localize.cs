using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace vMixController.Extensions
{
    public class Localize : MarkupExtension
    {
        [ConstructorArgument("key")]
        public string Key { get; set; }

        public Localize(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return LocalizationManager.Get(Key);
        }
    }
}
