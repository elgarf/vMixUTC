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
using System.Windows;
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

        [XmlIgnore]
        public ObservableCollection<string> Data
        {
            get { return (ObservableCollection<string>)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(ObservableCollection<string>), typeof(vMixControlExternalData), new PropertyMetadata(null));

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

        /// <summary>
        /// The <see cref="RestartData" /> property's name.
        /// </summary>
        public const string RestartDataPropertyName = "RestartData";

        private bool _restartData = true;

        /// <summary>
        /// Sets and gets the RestartData property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool RestartData
        {
            get
            {
                return _restartData;
            }

            set
            {
                if (_restartData == value)
                {
                    return;
                }

                _restartData = value;
                RaisePropertyChanged(RestartDataPropertyName);
            }
        }

        public vMixControlExternalData()
        {
            Data = new ObservableCollection<string>();
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
                catch (Exception)
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
            try
            {
                AssemblyName name;
                if (string.IsNullOrWhiteSpace(_dataProviderPath) || !File.Exists(_dataProviderPath))
                {
                    string fn = Path.GetTempFileName();
                    using (var fs = new FileStream(fn, FileMode.Create))
                    using (var sw = new BinaryWriter(fs))
                        sw.Write(value);
                    name = AssemblyName.GetAssemblyName(fn);
                }
                else
                    name = AssemblyName.GetAssemblyName(_dataProviderPath);

                var assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName == name.FullName).FirstOrDefault() ?? Assembly.Load(value);
                var aa = Assembly.GetAssembly(assembly.GetTypes().FirstOrDefault());
                var type = assembly.GetExportedTypes().Where(x => x.GetInterfaces().Contains(typeof(IvMixDataProvider))).FirstOrDefault();

                if (DataProvider?.GetType() != type && type != null)
                {
                    DataProvider = (IvMixDataProvider)assembly.CreateInstance(type.FullName);
                }

                if (_dataProviderProperties != null)
                {
                    DataProvider.SetProperties(_dataProviderProperties);
                    if (DataProvider is IvMixDataProviderTextInput)
                    {
                        ((IvMixDataProviderTextInput)DataProvider).PreviewKeyUp = PreviewKeyUp;
                        ((IvMixDataProviderTextInput)DataProvider).GotFocus = GotFocus;
                        ((IvMixDataProviderTextInput)DataProvider).LostFocus = LostFocus;
                    }
                }

                UpdateText(Paths);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error loading Data Provider!");
            }
        }

        internal override void UpdateText(IList<Pair<string, string>> paths)
        {


            ThreadPool.QueueUserWorkItem((x) =>
            {
                if (DataProvider == null) return;
                var values = DataProvider.Values;
                if (values == null || values.Length < 1)
                    return;

                Dispatcher.Invoke(() =>
                {
                    Data = new ObservableCollection<string>(values);
                });


                Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < paths.Count; i++)
                    {
                        var item = paths[i];
                        var value = values[i % values.Length];
                        if (!_restartData && i >= values.Length)
                            value = "";

                        if (value.StartsWith("@[cmd]"))
                        {
                            var command = value.Substring(6);
                            if (!string.IsNullOrWhiteSpace(command))
                                State.SendFunction(string.Format(command, item.A, item.B));
                            continue;
                        }

                        //var obj = GetValueByPath(State, string.Format("Inputs[{0}].Elements[{1}]", item.A, item.B));

                        var input = (Input)GetValueByPath(State, string.Format("Inputs[{0}]", item.A));
                        if (input != null)
                        {
                            var obj = input.Elements.Where(y => (y is InputText || y is InputImage) && (y as InputBase).Name == item.B).FirstOrDefault();
                            if (obj != null)
                                if (obj is vMixAPI.InputText)
                                    (obj as vMixAPI.InputText).Text = value;
                                else if (obj is vMixAPI.InputImage)
                                    (obj as vMixAPI.InputImage).Image = value;
                        }
                    }
                });
            });
        }

        public override void Update()
        {
            UpdateText(Paths);
            base.Update();
        }

        public override UserControl[] GetPropertiesControls()
        {
            IntControl periodInt = GetPropertyControl<IntControl>();
            periodInt.Title = LocalizationManager.Get("Update Period");
            periodInt.Value = Period;

            FilePathControl providerPath = GetPropertyControl<FilePathControl>();
            providerPath.Filter = "External Data|*.dll";
            providerPath.Value = DataProviderPath;
            periodInt.Value = Period;

            BoolControl loopBool = GetPropertyControl<BoolControl>();
            loopBool.Value = RestartData;
            loopBool.Title = LocalizationManager.Get("Loop Data");
            loopBool.Tag = "RD";

            var props = base.GetPropertiesControls();
            foreach (var prop in props.OfType<BoolControl>())
                prop.Visibility = Visibility.Collapsed;
            return (new UserControl[] { periodInt, providerPath, loopBool }).Concat(props).ToArray();
        }

        public override void SetProperties(vMixWidgetSettingsViewModel viewModel)
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
            RestartData = _controls.OfType<BoolControl>().Where(x=>(string)x.Tag == "RD").First().Value;
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

        protected override void Dispose(bool managed)
        {
            if (_disposed) return;

            if (managed)
            {
                _timer.Stop();
                if (DataProvider != null && DataProvider is IDisposable)
                    ((IDisposable)DataProvider).Dispose();
                base.Dispose(managed);
                GC.SuppressFinalize(this);
            }
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

        [NonSerialized]
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
