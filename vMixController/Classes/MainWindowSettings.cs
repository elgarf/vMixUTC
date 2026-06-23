using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using vMixController.Widgets;

namespace vMixController.Classes
{
    [Serializable]
    public class MainWindowSettings : ObservableObject
    {

        public MainWindowSettings()
        {

        }
        /// <summary>
        /// The <see cref="State" /> property's name.
        /// </summary>
        public const string StatePropertyName = "State";

        private WindowState _state = WindowState.Normal;

        /// <summary>
        /// Sets and gets the State property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public WindowState State
        {
            get
            {
                return _state;
            }

            set
            {
                if (_state == value)
                {
                    return;
                }

                _state = value;
                RaisePropertyChanged(StatePropertyName);
            }
        }



        private bool _useInfiniteCanvas = true;

        /// <summary>
        /// Sets and gets the Left property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool UseInfiniteCanvas
        {
            get
            {
                return _useInfiniteCanvas;
            }

            set
            {
                if (_useInfiniteCanvas == value)
                {
                    return;
                }

                _useInfiniteCanvas = value;
                RaisePropertyChanged(nameof(UseInfiniteCanvas));
            }
        }


        private bool _showLinks = false;

        /// <summary>
        /// Sets and gets the Left property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ShowLinks
        {
            get
            {
                return _showLinks;
            }

            set
            {
                if (_showLinks == value)
                {
                    return;
                }

                _showLinks = value;
                RaisePropertyChanged(nameof(ShowLinks));
            }
        }


        private string _linksStyle = Constants.LINKS_NONE;

        /// <summary>
        /// Sets and gets the Left property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string LinksStyle
        {
            get
            {
                return _linksStyle;
            }

            set
            {
                if (_linksStyle == value)
                {
                    return;
                }

                _linksStyle = value;
                RaisePropertyChanged(nameof(LinksStyle));
            }
        }

        private bool _enableLoopGuard = true;

        public bool EnableLoopGuard
        {
            get
            {
                return _enableLoopGuard;
            }

            set
            {
                if (_enableLoopGuard == value)
                {
                    return;
                }

                _enableLoopGuard = value;
                RaisePropertyChanged(nameof(EnableLoopGuard));
            }
        }

        /// <summary>
        /// The <see cref="Left" /> property's name.
        /// </summary>
        public const string LeftPropertyName = "Left";

        private double _left = 128;

