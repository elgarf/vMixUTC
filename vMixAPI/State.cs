using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
        private static string _login = "admin";
        private static string _password = "";
        private static State _base = new State();

        public static event EventHandler OnStateCreated;
        //public static event EventHandler<StateUpdatedEventArgs> OnStateUpdated;

        static StateFabrique()
        {
            _base.OnStateCreated += _base_OnStateCreated;
        }

        private static void _base_OnStateCreated(object sender, EventArgs e)
        {
            OnStateCreated?.Invoke(sender, e);
        }

        public static void Configure(string ip = "127.0.0.1", string port = "8088", string login = "admin", string password = "")
        {
            _ip = ip;
            _port = port;
            _login = login;
            _password = password;
            //_configured = true;
            _logger.Info("Configuring fabrique to {0}:{1}.", ip, port);
            _base.Configure(_ip, _port, _login, _password);
        }

        public static string GetUrl()
        {
            return GetUrl(_ip, _port);
        }

        public static bool IsUrlValid(string ip, string port)
        {
            bool isPortOk = int.TryParse(port, out int iport) && iport >= 0 && iport <= 65535;
            if (!isPortOk)
                return false;

            var url = GetUrl(ip, port);
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        public static string GetUrl(string _ip, string _port)
        {
            return string.Format("http://{0}:{1}/api?", _ip, _port);
        }

        public static string GetCredentials(string _login, string _password)
        {
            return string.Format("{0}:{1}", _login, _password);
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

        //private static bool _configured = false;
        private string _ip = "127.0.0.1";
        private string _port = "8088";
        private string _login = "admin";
        private string _password = "";

        private string _currentStateText;
        private List<string> _changedinputs = new List<string>();
        private const int _changedCounterConst = 1;

        public void Configure(string ip = "127.0.0.1", string port = "8088", string login = "admin", string password = "")
        {
            _ip = ip.Trim();
            _port = port.Trim();
            _login = login.Trim();
            _password = password.Trim();

            //_configured = true;
            _logger.Debug("Configuring to {0}:{1}.", ip, port);
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
            //_logger.Info(textstate);
            _logger.WithProperty("APIReturn", textstate);
            //_logger.Info("Creating vMix state form {0}.", textstate);

            try
            {
                XmlSerializer s = new XmlSerializer(typeof(State));
                if (!textstate.StartsWith("<?xml"))
                    textstate = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + textstate;
                textstate = textstate.
                    Replace(">False<", ">false<").
                    Replace(">True<", ">true<").
                    Replace("\"False\"", "\"false\"").
                    Replace("\"True\"", "\"true\"");

                var settings = new XmlReaderSettings()
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                };

                using (var reader = new StringReader(textstate))
                using (var xmlReader = XmlReader.Create(reader, settings))
                {
                    var state = (State)s.Deserialize(xmlReader);
                    state._currentStateText = textstate;

                    state.Password = Password;
                    state.Login = Login;
                    state.Ip = Ip;
                    state.Port = Port;

                    state.Inputs = state.Inputs ?? new List<Input>();
                    state.Outputs = state.Outputs ?? new List<Output>();
                    state.Overlays = state.Overlays ?? new List<Overlay>();
                    state.Audio = state.Audio ?? new List<Master>();
                    state.Transitions = state.Transitions ?? new List<Transition>();
                    state.Mixes = state.Mixes ?? new List<Mix>();

                    state._changedinputs.Clear();
                    foreach (var item in state.Inputs)
                        state._changedinputs.Add(item.Key);


                    // Add virtual inputs for referencing current Active/Preview in scripts and UI.
                    // These are resolved to real inputs by `Utils.FindInputKeyByVariable`.
                    state.Inputs.Insert(0, new Input() { Number = 0, Key = "0", Title = "[Preview]" });
                    state.Inputs.Insert(0, new Input() { Number = -1, Key = "-1", Title = "[Active]" });

                    if (state != null)
                        _logger.Debug("vMix state created.");
                    else
                        _logger.Error("vMix state not created.");

                    foreach (var item in state.Inputs)
                        item.ControlledState = state;
                    foreach (var item in state.Overlays)
                        item.ControlledState = state;

                    OnStateCreated?.Invoke(state, null);
                    state.Configure(_ip, _port, _login, _password);
                    return state;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "vMix state was not created.");
                return null;
            }
            finally
            {
                IsInitializing = false;
            }

        }

        public State Create()
        {
            return Create(SendFunction("", false, ignoreCache: true));
        }

        public void CreateAsync()
        {
            SendFunction("", true, (e) =>
            {
                if (string.IsNullOrEmpty(e)) return;

                Create(e);
            }, ignoreCache: true);
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

        private void ApplySnapshot(State snapshot)
        {
            if (snapshot == null)
                return;

            Version = snapshot.Version;
            Preview = snapshot.Preview;
            Active = snapshot.Active;
            FadeToBlack = snapshot.FadeToBlack;
            Recording = snapshot.Recording;
            External = snapshot.External;
            Streaming = snapshot.Streaming;
            PlayList = snapshot.PlayList;
            MultiCorder = snapshot.MultiCorder;
            Ip = snapshot.Ip;
            Port = snapshot.Port;
            Login = snapshot.Login;
            Password = snapshot.Password;
            ChangedInputs = snapshot.ChangedInputs;
        }

        public bool Update()
        {
            try
            {
                IsInitializing = true;
                _logger.Debug("Updating vMix state.");
                var _temp = Create();

                if (_temp == null)
                {
                    _logger.Debug("vMix is offline");
                    _logger.Debug("Firing \"updated\" event.");

                    OnStateSynced?.Invoke(this, new StateSyncedEventArgs() { Successfully = false });
                    IsInitializing = false;
                    return false;
                }

                _logger.Debug("Calculating difference.");
                ApplySnapshot(_temp);

                _logger.Debug("Updating inputs.");

                Inputs = Inputs ?? new List<Input>();
                Outputs = Outputs ?? new List<Output>();
                Overlays = Overlays ?? new List<Overlay>();
                Audio = Audio ?? new List<Master>();
                Transitions = Transitions ?? new List<Transition>();
                Mixes = Mixes ?? new List<Mix>();

                Inputs.Clear();
                foreach (var item in _temp.Inputs)
                    Inputs.Add(item);
                Overlays.Clear();
                foreach (var item in _temp.Overlays)
                    Overlays.Add(item);
                Outputs.Clear();
                foreach (var item in _temp.Outputs)
                    Outputs.Add(item);
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

                _logger.Debug("Firing \"updated\" event.");

                OnStateSynced?.Invoke(this, new StateSyncedEventArgs() { Successfully = true, OldInputs = null, NewInputs = null });
                
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception at Update");
                return false;
            }
            finally
            {
                IsInitializing = false;
            }
        }

        public void UpdateAsync()
        {
            SendFunction("", true, x =>
            {
                Action updateAction = () =>
                {
                    try
                    {
                        //var e = (DownloadStringCompletedEventArgs)x;

                        if (string.IsNullOrEmpty(x))
                        {
                            OnStateSynced?.Invoke(this, new StateSyncedEventArgs() { Successfully = false });
                            return;
                        }
                        /*if (e.UserState == null)
                            return;*/
                        IsInitializing = true;
                        _logger.Debug("Updating vMix state.");
                        var _temp = Create(x);

                        if (_temp == null)
                        {
                            _logger.Debug("vMix is offline");
                            _logger.Debug("Firing \"updated\" event.");

                            IsInitializing = false;
                            OnStateSynced?.Invoke(this, new StateSyncedEventArgs() { Successfully = false });
                            return;
                        }

                        _logger.Debug("Calculating difference.");
                        ApplySnapshot(_temp);

                        _logger.Debug("Updating inputs.");
                        if (Inputs == null)
                            Inputs = new List<Input>();
                        if (Overlays == null)
                            Overlays = new List<Overlay>();
                        if (Audio == null)
                            Audio = new List<Master>();
                        if (Transitions == null)
                            Transitions = new List<Transition>();
                        if (Outputs == null)
                            Outputs = new List<Output>();
                        if (Mixes == null)
                            Mixes = new List<Mix>();
                        Inputs.Clear();
                        foreach (var item in _temp.Inputs)
                            Inputs.Add(item);
                        Outputs.Clear();
                        foreach (var item in _temp.Outputs)
                            Outputs.Add(item);
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

                        _logger.Debug("Firing \"updated\" event.");

                        OnStateSynced?.Invoke(this, new StateSyncedEventArgs() { Successfully = true, OldInputs = null, NewInputs = null });
                        return;
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Exception at UpdateAsync");
                    }
                    finally
                    {
                        IsInitializing = false;
                    }
                };

                if (Dispatcher == null || Dispatcher.CheckAccess())
                    updateAction();
                else
                    Dispatcher.BeginInvoke(updateAction);

            });
        }

        // Hold callback state strongly until async response is received and callback is dispatched.
        private class SendFunctionAPIResponseHandler : IDisposable
        {
            public Action<string> Handler { get; set; }
            public System.Windows.Threading.Dispatcher Dispatcher { get; set; }
            private bool _isDisposed = false;
            public Action<Guid> OnDisposed { get; set; }

            public Guid GUID = Guid.NewGuid();

            public void SendFunctionAPIResponseAction(string response, Exception error)
            {
                if (_isDisposed) return;

                if (Dispatcher == null)
                {
                    Dispose();
                    return;
                }

                Action callbackAction = () =>
                {
                    try
                    {
                        if (_isDisposed) return;

                        if (error != null)
                        {
                            string result = "";
                            if (error is WebException webEx && webEx.Response != null)
                            {
                                try
                                {
                                    using (var stream = webEx.Response.GetResponseStream())
                                        if (stream != null)
                                        {
                                            using (var sr = new StreamReader(stream))
                                                result = sr.ReadToEnd();
                                        }
                                }
                                catch { }
                            }
                            _logger.Error(error, string.Format("Error while sending async function with result {0}.", result));
                        }
                        else
                        {
                            _logger.Debug("Async function sended, result is \"{0}\".", response);
                        }

                        Handler?.Invoke(response);
                    }
                    finally
                    {
                        Dispose();
                    }
                };

                try
                {
                    if (Dispatcher.CheckAccess())
                        callbackAction();
                    else
                        Dispatcher.BeginInvoke(callbackAction);
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            public void Dispose()
            {
                if (_isDisposed) return;

                _isDisposed = true;

                OnDisposed?.Invoke(GUID);

                Handler = null;
                Dispatcher = null;
            }
        }

        ConcurrentDictionary<Guid, SendFunctionAPIResponseHandler> handlers = new ConcurrentDictionary<Guid, SendFunctionAPIResponseHandler>();

        public string SendFunction(string textParameters, bool async = true, Action<string> handler = null, int timeout = 1000, bool ignoreCache = false)
        {
            _logger.Debug("Trying to send function <{0}> in {1} mode.", textParameters, async ? "async" : "sync");


            OnFunctionSend?.Invoke(this, new FunctionSendArgs() { Function = textParameters });

            string address = string.Format("http://{0}:{1}/api?", _ip, _port);
            textParameters = textParameters.Replace("#", "%23");
            var url = address + textParameters;

            if (!StateFabrique.IsUrlValid(_ip, _port))
            {
                _logger.Error("Function URL is not valid <{0}>.", url);
                return "";
            }

            _logger.Debug("Function URL is <{0}>.", url);

            if (async)
            {
                SendFunctionAPIResponseHandler h = new SendFunctionAPIResponseHandler()
                {
                    Handler = handler,
                    Dispatcher = Dispatcher,
                    OnDisposed = (guid) =>
                    handlers.TryRemove(guid, out _)
                };
                handlers.TryAdd(h.GUID, h);
                APIRequestManagerV2.GetApiResponseAsync(url, new WeakAction(h.SendFunctionAPIResponseAction), GetCredentials(), false, ignoreCache);
            }
            else
            {
                try
                {
                    // 1. Вызываем тот же менеджер, что и для асинхронных запросов.
                    //    Колбэк здесь не нужен, так как мы обрабатываем результат прямо на месте.
                    var task = APIRequestManagerV2.GetApiResponseAsync(url, auth: GetCredentials(), ignoreCache: ignoreCache);

                    // 2. Блокируем выполнение и ждем результат.
                    //    GetAwaiter().GetResult() предпочтительнее, чем .Result.
                    string result = task.GetAwaiter().GetResult();

                    _logger.Debug("Sync function sent, result is \"{0}\".", result);

                    // 3. Вызываем обработчик, если он предоставлен.
                    handler?.Invoke(result);

                    // 4. Возвращаем результат.
                    return result;
                }
                catch (Exception ex) // Ловим общее исключение, так как HttpClient может бросать не только WebException
                {
                    string errorBody = "";
                    // Попытка извлечь тело ответа из WebException для обратной совместимости
                    if (ex is WebException webEx && webEx.Response != null)
                    {
                        using (var sr = new StreamReader(webEx.Response.GetResponseStream()))
                            errorBody = sr.ReadToEnd();
                    }

                    _logger.Error(ex, "Sync function calling error, response body is '{0}'.", errorBody);

                    // Возвращаем тело ошибки, если удалось его получить, или пустую строку
                    return errorBody;
                }
            }
            return null;
        }

        /*private void _webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
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
                _logger.Debug("Async function sended, result is \"{0}\".", e.Result);

            ((Action<DownloadStringCompletedEventArgs>)e.UserState)?.Invoke(e);

            //(sender as WebClient).Dispose();
        }*/

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
            if (pairs == null || pairs.Length == 0)
                return SendFunction((Dictionary<string, string>)null);

            if (pairs.Length % 2 != 0)
                throw new ArgumentException("Pairs length must be even: key/value sequence expected.", nameof(pairs));

            var parameters = new Dictionary<string, string>();
            for (int i = 0; i < pairs.Length; i += 2)
                parameters.Add(pairs[i], pairs[i + 1]);
            return SendFunction(parameters);
        }

        public string GetUrl()
        {
            return string.Format("http://{0}:{1}/api?", _ip, _port);
        }

        public string GetCredentials()
        {
            return string.Format("{0}:{1}", _login, _password);
        }

        public State()
        {

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
        [XmlArray("outputs"), XmlArrayItem(ElementName = "output")]
        public List<Output> Outputs { get; set; }
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

        public string Login
        {
            get
            {
                return _login;
            }

            set
            {
                _login = value;
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                _password = value;
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

        public static void TitleSetColor(this State state, string input, string value, int index)
        {
            state?.SendFunction("Function", "SetColor",
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
