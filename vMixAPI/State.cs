using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
    public class StateUpdatedEventArgs : EventArgs
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

        public static event EventHandler OnStateCreated;
        public static event EventHandler<StateUpdatedEventArgs> OnStateUpdated;

        static StateFabrique()
        {
            _base.OnStateCreated += _base_OnStateCreated;
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

        private Stack<Func<EventArgs, bool>> _downloadHandlers = new Stack<Func<EventArgs, bool>>();

        //private static bool _configured = false;
        private string _ip = "127.0.0.1";
        private string _port = "8088";

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
                    doc.LoadXml(textstate.
                        Replace(">False<", ">false<").
                        Replace(">True<", ">true<").
                        Replace("\"False\"", "\"false\"").
                        Replace("\"True\"", "\"true\""));
                    doc.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var state = (State)s.Deserialize(ms);
                    IsInitializing = false;
                    if (state != null)
                        _logger.Info("vMix state created.");
                    else
                        _logger.Error("vMix state not created.");

                    foreach (var item in state.Inputs)
                        item.ControlledState = state;
                    foreach (var item in state.Overlays)
                        item.ControlledState = state;

                    if (OnStateCreated != null)
                        OnStateCreated(state, null);
                    state.Configure(_ip, _port);
                    return state;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "vMix state not created.");
                return null;
            }

        }

        public State Create()
        {
            return Create(SendFunction("", false));
        }

        public void CreateAsync()
        {
            SendFunction("", true);
            _downloadHandlers.Push(x =>
            {
                var e = (DownloadStringCompletedEventArgs)x;

                if (e.Error != null) return true;

                var state = Create(e.Result);
                if (OnStateCreated != null)
                    OnStateCreated(state, null);

                return true;
            });
        }

        public event EventHandler OnStateCreated;
        public event EventHandler<FunctionSendArgs> OnFunctionSend;
        public event EventHandler<StateUpdatedEventArgs> OnStateUpdated;

        private void Diff(object a, object b, bool lists = false)
        {
            var properties = a.GetType().GetProperties().Where(x => lists || (x.PropertyType != typeof(List<Input>) && x.PropertyType != typeof(ObservableCollection<InputBase>)));
            foreach (var item in properties)
                if (item.CanWrite)
                    item.GetSetMethod().Invoke(a, new object[] { item.GetValue(b) });
        }

        public bool Update()
        {
            IsInitializing = true;
            _logger.Info("Updating vMix state.");
            var _temp = Create();

            int[] oldInputs = Inputs.Select(y => y.ID).ToArray();
            int[] newInputs = _temp.Inputs.Select(y => y.ID).ToArray();

            if (_temp == null)
            {
                _logger.Info("vMix is offline");
                _logger.Info("Firing \"updated\" event.");

                if (OnStateUpdated != null)
                    OnStateUpdated(this, new StateUpdatedEventArgs() { Successfully = false });
                IsInitializing = false;
                return false;
            }

            _logger.Info("Calculating difference.");
            Diff(this, _temp);

            _logger.Info("Updating inputs.");

            Inputs.Clear();
            foreach (var item in _temp.Inputs)
                Inputs.Add(item);


            _logger.Info("Firing \"updated\" event.");

            if (OnStateUpdated != null)
                OnStateUpdated(this, new StateUpdatedEventArgs() { Successfully = true, OldInputs = oldInputs, NewInputs = newInputs });
            IsInitializing = false;
            return true;
        }

        public void UpdateAsync()
        {
            SendFunction("", true);
            _downloadHandlers.Push(x =>
            {
                var e = (DownloadStringCompletedEventArgs)x;

                if (e.Error != null)
                {
                    if (OnStateUpdated != null)
                        OnStateUpdated(this, new StateUpdatedEventArgs() { Successfully = false });
                    return true;
                }
                if (e.UserState == null)
                    return false;
                IsInitializing = true;
                _logger.Info("Updating vMix state.");
                var _temp = Create(e.Result);

                int[] oldInputs = Inputs.Select(y => y.ID).ToArray();
                int[] newInputs = _temp != null ? _temp.Inputs.Select(y => y.ID).ToArray() : new int[0];

                if (_temp == null)
                {
                    _logger.Info("vMix is offline");
                    _logger.Info("Firing \"updated\" event.");

                    IsInitializing = false;
                    if (OnStateUpdated != null)
                        OnStateUpdated(this, new StateUpdatedEventArgs() { Successfully = false });
                }

                _logger.Info("Calculating difference.");
                Diff(this, _temp);

                _logger.Info("Updating inputs.");

                Inputs.Clear();
                foreach (var item in _temp.Inputs)
                    Inputs.Add(item);


                _logger.Info("Firing \"updated\" event.");

                IsInitializing = false;
                if (OnStateUpdated != null)
                    OnStateUpdated(this, new StateUpdatedEventArgs() { Successfully = true, OldInputs = oldInputs, NewInputs = newInputs });
                return true;
            });
        }

        private void State_OnStateCreated(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        public string SendFunction(string textParameters, bool async = true)
        {
            _logger.Info("Trying to send function <{0}> in {1} mode.", textParameters, async ? "async" : "sync");


            OnFunctionSend?.Invoke(this, new FunctionSendArgs() { Function = textParameters });

            string address = string.Format("http://{0}:{1}/api?", _ip, _port);
            var url = address + textParameters;

            _logger.Info("Function URL is <{0}>.", url);

            if (async)
            {
                try
                {
                    WebClient _webClient = new vMixWebClient();
                    _webClient.DownloadStringCompleted += _webClient_DownloadStringCompleted;
                    var s = string.IsNullOrEmpty(textParameters) ? "state" : null;
                    _webClient.DownloadStringAsync(new Uri(url), s);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error while sending async function");
                }
            }
            else
            {
                try
                {
                    WebClient _webClient = new vMixWebClient();
                    _webClient.DownloadStringCompleted += _webClient_DownloadStringCompleted;
                    return _webClient.DownloadString(url);
                }
                catch (WebException)
                {
                }
            }
            return null;
        }

        private void _webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {

            if (e.Error != null)
                _logger.Error(e.Error, "Error while sending async function.");
            else
                _logger.Info("Async function sended, result is \"{0}\".", e.Result);

            Stack<Func<EventArgs, bool>> notCompleted = new Stack<Func<EventArgs, bool>>();
            Func<EventArgs, bool> func = null;
            while (_downloadHandlers.Count > 0)
                if (!(func = _downloadHandlers.Pop())(e))
                    notCompleted.Push(func);

            _downloadHandlers = notCompleted;
            //_downloadHandlers.Pop()(e);
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
            OnStateCreated += State_OnStateCreated;
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
            switch (e.Property.Name)
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
            }
        }


        [XmlElement(ElementName = "recording")]
        public bool Recording { get; set; }
        [XmlElement(ElementName = "external")]
        public bool External { get; set; }
        [XmlElement(ElementName = "streaming")]
        public bool Streaming { get; set; }
        [XmlElement(ElementName = "playList")]
        public bool PlayList { get; set; }
        [XmlElement(ElementName = "multiCoder")]
        public bool MultiCoder { get; set; }

        [XmlArray("inputs"), XmlArrayItem(ElementName = "input")]
        public List<Input> Inputs { get; set; }
        [XmlArray("overlays"), XmlArrayItem(ElementName = "overlay")]
        public List<Overlay> Overlays { get; set; }
        [XmlArray("transitions"), XmlArrayItem(ElementName = "transition")]
        public List<Transition> Transitions { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;
        //public event EventHandler Updated;
        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
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
            state.SendFunction(
                "Function", "Audio",
                "Input", input.ToString());
        }

        public static void PreviewInput(this State state, int input)
        {
            state.SendFunction(
                "Function", "PreviewInput",
                "Input", input.ToString());
        }

        public static void ActiveInput(this State state, int input)
        {
            state.SendFunction(
                "Function", "ActiveInput",
                "Input", input.ToString());
        }

        public static void FadeToBlack(this State state)
        {
            state.SendFunction(
                "Function", "FadeToBlack");
        }

        public static void TitleSetText(this State state, string input, string value, int index)
        {
            state.SendFunction("Function", "SetText",
                "Value", value,
                "Input", input.ToString(),
                "SelectedIndex", index.ToString());
        }

        public static void TitleSetImage(this State state, string input, string value, int index)
        {
            state.SendFunction("Function", "SetImage",
                "Value", value,
                "Input", input.ToString(),
                "SelectedIndex", index.ToString());
        }

        public static void InputSelectIndex(this State state, string input, int index)
        {
            state.SendFunction("Function", "SelectIndex",
                "Input", input,
                "Value", index.ToString());
        }

        public static void InputSetPosition(this State state, int input, int milliseconds)
        {
            state.SendFunction("Function", "SetPosition",
                "Input", input.ToString(),
                "Value", milliseconds.ToString());
        }

        public static void InputLoopOn(this State state, int input)
        {
            state.SendFunction(
                "Function", "LoopOn",
                "Input", input.ToString());
        }

        public static void InputLoopOff(this State state, int input)
        {
            state.SendFunction(
                "Function", "LoopOff",
                "Input", input.ToString());
        }


        public static void OverlayInputIn(this State state, string input, int overlay)
        {
            state.SendFunction(
                "Function", string.Format("OverlayInput{0}In", overlay),
                "Input", input);
        }

        public static void OverlayInputToggle(this State state, string input, int overlay)
        {
            state.SendFunction(
                "Function", string.Format("OverlayInput{0}", overlay),
                "Input", input);
        }

        public static void OverlayInputOff(this State state, int overlay)
        {
            state.SendFunction(
                "Function", string.Format("OverlayInput{0}Off", overlay));
        }

        public static void OverlayInputOut(this State state, int overlay)
        {
            state.SendFunction(
                "Function", string.Format("OverlayInput{0}Out", overlay));
        }


        public static void SetCountdown(this State state, string input, int index, string duration)
        {
            state.SendFunction("Function", "SetCountdown",
                "Input", input,
                "Value", duration,
                "SelectedIndex", index.ToString());
        }

        public static void StartCountdown(this State state, string input, int index)
        {
            state.SendFunction(
                "Function", "StartCountdown",
                "Input", input,
                "SelectedIndex", index.ToString());
        }

        public static void PauseCountdown(this State state, string input, int index)
        {
            state.SendFunction(
                "Function", "PauseCountdown",
                "Input", input,
                "SelectedIndex", index.ToString());
        }

        public static void StopCountdown(this State state, string input, int index)
        {
            state.SendFunction(
                "Function", "StopCountdown",
                "Input", input,
                "SelectedIndex", index.ToString());
        }
    }

}
