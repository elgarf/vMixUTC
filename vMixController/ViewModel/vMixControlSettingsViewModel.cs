using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using vMixController.Classes;
using vMixController.Widgets;
using System.IO;
using vMixController;
using System.Windows.Media;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using System.Windows.Controls;
using vMixController.Extensions;

namespace vMixController.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class vMixControlSettingsViewModel : ViewModelBase
    {

        public static List<Triple<Color, Color, string>> Colors = new List<Triple<Color, Color, string>>()
        {
            new Triple<Color, Color, string>(Color.FromRgb(41, 48, 56), Color.FromRgb(62, 72, 84), "Gray"),
            new Triple<Color, Color, string>(Color.FromRgb(26, 60, 117), Color.FromRgb(24, 72, 140), "Blue"),
            new Triple<Color, Color, string>(Color.FromRgb(0, 135, 255), Color.FromRgb(24, 202, 255), "Aqua"),
            new Triple<Color, Color, string>(Color.FromRgb(245, 37, 217), Color.FromRgb(247, 84, 255), "Fuchsia"),//
            new Triple<Color, Color, string>(Color.FromRgb(128, 0, 128), Color.FromRgb(192, 24, 192), "Purple"),
            new Triple<Color, Color, string>(Color.FromRgb(255, 140, 0), Color.FromRgb(255, 210, 24), "Orange"),
            new Triple<Color, Color, string>(Color.FromRgb(0, 100, 0), Color.FromRgb(24, 150, 24), "Green"),
            new Triple<Color, Color, string>(Color.FromRgb(55, 173, 95), Color.FromRgb(80, 200, 120), "Emerald"),//
            new Triple<Color, Color, string>(Color.FromRgb(139, 0, 0), Color.FromRgb(208, 24, 24), "Red")
        };

        /// <summary>
        /// The <see cref="WindowProperties" /> property's name.
        /// </summary>
        public const string WindowPropertiesPropertyName = "WindowProperties";

        private Quadriple<double?, double?, double?, double?> _windowProperties = new Quadriple<double?, double?, double?, double?>();

        /// <summary>
        /// Sets and gets the WindowProperties property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Quadriple<double?, double?, double?, double?> WindowProperties
        {
            get
            {
                return _windowProperties;
            }

            set
            {
                if (_windowProperties == value)
                {
                    return;
                }

                _windowProperties = value;
                RaisePropertyChanged(WindowPropertiesPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="PeriodVisibility" /> property's name.
        /// </summary>
        public const string PeriodVisibilityPropertyName = "PeriodVisibility";

        private Visibility _periodVisibility = Visibility.Visible;

        /// <summary>
        /// Sets and gets the PeriodVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility PeriodVisibility
        {
            get
            {
                return _periodVisibility;
            }

            set
            {
                if (_periodVisibility == value)
                {
                    return;
                }

                _periodVisibility = value;
                RaisePropertyChanged(PeriodVisibilityPropertyName);
            }
        }

        /*/// <summary>
        /// The <see cref="PathMappingVisibility" /> property's name.
        /// </summary>
        public const string PathMappingVisibilityPropertyName = "PathMappingVisibility";

        private Visibility _pathMappingVisibility = Visibility.Visible;

        /// <summary>
        /// Sets and gets the PathMappingVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility PathMappingVisibility
        {
            get
            {
                return _pathMappingVisibility;
            }

            set
            {
                if (_pathMappingVisibility == value)
                {
                    return;
                }

                _pathMappingVisibility = value;
                RaisePropertyChanged(PathMappingVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ScriptVisibility" /> property's name.
        /// </summary>
        public const string ScriptVisibilityPropertyName = "ScriptVisibility";

        private Visibility _scriptVisibility = Visibility.Visible;

        /// <summary>
        /// Sets and gets the ScriptVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility ScriptVisibility
        {
            get
            {
                return _scriptVisibility;
            }

            set
            {
                if (_scriptVisibility == value)
                {
                    return;
                }

                _scriptVisibility = value;
                RaisePropertyChanged(ScriptVisibilityPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ItemsVisibility" /> property's name.
        /// </summary>
        public const string ItemsVisibilityPropertyName = "ItemsVisibility";

        private Visibility _itemsVisibility = Visibility.Visible;

        /// <summary>
        /// Sets and gets the ItemsVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility ItemsVisibility
        {
            get
            {
                return _itemsVisibility;
            }

            set
            {
                if (_itemsVisibility == value)
                {
                    return;
                }

                _itemsVisibility = value;
                RaisePropertyChanged(ItemsVisibilityPropertyName);
            }
        }*/

        /// <summary>
        /// The <see cref="Model" /> property's name.
        /// </summary>
        public const string ModelPropertyName = "Model";

        private vMixAPI.State _model = null;

        /// <summary>
        /// Sets and gets the Model property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public vMixAPI.State Model
        {
            get
            {
                return _model;
            }

            set
            {
                if (_model == value)
                {
                    return;
                }

                _model = value;
                RaisePropertyChanged(ModelPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Name" /> property's name.
        /// </summary>
        public const string NamePropertyName = "Name";

        private string _name = "";

        /// <summary>
        /// Sets and gets the Name property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                RaisePropertyChanged(NamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Color" /> property's name.
        /// </summary>
        public const string ColorPropertyName = "Color";

        private Color _color = Colors[0].A;

        /// <summary>
        /// Sets and gets the Color property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Color Color
        {
            get
            {
                return _color;
            }

            set
            {
                if (_color == value)
                {
                    return;
                }

                _color = value;
                RaisePropertyChanged(ColorPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Hotkey" /> property's name.
        /// </summary>
        public const string HotkeyPropertyName = "Hotkey";

        private ObservableCollection<Hotkey> _hotkey = new ObservableCollection<Classes.Hotkey>();

        /// <summary>
        /// Sets and gets the Hotkey property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Hotkey> Hotkey
        {
            get
            {
                return _hotkey;
            }

            set
            {
                if (_hotkey == value)
                {
                    return;
                }

                _hotkey = value;
                RaisePropertyChanged(HotkeyPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Period" /> property's name.
        /// </summary>
        public const string PeriodPropertyName = "Period";

        private int _period = 0;

        /// <summary>
        /// Sets and gets the Period property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Period
        {
            get
            {
                return _period;
            }

            set
            {
                if (_period == value)
                {
                    return;
                }

                _period = value;
                RaisePropertyChanged(PeriodPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="WidgetPropertiesControls" /> property's name.
        /// </summary>
        public const string WidgetPropertiesControlsPropertyName = "WidgetPropertiesControls";

        private UserControl[] _widgetPropertiesControls = null;

        /// <summary>
        /// Sets and gets the WidgetPropertiesControls property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public UserControl[] WidgetPropertiesControls
        {
            get
            {
                return _widgetPropertiesControls;
            }

            set
            {
                if (_widgetPropertiesControls == value)
                {
                    return;
                }

                _widgetPropertiesControls = value;
                RaisePropertyChanged(WidgetPropertiesControlsPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="Type" /> property's name.
        /// </summary>
        public const string TypePropertyName = "Type";

        private string _type = "";

        /// <summary>
        /// Sets and gets the Type property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Type
        {
            get
            {
                return _type;
            }

            set
            {
                if (_type == value)
                {
                    return;
                }

                _type = value;
                RaisePropertyChanged(TypePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Control" /> property's name.
        /// </summary>
        public const string ControlPropertyName = "Control";

        private vMixControl _control = null;

        /// <summary>
        /// Sets and gets the Control property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public vMixControl Control
        {
            get
            {
                return _control;
            }

            set
            {
                if (_control == value)
                {
                    return;
                }

                _control = value;
                RaisePropertyChanged(ControlPropertyName);
            }
        }

        private RelayCommand _okCommand;

        /// <summary>
        /// Gets the OkCommand.
        /// </summary>
        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand
                    ?? (_okCommand = new RelayCommand(
                    () =>
                    {
                        MessengerInstance.Send(true);
                    }));
            }
        }


        private RelayCommand _cancelCommand;

        /// <summary>
        /// Gets the CancelCommand.
        /// </summary>
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                    () =>
                    {
                        MessengerInstance.Send(false);
                    }));
            }
        }

        private RelayCommand _saveTemplateCommand;

        /// <summary>
        /// Gets the SaveTemplateCommand.
        /// </summary>
        public RelayCommand SaveTemplateCommand
        {
            get
            {
                return _saveTemplateCommand
                    ?? (_saveTemplateCommand = new RelayCommand(
                    () =>
                    {
                        var viewModel = ServiceLocator.Current.GetInstance<vMixController.ViewModel.MainViewModel>();
                        var obj = viewModel.ControlTemplates.Select((x, i) => new { obj = x, idx = i }).Where(x => x.obj.A == Name).FirstOrDefault();
                        var cpy = Control.Copy();
                        cpy.SetProperties(this);
                        cpy.IsTemplate = true;
                        if (obj != null)
                            viewModel.ControlTemplates[obj.idx].B = cpy;
                        else
                            viewModel.ControlTemplates.Add(new Pair<string, vMixControl>(Name, cpy));
                        MessengerInstance.Send(true);
                    }));
            }
        }


        public void SetProperties(vMixControl p)
        {
            Model = p.State;
            Name = p.Name;
            Color = p.Color;
            Hotkey = new ObservableCollection<Classes.Hotkey>(p.Hotkey);
            Type = p.Type;

            WindowProperties = p.WindowProperties;
            if (!WindowProperties.A.HasValue ||
                !WindowProperties.B.HasValue ||
                !WindowProperties.C.HasValue ||
                !WindowProperties.D.HasValue)
            {
                WindowProperties.A = 512;
                WindowProperties.B = 512;
                WindowProperties.C = 0;
                WindowProperties.D = 0;
            }


            if (p is IvMixAutoUpdateWidget)
                Period = (p as IvMixAutoUpdateWidget).Period;

            PeriodVisibility = p is IvMixAutoUpdateWidget ? Visibility.Visible : Visibility.Collapsed;

            WidgetPropertiesControls = p.GetPropertiesControls();
        }

        private RelayCommand _closingCommand;

        /// <summary>
        /// Gets the ClosingCommand.
        /// </summary>
        public RelayCommand ClosingCommand
        {
            get
            {
                return _closingCommand
                    ?? (_closingCommand = new RelayCommand(
                    () =>
                    {

                    }));
            }
        }


        private RelayCommand<Hotkey> _learnKeyCommand;

        /// <summary>
        /// Gets the LearnKeyCommand.
        /// </summary>
        public RelayCommand<Hotkey> LearnKeyCommand
        {
            get
            {
                return _learnKeyCommand
                    ?? (_learnKeyCommand = new RelayCommand<Hotkey>(
                    p =>
                    {
                        var wnd = new KeyLearnWindow();
                        var result = wnd.ShowDialog();
                        if (result ?? true)
                        {
                            p.Key = wnd.PressedKey;
                            wnd.Close();
                        }
                    }));
            }
        }

        /// <summary>
        /// Initializes a new instance of the vMixControlSettingsViewModel class.
        /// </summary>
        public vMixControlSettingsViewModel()
        {
        }
    }
}