using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace vMixAPI
{
    public class NonConfiguredException : Exception
    {

    }
    public class StateSyncedEventArgs : EventArgs
    {
        public bool Successfully { get; set; }
        public int[] OldInputs { get; set; }
        public int[] NewInputs { get; set; }
    }

    public static class StateFabrique
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private static string _ip = "127.0.0.1";
        private static string _port = "8088";
        private static State _base = new State();
        private static Regex _regex;

        public static event EventHandler OnStateCreated;
        //public static event EventHandler<StateUpdatedEventArgs> OnStateUpdated;

        static StateFabrique()
        {
            _base.OnStateCreated += _base_OnStateCreated;
            _regex = new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$");
        }

        private static void _base_OnStateCreated(object sender, EventArgs e)
        {
            OnStateCreated?.Invoke(sender, e);
        }

        public static void Configure(string ip = "127.0.0.1", string port = "8088")
        {
            _ip = ip;
            _port = port;
            //_configured = true;
            _logger.Info("Configuring fabrique to {0}:{1}.", ip, port);
            _base.Configure(_ip, _port);
        }

        public static string GetUrl()
        {
            return GetUrl(_ip, _port);
        }

        public static bool IsUrlValid(string ip, string port)
        {
            int iport = 0;
            bool isPortOk = int.TryParse(port, out iport);
            isPortOk = isPortOk && iport >= 0 && iport <= 65535;
            var url = GetUrl(ip, port);
            Uri uri;
            return _regex.IsMatch(url) && Uri.TryCreate(url, UriKind.Absolute, out uri) && isPortOk;
        }

        public static string GetUrl(string _ip, string _port)
        {
            return string.Format("http://{0}:{1}/api?", _ip, _port);
        }

        public static void CreateAsync()
        {
            _base.CreateAsync();
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "vmix")]
    public class State : DependencyObject, INotifyPropertyChanged
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        Regex _regex;

        //private static bool _configured = false;
        private string _ip = "127.0.0.1";
        private string _port = "8088";

        private string _currentStateText;
        private List<string> _changedinputs = new List<string>();
        private const int _changedCounterConst = 1; //keep changed inputs for X state updates

        public void Configure(string ip = "127.0.0.1", string port = "8088")
        {
            _ip = ip.Trim();
            _port = port.Trim();

            //_configured = true;
            _logger.Info("Configuring to {0}:{1}.", ip, port);
        }

        /// <summary>
        /// Creates a vMix.State object from its xml representation.
        /// </summary>
        /// <param name="textstate">Xml representation of state object.</param>
        /// <returns></returns>
        public State Create(string textstate)
        {
            if (string.IsNullOrWhiteSpace(textstate))
                return null;
            if (!textstate.StartsWith("<vmix>"))
            {
                _logger.Error("vMix state was not created, got not xml value.");
                return null;
            }

            IsInitializing = true;
            _logger.Info("Creating vMix state form {0}.", textstate);

            try
            {
                XmlSerializer s = new XmlSerializer(typeof(State));
                using (var ms = new MemoryStream())
                {
                    XmlDocument doc = new XmlDocument();
                    if (!textstate.StartsWith("<?xml"))
                        textstate = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + textstate;
                    textstate = textstate.
                        Replace(">False<", ">false<").
                        Replace(">True<", ">true<").
                        Replace("\"False\"", "\"false\"").
                        Replace("\"True\"", "\"true\"");
                    doc.LoadXml(textstate);
                    doc.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var state = (State)s.Deserialize(ms);
                    state._currentStateText = textstate;

                    state._changedinputs.Clear();
                    foreach (var item in state.Inputs)
                        state._changedinputs.Add(item.Key);

                    state.Inputs.Insert(0, new Input() { Key = "0", Title = "[Preview]" });
                    state.Inputs.Insert(0, new Input() { Key = "-1", Title = "[Active]" });



                    IsInitializing = false;
                    if (state != null)
                        _logger.Info("vMix state created.");
                    else
                        _logger.Error("vMix state not created.");

                    foreach (var item in state.Inputs)
                        item.ControlledState = state;
                    foreach (var item in state.Overlays)
                        item.ControlledState = state;

                    OnStateCreated?.Invoke(state, null);
                    state.Configure(_ip, _port);
                    return state;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "vMix state was not created.");
                return null;
            }

        }

        public State Create()
        {
            return Create(SendFunction("", false));
        }

        public void CreateAsync()
        {
            SendFunction("", true, e =>
            {
                if (e.Error != null) return;

                var state = Create(e.Result);
                if (state != null)
                    OnStateCreated?.Invoke(state, null);

                return;
            });
        }

        public event EventHandler OnStateCreated;
        public event EventHandler<FunctionSendArgs> OnFunctionSend;
        public event EventHandler<StateSyncedEventArgs> OnStateSynced;

        /*private void UpdateChangedInputs(string _a, string _b)
        {
            if (_changedCounter++ > _changedCounterConst)
            {
                _changedinputs.Clear();
                _changedCounter = 0;
            }
            Dictionary<string, string> _keys = new Dictionary<string, string>();

            try
            {
                XmlDocument curr = new XmlDocument();
                curr.LoadXml(_b);
                XmlDocument prev = new XmlDocument();
                prev.LoadXml(_a);
                var nodes = prev.SelectNodes(".//inputs/input").OfType<XmlNode>();
                foreach (XmlNode item in nodes)
                {
                    var node = curr.SelectSingleNode(".//inputs/input[@key=\"" + item.Attributes["key"].InnerText + "\"]");
                    if (node == null) continue;
                    if (item.InnerXml.GetHashCode() != node.InnerXml.GetHashCode())
                        _changedinputs.Add(item.Attributes["key"].InnerText);
                    _keys.Add(item.Attributes["number"].InnerText, item.Attributes["key"].InnerText);
                }
                var a = prev.SelectSingleNode(".//overlays");
                var b = curr.SelectSingleNode(".//overlays");
                if (a.InnerXml.GetHashCode() != b.InnerXml.GetHashCode())
                {
                    nodes = prev.SelectNodes(".//overlays/overlay").OfType<XmlNode>().Concat(curr.SelectNodes(".//overlays/overlay").OfType<XmlNode>());
                    foreach (XmlNode item in nodes)
                    {
                        if (item == null) continue;
                        if (string.IsNullOrWhiteSpace(item.InnerText)) continue;
                        if (!_changedinputs.Contains(_keys[item.InnerText]))
                            _changedinputs.Add(_keys[item.InnerText]);
                    }
                }
            }
            catch (Exception) { }
        }*/

        private void Diff(object a, object b, bool lists = false)
        {
            if (a == null || b == null) return;
            var properties = a.GetType().GetProperties().Where(x => lists || (!x.PropertyType.GetInterfaces().Contains(typeof(IList))));
            foreach (var item in properties)
                if (item.CanWrite)
                    item.GetSetMethod().Invoke(a, new object[] { item.GetValue(b) });
        }

        public bool Update()
        {
            try
            {
                IsInitializing = true;
                _logger.Info("Updating vMix state.");
                var _temp = Create();

                if (_temp == null)
                {
                    _logger.Info("vMix is offline");
                    _logger.Info("Firing \"updated\" event.");

                    OnStateSynced?.Invoke(this, new StateSyncedEventArgs() { Successfully = false });
                    IsInitializing = false;
                    return false;
                }

                _logger.Info("Calculating difference.");
                Diff(this, _temp);

                _logger.Info("Updating inputs.");

                Inputs.Clear();
                foreach (var item in _temp.Inputs)
                    Inputs.Add(item);
                Overlays.Clear();
                foreach (var item in _temp.Overlays)
                    Overlays.Add(item);
                Audio.Clear();
                foreach (var item in _temp.Audio)
                    Audio.Add(item);
                Transitions.Clear();
                foreach (var item in _temp.Transitions)
                    Transitions.Add(item);
                Mixes.Clear();
                foreach (var item in _temp.Mixes)
                    Mixes.Add(item);

                //UpdateChangedInputs(_currentStateText, _temp._currentStateText);
                if (_currentStateText != _temp._currentStateText)
                    _currentStateText = _temp._currentStateText;

                _logger.Info("Firing \"updated\" event.");

                OnStateSynced?.Invoke(this, new StateSyncedEventArgs() { Successfully = true, OldInputs = null, NewInputs = null });
                IsInitializing = false;
                return true;
            }
            catch (Exception e)
            {
                IsInitializing = false;
                _logger.Error(e, "Exception at Update");
                return false;
            }
        }

        public void UpdateAsync()
        {
            SendFunction("", true, x =>
            {
                try
                {
                    var e = (DownloadStringCompletedEventArgs)x;

                    if (e.Error != null)
                    {
                        OnStateSynced?.Invoke(this, new StateSyncedEventArgs() { Successfully = false });
                        return;
                    }
                    if (e.UserState == null)
                        return;
                    IsInitializing = true;
                    _logger.Info("Updating vMix state.");
                    var _temp = Create(e.Result);

                    if (_temp == null)
                    {
                        _logger.Info("vMix is offline");
                        _logger.Info("Firing \"updated\" event.");

                        IsInitializing = false;
                        OnStateSynced?.Invoke(this, new StateSyncedEventArgs() { Successfully = false });
                    }

                    _logger.Info("Calculating difference.");
                    Diff(this, _temp);

                    _logger.Info("Updating inputs.");
                    if (Inputs == null)
                        Inputs = new List<Input>();
                    if (Overlays == null)
                        Overlays = new List<Overlay>();
                    if (Audio == null)
                        Audio = new List<Master>();
                    if (Transitions == null)
                        Transitions = new List<Transition>();
                    Inputs.Clear();
                    foreach (var item in _temp.Inputs)
                        Inputs.Add(item);
                    Overlays.Clear();
                    foreach (var item in _temp.Overlays)
                        Overlays.Add(item);
                    Audio.Clear();
                    foreach (var item in _temp.Audio)
                        Audio.Add(item);
                    Transitions.Clear();
                    foreach (var item in _temp.Transitions)
                        Transitions.Add(item);
                    Mixes.Clear();
                    foreach (var item in _temp.Mixes)
                        Mixes.Add(item);


                    if (_currentStateText != _temp._currentStateText)
                        _currentStateText = _temp._currentStateText;

                    _logger.Info("Firing \"updated\" event.");

                    IsInitializing = false;
                    OnStateSynced?.Invoke(this, new StateSyncedEventArgs() { Successfully = true, OldInputs = null, NewInputs = null });
                    return;
                }
                catch (Exception e)
                {
                    IsInitializing = false;
                    _logger.Error(e, "Exception at UpdateAsync");
                }
            });
        }

        public string SendFunction(string textParameters, bool async = true, Action<DownloadStringCompletedEventArgs> handler = null)
        {
            _logger.Info("Trying to send function <{0}> in {1} mode.", textParameters, async ? "async" : "sync");


            OnFunctionSend?.Invoke(this, new FunctionSendArgs() { Function = textParameters });

            string address = string.Format("http://{0}:{1}/api?", _ip, _port);
            var url = address + textParameters;

            if (!StateFabrique.IsUrlValid(_ip, _port))
            {
                _logger.Error("Function URL is not valid <{0}>.", url);
                return "";
            }

            _logger.Info("Function URL is <{0}>.", url);

            if (async)
            {
                WebClient _webClient = new vMixWebClient();
                _webClient.DownloadStringCompleted += _webClient_DownloadStringCompleted;
                _webClient.DownloadStringAsync(new Uri(url), handler);
            }
            else
            {
                WebClient _webClient = new vMixWebClient();
                _webClient.DownloadStringCompleted += (sender, e) =>
                {
                    if (e.Error != null)
                        _logger.Error(e.Error, "Error while sending async function.");
                    else
                        _logger.Info("Async function sended, result is \"{0}\".", e.Result);

                    handler?.Invoke(e);

                    (sender as WebClient).Dispose();
                };
                try
                {
                    return _webClient.DownloadString(url);
                }
                catch (WebException ex)
                {
                    string result = "";
                    if (ex.Response != null)
                        using (var sr = new StreamReader(ex.Response.GetResponseStream()))
                            result = sr.ReadToEnd();
                    _logger.Error(ex, string.Format("Function calling error, result is {0}.", result));
                    return result;
                }
            }
            return null;
        }

        private void _webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {

            if (e.Error != null)
            {
                string result = "";
                if (e.Error is WebException && (e.Error as WebException).Response != null)
                {
                    using (StreamReader sr = new StreamReader((e.Error as WebException).Response.GetResponseStream()))
                        result = sr.ReadToEnd();
                }
                _logger.Error(e.Error, string.Format("Error while sending async function with result {0}.", result));
            }
            else
                _logger.Info("Async function sended, result is \"{0}\".", e.Result);

            ((Action<DownloadStringCompletedEventArgs>)e.UserState)?.Invoke(e);

            (sender as WebClient).Dispose();
        }

        public string SendFunction(Dictionary<string, string> parameters = null)
        {

            string address = string.Format("http://{0}:{1}/api?", _ip, _port);
            string textParameters = "";
            if (parameters != null)
                textParameters = parameters.Select(x => x.Key + "=" + System.Web.HttpUtility.UrlEncode(x.Value)).Aggregate((x, y) => x + "&" + y);
            return SendFunction(textParameters);
        }

        public string SendFunction(params string[] pairs)
        {
            var parameters = new Dictionary<string, string>();
            for (int i = 0; i < pairs.Length; i += 2)
                parameters.Add(pairs[i], pairs[i + 1]);
            return SendFunction(parameters);
        }

        public string GetUrl()
        {
            return string.Format("http://{0}:{1}/api?", _ip, _port);
        }

        public State()
        {
            _regex = new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?$");
        }

        [XmlElement(ElementName = "version")]
        public string Version { get; set; }

        [XmlElement(ElementName = "preview")]
        public int Preview
        {
            get { return (int)GetValue(PreviewProperty); }
            set { SetValue(PreviewProperty, value); RaisePropertyChanged("Preview"); }
        }

        // Using a DependencyProperty as the backing store for Preview.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreviewProperty =
            DependencyProperty.Register("Preview", typeof(int), typeof(State), new PropertyMetadata(0, InternalPropertyChanged));



        [XmlElement(ElementName = "active")]
        public int Active
        {
            get { return (int)GetValue(ActiveProperty); }
            set { SetValue(ActiveProperty, value); RaisePropertyChanged("Active"); }
        }

        // Using a DependencyProperty as the backing store for Active.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveProperty =
            DependencyProperty.Register("Active", typeof(int), typeof(State), new PropertyMetadata(0, InternalPropertyChanged));



        [XmlElement(ElementName = "fadeToBlack")]
        public bool FadeToBlack
        {
            get { return (bool)GetValue(FadeToBlackProperty); }
            set { SetValue(FadeToBlackProperty, value); RaisePropertyChanged("FadeToBlack"); }
        }

        // Using a DependencyProperty as the backing store for FadeToBlack.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FadeToBlackProperty =
            DependencyProperty.Register("FadeToBlack", typeof(bool), typeof(State), new PropertyMetadata(false, InternalPropertyChanged));




        private static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (State.IsInitializing) return;
            /*switch (e.Property.Name)
            {
                case "Preview":
                    (d as State).PreviewInput((int)e.NewValue);
                    break;
                case "Active":
                    (d as State).ActiveInput((int)e.NewValue);
                    break;
                case "FadeToBlack":
                    if (e.NewValue != e.OldValue)
                        (d as State).FadeToBlack();
                    break;
            }*/
        }


        [XmlElement(ElementName = "recording")]
        public Recording Recording { get; set; }
        [XmlElement(ElementName = "external")]
        public bool External { get; set; }
        [XmlElement(ElementName = "streaming")]
        public bool Streaming { get; set; }
        [XmlElement(ElementName = "playList")]
        public bool PlayList { get; set; }
        [XmlElement(ElementName = "multiCorder")]
        public bool MultiCorder { get; set; }

        [XmlArray("inputs"), XmlArrayItem(ElementName = "input")]
        public List<Input> Inputs { get; set; }
        [XmlArray("overlays"), XmlArrayItem(ElementName = "overlay")]
        public List<Overlay> Overlays { get; set; }
        [XmlArray("transitions"), XmlArrayItem(ElementName = "transition")]
        public List<Transition> Transitions { get; set; }
        [XmlArray("audio"),
            XmlArrayItem("master", typeof(Master)),
            XmlArrayItem("busA", typeof(BusA)),
            XmlArrayItem("busB", typeof(BusB)),
            XmlArrayItem("busC", typeof(BusC)),
            XmlArrayItem("busD", typeof(BusD)),
            XmlArrayItem("busE", typeof(BusE)),
            XmlArrayItem("busF", typeof(BusF)),
            XmlArrayItem("busG", typeof(BusG))]
        public List<Master> Audio { get; set; }
        [XmlElement("mix", typeof(Mix))]
        public List<Mix> Mixes { get; set; }

        public static bool IsInitializing { get; set; }

        public string Ip
        {
            get
            {
                return _ip;
            }

            set
            {
                _ip = value;
            }
        }

        public string Port
        {
            get
            {
                return _port;
            }

            set
            {
                _port = value;
            }
        }

        public List<string> ChangedInputs
        {
            get
            {
                return _changedinputs;
            }

            set
            {
                _changedinputs = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        //public event EventHandler Updated;
        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }

    public class FunctionSendArgs : EventArgs
    {
        public string Function { get; set; }
    }

    public static class StateExtensions
    {
        public static void Audio(this State state, int input)
        {
            state?.SendFunction(
                "Function", "Audio",
                "Input", input.ToString());
        }

        public static void PreviewInput(this State state, int input)
        {
            state?.SendFunction(
                "Function", "PreviewInput",
                "Input", input.ToString());
        }

        public static void ActiveInput(this State state, int input)
        {
            state?.SendFunction(
                "Function", "ActiveInput",
                "Input", input.ToString());
        }

        public static void FadeToBlack(this State state)
        {
            state?.SendFunction(
                "Function", "FadeToBlack");
        }

        public static void TitleSetText(this State state, string input, string value, int index)
        {
            state?.SendFunction("Function", "SetText",
                "Value", value,
                "Input", input.ToString(),
                "SelectedIndex", index.ToString());
        }

        public static void TitleSetImage(this State state, string input, string value, int index)
        {
            state?.SendFunction("Function", "SetImage",
                "Value", value,
                "Input", input.ToString(),
                "SelectedIndex", index.ToString());
        }

        public static void InputSelectIndex(this State state, string input, int index)
        {
            state?.SendFunction("Function", "SelectIndex",
                "Input", input,
                "Value", index.ToString());
        }

        public static void InputSetPosition(this State state, int input, int milliseconds)
        {
            state?.SendFunction("Function", "SetPosition",
                "Input", input.ToString(),
                "Value", milliseconds.ToString());
        }

        public static void InputLoopOn(this State state, int input)
        {
            state?.SendFunction(
                "Function", "LoopOn",
                "Input", input.ToString());
        }

        public static void InputLoopOff(this State state, int input)
        {
            state?.SendFunction(
                "Function", "LoopOff",
                "Input", input.ToString());
        }


        public static void OverlayInputIn(this State state, string input, int overlay)
        {
            state?.SendFunction(
                "Function", string.Format("OverlayInput{0}In", overlay),
                "Input", input);
        }

        public static void OverlayInputToggle(this State state, string input, int overlay)
        {
            state?.SendFunction(
                "Function", string.Format("OverlayInput{0}", overlay),
                "Input", input);
        }

        public static void OverlayInputOff(this State state, int overlay)
        {
            state?.SendFunction(
                "Function", string.Format("OverlayInput{0}Off", overlay));
        }

        public static void OverlayInputOut(this State state, int overlay)
        {
            state?.SendFunction(
                "Function", string.Format("OverlayInput{0}Out", overlay));
        }


        public static void SetCountdown(this State state, string input, int index, string duration)
        {
            state?.SendFunction("Function", "SetCountdown",
                "Input", input,
                "Value", duration,
                "SelectedIndex", index.ToString());
        }

        public static void StartCountdown(this State state, string input, int index)
        {
            state?.SendFunction(
                "Function", "StartCountdown",
                "Input", input,
                "SelectedIndex", index.ToString());
        }

        public static void PauseCountdown(this State state, string input, int index)
        {
            state?.SendFunction(
                "Function", "PauseCountdown",
                "Input", input,
                "SelectedIndex", index.ToString());
        }

        public static void StopCountdown(this State state, string input, int index)
        {
            state?.SendFunction(
                "Function", "StopCountdown",
                "Input", input,
                "SelectedIndex", index.ToString());
        }
    }

}