        /// <summary>
        /// Sets and gets the Left property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Left
        {
            get
            {
                return _left;
            }

            set
            {
                if (_left == value)
                {
                    return;
                }

                _left = value;
                RaisePropertyChanged(LeftPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Top" /> property's name.
        /// </summary>
        public const string TopPropertyName = "Top";

        private double _top = 128;

        /// <summary>
        /// Sets and gets the Top property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Top
        {
            get
            {
                return _top;
            }

            set
            {
                if (_top == value)
                {
                    return;
                }

                _top = value;
                RaisePropertyChanged(TopPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Width" /> property's name.
        /// </summary>
        public const string WidthPropertyName = "Width";

        private double _width = 512;

        /// <summary>
        /// Sets and gets the Width property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Width
        {
            get
            {
                return _width;
            }

            set
            {
                if (_width == value)
                {
                    return;
                }

                _width = value;
                RaisePropertyChanged(WidthPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="Height" /> property's name.
        /// </summary>
        public const string HeightPropertyName = "Height";

        private double _height = 384;

        /// <summary>
        /// Sets and gets the Height property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double Height
        {
            get
            {
                return _height;
            }

            set
            {
                if (_height == value)
                {
                    return;
                }

                _height = value;
                RaisePropertyChanged(HeightPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IP" /> property's name.
        /// </summary>
        public const string IPPropertyName = "IP";

        private string _ip = "127.0.0.1";

        /// <summary>
        /// Sets and gets the IP property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string IP
        {
            get
            {
                return _ip;
            }

            set
            {
                if (_ip == value)
                {
                    return;
                }

                _ip = value;
                vMixAPI.StateFabrique.Configure(IP, Port, HttpLogin, HttpPassword);
                XmlDocumentMessenger.Url = vMixAPI.StateFabrique.GetUrl(IP, Port);
                XmlDocumentMessenger.Credentials = vMixAPI.StateFabrique.GetCredentials(HttpLogin, HttpPassword);

                RaisePropertyChanged(IPPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Port" /> property's name.
        /// </summary>
        public const string PortPropertyName = "Port";

        private string _port = "8088";

        /// <summary>
        /// Sets and gets the Port property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Port
        {
            get
            {
                return _port;
            }

            set
            {
                if (_port == value)
                {
                    return;
                }

                _port = value;
                vMixAPI.StateFabrique.Configure(IP, Port, HttpLogin, HttpPassword);
                XmlDocumentMessenger.Url = vMixAPI.StateFabrique.GetUrl(IP, Port);
                XmlDocumentMessenger.Credentials = vMixAPI.StateFabrique.GetCredentials(HttpLogin, HttpPassword);

                RaisePropertyChanged(PortPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="HttpLogin" /> property's name.
        /// </summary>
        public const string HttpLoginPropertyName = "HttpLogin";

        private string _httpLogin = "";

        /// <summary>
        /// Sets and gets the HttpLogin property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string HttpLogin
        {
            get
            {
                return _httpLogin;
            }

            set
            {
                if (_httpLogin == value)
                {
                    return;
                }

                _httpLogin = value;
                // Используем новые свойства HttpLogin и HttpPassword для конфигурации
                vMixAPI.StateFabrique.Configure(IP, Port, HttpLogin, HttpPassword);
                XmlDocumentMessenger.Url = vMixAPI.StateFabrique.GetUrl(IP, Port);
                XmlDocumentMessenger.Credentials = vMixAPI.StateFabrique.GetCredentials(HttpLogin, HttpPassword);

                RaisePropertyChanged(HttpLoginPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="HttpPassword" /> property's name.
        /// </summary>
        public const string HttpPasswordPropertyName = "HttpPassword";

        private string _httpPassword = "";

        /// <summary>
        /// Sets and gets the HttpPassword property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string HttpPassword
        {
            get
            {
                return _httpPassword;
            }

            set
            {
                if (_httpPassword == value)
                {
                    return;
                }

                _httpPassword = value;
                // Используем новые свойства HttpLogin и HttpPassword для конфигурации
                vMixAPI.StateFabrique.Configure(IP, Port, HttpLogin, HttpPassword);
                XmlDocumentMessenger.Url = vMixAPI.StateFabrique.GetUrl(IP, Port);
                XmlDocumentMessenger.Credentials = vMixAPI.StateFabrique.GetCredentials(HttpLogin, HttpPassword);

                RaisePropertyChanged(HttpPasswordPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Locked" /> property's name.
        /// </summary>
        public const string LockedPropertyName = "Locked";

        private bool _locked = false;

        /// <summary>
        /// Sets and gets the Locked property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Locked
        {
            get
            {
                return _locked;
            }

            set
            {
                if (_locked == value)
                {
                    return;
                }

                _locked = value;
                RaisePropertyChanged(LockedPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="UIScale" /> property's name.
        /// </summary>
        public const string UIScalePropertyName = "UIScale";

        private double _uiScale = 1.0;

        /// <summary>
        /// Sets and gets the UIScale property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double UIScale
        {
            get
            {
                return _uiScale;
            }

            set
            {
                if (_uiScale == value)
                {
                    return;
                }

                _uiScale = value;
                RaisePropertyChanged(UIScalePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="EnableLog" /> property's name.
        /// </summary>
        public const string EnableLogPropertyName = "EnableLog";

        private bool _enableLog = true;

        /// <summary>
        /// Sets and gets the EnableLog property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool EnableLog
        {
            get
            {
                return _enableLog;
            }

            set
            {
                if (_enableLog == value)
                {
                    return;
                }

                _enableLog = value;
                if (value)
                {
                    if (!NLog.LogManager.IsLoggingEnabled())
                        NLog.LogManager.ResumeLogging();
                }
                else
                {
                    if (NLog.LogManager.IsLoggingEnabled())
                        NLog.LogManager.SuspendLogging();
                }
                RaisePropertyChanged(EnableLogPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="OpenLastAtStart" /> property's name.
        /// </summary>
        public const string OpenLastAtStartPropertyName = "OpenLastAtStart";

        private bool _openLastAtStart = false;

        /// <summary>
        /// Sets and gets the OpenLastAtStart property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool OpenLastAtStart
        {
            get
            {
                return _openLastAtStart;
            }

            set
            {
                if (_openLastAtStart == value)
                {
                    return;
                }

                _openLastAtStart = value;
                RaisePropertyChanged(OpenLastAtStartPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="PollTime" /> property's name.
        /// </summary>
        public const string PollTimePropertyName = "PollTime";

        private int _pollTime = 1000;

        /// <summary>
        /// Sets and gets the PollTime property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int PollTime
        {
            get
            {
                return _pollTime;
            }

            set
            {
                if (_pollTime == value)
                {
                    return;
                }

                _pollTime = value;
                vMixControl.ShadowUpdatePollTime = TimeSpan.FromMilliseconds(value);
                RaisePropertyChanged(PollTimePropertyName);
            }
        }

        private bool _isTopmost = false;

        public bool IsTopmost
        {
            get
            {
                return _isTopmost;
            }

            set
            {
                _isTopmost = value;

                App.Current.MainWindow.Topmost = value;

                RaisePropertyChanged("IsTopmost");
            }
        }

        /// <summary>
        /// The <see cref="ShowIndividualLock" /> property's name.
        /// </summary>
        public const string ShowIndividualLockPropertyName = "ShowIndividualLock";

        private bool _showIndividualLock = true;

        /// <summary>
        /// Sets and gets the ShowIndividualLock property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ShowIndividualLock
        {
            get
            {
                return _showIndividualLock;
            }

            set
            {
                if (_showIndividualLock == value)
                {
                    return;
                }

                _showIndividualLock = value;
                RaisePropertyChanged(ShowIndividualLockPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="BlinkLights" /> property's name.
        /// </summary>
        public const string BlinkLightsPropertyName = "BlinkLights";

        private bool _blinkLights = true;

        /// <summary>
        /// Sets and gets the ShowIndividualLock property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool BlinkLights
        {
            get
            {
                return _blinkLights;
            }

            set
            {
                if (_blinkLights == value)
                {
                    return;
                }

                _blinkLights = value;
                RaisePropertyChanged(BlinkLightsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Password" /> property's name.
        /// </summary>
        public const string PasswordPropertyName = "Password";

        private string _password = null;

        /// <summary>
        /// Sets and gets the Password property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                if (_password == value)
                {
                    return;
                }

                _password = value;
                RaisePropertyChanged(PasswordPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="UserName" /> property's name.
        /// </summary>
        public const string UserNamePropertyName = "UserName";

        private string _userName = "";

        /// <summary>
        /// Sets and gets the UserName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string UserName
        {
            get
            {
                return _userName;
            }

            set
            {
                if (_userName == value)
                {
                    return;
                }

                _userName = value;
                RaisePropertyChanged(UserNamePropertyName);
            }
        }


        /// <summary>
        /// The <see cref="Pages" /> property's name.
        /// </summary>
        public const string PagesPropertyName = "Pages";

        private ObservableCollection<string> _pages = new ObservableCollection<string>();

        /// <summary>
        /// Sets and gets the Pages property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> Pages
        {
            get
            {
                return _pages;
            }

            set
            {
                if (_pages == value)
                {
                    return;
                }

                _pages = value;
                RaisePropertyChanged(PagesPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="RecentFiles" /> property's name.
        /// </summary>
        public const string RecentFilesPropertyName = "RecentFiles";

        private ObservableCollection<string> _recentFiles = new ObservableCollection<string>();

        /// <summary>
        /// Sets and gets the RecentFiles property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> RecentFiles
        {
            get
            {
                return _recentFiles;
            }

            set
            {
                if (_recentFiles == value)
                {
                    return;
                }

                _recentFiles = value;
                RaisePropertyChanged(RecentFilesPropertyName);
            }
        }

        public void UpdatePages()
        {
            RaisePropertyChanged(PagesPropertyName);
        }

        private bool _autoSync = false;

        public bool AutoSync
        {
            get { return _autoSync; }
            set
            {
                if (_autoSync == value) return;
                _autoSync = value;
                RaisePropertyChanged(nameof(AutoSync));
            }
        }

        internal void AddRecentFile(string fileName)
        {
            if (RecentFiles == null)
                RecentFiles = new ObservableCollection<string>();
            if (RecentFiles.Contains(fileName))
                RecentFiles.Remove(fileName);
            RecentFiles.Insert(0, fileName);
            while (RecentFiles.Count > 5)
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
    }
}
