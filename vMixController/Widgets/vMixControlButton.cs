using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;
using vMixController.PropertiesControls;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlButton: vMixControl
    {
        [NonSerialized]
        DispatcherTimer _timer;
        int _pointer;

        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Button");
            }
        }

        /// <summary>
        /// The <see cref="Commands" /> property's name.
        /// </summary>
        public const string CommandsPropertyName = "Commands";

        private ObservableCollection<vMixControlButtonCommand> _commands = new ObservableCollection<vMixControlButtonCommand>();

        /// <summary>
        /// Sets and gets the Actions property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<vMixControlButtonCommand> Commands
        {
            get
            {
                return _commands;
            }

            set
            {
                if (_commands == value)
                {
                    return;
                }

                _commands = value;
                RaisePropertyChanged(CommandsPropertyName);
            }
        }

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

        [NonSerialized]
        private RelayCommand _executeScriptCommand;

        /// <summary>
        /// Gets the ExecuteScriptCommand.
        /// </summary>
        [XmlIgnore]
        public RelayCommand ExecuteScriptCommand
        {
            get
            {
                return _executeScriptCommand
                    ?? (_executeScriptCommand = new RelayCommand(
                    () =>
                    {
                        Enabled = false;
                        _pointer = 0;
                        _timer.Interval = TimeSpan.FromMilliseconds(0);
                        _timer.Start();
                    }));
            }
        }

        [NonSerialized]
        private RelayCommand _stopScriptCommand;

        /// <summary>
        /// Gets the StopScriptCommand.
        /// </summary>
        [XmlIgnore]
        public RelayCommand StopScriptCommand
        {
            get
            {
                return _stopScriptCommand
                    ?? (_stopScriptCommand = new RelayCommand(
                    () =>
                    {
                        _timer.Stop();
                        Enabled = true;
                    }));
            }
        }

        public vMixControlButton()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            _pointer = 0;
            Enabled = true;
        }

        public override Hotkey[] GetHotkeys()
        {
            return new Classes.Hotkey[] { new Classes.Hotkey() { Name = "Execute" }, new Classes.Hotkey() { Name = "Reset" } };
        }

        private string GetInputNumber(int input)
        {
            try
            {
                return State.Inputs[input].Number.ToString();
            }
            catch (Exception)
            {
                return "-1";
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (_pointer >= _commands.Count)
            {
                _timer.Stop();
                Enabled = true;
                return;
            }
            _timer.Interval = TimeSpan.FromMilliseconds(0);
            var cmd = _commands[_pointer];
            if (cmd.Action.Native)
                switch (cmd.Action.Function)
                {
                    case "Timer":
                        _timer.Interval = TimeSpan.FromMilliseconds(cmd.Parameter);
                        break;
                    case "UpdateState":
                        if (State != null)
                            State.Update();
                        break;
                    case "GoTo":
                        _pointer = cmd.Parameter - 1;
                        break;
                    case "ExecLink":
                        Messenger.Default.Send<string>(cmd.StringParameter);
                        break;
                }
            else
                State.SendFunction(string.Format(cmd.Action.FormatString, GetInputNumber(cmd.Input), cmd.Parameter, cmd.StringParameter));
            
            _pointer++;
            
        }

        public override void ExecuteHotkey(int index)
        {
            base.ExecuteHotkey(index);
            switch (index)
            {
                case 0: ExecuteScriptCommand.Execute(null);
                    break;
                case 1: StopScriptCommand.Execute(null);
                    break;
            }
        }

        public override UserControl[] GetPropertiesControls()
        {
            ScriptControl control = GetPropertyControl<ScriptControl>();
            control.Commands.Clear();
            foreach (var item in Commands)
            {
                control.Commands.Add(new vMixControlButtonCommand() { Action = item.Action, Input = item.Input, Parameter = item.Parameter, StringParameter = item.StringParameter });
            }
            return base.GetPropertiesControls().Concat(new UserControl[] { control }).ToArray();
        }

        public override void SetProperties(vMixControlSettingsViewModel viewModel)
        {
            base.SetProperties(viewModel);
        }

        public override void SetProperties(UserControl[] _controls)
        {
            Commands.Clear();
            foreach (var item in (_controls.OfType<ScriptControl>().First()).Commands)
                Commands.Add(new vMixControlButtonCommand() { Action = item.Action, Input = item.Input, Parameter = item.Parameter, StringParameter = item.StringParameter });

            base.SetProperties(_controls);
        }

        public override void Dispose()
        {
            _timer.Stop();
            base.Dispose();
        }
    }
}
