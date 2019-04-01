using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace vMixController.Classes
{
    [Serializable]
    public class Pair<T> : ObservableObject
    {
        private T a;
        private T b;

        public T A
        {
            get
            {
                return a;
            }

            set
            {
                a = value;
                RaisePropertyChanged("A");
            }
        }

        public T B
        {
            get
            {
                return b;
            }

            set
            {
                b = value;
                RaisePropertyChanged("B");
            }
        }
    }

    public class Pair<T1, T2> : DependencyObject
    {

        public Pair()
        {

        }
        public Pair(T1 a, T2 b)
        {
            A = a;
            B = b;
        }

        public T1 A
        {
            get { return (T1)GetValue(AProperty); }
            set { SetValue(AProperty, value); }
        }

        // Using a DependencyProperty as the backing store for A.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AProperty =
            DependencyProperty.Register("A", typeof(T1), typeof(Pair<T1, T2>), new PropertyMetadata(default(T1), InternalPropertyChanged));

        private static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }

        public T2 B
        {
            get { return (T2)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }

        // Using a DependencyProperty as the backing store for B.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register("B", typeof(T2), typeof(Pair<T1, T2>), new PropertyMetadata(default(T2), InternalPropertyChanged));
    }

    public class ControlIntParameter : Pair<Widgets.vMixControlTextField, int>
    {
        public ControlIntParameter() : base()
        {

        }
        public ControlIntParameter(Widgets.vMixControlTextField a, int b) : base(a, b)
        {

        }
    }

    public class ColorInfo: Pair<string, Widgets.vMixControl>
    {
        public ColorInfo()
        {
            A = "";
            B = new Widgets.vMixControl() { };
        }
    }
}
