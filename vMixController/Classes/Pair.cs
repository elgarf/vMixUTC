using CommunityToolkit.Mvvm.ComponentModel;
using Melanchall.DryWetMidi.Core;
using NLog.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using vMixController.Extensions;

namespace vMixController.Classes
{
    [Serializable]
    public class Pair<T> : ObservableObject, ICloneable
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
                if (a is ObservableObject)
                    (a as ObservableObject).PropertyChanged -= AChanged;
                a = value;
                if (value is ObservableObject)
                    (value as ObservableObject).PropertyChanged += AChanged;
                OnPropertyChanged(nameof(A));
            }
        }

        private void AChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(A));
        }

        private void BChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(B));
        }

        public object Clone()
        {
            return new Pair<T>() { A = (T)ObjectCopier.Copy(A) };
        }

        public T B
        {
            get
            {
                return b;
            }

            set
            {
                if (b is ObservableObject)
                    (b as ObservableObject).PropertyChanged -= BChanged;
                b = value;
                if (value is ObservableObject)
                    (value as ObservableObject).PropertyChanged += BChanged;
                OnPropertyChanged(nameof(B));
            }
        }
    }

    [Serializable]
    public class Pair<T1, T2> : DependencyObject, ICloneable
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

        protected static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //((Pair<T1, T2>)d).OnPropertyChanged(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            PropertyChanged?.Invoke(this, e);
        }

        public object Clone()
        {
            
            return new Pair<T1, T2>((T1)ObjectCopier.Copy(A), (T2)ObjectCopier.Copy(B));
        }

        public T2 B
        {
            get { return (T2)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }

        // Using a DependencyProperty as the backing store for B.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register("B", typeof(T2), typeof(Pair<T1, T2>), new PropertyMetadata(default(T2), InternalPropertyChanged));

        public event DependencyPropertyChangedEventHandler PropertyChanged;
     }

    public class ControlIntParameter : Pair<Widgets.vMixControl, int>
    {
        public ControlIntParameter() : base()
        {

        }
        public ControlIntParameter(Widgets.vMixControl a, int b) : base(a, b)
        {

        }
    }

}

