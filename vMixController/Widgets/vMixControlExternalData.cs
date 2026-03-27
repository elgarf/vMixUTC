using CommunityToolkit.Mvvm.Input;
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
    public partial class vMixControlExternalData : vMixControlTextField, IvMixAutoUpdateWidget
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
            DependencyProperty.Register(nameof(Data), typeof(ObservableCollection<string>), typeof(vMixControlExternalData), new PropertyMetadata(null));

        private bool _enabled = true;

        /// <summary>
        /// Sets and gets the Enabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set => SetPropertyValue(ref _enabled, value, nameof(Enabled));
        }

        private bool _restartData = true;

        /// <summary>
        /// Sets and gets the RestartData property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool RestartData
        {
            get => _restartData;
            set => SetPropertyValue(ref _restartData, value, nameof(RestartData));
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
            get => _period;
            set
            {
                var normalized = value >= 100 ? value : 100;
                if (_period == normalized)
                    return;

                SetPropertyValue(ref _period, normalized, nameof(Period), period =>
                {
                    _timer.Interval = TimeSpan.FromMilliseconds(period);
                    if (_dataProvider != null)
                        _dataProvider.Period = period;
                });
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
            get => _dataProvider;
            set => SetPropertyValue(ref _dataProvider, value, nameof(DataProvider));
        }

        private List<object> _dataProviderProperties = null;
        private bool _providerWarningActive;
        private string _providerWarningText = string.Empty;

        [XmlIgnore]
        public bool ProviderWarningActive
        {
            get => _providerWarningActive;
            set => SetPropertyValue(ref _providerWarningActive, value, nameof(ProviderWarningActive));
        }

        [XmlIgnore]
        public string ProviderWarningText
        {
            get => _providerWarningText;
            set => SetPropertyValue(ref _providerWarningText, value, nameof(ProviderWarningText));
        }

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
                SetPropertyValue(ref _dataProviderProperties, value, nameof(DataProviderProperties), _ =>
                {
                    if (_dataProvider != null)
                        _dataProvider.SetProperties(value);
                });
            }
        }

        /// <summary>
        /// Legacy property kept for backwards compatibility with old saved files.
        /// Binary provider payload is no longer stored.
        /// </summary>
        [XmlIgnore]
        public string DataProviderContent
        {
            get => string.Empty;
            set { }
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
                SetPropertyValue(ref _dataProviderPath, value, nameof(DataProviderPath), _ =>
                {
                    try
                    {
                        if (DataProvider is IDisposable disposableProvider)
                            disposableProvider.Dispose();
                        DataProvider = null;

                        InitializeDataProvider();
                    }
                    catch (Exception)
                    {

                    }
                });
            }
        }

        private string BuildProviderCaption()
        {
            if (string.IsNullOrWhiteSpace(_dataProviderPath))
                return "(path is empty)";

            var fileName = Path.GetFileName(_dataProviderPath);
            return string.IsNullOrWhiteSpace(fileName) ? _dataProviderPath : fileName;
        }

        private void SetProviderWarning(string message)
        {
            ProviderWarningActive = true;
            ProviderWarningText = message;
            Text = message;
            Data = new ObservableCollection<string> { message };
        }

        private void ClearProviderWarning()
        {
            if (!ProviderWarningActive)
                return;

            ProviderWarningActive = false;
            ProviderWarningText = string.Empty;
            if (!string.IsNullOrWhiteSpace(Text))
                Text = string.Empty;
        }

        private void InitializeDataProvider()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_dataProviderPath))
                {
                    SetProviderWarning("Provider is not set");
                    return;
                }

                var resolvedProviderPath = Classes.Utils.ResolvePortablePath(_dataProviderPath);
                if (!File.Exists(resolvedProviderPath))
                {
                    SetProviderWarning($"Provider not found: {BuildProviderCaption()} ({_dataProviderPath})");
                    return;
                }

                var assemblyPath = Path.GetFullPath(resolvedProviderPath);

                var name = AssemblyName.GetAssemblyName(assemblyPath);
                var assembly = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(x => x.FullName == name.FullName)
                    ?? Assembly.LoadFrom(assemblyPath);

                var type = assembly.GetExportedTypes().Where(x => x.GetInterfaces().Contains(typeof(IvMixDataProvider))).FirstOrDefault();
                if (type == null)
                {
                    SetProviderWarning($"Provider type is invalid: {BuildProviderCaption()}");
                    return;
                }

                if (DataProvider?.GetType() != type)
                {
                    DataProvider = (IvMixDataProvider)assembly.CreateInstance(type.FullName);
                }
                if (DataProvider == null)
                {
                    SetProviderWarning($"Provider failed to create: {BuildProviderCaption()}");
                    return;
                }

                if (_dataProviderProperties != null)
                {
                    DataProvider.SetProperties(_dataProviderProperties);
                    if (DataProvider is IvMixDataProviderTextInput)
                    {
                        ((IvMixDataProviderTextInput)DataProvider).PreviewKeyUp = PreviewKeyUpCommand;
                        ((IvMixDataProviderTextInput)DataProvider).GotFocus = GotFocusCommand;
                        ((IvMixDataProviderTextInput)DataProvider).LostFocus = LostFocusCommand;
                    }
                }

                ClearProviderWarning();
                UpdateText(Paths);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error loading Data Provider!");
                SetProviderWarning($"Provider load error: {BuildProviderCaption()}");
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

                        for (int i = 0; i < pathsSnapshot.Length; i++)
                        {
                            var item = pathsSnapshot[i];
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

                            var input = (Input)GetValueByPath(State, string.Format("Inputs[{0}]", item.A));
                            if (input != null)
                            {
                                var obj = input.Elements.Where(y => (y is InputText || y is InputImage) && y.Name == item.B).FirstOrDefault();
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

        [RelayCommand]
        private void OpenProperties()
        {
            if (DataProvider != null)
            {
                DataProvider.ShowProperties(App.Current.Windows.OfType<MainWindow>().FirstOrDefault());
                UpdateText(Paths);
            }
        }

        [RelayCommand]
        private void ToggleEnabled()
        {
            Enabled = !Enabled;
        }
    }
}
