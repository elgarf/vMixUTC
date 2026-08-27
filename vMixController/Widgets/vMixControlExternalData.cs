using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;
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
                RaisePropertyChanged(nameof(Enabled));
            }
        }

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
                RaisePropertyChanged(nameof(RestartData));
            }
        }

        private string _selectionSourceName = "";

        /// <summary>
        /// Sets and gets the SelectionSourceName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SelectionSourceName
        {
            get
            {
                return _selectionSourceName;
            }

            set
            {
                if (_selectionSourceName == value)
                {
                    return;
                }

                _selectionSourceName = value;
                RaisePropertyChanged(nameof(SelectionSourceName));
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

                _period = value >= 100 ? value : 100;
                _timer.Interval = TimeSpan.FromMilliseconds(_period);
                if (_dataProvider != null)
                    _dataProvider.Period = _period;
                RaisePropertyChanged(nameof(Period));
            }
        }


        public override string Type
        {
            get
            {
                return "External Data";
            }
        }

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
                RaisePropertyChanged(nameof(DataProvider));
            }
        }

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

                RaisePropertyChanged(nameof(DataProviderProperties));
            }
        }

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
                RaisePropertyChanged(nameof(DataProviderContent));
            }
        }

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
                    else
                        InitializeDataProvider(Convert.FromBase64String(DataProviderContent));
                }
                catch (Exception)
                {

                }
                RaisePropertyChanged(nameof(DataProviderPath));
            }
        }

        private void InitializeDataProvider(byte[] value)
        {
            try
            {
                AssemblyName name;
                Assembly assembly;
                if (!string.IsNullOrWhiteSpace(_dataProviderPath))
                {

                    if (!File.Exists(_dataProviderPath))
                    {
                        string fn = Path.GetTempFileName();
                        using (var fs = new FileStream(fn, FileMode.Create))
                        using (var sw = new BinaryWriter(fs))
                            sw.Write(value);
                        name = AssemblyName.GetAssemblyName(fn);
                    }
                    else
                        name = AssemblyName.GetAssemblyName(_dataProviderPath);
                    assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName == name.FullName).FirstOrDefault() ?? Assembly.Load(value);
                }
                else
                    return;
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
            var pathsSnapshot = paths?.ToArray() ?? Array.Empty<Pair<string, string>>();
            var schedulerKey = string.Format("external-data:{0}", WidgetId);
            UpdateScheduler.ScheduleLatest(schedulerKey, () =>
            {
                try
                {


                    Dispatcher.Invoke(() =>
                    {
                        if (DataProvider == null)
                            return;

                        var values = DataProvider.Values;
                        if (values == null || values.Length < 1)
                            return;

                        Data = new ObservableCollection<string>(values);

                        if (State == null)
                            return;

                        int offset = 0;
                        if (!string.IsNullOrWhiteSpace(SelectionSourceName))
                        {
                            var listWidget = Singleton<SharedData>.Instance.GetDataSource(SelectionSourceName) as vMixControlList;
                            if (listWidget != null && listWidget.SelectedIndex >= 0)
                                offset = listWidget.SelectedIndex;
                        }

                        for (int i = 0; i < pathsSnapshot.Length; i++)
                        {
                            var item = pathsSnapshot[i];
                            var value = values[(offset + i) % values.Length];
                            if (!_restartData && offset + i >= values.Length)
                                value = "";

                            if (value.StartsWith("@[cmd]"))
                            {
                                var command = value.Substring(6);
                                if (!string.IsNullOrWhiteSpace(command))
                                    State.SendFunction(string.Format(command, item.A, item.B));
                                continue;
                            }

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
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error while updating external data.");
                }
            });
        }

        public override void Update()
        {
            UpdateText(Paths);
            base.Update();
        }

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        public override void AfterPropertiesChanged()
        {
            base.AfterPropertiesChanged();

            if (DataProviderProperties != null)
                DataProviderProperties.Clear();
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
                _timer.Tick -= _timer_Tick;
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
