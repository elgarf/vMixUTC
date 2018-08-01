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
        }

        private void VMixControlMoveThumb_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (e.NewValue as Widgets.vMixWidget) as vMixController.Widgets.vMixWidget;
            if (ctrl != null)
            {
                Locked = ctrl.Locked;
                ctrl.PropertyChanged += Ctrl_PropertyChanged;
            }
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Locked"));
        }

        private void Ctrl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                Locked = (sender as Widgets.vMixWidget).Locked;
                PropertyChanged(this, e);
            }
        }

        void PhotoMoveThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            vMixController.Widgets.vMixWidget item = this.DataContext as vMixController.Widgets.vMixWidget;
            //item.IsSelected = true;
        }

        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            vMixController.Widgets.vMixWidget item = this.DataContext as vMixController.Widgets.vMixWidget;

            if (item != null && !item.Locked)
            {
                item.Left = Math.Round(item.Left + e.HorizontalChange);
                item.Top = Math.Round(item.Top + e.VerticalChange);
                item.AlignByGrid();

                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<Triple<vMixWidget, double, double>>(new Triple<vMixWidget, double, double>(item, e.HorizontalChange, e.VerticalChange));
            }

            
        }

    }
}
