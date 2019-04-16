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
                if (a is ObservableObject)
                    (a as ObservableObject).PropertyChanged -= One_PropertyChanged;
                a = value;
                if (value is ObservableObject)
                    (value as ObservableObject).PropertyChanged += One_PropertyChanged;
                RaisePropertyChanged("A");
            }
        }

        private void One_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("A");
        }
    }

}
