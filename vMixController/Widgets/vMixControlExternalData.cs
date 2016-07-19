using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using vMixControllerDataProvider;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlExternalData : vMixControlTextField, IvMixAutoUpdateWidget
    {
        [NonSerialized]
        DispatcherTimer _timer = new DispatcherTimer();

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

        public vMixControlExternalData()
        {
            _timer.Tick += _timer_Tick;
            _timer.Interval = TimeSpan.FromMilliseconds(_period);
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (IsTemplate) _timer.Stop();
            if (Enabled)
                UpdateText(Paths);
        }

        /// <summary>
        /// The <see cref="Period" /> property's name.
        /// </summary>
        public const string PeriodPropertyName = "Period";

        private int _period = 1000;

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

                _period = value >= 1000 ? value : 1000;
                _timer.Interval = TimeSpan.FromMilliseconds(value);
                if (_dataProvider != null)
                    _dataProvider.Period = _period;
                RaisePropertyChanged(PeriodPropertyName);
            }
        }


        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("External Data");
            }
        }

        /// <summary>
        /// The <see cref="DataProvider" /> property's name.
        /// </summary>
        public const string DataProviderPropertyName = "DataProvider";

        private IvMixDataProvider _dataProvider = null;

        /// <summary>
        /// Sets and gets the DataProvider property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        [XmlIgnore]
        public IvMixDataProvider DataProvider
        {
            get
            {
                return _dataProvider;
            }

            set
            {
                if (_dataProvider == value)
                {
                    return;
                }

                _dataProvider = value;
                RaisePropertyChanged(DataProviderPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="DataProviderProperties" /> property's name.
        /// </summary>
        public const string DataProviderPropertiesPropertyName = "DataProviderProperties";

        private List<object> _dataProviderProperties = null;

        /// <summary>
        /// Sets and gets the DataProviderProperties property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<object> DataProviderProperties
        {
            get
            {
                if (DataProvider != null)
                    return DataProvider.GetProperties();
                else
                    return _dataProviderProperties;
            }

            set
            {
                if (_dataProviderProperties == value)
                {
                    return;
                }

                _dataProviderProperties = value;

                if (_dataProvider != null)
                    _dataProvider.SetProperties(value);

                RaisePropertyChanged(DataProviderPropertiesPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="DataProviderContent" /> property's name.
        /// </summary>
        public const string DataProviderContentPropertyName = "DataProviderContent";

        private string _dataProviderContent = "";

        /// <summary>
        /// Sets and gets the DataProviderContent property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string DataProviderContent
        {
            get
            {
                return _dataProviderContent;
            }

            set
            {
                if (_dataProviderContent == value)
                {
                    return;
                }
                try
                {
                    InitializeDataProvider(Convert.FromBase64String(value));
                }
                catch (Exception ex)
                {
                   
                }
                _dataProviderContent = value;
                RaisePropertyChanged(DataProviderContentPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="DataProviderPath" /> property's name.
        /// </summary>
        public const string DataProviderPathPropertyName = "DataProviderPath";

        private string _dataProviderPath = "";

        /// <summary>
        /// Sets and gets the DataProviderPath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string DataProviderPath
        {
            get
            {
                return _dataProviderPath;
            }

            set
            {
                if (_dataProviderPath == value)
                {
                    return;
                }

                _dataProviderPath = value;

                try
                {
                    if (File.Exists(value))
                    {
                        if (DataProvider != null && DataProvider is IDisposable)
                            ((IDisposable)DataProvider).Dispose();

                        DataProviderContent = Convert.ToBase64String(File.ReadAllBytes(value));

                        InitializeDataProvider(File.ReadAllBytes(value));
                    }
                }
                catch (Exception)
                {

                }
                RaisePropertyChanged(DataProviderPathPropertyName);
            }
        }

        private void InitializeDataProvider(byte[] value)
        {
            var assembly = Assembly.Load(value);
            var type = assembly.GetExportedTypes().Where(x => x.GetInterfaces().Contains(typeof(IvMixDataProvider))).FirstOrDefault();
            if (type != null)
                DataProvider = (IvMixDataProvider)assembly.CreateInstance(type.FullName);

            DataProvider.SetProperties(_dataProviderProperties);

            UpdateText(Paths);
        }

        internal override void UpdateText(IList<Pair<int, string>> paths)
        {


            ThreadPool.QueueUserWorkItem((x) =>
            {
                if (DataProvider == null) return;
                var values = DataProvider.Values;
                if (values == null || values.Length < 1)
                    return;

                Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < paths.Count; i++)
                    {
                        var item = paths[i];
                        var value = values[i % values.Length];

                        //var obj = GetValueByPath(State, string.Format("Inputs[{0}].Elements[{1}]", item.A, item.B));

                        var input = (Input)GetValueByPath(State, string.Format("Inputs[{0}]", item.A));
                        if (input != null)
                        {
                            var obj = input.Elements.Where(y => (y is InputText) && (y as InputText).Name == item.B).FirstOrDefault();
                            if (obj != null && obj is vMixAPI.InputText)
                                (obj as vMixAPI.InputText).Text = value;
                        }
                    }
                });
            });
        }

        public override UserControl[] GetPropertiesControls()
        {
            IntControl control = GetPropertyControl<IntControl>();
            control.Title = LocalizationManager.Get("Update Period");
            control.Value = Period;

            FilePathControl control1 = GetPropertyControl<FilePathControl>();
            control1.Filter = "External Data|*.dll";
            control1.Value = DataProviderPath;
            control.Value = Period;

            var props = base.GetPropertiesControls();
            props.OfType<BoolControl>().First().Visibility = System.Windows.Visibility.Collapsed;
            return (new UserControl[] { control, control1 }).Concat(props).ToArray();
        }

        public override void SetProperties(vMixControlSettingsViewModel viewModel)
        {
            base.SetProperties(viewModel);
            
        }

        public override void SetProperties(UserControl[] _controls)
        {
            base.SetProperties(_controls);

            if (DataProviderProperties != null)
                DataProviderProperties.Clear();
            Period = _controls.OfType<IntControl>().First().Value;
            DataProviderPath = _controls.OfType<FilePathControl>().First().Value;
        }

        public override Hotkey[] GetHotkeys()
        {
            return base.GetHotkeys().Concat(new Hotkey[] { new Classes.Hotkey() { Name = "Toggle\nEnabled" } }).ToArray();
        }

        public override void ExecuteHotkey(int index)
        {
            if (index == 0)
                Enabled = !Enabled;
        }

        public override void Dispose()
        {
            _timer.Stop();
            if (DataProvider != null && DataProvider is IDisposable)
                ((IDisposable)DataProvider).Dispose();

            base.Dispose();
        }

        [NonSerialized]
        private RelayCommand _openPropertiesCommand;

        /// <summary>
        /// Gets the OpenPropertiesCommand.
        /// </summary>
        public RelayCommand OpenPropertiesCommand
        {
            get
            {
                return _openPropertiesCommand
                    ?? (_openPropertiesCommand = new RelayCommand(
                    () =>
                    {
                        if (DataProvider != null)
                        {
                            DataProvider.ShowProperties(App.Current.Windows.OfType<MainWindow>().FirstOrDefault());
                            UpdateText(Paths);
                        }
                    }));
            }
        }


        private RelayCommand _toggleEnabledCommand;

        /// <summary>
        /// Gets the ToggleEnabled.
        /// </summary>
        public RelayCommand ToggleEnabledCommand
        {
            get
            {
                return _toggleEnabledCommand
                    ?? (_toggleEnabledCommand = new RelayCommand(
                    () =>
                    {
                        Enabled = !Enabled;
                    }));
            }
        }
    }
}
