using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixController
{
    public static class vMixControlUtils
    {
        public static void AlignByGrid(this vMixController.Widgets.vMixWidget item)
        {
            var pleft = item.Left;
            item.Left = ((int)item.Left / 8) * 8;
            if (pleft > 0)
                item.Width += pleft - item.Left;
            item.Width = ((int)item.Width / 8) * 8;
            item.Top = ((int)item.Top / 8) * 8;
        }
    }
}
