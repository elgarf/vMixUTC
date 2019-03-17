using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixController.Widgets
{
    public class vMixControlRegion: vMixControl
    {
        public vMixControlRegion()
        {
            ZIndex = -1;
        }

        public override bool IsCaptionOn { get => true; set => base.IsCaptionOn = true; }

        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Region");
            }
        }
    }
}
