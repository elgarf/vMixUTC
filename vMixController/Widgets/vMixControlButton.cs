//#define OBJECTDEPENDENCY
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NCalc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;
using vMixController.Classes.Scripting;
using vMixControllerSkin.Localization;
using vMixController.Messages;
using vMixController.ViewModel;

namespace vMixController.Widgets
{

    [Serializable]
    public class vMixControlButton : vMixControl
    {
        public override bool IsResizeableVertical => true;

        static XmlDocument _latestDocument;

        Regex _isExpression = new Regex(@"([\+|\-])\=(\d+\.?\d*)");
        const string VARIABLEPREFIX = "_var";
        const string parameterName = "P";
        object parameterValue = null;
        [NonSerialized]
        private ScriptExecutionChainToken _executionChainToken = null;
        Stack<bool?> _conditions = new Stack<bool?>();
        [NonSerialized]
        CultureInfo _culture;
        [NonSerialized]
        private CancellationTokenSource _executionCts;
        [NonSerialized]
        private Task _currentExecutionTask;


        [NonSerialized]
        Dictionary<string, string> _trackedValues = new Dictionary<string, string>();

        [NonSerialized]
        DateTime _previousQuery = DateTime.Now;
        [NonSerialized]
        static DateTime _previousInternalStateUpdating = DateTime.Now;
        [NonSerialized]
        int _lastHandledStateRecalcVersion = -1;

