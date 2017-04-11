using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace vMixController.Classes
{
    [Serializable]
    public class One<T> : ObservableObject
    {
        private T a;

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
    }

}
