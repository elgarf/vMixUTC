using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace vMixController.Classes
{
    [Serializable]
    public class Hotkey: INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Link { get; set; }
        /// <summary>
        /// The <see cref="Key" /> property's name.
        /// </summary>
        public const string KeyPropertyName = "Key";

        private Key _key = Key.None;

        /// <summary>
        /// Sets and gets the Key property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Key Key
        {
            get
            {
                return _key;
            }

            set
            {
                if (_key == value)
                {
                    return;
                }

                _key = value;
                RaisePropertyChanged(KeyPropertyName);
            }
        }

        private void RaisePropertyChanged(string keyPropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(keyPropertyName));
        }

        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public bool OnPress { get; set; } = true;

        /// <summary>
        /// The <see cref="Active" /> property's name.
        /// </summary>
        public const string ActivePropertyName = "Active";

        private bool _active = false;

        /// <summary>
        /// Sets and gets the Active property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Active
        {
            get
            {
                return _active;
            }

            set
            {
                if (_active == value)
                {
                    return;
                }

                _active = value;
                RaisePropertyChanged(ActivePropertyName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
