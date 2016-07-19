using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using vMixController.Classes;
using System.Windows.Controls;
using vMixController.PropertiesControls;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlScore : vMixControlTextField
    {
        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Score");
            }
        }
        public vMixControlScore()
        {
            Text = "0";
        }

        public override Hotkey[] GetHotkeys()
        {
            return new Classes.Hotkey[] { new Classes.Hotkey() { Name = "Reset" },
            new Classes.Hotkey() { Name = "+1" },
            new Classes.Hotkey() { Name = "+2" },
            new Classes.Hotkey() { Name = "+3" }};
        }

        public override void ExecuteHotkey(int index)
        {
            int i = 0;
            int.TryParse(Text, out i);
            switch (index)
            {
                case 0:
                    Text = "0";
                    break;
                case 1:
                    Text = (i + 1).ToString();
                    break;
                case 2:
                    Text = (i + 2).ToString();
                    break;
                case 3:
                    Text = (i + 3).ToString();
                    break;
                default:
                    break;
            }

            base.ExecuteHotkey(index);
        }

        [NonSerialized]
        private RelayCommand<ControlIntParameter> _addScoreCommand;

        /// <summary>
        /// Gets the AddScoreCommand.
        /// </summary>
        [XmlIgnore]
        public RelayCommand<ControlIntParameter> AddScoreCommand
        {
            get
            {
                return _addScoreCommand
                    ?? (_addScoreCommand = new RelayCommand<ControlIntParameter>(
                    p =>
                    {
                        int _out = 0;
                        int.TryParse(p.A.Text, out _out);
                        _out += p.B;
                        p.A.Text = _out.ToString();
                    }));
            }
        }

        [NonSerialized]
        private RelayCommand _resetScoreCommand;

        /// <summary>
        /// Gets the ResetScoreCommand.
        /// </summary>
        [XmlIgnore]
        public RelayCommand ResetScoreCommand
        {
            get
            {
                return _resetScoreCommand
                    ?? (_resetScoreCommand = new RelayCommand(
                    () =>
                    {
                        Text = "0";
                    }));
            }
        }

        public override UserControl[] GetPropertiesControls()
        {
            var props = base.GetPropertiesControls();
            props.OfType<BoolControl>().First().Visibility = System.Windows.Visibility.Collapsed;
            return props;
        }
    }
}
