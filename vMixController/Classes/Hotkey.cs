using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace vMixController.Classes
{
    [Serializable]
    public class Hotkey: INotifyPropertyChanged, ICloneable
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
            get => _key;
            set => SetPropertyValue(ref _key, value, KeyPropertyName);
        }

        private void RaisePropertyChanged(string keyPropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(keyPropertyName));
        }

        public object Clone()
        {
            return new Hotkey() { Active = Active, Alt = Alt, Ctrl = Ctrl, Key = Key, Link = Link, Name = Name, OnPress = OnPress, Shift = Shift };
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
            get => _active;
            set => SetPropertyValue(ref _active, value, ActivePropertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool SetPropertyValue<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}
