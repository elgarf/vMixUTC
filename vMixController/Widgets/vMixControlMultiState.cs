using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;

namespace vMixController.Widgets
{
    [Serializable]
    public partial class vMixControlMultiState : vMixControl
    {

        private string _ip;
        private string _port;

        public string IP
        {
            get => _ip;
            set => SetPropertyValue(ref _ip, value, nameof(IP));
        }

        public string Port
        {
            get => _port;
            set => SetPropertyValue(ref _port, value, nameof(Port));
        }
        public string Login { get; set; }
        public string Password { get; set; }

        [XmlIgnore]
        public State DummyState { get; set; }

        private bool _enabled = true;

        /// <summary>
        /// Sets and gets the Enabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set => SetPropertyValue(ref _enabled, value, nameof(Enabled));
        }

        public override string Type
        {
            get
            {
                return "Multi State";
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
            DummyState.Configure(IP, Port, Login, Password);
            if (Enabled)
                DummyState.SendFunction(e.Function);
        }

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        public override void AfterPropertiesChanged()
        {
            base.AfterPropertiesChanged();
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
            Login = "admin";
            Password = "";
        }

        [RelayCommand]
        private void ToggleEnabled()
        {
            Enabled = !Enabled;
        }
    }
}
