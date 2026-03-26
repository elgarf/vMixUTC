using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.Messaging;
using vMixController.Classes;
using vMixController.Messages;
using vMixController.Widgets;
using vMixControllerSkin;

namespace vMixController.Controls
{
    public class vMixControlMoveThumb : DraggableThumb
    {
        private IMessenger Messenger => AppServices.IsRegistered<IMessenger>()
            ? AppServices.GetRequiredService<IMessenger>()
            : WeakReferenceMessenger.Default;

        public vMixControlMoveThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
            this.DragStarted += PhotoMoveThumb_DragStarted;
            this.DragCompleted += VMixControlMoveThumb_DragCompleted;
        }

        private void VMixControlMoveThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (this.DataContext is vMixControl item && !item.Locked)
            {
                Messenger.Send(new WidgetMoveStateMessage() { Widget = item, IsStarted = false });
                Messenger.Send(new WidgetEditMessage { Widget = item, Action = WidgetEditAction.Move, IsStarted = false });
            }
        }

        void PhotoMoveThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            vMixController.Widgets.vMixControl item = this.DataContext as vMixController.Widgets.vMixControl;
            Messenger.Send(new WidgetMoveStateMessage() { Widget = item, IsStarted = true });
            Messenger.Send(new WidgetEditMessage { Widget = item, Action = WidgetEditAction.Move, IsStarted = true });
            //item.IsSelected = true;
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {

            if (this.DataContext is vMixControl item && !item.Locked)
            {
                var px = item.Left;
                var py = item.Top;

                item.Left += e.HorizontalChange;
                item.Top += e.VerticalChange;

                item.AlignPositionByGrid();

                Messenger.Send(new WidgetMoveDeltaMessage() { Widget = item, DeltaX = item.Left - px, DeltaY = item.Top - py });
            }


        }

    }
}


