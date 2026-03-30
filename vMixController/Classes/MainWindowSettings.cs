using CommunityToolkit.Mvvm.ComponentModel;
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
            get => _state;
            set => SetProperty(ref _state, value, StatePropertyName);
        }



        private bool _useInfiniteCanvas = true;

        /// <summary>
        /// Sets and gets the Left property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool UseInfiniteCanvas
        {
            get => _useInfiniteCanvas;
            set => SetProperty(ref _useInfiniteCanvas, value);
        }


        private bool _showLinks = false;

        /// <summary>
        /// Sets and gets the Left property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool ShowLinks
        {
            get => _showLinks;
            set => SetProperty(ref _showLinks, value);
        }


        private string _linksStyle = Constants.LINKS_NONE;

        /// <summary>
        /// Sets and gets the Left property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string LinksStyle
        {
            get => _linksStyle;
            set => SetProperty(ref _linksStyle, value);
        }

        private bool _enableLoopGuard = true;

        public bool EnableLoopGuard
        {
            get => _enableLoopGuard;
            set => SetProperty(ref _enableLoopGuard, value);
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
            get => _left;
            set => SetProperty(ref _left, value, LeftPropertyName);
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
            get => _top;
            set => SetProperty(ref _top, value, TopPropertyName);
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
            get => _width;
            set => SetProperty(ref _width, value, WidthPropertyName);
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
            get => _height;
            set => SetProperty(ref _height, value, HeightPropertyName);
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

                OnPropertyChanged(IPPropertyName);
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

                OnPropertyChanged(PortPropertyName);
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

                OnPropertyChanged(HttpLoginPropertyName);
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

                OnPropertyChanged(HttpPasswordPropertyName);
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
            get => _locked;
            set => SetProperty(ref _locked, value, LockedPropertyName);
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
            get => _uiScale;
            set => SetProperty(ref _uiScale, value, UIScalePropertyName);
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
                OnPropertyChanged(EnableLogPropertyName);
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
            get => _openLastAtStart;
            set => SetProperty(ref _openLastAtStart, value, OpenLastAtStartPropertyName);
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
                OnPropertyChanged(PollTimePropertyName);
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

                OnPropertyChanged(nameof(IsTopmost));
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
            get => _showIndividualLock;
            set => SetProperty(ref _showIndividualLock, value, ShowIndividualLockPropertyName);
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
            get => _blinkLights;
            set => SetProperty(ref _blinkLights, value, BlinkLightsPropertyName);
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
            get => _password;
            set => SetProperty(ref _password, value, PasswordPropertyName);
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
            get => _userName;
            set => SetProperty(ref _userName, value, UserNamePropertyName);
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
            get => _pages;
            set => SetProperty(ref _pages, value, PagesPropertyName);
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
            get => _recentFiles;
            set => SetProperty(ref _recentFiles, value, RecentFilesPropertyName);
        }

        public void UpdatePages()
        {
            OnPropertyChanged(PagesPropertyName);
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

