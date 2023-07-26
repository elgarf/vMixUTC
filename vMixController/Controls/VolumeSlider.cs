using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace vMixController.Controls
{
    public class VolumeSlider: Slider
    {
        public event EventHandler ThumbDragStarted;
        public event EventHandler ThumbDragCompleted;

        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            ThumbDragStarted?.Invoke(this, new EventArgs());
            base.OnThumbDragStarted(e);
        }

        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            ThumbDragCompleted?.Invoke(this, new EventArgs());
            base.OnThumbDragCompleted(e);
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            e.Handled = true;
            base.OnTouchDown(e);
        }
    }
}
