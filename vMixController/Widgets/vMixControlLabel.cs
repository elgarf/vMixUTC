using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixController.Widgets
{
    public class vMixControlLabel: vMixWidget
    {
        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Label");
            }
        }
    }
}
