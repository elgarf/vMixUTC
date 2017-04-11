using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;
using vMixController.Extensions;
using vMixController.PropertiesControls;
using vMixController.ViewModel;
using System.Globalization;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlButton : vMixControl
    {
        const string VARIABLEPREFIX = "_var";
        [NonSerialized]
        DispatcherTimer _timer;
        int _pointer;
        int _waitBeforeUpdate = -1;
        static DateTime _lastShadowUpdate = DateTime.Now;
        Stack<bool> _conditions = new Stack<bool>();
        CultureInfo _culture;

        /// <summary>
        /// The <see cref="HasScriptErrors" /> property's name.
        /// </summary>
        public const string HasScriptErrorsPropertyName = "HasScriptErrors";

        private bool _hasScriptErrors = false;

        /// <summary>
        /// Sets and gets the HasScriptErrors property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool HasScriptErrors
        {
            get
            {
                return _hasScriptErrors;
            }

            set
            {
                if (_hasScriptErrors == value)
                {
                    return;
                }

                _hasScriptErrors = value;
                RaisePropertyChanged(HasScriptErrorsPropertyName);
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
                if (_internalState != null)
                {
                    _internalState.OnStateUpdated -= State_OnStateUpdated;
                    //_internalState.OnFunctionSend += State_OnFunctionSend;
                }

                base.State = value;

                if (_internalState != null)
                {
                    _internalState.OnStateUpdated += State_OnStateUpdated;
                    //_internalState.OnFunctionSend += State_OnFunctionSend;
                }
            }
        }

        public override void ShadowUpdate()
        {

            if (IsStateDependent && _internalState != null && (DateTime.Now - _lastShadowUpdate).TotalSeconds > 5)
            {
                _internalState.UpdateAsync();
                _lastShadowUpdate = DateTime.Now;
            }
            base.ShadowUpdate();
        }

        /*private void State_OnFunctionSend(object sender, FunctionSendArgs e)
        {
            if (e.Function != "") ;
                //UpdateActiveProperty();
            //throw new NotImplementedException();
        }*/

        private void State_OnStateUpdated(object sender, StateUpdatedEventArgs e)
        {
            if (e.Successfully)
                RealUpdateActiveProperty();
        }

        private void RealUpdateActiveProperty(bool skipStateDependency = false, vMixAPI.State stateToCheck = null)
        {
            if (stateToCheck == null) stateToCheck = _internalState;
            if ((!IsStateDependent && skipStateDependency) || stateToCheck == null) return;
            var result = true;
            HasScriptErrors = false;
            Stack<bool> conds = new Stack<bool>();
            for (int i = 0; i < _commands.Count; i++)
            {
                var item = _commands[i];
                /*if (item.Action.Function.StartsWith(CONDITION))
                {
                    if (item.Action.Function == CONDITION)
                        conds.Push(TestCondition(item));
                    else if (conds.Count > 0)
                        conds.Pop();
                }
                else */
                if (conds.Count == 0 || conds.Peek())
                {
                    /*if (item.Action.Function == GOTO)
                    {
                        i = item.Parameter;
                        continue;
                    }*/

                    if (State == null) return;

                    var input = State.Inputs.Where(x => x.Key == item.InputKey).FirstOrDefault()?.Number;
                    if (string.IsNullOrWhiteSpace(item.Action.ActiveStatePath)) continue;
                    var path = string.Format(item.Action.ActiveStatePath, item.InputKey, item.Parameter, item.StringParameter, item.Parameter - 1, input.HasValue ? input.Value : -1);
                    var nval = GetValueByPath(stateToCheck, path);
                    var val = nval == null ? "" : nval.ToString();
                    HasScriptErrors = HasScriptErrors || nval == null;
                    var aval = string.Format(item.Action.ActiveStateValue, GetInputNumber(item.InputKey), item.Parameter, item.StringParameter, item.Parameter - 1, input.HasValue ? input.Value : -1);
                    var realval = aval;
                    aval = aval.TrimStart('!');
                    bool mult = (aval == "-" && ((val is string && string.IsNullOrWhiteSpace((string)val)) || (val == null) /*|| (val is bool && (bool)val == false)*/)) ||
                        (aval == "*") ||
                        (val != null && !(val is string) && aval == val.ToString()) ||
                        (val is string && (string)val == aval);
                    if (!string.IsNullOrWhiteSpace(aval) && aval[0] == '!')
                        mult = !mult;
                    result = result && mult;
                }

            }
            Active = result;
        }

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


        /// <summary>
        /// The <see cref="IsStateDependent" /> property's name.
        /// </summary>
        public const string IsStateDependentPropertyName = "IsStateDependent";

        private bool _isStateDependent = false;

        /// <summary>
        /// Sets and gets the IsStateDependent property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsStateDependent
        {
            get
            {
                return _isStateDependent;
            }

            set
            {
                if (_isStateDependent == value)
                {
                    return;
                }

                _isStateDependent = value;
                RaisePropertyChanged(IsStateDependentPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="Variables" /> property's name.
        /// </summary>
        public const string VariablesPropertyName = "Variables";

        private List<Pair<int, object>> _variables = new List<Pair<int, object>>();

        /// <summary>
        /// Sets and gets the Variables property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<Pair<int, object>> Variables
        {
            get
            {
                return _variables;
            }

            set
            {
                if (_variables == value)
                {
                    return;
                }

                _variables = value;
                RaisePropertyChanged(VariablesPropertyName);
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

        private void UpdateActiveProperty()
        {
            if (!IsStateDependent) return;
            State.UpdateAsync();

        }

        public vMixControlButton()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            _pointer = 0;
            _enabled = true;
            _culture = new CultureInfo(CultureInfo.InvariantCulture.Name);
            _culture.NumberFormat.NumberDecimalDigits = 5;
            _culture.NumberFormat.CurrencyDecimalDigits = 5;

        }

        public override Hotkey[] GetHotkeys()
        {
            return new Classes.Hotkey[] { new Classes.Hotkey() { Name = "Execute" }, new Classes.Hotkey() { Name = "Reset" }, new Classes.Hotkey { Name = "Clear Variables" } };
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

        private string GetInputNumber(string key)
        {
            try
            {
                return State.Inputs.Where(x => x.Key == key).FirstOrDefault().Number.ToString();
            }
            catch (Exception)
            {
                return "-1";
            }
        }

        private void PopulateVariables(NCalc.Expression exp)
        {
            foreach (var item in _variables)
                exp.Parameters.Add(string.Format("{0}{1}", VARIABLEPREFIX, item.A), item.B);
        }

        private bool Calculate(string s)
        {
            
            try
            {
                NCalc.Expression exp = new NCalc.Expression(s);
                PopulateVariables(exp);
                return (bool)exp.Evaluate();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private object CalculateExpression(string s)
        {
            try
            {
                NCalc.Expression exp = new NCalc.Expression(s);
                PopulateVariables(exp);
                return exp.Evaluate();
            }
            catch (Exception)
            {
                return s;
            }
        }

        private object EscapeString(object o)
        {
            if (o is string)
                return string.Format("'{0}'", o);
            return o;
        }

        private object GetPathOrValue(object target, string s)
        {
            if (s.StartsWith("@@"))
                return s.Substring(1, s.Length - 1);
            else if (s.StartsWith("@"))
                return EscapeString(GetValueByPath(target, s.TrimStart('@')));
            return s;
        }

        private bool TestCondition(vMixControlButtonCommand cmd)
        {
            if (cmd.AdditionalParameters == null || cmd.AdditionalParameters.Count == 0)
                return false;
            object part1 = null;
            object part2 = null;

            part1 = GetPathOrValue(_isStateDependent ? _internalState ?? State : State, string.Format(cmd.AdditionalParameters[1].A, cmd.InputKey, cmd.AdditionalParameters[0].A));
            part2 = GetPathOrValue(_isStateDependent ? _internalState ?? State : State, string.Format(cmd.AdditionalParameters[3].A, cmd.InputKey, cmd.AdditionalParameters[0].A));

            Thread.CurrentThread.CurrentCulture = _culture;
            Thread.CurrentThread.CurrentUICulture = _culture;
            return Calculate(string.Format("{0}{1}{2}", part1.ToString(), cmd.AdditionalParameters[2].A, part2.ToString()));//(CompareObjects(part1, part2, cmd.AdditionalParameters[2].A));
        }

        private int GetVariableIndex(int number)
        {
            for (int i = 0; i < _variables.Count; i++)
            {
                if (_variables[i].A == number)
                    return i;
            }
            return -1;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (_pointer >= _commands.Count)
            {
                _conditions.Clear();
                _timer.Stop();
                Enabled = true;
                //_waitBeforeUpdate++;
                //Active = !Active;
                ThreadPool.QueueUserWorkItem((x) =>
                {
                    Thread.Sleep(_waitBeforeUpdate);
                    _waitBeforeUpdate = -1;
                    Dispatcher.Invoke(() => _internalState.UpdateAsync());

                });
                return;
            }
            _timer.Interval = TimeSpan.FromMilliseconds(0);
            var cmd = _commands[_pointer];
            if (_conditions.Count == 0 || _conditions.Peek() || cmd.Action.Function == NativeFunctions.CONDITIONEND)
                if (cmd.Action.Native)
                    switch (cmd.Action.Function)
                    {
                        case NativeFunctions.TIMER:
                            _timer.Interval = TimeSpan.FromMilliseconds(cmd.Parameter);
                            break;
                        case NativeFunctions.UPDATESTATE:
                            if (State != null)
                                State.Update();
                            break;
                        case NativeFunctions.UPDATEINTERNALBUTTONSTATE:
                            _internalState?.Update();
                            break;
                        case NativeFunctions.GOTO:
                            _pointer = cmd.Parameter - 1;
                            break;
                        case NativeFunctions.EXECLINK:
                            Messenger.Default.Send<string>(cmd.StringParameter);
                            break;
                        case NativeFunctions.CONDITION:
                            //0 - input 2
                            //1 - left part
                            //2 - middle part
                            //3 - right part
                            _conditions.Push(TestCondition(cmd));
                            break;
                        case NativeFunctions.HASVARIABLE:
                            _conditions.Push(GetVariableIndex(cmd.Parameter) != -1);
                            break;
                        case NativeFunctions.CONDITIONEND:
                            _conditions.Pop();
                            break;
                        case NativeFunctions.SETVARIABLE:
                            var idx = GetVariableIndex(cmd.Parameter);
                            if (idx == -1)
                                _variables.Add(new Pair<int, object>() { A = cmd.Parameter, B = CalculateObjectParameter(cmd) });
                            else
                                _variables[idx].B = CalculateObjectParameter(cmd);
                            break;
                    }
                else if (State != null)
                {
                    var input = State.Inputs.Where(x => x.Key == cmd.InputKey).FirstOrDefault()?.Number;

                    if (!cmd.Action.StateDirect)
                    {

                        State.SendFunction(string.Format(cmd.Action.FormatString, cmd.InputKey, cmd.Parameter, CalculateObjectParameter(cmd), cmd.Parameter - 1, input.HasValue ? input.Value : 0));
                    }
                    else
                    {
                        var path = string.Format(cmd.Action.ActiveStatePath, cmd.InputKey, cmd.Parameter, CalculateObjectParameter(cmd), cmd.Parameter - 1, input.HasValue ? input.Value : 0);
                        SetValueByPath(State, path, cmd.Action.StateValue == "Input" ? (object)cmd.InputKey : (cmd.Action.StateValue == "String" ? CalculateObjectParameter(cmd).ToString() : (object)cmd.Parameter));
                    }
                    _waitBeforeUpdate = Math.Max(_internalState.Transitions[cmd.Action.TransitionNumber].Duration, _waitBeforeUpdate);
                }
            _pointer++;

        }

        private object CalculateObjectParameter(vMixControlButtonCommand cmd)
        {
            return CalculateExpression(GetPathOrValue(_isStateDependent ? _internalState ?? State : State, string.Format(cmd.StringParameter, cmd.InputKey)).ToString());
        }

        public override void ExecuteHotkey(int index)
        {
            base.ExecuteHotkey(index);
            switch (index)
            {
                case 0:
                    ExecuteScriptCommand.Execute(null);
                    break;
                case 1:
                    StopScriptCommand.Execute(null);
                    break;
                case 2:
                    Variables.Clear();
                    break;
            }
        }

        public override UserControl[] GetPropertiesControls()
        {
            //!!!!!
            BoolControl boolctrl = new BoolControl() { Title = LocalizationManager.Get("State Dependent"), Value = IsStateDependent, Visibility = System.Windows.Visibility.Visible };
            ScriptControl control = GetPropertyControl<ScriptControl>();
            control.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            control.Commands.Clear();
            foreach (var item in Commands)
            {
                if (item.AdditionalParameters.Count == 0)
                    for (int i = 0; i < 10; i++)
                        item.AdditionalParameters.Add(new One<string>());
                control.Commands.Add(new vMixControlButtonCommand() { Action = item.Action, Collapsed = item.Collapsed, Input = item.Input, InputKey = item.InputKey, Parameter = item.Parameter, StringParameter = item.StringParameter, AdditionalParameters = item.AdditionalParameters });
            }
            return base.GetPropertiesControls().Concat(new UserControl[] { boolctrl, control }).ToArray();
        }

        public override void SetProperties(vMixControlSettingsViewModel viewModel)
        {
            base.SetProperties(viewModel);
        }

        public override void SetProperties(UserControl[] _controls)
        {
            Commands.Clear();
            foreach (var item in (_controls.OfType<ScriptControl>().First()).Commands)
                Commands.Add(new vMixControlButtonCommand() { Action = item.Action, Collapsed = item.Collapsed, Input = item.Input, InputKey = item.InputKey, Parameter = item.Parameter, StringParameter = item.StringParameter, AdditionalParameters = item.AdditionalParameters });

            IsStateDependent = _controls.OfType<BoolControl>().First().Value;

            RealUpdateActiveProperty(true, State);
            base.SetProperties(_controls);
        }

        public override void Update()
        {
            base.Update();
            
            RealUpdateActiveProperty();
        }

        protected override void Dispose(bool managed)
        {
            if (_disposed) return;

            if (managed)
            {
                if (_internalState != null)
                    _internalState.OnStateUpdated -= State_OnStateUpdated;
                _timer.Stop();
                base.Dispose(managed);
                GC.SuppressFinalize(this);
            }
        }
    }
}
