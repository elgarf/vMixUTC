using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace vMixController.Controls
{
    public class TBarSlider: Slider
    {

        public TBarSlider()
        {
            
        }

        public void CancelDrag()
        {
            
            var track = (Track)this.Template.FindName("PART_Track", this);
            
            track.Thumb.CancelDrag();
        }


    }
}
