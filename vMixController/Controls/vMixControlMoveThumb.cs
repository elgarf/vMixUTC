using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using vMixController.Classes;
using vMixController.Widgets;

namespace vMixController.Controls
{
    public class vMixControlMoveThumb : Thumb, INotifyPropertyChanged
    {


        public bool Locked
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public vMixControlMoveThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
            this.DragStarted += PhotoMoveThumb_DragStarted;
            this.DataContextChanged += VMixControlMoveThumb_DataContextChanged;
            this.DragCompleted += VMixControlMoveThumb_DragCompleted;
        }

        private void VMixControlMoveThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (this.DataContext is vMixControl item && !item.Locked)
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Pair<vMixControl, bool>(item, false));
        }

        private void VMixControlMoveThumb_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is vMixControl ctrl)
            {
                Locked = ctrl.Locked;
                ctrl.PropertyChanged += Ctrl_PropertyChanged;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Locked"));
        }

        private void Ctrl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                Locked = (sender as Widgets.vMixControl).Locked;
                PropertyChanged(this, e);
            }
        }

        void PhotoMoveThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            vMixController.Widgets.vMixControl item = this.DataContext as vMixController.Widgets.vMixControl;
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Pair<vMixControl, bool>(item, true));
            //item.IsSelected = true;
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {

            if (this.DataContext is vMixControl item && !item.Locked)
            {
                var px = item.Left;
                var py = item.Top;

                item.Left = Math.Round(item.Left + e.HorizontalChange);
                item.Top = Math.Round(item.Top + e.VerticalChange);

                item.AlignByGrid();

                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Triple<vMixControl, double, double>(item, item.Left - px, item.Top - py));
            }


        }

    }
}
