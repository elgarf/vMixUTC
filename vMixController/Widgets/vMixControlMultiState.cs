using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlMultiState : vMixWidget
    {

        public string IP { get; set; }
        public string Port { get; set; }

        [XmlIgnore]
        public State DummyState { get; set; }


        /// <summary>
        /// The <see cref="Enabled" /> property's name.
        /// </summary>
        public const string EnabledPropertyName = "Enabled";

        private bool _enabled = true;

        /// <summary>
        /// Sets and gets the Enabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                if (_enabled == value)
                {
                    return;
                }

                _enabled = value;
                RaisePropertyChanged(EnabledPropertyName);
            }
        }

        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Multi State");
            }
        }

        [XmlIgnore]
        public override State State
        {
            get
            {
                return base.State;
            }

            set
            {
                if (base.State != null)
                    base.State.OnFunctionSend -= State_OnFunctionSend;
                base.State = value;
                if (base.State != null)
                    base.State.OnFunctionSend += State_OnFunctionSend;
            }
        }

        private void State_OnFunctionSend(object sender, FunctionSendArgs e)
        {
            if (DummyState == null)
                DummyState = new State();
            DummyState.Configure(IP, Port);
            if (Enabled)
                DummyState.SendFunction(e.Function);
        }

        public override UserControl[] GetPropertiesControls()
        {
            List<UserControl> _temp = new List<UserControl>();
            var uc = GetPropertyControl<PropertiesControls.StringControl>();
            uc.Title = "IP";
            uc.Value = IP;
            _temp.Add(uc);
            uc = GetPropertyControl<PropertiesControls.StringControl>();
            uc.Title = "Port";
            uc.Value = Port;
            _temp.Add(uc);
            return base.GetPropertiesControls().Concat(_temp).ToArray();
        }

        public override void SetProperties(UserControl[] _controls)
        {
            var ctrls = _controls.OfType<PropertiesControls.StringControl>().ToArray();
            IP = ctrls[0].Value;
            Port = ctrls[1].Value;
            base.SetProperties(_controls);
        }

        public override Hotkey[] GetHotkeys()
        {

            return base.GetHotkeys().Concat(new Hotkey[] { new Hotkey() { Name = "Toggle\nEnabled" } }).ToArray();
        }

        public override void ExecuteHotkey(int index)
        {
            if (index == 0)
                Enabled = !Enabled;
        }

        public vMixControlMultiState()
        {
            IP = "127.0.0.1";
            Port = "8088";
        }

        [NonSerialized]
        private RelayCommand _toggleEnabledCommand;

        /// <summary>
        /// Gets the ToggleEnabled.
        /// </summary>
        public RelayCommand ToggleEnabledCommand
        {
            get
            {
                return _toggleEnabledCommand
                    ?? (_toggleEnabledCommand = new RelayCommand(
                    () =>
                    {
                        Enabled = !Enabled;
                    }));
            }
        }
    }
}