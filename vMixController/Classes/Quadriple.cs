using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace vMixController.Classes
{
    [Serializable]
    public class Quadriple<T1, T2, T3, T4> : Triple<T1, T2, T3>
    {
        public Quadriple()
        {

        }

        public Quadriple(T1 a, T2 b, T3 c, T4 d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public T4 D
        {
            get { return (T4)GetValue(DProperty); }
            set { SetValue(DProperty, value); }
        }

        // Using a DependencyProperty as the backing store for C.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DProperty =
            DependencyProperty.Register("D", typeof(T4), typeof(Quadriple<T1, T2, T3, T4>), new PropertyMetadata(default(T4)));
    }
}
