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
            Pages.Add("Main");
            Pages.Add("Data");
            Pages.Add("Page 1");
            Pages.Add("Page 2");
            Pages.Add("Page 3");
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
                vMixAPI.StateFabrique.Configure(IP, Port);

                XmlDocumentMessenger.Url = vMixAPI.StateFabrique.GetUrl(IP, Port);

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
                vMixAPI.StateFabrique.Configure(IP, Port);

                XmlDocumentMessenger.Url = vMixAPI.StateFabrique.GetUrl(IP, Port);

                RaisePropertyChanged(PortPropertyName);
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

        private bool _enableLog = false;

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
                        NLog.LogManager.EnableLogging();
                }
                else
                {
                    if (NLog.LogManager.IsLoggingEnabled())
                        NLog.LogManager.DisableLogging();
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

    }
}
