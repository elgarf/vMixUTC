using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace vMixController.Controls
{
    /// <summary>
    /// Логика взаимодействия для vMixControlContainer.xaml
    /// </summary>
    public partial class vMixControlContainer : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public Action<object, SizeChangedEventArgs> OnSizeChanged { get; set; }

        /// <summary>
        /// The <see cref="ParentContainer" /> property's name.
        /// </summary>
        public const string ParentContainerPropertyName = "ParentContainer";

        private vMixControlContainerDummy _parentContainer = null;

        /// <summary>
        /// Sets and gets the ParentContainer property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public vMixControlContainerDummy ParentContainer
        {
            get
            {
                return _parentContainer;
            }

            set
            {
                if (_parentContainer == value)
                {
                    return;
                }

                _parentContainer = value;
                RaisePropertyChanged(ParentContainerPropertyName);
            }
        }

        private void RaisePropertyChanged(string parentContainerPropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(parentContainerPropertyName));
        }

        public vMixControlContainer()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void CC_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnSizeChanged?.Invoke(sender, e);
        }

        private void Caption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CC_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            moveThumb.Visibility = Keyboard.IsKeyDown(Key.LeftAlt) ? Visibility.Visible : Visibility.Collapsed;

        }

        private void Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var sb = ((Storyboard)Resources["OpacityOn"]);
            sb.Begin(RightButtons);
            //sb.Begin(LockButton);
            RightButtons.IsHitTestVisible = true;
            //LockButton.IsHitTestVisible = true;
        }

        private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var sb = ((Storyboard)Resources["OpacityOff"]);
            if (RightButtons.Opacity == 1)
                sb.BeginTime = TimeSpan.FromSeconds(2);
            else
                sb.BeginTime = TimeSpan.Zero;
            sb.Begin(RightButtons);
            //sb.Begin(LockButton);
            RightButtons.IsHitTestVisible = false;
            //LockButton.IsHitTestVisible = false;
        }
    }

}
