using System;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.Messaging;
using vMixController.Classes;
using vMixController.Messages;
using vMixControllerSkin;

namespace vMixController.Controls
{
    public class vMixControlResizeThumb : DraggableThumb
    {
        private IMessenger Messenger => AppServices.IsRegistered<IMessenger>()
            ? AppServices.GetRequiredService<IMessenger>()
            : WeakReferenceMessenger.Default;

        public vMixControlResizeThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
            DragStarted += ResizeThumb_DragStarted;
            DragCompleted += ResizeThumb_DragCompleted;
        }

        private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (DataContext is vMixController.Widgets.vMixControl item && !item.Locked)
                Messenger.Send(new WidgetEditMessage { Widget = item, Action = WidgetEditAction.Resize, IsStarted = true });
        }

        private void ResizeThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (DataContext is vMixController.Widgets.vMixControl item && !item.Locked)
                Messenger.Send(new WidgetEditMessage { Widget = item, Action = WidgetEditAction.Resize, IsStarted = false });
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.DataContext is vMixController.Widgets.vMixControl item && !item.Locked)
            {
                var left = item.Left;
                var top = item.Top;
                var right = item.Left + item.Width;
                var bottom = item.Top + item.Height;

                var newLeft = left;
                var newTop = top;
                var newRight = right;
                var newBottom = bottom;

                switch (HorizontalAlignment)
                {
                    case System.Windows.HorizontalAlignment.Left:
                        newLeft = vMixControlUtils.SnapToGrid(left + e.HorizontalChange);
                        break;
                    case System.Windows.HorizontalAlignment.Right:
                        newRight = vMixControlUtils.SnapToGrid(right + e.HorizontalChange);
                        break;
                }

                switch (VerticalAlignment)
                {
                    case System.Windows.VerticalAlignment.Top:
                        newTop = vMixControlUtils.SnapToGrid(top + e.VerticalChange);
                        break;
                    case System.Windows.VerticalAlignment.Bottom:
                        newBottom = vMixControlUtils.SnapToGrid(bottom + e.VerticalChange);
                        break;
                }

                if (newRight - newLeft < 64)
                {
                    if (HorizontalAlignment == System.Windows.HorizontalAlignment.Left)
                        newLeft = newRight - 64;
                    else
                        newRight = newLeft + 64;
                }

                if (newBottom - newTop < vMixControlUtils.GridSize)
                {
                    if (VerticalAlignment == System.Windows.VerticalAlignment.Top)
                        newTop = newBottom - vMixControlUtils.GridSize;
                    else
                        newBottom = newTop + vMixControlUtils.GridSize;
                }

                item.Left = newLeft;
                item.Top = newTop;
                item.Width = newRight - newLeft;
                item.Height = newBottom - newTop;
            }

            e.Handled = true;
        }
    }
}


