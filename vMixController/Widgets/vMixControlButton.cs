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
using System.ComponentModel;
using System.Xml;
using System.Windows.Media;
using System.Windows;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlButton : vMixControl
    {
        NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        const string VARIABLEPREFIX = "_var";
        static DateTime _lastShadowUpdate = DateTime.Now;
        static object _locker = new object();
        Stack<bool?> _conditions = new Stack<bool?>();
        CultureInfo _culture;
        BackgroundWorker _activeStateUpdateWorker, _executionWorker;
        DispatcherTimer _blinker;
        Color _defaultBorderColor;

        [NonSerialized]
        bool _stopThread = false;

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
                    _internalState.OnStateUpdated -= State_OnStateUpdated;

                base.State = value;

                if (_internalState != null)
                    _internalState.OnStateUpdated += State_OnStateUpdated;
            }
        }


        /// <summary>
        /// The <see cref="BlinkBorderColor" /> property's name.
        /// </summary>
        public const string BlinkBorderColorPropertyName = "BlinkBorderColor";
        [NonSerialized]
        private Color _blinkBorderColor = ViewModel.vMixControlSettingsViewModel.Colors[0].B;

        /// <summary>
        /// Sets and gets the BorderColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Color BlinkBorderColor
        {
            get
            {
                return _blinkBorderColor;
            }

            set
            {
                if (_blinkBorderColor == value)
                {
                    return;
                }

                _blinkBorderColor = value;
                RaisePropertyChanged(BlinkBorderColorPropertyName);
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

        private void State_OnStateUpdated(object sender, StateUpdatedEventArgs e)
        {
            if (e.Successfully)
                RealUpdateActiveProperty(false, null, State);

        }

        private void RealUpdateActiveProperty(bool skipStateDependency = false, vMixAPI.State stateToCheck = null, vMixAPI.State currentState = null)
        {
            stateToCheck = stateToCheck ?? _internalState;
            currentState = currentState ?? State;

            if ((!IsStateDependent && skipStateDependency) || stateToCheck == null || _activeStateUpdateWorker.IsBusy) return;
            _activeStateUpdateWorker.RunWorkerAsync(new State[] { stateToCheck, currentState });
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
        [XmlIgnore]
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
        [XmlIgnore]
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

        [NonSerialized]
        private List<Pair<int, object>> _variables = new List<Pair<int, object>>();

        /// <summary>
        /// Sets and gets the Variables property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
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
                        if (_executionWorker != null && _executionWorker.IsBusy)
                        {
                            _stopThread = true;
                            _executionWorker.CancelAsync();
                        }
                        _stopThread = false;

                        if (!_executionWorker.IsBusy)
                            _executionWorker.RunWorkerAsync(State);

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
                        _stopThread = true;
                        if (_executionWorker != null && _executionWorker.IsBusy)
                            _executionWorker.CancelAsync();

                        _conditions.Clear();
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
            _activeStateUpdateWorker = new BackgroundWorker();
            _activeStateUpdateWorker.RunWorkerCompleted += _worker_RunWorkerCompleted;
            _activeStateUpdateWorker.DoWork += _activeStateUpdateWorker_DoWork;

            _executionWorker = new BackgroundWorker();
            _executionWorker.DoWork += _executionWorker_DoWork;
            _executionWorker.WorkerSupportsCancellation = true;

            _enabled = true;
            _culture = new CultureInfo(CultureInfo.InvariantCulture.Name);
            _culture.NumberFormat.NumberDecimalDigits = 5;
            _culture.NumberFormat.CurrencyDecimalDigits = 5;


            _blinker = new DispatcherTimer();
            _blinker.Tick += _blinker_Tick;
            _blinker.Interval = TimeSpan.FromSeconds(1);
            _blinker.Start();
        }

        private void _blinker_Tick(object sender, EventArgs e)
        {
            if (_defaultBorderColor.A == 0)
                _defaultBorderColor = BorderColor;
            if (!Enabled)
            {
                if (BlinkBorderColor != BorderColor)
                    BlinkBorderColor = BorderColor;
                else
                    BlinkBorderColor = Colors.Lime;
            }
            else
                BlinkBorderColor = BorderColor;
        }

        private void _executionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ExecutionThread((State)e.Argument);
        }

        private void _activeStateUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            lock (_locker)
            {

                var stateToCheck = ((State[])e.Argument)[0];
                var currentState = ((State[])e.Argument)[1];
                var result = true;

                var cnt = 0;
                for (int i = 0; i < _commands.Count; i++)
                {
                    var item = _commands[i];
                    if (stateToCheck.ChangedInputs.Contains(item.InputKey))
                        cnt++;
                }
                if (cnt == 0)
                {
                    e.Result = Active;
                    return;
                }


                HasScriptErrors = false;
                Stack<bool> conds = new Stack<bool>();
                for (int i = 0; i < _commands.Count; i++)
                {
                    var item = _commands[i];

                    if (conds.Count == 0 || conds.Peek())
                    {
                        if (currentState == null) return;

                        var input = currentState.Inputs.Where(x => x.Key == item.InputKey).FirstOrDefault()?.Number;
                        if (string.IsNullOrWhiteSpace(item.Action.ActiveStatePath)) continue;
                        var path = string.Format(item.Action.ActiveStatePath, item.InputKey, CalculateExpression<int>(item.Parameter), item.StringParameter, CalculateExpression<int>(item.Parameter) - 1, input.HasValue ? input.Value : -1);
                        var nval = GetValueByPath(stateToCheck, path);
                        var val = nval == null ? "" : nval.ToString();
                        HasScriptErrors = HasScriptErrors || nval == null;
                        var aval = string.Format(item.Action.ActiveStateValue, GetInputNumber(item.InputKey, stateToCheck), CalculateExpression<int>(item.Parameter), item.StringParameter, CalculateExpression<int>(item.Parameter) - 1, input.HasValue ? input.Value : -1);
                        var realval = aval;
                        aval = aval.TrimStart('!');
                        bool mult = (aval == "-" && ((val is string && string.IsNullOrWhiteSpace((string)val)) || (val == null))) ||
                            (aval == "*") ||
                            (val != null && !(val is string) && aval == val.ToString()) ||
                            (val is string && (string)val == aval);
                        if (!string.IsNullOrWhiteSpace(aval) && aval[0] == '!')
                            mult = !mult;
                        result = result && mult;
                    }

                }
                e.Result = result;
            }
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            Active = (bool)(e.Result ?? false);

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

        private string GetInputNumber(string key, vMixAPI.State state = null)
        {
            try
            {
                return (state ?? State).Inputs.Where(x => x.Key == key).FirstOrDefault().Number.ToString();
            }
            catch (Exception)
            {
                return "-1";
            }
        }

        private void PopulateVariables(NCalc.Expression exp)
        {
            foreach (var item in _variables)
            {
                var x = Dispatcher.Invoke(() => new { A = item.A, B = item.B });
                exp.Parameters.Add(string.Format("{0}{1}", VARIABLEPREFIX, x.A), x.B);
            }
            exp.EvaluateFunction += Exp_EvaluateFunction;
        }

        private void Exp_EvaluateFunction(string name, NCalc.FunctionArgs args)
        {
            var p = args.EvaluateParameters();
            args.HasResult = false;
            switch (name)
            {
                case "_":
                    if (p.Length > 0)
                    {
                        args.HasResult = true;
                        if (_isStateDependent && _internalState != null)
                            args.Result = GetValueByPath(_internalState, p[0].ToString());
                        else
                            args.Result = Dispatcher.Invoke<object>(() => GetValueByPath(State, p[0].ToString()));
                    }
                    break;
                //string functions
                case "split":
                    if (p.Length > 1 && p[0] is string && p[1] is string)
                    {
                        args.HasResult = true;
                        args.Result = ((string)p[0]).Split(new string[] { (string)p[1] }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    break;
                case "trim":
                    if (p.Length > 0 && p[0] is string)
                    {
                        args.HasResult = true;
                        args.Result = ((string)p[0]).Trim();
                    }
                    break;
                //vMix functions
                case "API":
                    //state.SendFunction(string.Format(cmd.Action.FormatString, cmd.InputKey, CalculateExpression<int>(cmd.Parameter), Dispatcher.Invoke(() => CalculateObjectParameter(cmd)), CalculateExpression<int>(cmd.Parameter) - 1, input.HasValue ? input.Value : 0), false);
                    break;
                //array functions
                case "getvalue":
                    if (p.Length > 1 && p[0] is Array && p[1] is int)
                    {
                        args.HasResult = true;
                        args.Result = ((Array)p[0]).GetValue((int)p[1]);
                    }
                    break;
            }
        }

        private bool Calculate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            NCalc.Expression exp = new NCalc.Expression(s);
            PopulateVariables(exp);
            if (exp.HasErrors())
                return false;
            else
                try
                {
                    return (bool)exp.Evaluate();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Calculating expression failde");
                    return false;
                }
        }

        private object CalculateExpression(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            NCalc.Expression exp = new NCalc.Expression(s);
            PopulateVariables(exp);
            if (exp.HasErrors())
                return s;
            else
                try
                {
                    return exp.Evaluate();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Calculating expression failde");
                    return s;
                }
        }

        private T CalculateExpression<T>(string s)
        {
            var result = CalculateExpression(s);
            if (result is T)
                return (T)result;
            return default(T);
        }

        private object EscapeString(object o)
        {
            if (o is string)
            {
                float val = 0;
                if (!float.TryParse((string)o, out val))
                    return string.Format("'{0}'", o);
                return
                    val;
            }
            return o;
        }

        private bool TestCondition(vMixControlButtonCommand cmd)
        {
            if (cmd.AdditionalParameters == null || cmd.AdditionalParameters.Count == 0)
                return false;
            object part1 = null;
            object part2 = null;

            part1 = string.Format(cmd.AdditionalParameters[1].A, cmd.InputKey, cmd.AdditionalParameters[0].A);
            part2 = string.Format(cmd.AdditionalParameters[3].A, cmd.InputKey, cmd.AdditionalParameters[0].A);

            Thread.CurrentThread.CurrentCulture = _culture;
            Thread.CurrentThread.CurrentUICulture = _culture;
            return Calculate(string.Format("{0}{1}{2}", part1?.ToString() ?? "", cmd.AdditionalParameters[2].A, part2?.ToString() ?? ""));
        }

        private int GetVariableIndex(int number)
        {
            for (int i = 0; i < _variables.Count; i++)
            {
                if (Dispatcher.Invoke(() => _variables[i].A) == number)
                    return i;
            }
            return -1;
        }

        private void ExecutionThread(object _state)
        {
            vMixAPI.State state = (vMixAPI.State)_state;
            Stack<bool?> _conditions = new Stack<bool?>();
            int _waitBeforeUpdate = -1;
            for (int _pointer = 0; _pointer < _commands.Count; _pointer++)
            {
                if (_stopThread) return;
                var cmd = _commands[_pointer];
                var cond = new bool?(true);
                if (_conditions.Count > 0)
                    cond = _conditions.Peek();
                if ((cond.HasValue && cond.Value) ||
                    (cmd.Action.Function == NativeFunctions.CONDITIONEND ||
                     cmd.Action.Function == NativeFunctions.CONDITION ||
                     cmd.Action.Function == NativeFunctions.HASVARIABLE))
                    if (cmd.Action.Native)
                        switch (cmd.Action.Function)
                        {
                            case NativeFunctions.TIMER:
                                Thread.Sleep(CalculateExpression<int>(cmd.Parameter));
                                break;
                            case NativeFunctions.UPDATESTATE:
                                Dispatcher.Invoke(() => state?.UpdateAsync());
                                break;
                            case NativeFunctions.UPDATEINTERNALBUTTONSTATE:
                                Dispatcher.Invoke(() => _internalState?.UpdateAsync());
                                break;
                            case NativeFunctions.GOTO:
                                _pointer = CalculateExpression<int>(cmd.Parameter) - 1;
                                break;
                            case NativeFunctions.EXECLINK:
                                Dispatcher.Invoke(() => Messenger.Default.Send<string>(cmd.StringParameter));
                                break;
                            case NativeFunctions.CONDITION:
                                _conditions.Push(cond.HasValue && cond.Value ? new bool?(TestCondition(cmd)) : null);
                                break;
                            case NativeFunctions.HASVARIABLE:
                                _conditions.Push(cond.HasValue && cond.Value ? new bool?(GetVariableIndex(CalculateExpression<int>(cmd.Parameter)) != -1) : null);
                                break;
                            case NativeFunctions.CONDITIONEND:
                                _conditions.Pop();
                                break;
                            case NativeFunctions.SETVARIABLE:
                                var idx = GetVariableIndex(CalculateExpression<int>(cmd.Parameter));
                                if (idx == -1)
                                    Dispatcher.Invoke(() => _variables.Add(new Pair<int, object>() { A = CalculateExpression<int>(cmd.Parameter), B = CalculateObjectParameter(cmd) }));
                                else
                                {
                                    Dispatcher.Invoke(() => _variables[idx].B = CalculateObjectParameter(cmd));
                                }
                                break;
                        }
                    else if (state != null)
                    {
                        var input = state.Inputs.Where(x => x.Key == cmd.InputKey).FirstOrDefault()?.Number;

                        if (!cmd.Action.StateDirect)
                            state.SendFunction(string.Format(cmd.Action.FormatString, cmd.InputKey, CalculateExpression<int>(cmd.Parameter), Dispatcher.Invoke(() => CalculateObjectParameter(cmd)), CalculateExpression<int>(cmd.Parameter) - 1, input.HasValue ? input.Value : 0), false);
                        else
                        {
                            var path = string.Format(cmd.Action.ActiveStatePath, cmd.InputKey, CalculateExpression<int>(cmd.Parameter), Dispatcher.Invoke(() => CalculateObjectParameter(cmd)), CalculateExpression<int>(cmd.Parameter) - 1, input.HasValue ? input.Value : 0);
                            var value = cmd.Action.StateValue == "Input" ? (object)cmd.InputKey : (cmd.Action.StateValue == "String" ? Dispatcher.Invoke(() => CalculateObjectParameter(cmd)).ToString() : (object)CalculateExpression<int>(cmd.Parameter));
                            SetValueByPath(state, path, value);
                            while (GetValueByPath(state, path) != value)
                            {
                                Thread.Sleep(50);
                                if (_stopThread)
                                    return;
                            }
                        }
                        _waitBeforeUpdate = Math.Max(_internalState.Transitions[cmd.Action.TransitionNumber].Duration, _waitBeforeUpdate);
                    }
            }
            _conditions.Clear();
            ThreadPool.QueueUserWorkItem((x) =>
            {
                Enabled = true;
                Thread.Sleep(_waitBeforeUpdate);
                _waitBeforeUpdate = -1;
                Dispatcher.Invoke(() => _internalState.UpdateAsync());
            });
        }

        private object CalculateObjectParameter(vMixControlButtonCommand cmd)
        {
            return CalculateExpression(string.Format(cmd.StringParameter, cmd.InputKey)?.ToString() ?? "");
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
            _blinker.Stop();
            base.SetProperties(viewModel);
            //_defaultBorderColor = BorderColor;
            _blinker.Start();
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
                _blinker.Stop();
                if (_internalState != null)
                    _internalState.OnStateUpdated -= State_OnStateUpdated;
                _stopThread = true;
                
                if (_executionWorker != null && _executionWorker.IsBusy)
                {
                    _executionWorker.CancelAsync();
                    while (_executionWorker.CancellationPending)
                        Thread.Sleep(100);
                    _executionWorker.Dispose();
                }

                _activeStateUpdateWorker.Dispose();

                base.Dispose(managed);
                GC.SuppressFinalize(this);
            }
        }
    }
}
