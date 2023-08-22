using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using vMixController.PropertiesControls;

namespace vMixController.Widgets
{
    public class vMixControlRegion: vMixControl
    {
        public vMixControlRegion()
        {
            ZIndex = -1;
        }

        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Region");
            }
        }

        public override bool IsResizeableVertical => true;

        /// <summary>
        /// The <see cref="Text" /> property's name.
        /// </summary>
        public const string TextPropertyName = "Text";

        private string _Text = "";

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Text
        {
            get
            {
                return _Text;
            }

            set
            {
                if (_Text == value)
                {
                    return;
                }

                _Text = value;
                RaisePropertyChanged(TextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Sticky" /> property's name.
        /// </summary>
        public const string StickyPropertyName = "Sticky";

        private bool _sticky = false;

        /// <summary>
        /// Sets and gets the Magnet property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Sticky
        {
            get
            {
                return _sticky;
            }

            set
            {
                if (_sticky == value)
                {
                    return;
                }

                _sticky = value;
                RaisePropertyChanged(StickyPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsEditable" /> property's name.
        /// </summary>
        public const string IsEditablePropertyName = "IsEditable";

        private bool _isEditable = false;

        /// <summary>
        /// Sets and gets the Magnet property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsEditable
        {
            get
            {
                return _isEditable;
            }

            set
            {
                if (_isEditable == value)
                {
                    return;
                }

                _isEditable = value;
                RaisePropertyChanged(IsEditablePropertyName);
            }
        }

        [NonSerialized]
        private RelayCommand<object> _mouseDoubleClick;

        /// <summary>
        /// Gets the ExecutePushOn.
        /// </summary>
        public RelayCommand<object> MouseDoubleClick
        {
            get
            {
                return _mouseDoubleClick
                    ?? (_mouseDoubleClick = new RelayCommand<object>(
                    (p) =>
                    {
                        //MouseEventArgs

                        IsEditable = true;
                        //p.Handled = true;

                    }));
            }
        }

        public override UserControl[] GetPropertiesControls()
        {
            var infoString = GetPropertyControl<StringControl>();
            infoString.AcceptReturn = true;
            infoString.TextWrapping = System.Windows.TextWrapping.Wrap;
            infoString.Title = "Info";
            infoString.Value = Text;
            return base.GetPropertiesControls().Concat(new UserControl[] { infoString }).ToArray();
        }

        public override void SetProperties(UserControl[] _controls)
        {
            base.SetProperties(_controls);
            Text = _controls.OfType<StringControl>().First().Value;
        }

        public override void Update()
        {
            Height++;
            Height--;
            base.Update();
        }
    }
}
