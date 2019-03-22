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

        public override UserControl[] GetPropertiesControls()
        {
            var sc = GetPropertyControl<StringControl>();
            sc.AcceptReturn = true;
            sc.Title = "Info";
            sc.Value = Text;
            return base.GetPropertiesControls().Concat(new UserControl[] { sc }).ToArray();
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
