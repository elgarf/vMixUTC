using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace vMixController.Classes
{
    [Serializable]
    public class Triple<T1, T2, T3>: Pair<T1, T2>
    {
        public Triple()
        {

        }

        public Triple(T1 a, T2 b, T3 c)
        {
            A = a;
            B = b;
            C = c;
        }

        /*public T1 A
        {
            get { return (T1)GetValue(AProperty); }
            set { SetValue(AProperty, value); }
        }

        // Using a DependencyProperty as the backing store for A.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AProperty =
            DependencyProperty.Register("A", typeof(T1), typeof(Triple<T1, T2, T3>), new PropertyMetadata(default(T1)));



        public T2 B
        {
            get { return (T2)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }

        // Using a DependencyProperty as the backing store for B.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register("B", typeof(T2), typeof(Triple<T1, T2, T3>), new PropertyMetadata(default(T2)));*/



        public T3 C
        {
            get { return (T3)GetValue(CProperty); }
            set { SetValue(CProperty, value); }
        }

        // Using a DependencyProperty as the backing store for C.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CProperty =
            DependencyProperty.Register("C", typeof(T3), typeof(Triple<T1, T2, T3>), new PropertyMetadata(default(T3)));




    }
}
