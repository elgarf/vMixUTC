using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vMixController
{
    public static class vMixControlUtils
    {
        public static void AlignByGrid(this vMixController.Widgets.vMixControl item)
        {
            var pleft = item.Left;
            item.Left = ((int)item.Left / 8) * 8;
            if (pleft > 0)
                item.Width += pleft - item.Left;
            item.Width = ((int)item.Width / 8) * 8;
            item.Height = ((int)item.Height / 8) * 8;
            item.Top = ((int)item.Top / 8) * 8;

            //Avoid widget to be unreachable
            if (item.Top < 0)
                item.Top = 0;
            if (item.Left < 0)
                item.Left = 0;

        }
    }
}