        static List<vMixControlButton> _instances = new List<vMixControlButton>();

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
                RaisePropertyChanged(nameof(HasScriptErrors));
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
                base.State = value;
            }
        }

        private string _log = "";

        /// <summary>
        /// Sets and gets the Log property.
        /// Changes to that property's value raise the PropertyChanged event. 
        [XmlIgnore]
        public string Log
        {
            get
            {
                return _log.TrimStart();
            }

            set
            {
                if (_log == value)
                {
                    return;
                }

                _log = value;
                RaisePropertyChanged(nameof(Log));
            }
        }

        private void AddLog(string s, params object[] p)
        {
            Log += "\r\n" + string.Format("[{0}]", DateTime.Now.ToLongTimeString()) + string.Format(s, p);
        }

        private void ClearLog()
        {
            Log = "";
        }


        [NonSerialized]
        private Color _blinkBorderColor = ViewModel.vMixWidgetSettingsViewModel.Colors[0].B;

        /// <summary>
        /// Sets and gets the BorderColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
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
                RaisePropertyChanged(nameof(BlinkBorderColor));
            }
        }

        public override string Type
        {
            get
            {
                return "Button";
            }
        }

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
                RaisePropertyChanged(nameof(Commands));
            }
        }

        private bool _autoStart = false;

        /// <summary>
        /// Sets and gets the AutoStart property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool AutoStart
        {
            get
            {
                return _autoStart;
            }

            set
            {
                if (_autoStart == value)
                {
                    return;
                }

                _autoStart = value;
                RaisePropertyChanged(nameof(AutoStart));
            }
        }

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
                RaisePropertyChanged(nameof(Enabled));
            }
        }

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
                /*if (_active == value)
                {
                    return;
                }*/

                if (Style == Constants.BUTTON_STYLE_MOMENTARY)
                    IsPushed = value;

                _active = value;
                RaisePropertyChanged(nameof(Active));
            }
        }

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
                RaisePropertyChanged(nameof(IsStateDependent));
            }
        }

        private bool _isColorized = false;

        /// <summary>
        /// Sets and gets the IsColorized property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsColorized
        {
            get
            {
                return _isColorized;
            }

            set
            {
                if (_isColorized == value)
                {
                    return;
                }

                _isColorized = value;
                RaisePropertyChanged(nameof(IsColorized));
            }
        }

        private string _image = "";

        /// <summary>
        /// Sets and gets the Image property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Image
        {
            get
            {
                return _image;
            }

            set
            {
                if (_image == value)
                {
                    return;
                }

                _image = value;
                RaisePropertyChanged(nameof(Image));
            }
        }

        private int _imageMax = 1;

        /// <summary>
        /// Sets and gets the ImageMax property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlElement(ElementName = "ImageMax")]
        public int ImageType
        {
            get
            {
                return _imageMax;
            }

            set
            {
                if (_imageMax == value)
                {
                    return;
                }

                _imageMax = value;
                RaisePropertyChanged(nameof(ImageType));
            }
        }

        private int _imageNumber = 0;

        /// <summary>
        /// Sets and gets the ImageNumber property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int ImageNumber
        {
            get
            {
                return _imageNumber;
            }

            set
            {
                if (_imageNumber == value)
                {
                    return;
                }

                _imageNumber = value;
                RaisePropertyChanged(nameof(ImageNumber));
            }
        }


        [NonSerialized]
        private ConcurrentDictionary<int, object> _variables = new ConcurrentDictionary<int, object>();
        [NonSerialized]
        private Dictionary<string, object> _globalVariablesSnapshot = new Dictionary<string, object>(StringComparer.Ordinal);

        /// <summary>
        /// Sets and gets the Variables property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public ConcurrentDictionary<int, object> Variables
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
                RaisePropertyChanged(nameof(Variables));
            }
        }

        private bool _isPushed = false;

        /// <summary>
        /// Sets and gets the IsPushed property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPushed
        {
            get
            {
                return _isPushed;
            }

            set
            {
                if (_isPushed == value)
                {
                    return;
                }

                if (_imageMax == 2 && value)
                    ImageNumber = 1;
                if (!value)
                    ImageNumber = 0;

                _isPushed = value;
                RaisePropertyChanged(nameof(IsPushed));
            }
        }

        private string _style = Constants.BUTTON_STYLE_MOMENTARY;

        /// <summary>
        /// Sets and gets the Style property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Style
        {
            get
            {
                return _style;
            }

            set
            {
                if (_style == value)
                {
                    return;
                }

                _style = value;
                RaisePropertyChanged(nameof(Style));
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
                    ?? (_executeScriptCommand = new RelayCommand(async () => // Make it async
                    {
                        if (Style == Constants.BUTTON_STYLE_MOMENTARY)
                            Enabled = false; // Enabled handles Dispatcher.Invoke

                        // Cancel any existing execution
                        if (_currentExecutionTask != null && !_currentExecutionTask.IsCompleted)
                        {
                            _executionCts?.Cancel();
                            try
                            {
                                await _currentExecutionTask; // Wait for it to finish cancelling
                            }
                            catch (OperationCanceledException)
                            {
                                // Expected exception
                            }
                            catch (Exception)
                            {
                                // Assuming _logger is available. If not, replace with Console.WriteLine or similar.
                                //_logger.Error(ex, "Error while waiting for previous script to cancel.");
                            }
                        }

                        _executionCts = new CancellationTokenSource(); // Create a new CTS for the new execution
                        _currentExecutionTask = ExecuteScriptAsync(State, _executionCts.Token); // Start the new task
                    }));
            }
        }

        private async Task ExecuteScriptAsync(State state, CancellationToken cancellationToken)
        {
            ClearLog();
            var previousChain = ScriptExecutionDispatchRuntime.Push(_executionChainToken ?? ScriptExecutionDispatchRuntime.CurrentChain);
            try
            {
                BlinkBorderColor = Colors.Lime;
                await ExecutionThread((object)state, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                AddLog("Script execution cancelled.");
            }
            catch (Exception ex)
            {
                AddLog($"Script execution error: {ex.Message}");
            }
            finally
            {
                ScriptExecutionDispatchRuntime.Pop(previousChain);
                _executionChainToken = null;
                BlinkBorderColor = BorderColor; // BlinkBorderColor handles Dispatcher.Invoke
                Enabled = true; // Enabled handles Dispatcher.Invoke
                _trackedValues.Clear();
                _conditions.Clear();
                _previousQuery = _previousQuery.AddMilliseconds(-ShadowUpdatePollTime.TotalMilliseconds * 2);
                XmlDocumentMessenger.RequestImmediateUpdateWithForcedStateRecalc();

                _executionCts?.Dispose();
                _executionCts = null;
            }
        }

        [NonSerialized]
        private RelayCommand<object> _executePushOn;

        /// <summary>
        /// Gets the ExecutePushOn.
        /// </summary>
        public RelayCommand<object> ExecutePushOn
        {
            get
            {
                return _executePushOn
                    ?? (_executePushOn = new RelayCommand<object>(
                    (p) =>
                    {
                        //MouseEventArgs

                        switch (Style)
                        {
                            case Constants.BUTTON_STYLE_PRESS:
                                IsPushed = true;
                                ExecuteScriptCommand.Execute(null);
                                break;
                            case Constants.BUTTON_STYLE_MOMENTARY: if (!IsStateDependent) IsPushed = true; break;
                            case Constants.BUTTON_STYLE_TOGGLE:
                                IsPushed = !IsPushed;
                                ExecuteScriptCommand.Execute(null);
                                break;
                        }
                        //p.Handled = true;

                    }));
            }
        }

        [NonSerialized]
        private RelayCommand<object> _executePushOff;

        /// <summary>
        /// Gets the ExecutePushOff.
        /// </summary>
        public RelayCommand<object> ExecutePushOff
        {
            get
            {
                return _executePushOff
                    ?? (_executePushOff = new RelayCommand<object>(
                    (p) =>
                    {
                        Mouse.Capture(null);
                        switch (Style)
                        {
                            case Constants.BUTTON_STYLE_PRESS:
                                IsPushed = false;
                                ExecuteScriptCommand.Execute(null);
                                break;
                            case Constants.BUTTON_STYLE_MOMENTARY:
                                if (!IsStateDependent) IsPushed = false;
                                ExecuteScriptCommand.Execute(null);
                                break;
                            case Constants.BUTTON_STYLE_TOGGLE: break;
                        }
                        //p.Handled = true;

                    }));
            }
        }

        [NonSerialized]
        private RelayCommand _stopScriptCommand;

        private bool _potentialLoopWarning = false;
        /// <summary>
        /// Sets and gets the HasScriptErrors property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public bool PotentialLoopWarning
        {
            get
            {
                return _potentialLoopWarning;
            }

            set
            {
                if (_potentialLoopWarning == value)
                {
                    return;
                }

                _potentialLoopWarning = value;
                RaisePropertyChanged(nameof(PotentialLoopWarning));
            }
        }

        private string _potentialLoopParticipants = string.Empty;
        [XmlIgnore]
        public string PotentialLoopParticipants
        {
            get => _potentialLoopParticipants;
            set
            {
                if (_potentialLoopParticipants == value)
                    return;

                _potentialLoopParticipants = value;
                RaisePropertyChanged(nameof(PotentialLoopParticipants));
            }
        }

        private bool _isPropertiesEditing;

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
                        _executionCts?.Cancel();

                        BlinkBorderColor = BorderColor;

                        _trackedValues.Clear();
                        _conditions.Clear();
                        Enabled = true;
                    }));
            }
        }

        public vMixControlButton()
        {
            _enabled = true;
            _culture = new CultureInfo(CultureInfo.InvariantCulture.Name);
            _culture.NumberFormat.NumberDecimalDigits = 5;
            _culture.NumberFormat.CurrencyDecimalDigits = 5;

            XmlDocumentMessenger.OnDocumentDownloaded += OnXmlDocumentDownloaded;
        }

        private void PopulateVariables(NCalc.SafeExpression exp)
        {
            foreach (var item in _variables)
                exp.Parameters.Add(string.Format("{0}{1}", VARIABLEPREFIX, item.Key), Utils.NormalizeParameterValue(item.Value));

            _globalVariablesSnapshot = CaptureGlobalVariablesSnapshot();

            var globals = _globalVariablesSnapshot;
            if (globals != null)
            {
                foreach (var item in globals)
                {
                    if (!exp.Parameters.ContainsKey(item.Key))
                        exp.Parameters.Add(item.Key, Utils.NormalizeParameterValue(item.Value));
                }
            }
            exp.Parameters.Add(parameterName, Utils.NormalizeParameterValue(parameterValue));
        }

        private Dictionary<string, object> CaptureGlobalVariablesSnapshot()
        {
            /*var snapshot = new Dictionary<string, object>(StringComparer.Ordinal);

            var cachedVariables = ((ViewModelLocator)App.Current.FindResource("Locator"))?.GlobalSettings?.Variables;
            if (cachedVariables == null)
                return snapshot;

            foreach (var item in cachedVariables)
                snapshot[item.A] = item.B;

            return snapshot;*/
            return GlobalVariablesViewModel.GetVariablesSnapshot();
        }

        private void ExpressionEvaluateFunction(string name, FunctionArgs args)
        {
            var p = args.EvaluateParameters();
            //args.HasResult = false;
            switch (name)
            {
                case "_":
                    if (p.Length > 0)
                    {
                        //args.HasResult = true;
                        if (_isStateDependent && _internalState != null)
                            args.Result = GetValueByPath(_internalState, p[0].ToString());
                        else
                            args.Result = Dispatcher.Invoke<object>(() => GetValueByPath(State, p[0].ToString()));
                    }
                    break;
                case "expandvariables":
                    if (p.Length > 0)
                    {
                        //args.HasResult = true;
                        args.Result = Environment.ExpandEnvironmentVariables(p[0].ToString());
                    }
                    break;
                //string functions
                case "split":
                    if (p.Length > 1 && p[0] is string && p[1] is string)
                    {
                        //args.HasResult = true;
                        args.Result = ((string)p[0]).Split(new string[] { (string)p[1] }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    break;
                case "trim":
                    if (p.Length > 0 && p[0] is string)
                    {
                        //args.HasResult = true;
                        args.Result = ((string)p[0]).Trim();
                    }
                    break;
                //vMix functions
                case "xpath":
                    if (_latestDocument != null)
                    {
                        //args.HasResult = true;
                        if (p.Length > 0 && p[0] is string par)
                        {
                            var node = _latestDocument.SelectSingleNode(par);
                            if (node != null)
                                args.Result = node is XmlAttribute ? node.Value : node.InnerText;
                            else
                                args.Result = "XmlNode Not Found!";
                        }
                    }
                    //state.SendFunction(string.Format(cmd.Action.FormatString, cmd.InputKey, CalculateExpression<int>(cmd.Parameter), Dispatcher.Invoke(() => CalculateObjectParameter(cmd)), CalculateExpression<int>(cmd.Parameter) - 1, input.HasValue ? input.Value : 0), false);
                    break;
                //array functions
                case "getvalue":
                    if (p.Length > 1 && p[0] is Array && p[1] is int)
                    {
                        //args.HasResult = true;
                        args.Result = ((Array)p[0]).GetValue((int)p[1]);
                    }
                    break;
            }
        }

        private void OnXmlDocumentDownloaded(XmlDocument doc, DateTime timestamp)
        {
            var stateRecalcVersion = XmlDocumentMessenger.StateRecalcVersion;
            var forceStateRecalc = stateRecalcVersion != _lastHandledStateRecalcVersion;
            if (forceStateRecalc)
                _lastHandledStateRecalcVersion = stateRecalcVersion;

            if (!_isPropertiesEditing && IsStateDependent && (forceStateRecalc || (timestamp - _previousQuery).TotalMilliseconds >= ShadowUpdatePollTime.TotalMilliseconds))
            {
                var eventTimestampTicksUtc = timestamp.ToUniversalTime().Ticks;
                _globalVariablesSnapshot = CaptureGlobalVariablesSnapshot();
                var schedulerKey = string.Format("button-state:{0}", WidgetId);
                UpdateScheduler.ScheduleLatest(schedulerKey, () =>
                {

                    try
                    {
                        _latestDocument = doc;
                        var globals = _globalVariablesSnapshot;
                        var result = _commands.CalculateStateDependency(doc, (inputKey) =>
                        {
                            if (inputKey == null) return "NOINPUT";
                            if (globals != null && globals.TryGetValue(inputKey, out var value) && value != null)
                                return value.ToString();
                            return inputKey;
                        }, PopulateVariables, ExpressionEvaluateFunction);
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (eventTimestampTicksUtc < XmlDocumentMessenger.LastDocumentTimestampTicksUtc)
                                return;
                            Active = result.IsStateDependent;
                            HasScriptErrors = result.HasErrors;
                        }));

                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Error while checking state dependency!");
                    }
                });

                if ((timestamp - _previousInternalStateUpdating).TotalMilliseconds >= ShadowUpdatePollTime.TotalMilliseconds)
                {
                    _internalState?.UpdateAsync();
                    _previousInternalStateUpdating = timestamp;
                }

                _previousQuery = timestamp;
            }
        }

        public override Hotkey[] GetHotkeys()
        {
            return new Classes.Hotkey[] {
                new Classes.Hotkey { Name = "Execute" },
                new Classes.Hotkey { Name = "Reset" },
                new Classes.Hotkey { Name = "Clear Variables" },
                new Classes.Hotkey { Name = "Press" },
                new Classes.Hotkey { Name = "Release" }
            };
        }

        private bool TestCondition(vMixControlButtonCommand cmd)
        {
            if (cmd.AdditionalParameters == null || cmd.AdditionalParameters.Count == 0)
                return false;

            var part1 = string.Format(cmd.AdditionalParameters[1].A, cmd.InputKey, cmd.AdditionalParameters[0].A)?.ToString() ?? "";
            var part2 = string.Format(cmd.AdditionalParameters[3].A, cmd.InputKey, cmd.AdditionalParameters[0].A)?.ToString() ?? "";
            Thread.CurrentThread.CurrentCulture = _culture;
            Thread.CurrentThread.CurrentUICulture = _culture;

            vMixControlButtonHelper.CalculateExpression<object>(part1, PopulateVariables, ExpressionEvaluateFunction, out object expr1);
            vMixControlButtonHelper.CalculateExpression<object>(part2, PopulateVariables, ExpressionEvaluateFunction, out object expr2);

            //put expressions into variables for comparing
            var idx1 = GetVariableIndex(65534);
            var idx2 = GetVariableIndex(65535);

            if (idx1 < 0 || idx2 < 0)
            {
                _variables.AddOrUpdate(65534, expr1, (k, v) => expr1);
                _variables.AddOrUpdate(65535, expr2, (k, v) => expr2);
            }
            else
            {
                _variables.AddOrUpdate(idx1, expr1, (k, v) => expr1);
                _variables.AddOrUpdate(idx2, expr2, (k, v) => expr2);
            }

            string expression = string.Format("{0}{1}{2}", "_var65534", cmd.AdditionalParameters[2].A, "_var65535");
            AddLog("CONDITION CHECK {0}{1}{2}", expr1, cmd.AdditionalParameters[2].A, expr2);
            bool result = false;
            vMixControlButtonHelper.CalculateExpression<bool>(expression, PopulateVariables, ExpressionEvaluateFunction, out result);
            return result;
        }

        private int GetVariableIndex(int number)
        {
            return _variables.ContainsKey(number) ? number : -1;
        }

        private async Task ExecutionThread(object _state, CancellationToken cancellationToken)
        {
            vMixAPI.State state = (vMixAPI.State)_state;
            Stack<bool?> _conditions = new Stack<bool?>();
            int _waitBeforeUpdate = -1;
            int _jumpCount = 0;
            ClearLog();
            DateTime _timerStartedAt = DateTime.Now;
            var inputNumberByKey = state?.Inputs?.GroupBy(x => x.Key)
                .ToDictionary(g => g.Key, g => (int?)g.First().Number)
                ?? new Dictionary<string, int?>();
            // Снимок коллекции команд, чтобы её изменение пользователем во время выполнения
            // не приводило к InvalidOperationException при обращении к ObservableCollection.
            var commands = _commands.ToList();
            for (int _pointer = 0; _pointer < commands.Count; _pointer++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int parameter = 0;
                string strparameter = "";
                bool? conditionResult = null;
                var cmd = commands[_pointer];
                object cachedObjectParameter = null;
                bool cachedObjectParameterSet = false;
                object GetObjectParameter()
                {
                    if (!cachedObjectParameterSet)
                    {
                        cachedObjectParameter = CalculateObjectParameter(cmd);
                        cachedObjectParameterSet = true;
                    }

                    return cachedObjectParameter;
                }

                if (!cmd.IsExecutable) continue;

                var cond = new bool?(true);
                if (_conditions.Count > 0)
                    cond = _conditions.Peek();
                if ((cond.HasValue && cond.Value) ||
                    (cmd.Action.Function == NativeFunctions.CONDITIONEND ||
                     cmd.Action.Function == NativeFunctions.CONDITION ||
                     cmd.Action.Function == NativeFunctions.HASVARIABLE ||
                     cmd.Action.Function == NativeFunctions.ISPRESSED ||
                     cmd.Action.Function == NativeFunctions.ELSE))
                    if (cmd.Action.Native)
                    {
                        switch (cmd.Action.Function)
                        {
                            case NativeFunctions.NEXTPAGE:
                                Messenger.Default.Send(new PageNavigationMessage() { Mode = PageNavigationMode.Next });
                                break;
                            case NativeFunctions.PREVPAGE:
                                Messenger.Default.Send(new PageNavigationMessage() { Mode = PageNavigationMode.Previous });
                                break;
                            case NativeFunctions.SETPAGE:
                                Messenger.Default.Send(new PageNavigationMessage() { Mode = PageNavigationMode.SetIndex, PageIndex = int.Parse(cmd.Parameter) });
                                break;
                            case NativeFunctions.WIN:
                                Process.Start(cmd.StringParameter);
                                break;
                            case NativeFunctions.API:

                                strparameter = string.Format("http://{0}", GetObjectParameter().ToString());
                                Uri uri;
                                if (Uri.TryCreate(strparameter, UriKind.Absolute, out uri))
                                {
                                    await vMixAPI.APIRequestManagerV2.GetApiResponseAsync(strparameter);
                                    AddLog("{1}) API {0}", strparameter, _pointer + 1);
                                }
                                else
                                    AddLog("{1}) API WRONG URL = {0}", strparameter, _pointer + 1);
                                break;
                            case NativeFunctions.API_POST:
                                strparameter = string.Format("http://{0}", GetObjectParameter().ToString());
                                Uri uripost;
                                if (Uri.TryCreate(strparameter, UriKind.Absolute, out uripost))
                                {
                                    await vMixAPI.APIRequestManagerV2.GetApiResponseAsync(strparameter, post: true);
                                    AddLog("{1}) API POST {0}", strparameter, _pointer + 1);
                                }
                                else
                                    AddLog("{1}) API POST WRONG URL = {0}", strparameter, _pointer + 1);
                                break;
                            case NativeFunctions.TIMER:
                                vMixControlButtonHelper.CalculateExpression<int>(cmd.Parameter, PopulateVariables, ExpressionEvaluateFunction, out parameter);
                                AddLog("{2}) TIMER {0} [{1}]", cmd.Parameter, parameter, _pointer + 1);
                                _timerStartedAt = DateTime.Now;
                                await Task.Delay(parameter, cancellationToken);
                                AddLog("{1}) TIMER СOMPLETED IN {0}ms", (DateTime.Now - _timerStartedAt).TotalMilliseconds, _pointer);
                                break;
                            case NativeFunctions.UPDATESTATE:
                            case NativeFunctions.SYNC:
                                AddLog("{0}) STATE UPDATING", _pointer + 1);
                                RunOnUiThread(() => Messenger.Default.Send(new SyncStateRequestMessage() { Force = true }));
                                break;
                            case NativeFunctions.UPDATEINTERNALBUTTONSTATE:
                            case NativeFunctions.SYNCINTERNALBUTTONSTATE:
                                AddLog("{0}) INTERNAL BUTTON STATE UPDATING", _pointer + 1);
                                RunOnUiThread(() => _internalState?.UpdateAsync());
                                break;
                            case NativeFunctions.GOTO:
                                if (_jumpCount >= 5)
                                {
                                    ClearLog();
                                    _jumpCount = 0;
                                }
                                vMixControlButtonHelper.CalculateExpression<int>(cmd.Parameter, PopulateVariables, ExpressionEvaluateFunction, out parameter);
                                AddLog("{2}) GOTO {0} [{1}]", cmd.Parameter, parameter, _pointer + 1);
                                _pointer = parameter - 1;
                                _jumpCount++;
                                break;
                            case NativeFunctions.EXECLINK:
                                strparameter = GetObjectParameter()?.ToString() ?? string.Empty;
                                AddLog("{2}) EXECLINK {0} [{1}]", cmd.StringParameter, strparameter, _pointer + 1);
                                RunOnUiThread(() => Messenger.Default.Send(new HotkeyLinkMessage() { Link = strparameter, Parameter = ScriptExecutionDispatchRuntime.CreateOutgoingParameter(null) }));
                                break;
                            case NativeFunctions.LIVETOGGLE:
                                AddLog("{0}) LIVETOGGLE", _pointer + 1);
                                RunOnUiThread(() => Messenger.Default.Send(new LIVEToggleMessage() { State = 2 }));
                                break;
                            case NativeFunctions.LIVEOFF:
                                AddLog("{0}) LIVEOFF", _pointer + 1);
                                RunOnUiThread(() => Messenger.Default.Send(new LIVEToggleMessage() { State = 0 }));
                                break;
                            case NativeFunctions.LIVEON:
                                AddLog("{0}) LIVEON", _pointer + 1);
                                RunOnUiThread(() => Messenger.Default.Send(new LIVEToggleMessage() { State = 1 }));
                                break;
                            case NativeFunctions.CONDITION:
                                conditionResult = cond.HasValue && cond.Value ? new bool?(TestCondition(cmd)) : null;
                                AddLog("{1}) CONDITION IS {0}", conditionResult, _pointer + 1);
                                _conditions.Push(conditionResult);
                                break;
                            case NativeFunctions.ELSE:
                                AddLog("{0}) ELSE EXECUTED", _pointer + 1);
                                _conditions.Push(!_conditions.Pop());
                                break;
                            case NativeFunctions.HASVARIABLE:
                                vMixControlButtonHelper.CalculateExpression<int>(cmd.Parameter, PopulateVariables, ExpressionEvaluateFunction, out parameter);
                                conditionResult = cond.HasValue && cond.Value ? new bool?(GetVariableIndex(parameter) != -1) : null;
                                AddLog("{2}) HASVARIABLE {0} IS {1}", parameter, conditionResult, _pointer + 1);
                                _conditions.Push(conditionResult);
                                break;
                            case NativeFunctions.ISPRESSED:
                                AddLog("{1}) BUTTON PUSHED = {0}", IsPushed, _pointer + 1);
                                _conditions.Push(IsPushed);
                                break;
                            case NativeFunctions.CONDITIONEND:
                                AddLog("{0}) CONDITIONEND", _pointer + 1);
                                _conditions.Pop();
                                break;
                            case NativeFunctions.SETVARIABLE:

                                vMixControlButtonHelper.CalculateExpression<int>(cmd.Parameter, PopulateVariables, ExpressionEvaluateFunction, out int calc);
                                var idx = GetVariableIndex(calc);
                                var tobj = GetObjectParameter();
                                AddLog("{2}) SETVARIABLE {0} TO {1}", idx, tobj, _pointer + 1);
                                if (idx == -1)
                                {
                                    vMixControlButtonHelper.CalculateExpression<int>(cmd.Parameter, PopulateVariables, ExpressionEvaluateFunction, out var variableValue);
                                    _variables.AddOrUpdate(variableValue, tobj, (k, v) => tobj);
                                }
                                else
                                    _variables.AddOrUpdate(idx, tobj, (k, v) => tobj);
                                break;
                            case NativeFunctions.SETGLOBALVARIABLE:
                                var isgidx = vMixControlButtonHelper.CalculateExpression<int>(cmd.Parameter, PopulateVariables, ExpressionEvaluateFunction, out var gidx);
                                var isgstr = vMixControlButtonHelper.CalculateExpression<string>(cmd.Parameter, PopulateVariables, ExpressionEvaluateFunction, out var gname);
                                var gtobj = GetObjectParameter();
                                AddLog("{2}) SETGLOBALVARIABLE {0} TO {1}", isgidx ? gidx.ToString() : gname, gtobj, _pointer + 1);
                                if (!isgidx)
                                    Messenger.Default.Send(new SetGlobalVariable() { Index = gidx, Value = gtobj.ToString() });
                                else
                                    Messenger.Default.Send(new SetGlobalVariable() { Name = gname, Value = gtobj.ToString() });
                                break;
                            case NativeFunctions.VALUECHANGED:

                                var obj = GetObjectParameter().ToString();
                                var key = (string.Format(cmd.StringParameter, cmd.InputKey));
                                var hasKey = _trackedValues.ContainsKey(key);
                                AddLog("{2}) VALUECHANGED {0} IS {1}", obj, hasKey, _pointer + 1);
                                _conditions.Push(hasKey ? obj != _trackedValues[key] : false);
                                _trackedValues[key] = obj;
                                break;
                        }
                    }
                    else if (state != null)
                    {
                        var key = Utils.FindInputKeyByVariable(cmd.InputKey ?? "NOKEY", Dispatcher);

                        int p1 = 0;
                        float p1f = 0.0f;

                        inputNumberByKey.TryGetValue(key, out var input);

                        vMixControlButtonHelper.CalculateExpression(cmd.Parameter, PopulateVariables, ExpressionEvaluateFunction, out p1);
                        vMixControlButtonHelper.CalculateExpression(cmd.FloatParameter, PopulateVariables, ExpressionEvaluateFunction, out p1f);
                        string p1fs = p1f.ToString(CultureInfo.InvariantCulture);
                        if (cmd.Action.CommaFloatDelimiter)
                            p1fs = p1fs.Replace('.', ',');

                        var objectParameter = GetObjectParameter();
                        var command = string.Format(cmd.Action.FormatString, key, p1, System.Web.HttpUtility.UrlEncode(Convert.ToString(objectParameter, CultureInfo.InvariantCulture)), p1 - 1, input ?? 0, string.IsNullOrWhiteSpace(key) ? "" : "Input=", p1fs);

                        if (!cmd.Action.StateDirect)
                            AddLog("{2}) SEND {0} WITH RESULT {1}", command, state.SendFunction(command, false, timeout: cmd.Action.Timeout), _pointer + 1);
                        else
                        {
                            var path = string.Format(cmd.Action.StatePath, key, p1, objectParameter, p1 - 1, input ?? 0, string.IsNullOrWhiteSpace(key) ? "" : "Input=");
                            object value;
                            switch (cmd.Action.StateValue)
                            {
                                case "Input":
                                    value = (object)key;
                                    break;
                                case "String":
                                    value = objectParameter?.ToString() ?? "";
                                    break;
                                default:
                                    value = p1;
                                    break;
                            }
                            AddLog("{2}) SET {0} TO {1}", path, value, _pointer + 1);

                            //translate (+/-=number) into expression
                            if (value is string strvalue && _isExpression.IsMatch(strvalue))
                            {
                                var expr = _isExpression.Split(strvalue);
                                object p2 = null;
                                vMixControlButtonHelper.CalculateExpression<object>(string.Format("1 * _('{0}') {1} {2}", path, expr[1], expr[2]), PopulateVariables, ExpressionEvaluateFunction, out p2);
                                value = p2?.ToString() ?? "";
                            }

                            SetValueByPath(state, path, value);
                            int flag = 0;
                            while (GetValueByPath(state, path) != value)
                            {
                                await Task.Delay(50, cancellationToken);
                                if (++flag > 10)
                                    break;
                            }
                        }
                        _waitBeforeUpdate = Math.Max((_internalState ?? state).Transitions[cmd.Action.TransitionNumber].Duration, _waitBeforeUpdate);
                    }
                    else
                        AddLog("{0}) {1} IS NOT EXECUTED", _pointer + 1, cmd.Action.Function);
            }
            _conditions.Clear();
            return;
        }

        private object CalculateObjectParameter(vMixControlButtonCommand cmd)
        {
            object result = null;
            vMixControlButtonHelper.CalculateExpression<object>(string.Format(cmd.StringParameter, Utils.FindInputKeyByVariable(cmd.InputKey, Dispatcher))?.ToString() ?? "", PopulateVariables, ExpressionEvaluateFunction, out result);
            return result ?? cmd.StringParameter;
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
                case 3:
                    IsPushed = true;
                    ExecuteScriptCommand.Execute(null);
                    break;
                case 4:
                    IsPushed = false;
                    ExecuteScriptCommand.Execute(null);
                    break;
            }
        }

        public override void ExecuteHotkey(int index, object parameter)
        {
            if (parameter is ScriptExecutionDispatchPayload dispatchPayload)
            {
                _executionChainToken = dispatchPayload.ChainToken;
                parameterValue = dispatchPayload.Payload;
            }
            else
            {
                _executionChainToken = ScriptExecutionDispatchRuntime.CurrentChain;
                parameterValue = parameter;
            }

            base.ExecuteHotkey(index, parameter);
            ExecuteHotkey(index);
        }

        public override void BeforePropertiesChanged()
        {
            _isPropertiesEditing = true;
            base.BeforePropertiesChanged();
        }

        private void CheckScriptLoop()
        {
            ScriptLoopAnalyzer.RefreshPotentialLoopWarnings();
        }

        public override void AfterPropertiesChanged()
        {
            base.AfterPropertiesChanged();

            BlinkBorderColor = BorderColor;

            bool hasGoToOrTimer = false;
            int p;
            int i = 0;
            foreach (var item in Commands)
            {
                if (item == null || item.Action == null) continue;

                hasGoToOrTimer |= item.Action?.Function == NativeFunctions.TIMER;
                hasGoToOrTimer |= item.Action?.Function == NativeFunctions.GOTO && ((int.TryParse(item.Parameter, out p) && p < i) || !int.TryParse(item.Parameter, out p));
                i++;
            }

            if (hasGoToOrTimer && Style != Constants.BUTTON_STYLE_MOMENTARY)
            {
                var d = new Ookii.Dialogs.Wpf.TaskDialog();
                d.WindowTitle = LocalizationManager.Instance["Dialog.Warning.PossibleScriptError.Title"];
                d.MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Warning;
                d.Content = LocalizationManager.Instance["Dialog.Warning.PossibleScriptError.Content"];
                d.Buttons.Add(new Ookii.Dialogs.Wpf.TaskDialogButton(Ookii.Dialogs.Wpf.ButtonType.Ok));
                d.ShowDialog();
            }

            CheckScriptLoop();

            _isPropertiesEditing = false;
        }

        public override void Update()
        {
            base.Update();
            if (AutoStart)
                ExecuteScriptCommand.Execute(null);
            //CheckScriptLoop();
        }

        protected override void Dispose(bool managed)
        {
            if (_disposed) return;
            Messenger.Default.Unregister(this);
            XmlDocumentMessenger.OnDocumentDownloaded -= OnXmlDocumentDownloaded;
            if (managed)
            {
                // Request cancellation and dispose CancellationTokenSource
                _executionCts?.Cancel();
                _executionCts?.Dispose();
                _executionCts = null;

                // Dispose the task if it's still running (though cancellation should handle it)
                if (_currentExecutionTask != null && !_currentExecutionTask.IsCompleted)
                {
                    _ = _currentExecutionTask.ContinueWith(t =>
                    {
                        var ignored = t.Exception;
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }
                _currentExecutionTask = null;

                base.Dispose(managed);
                GC.SuppressFinalize(this);
            }
        }
    }
}
