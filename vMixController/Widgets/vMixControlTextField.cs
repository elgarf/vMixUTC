using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using vMixAPI;
using vMixController.Classes;
using vMixController.Converters;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    [Serializable]
    public partial class vMixControlTextField : vMixControl
    {
        protected string _defaultValue = null;

        // �������� Queue �� Dictionary ��� ������������ ��������� ����������
        // ����: (DependencyObject, DependencyProperty), ��������: ����� ���������� ����������
        protected static Dictionary<Tuple<DependencyObject, DependencyProperty>, DateTime> _pendingUpdates =
            new Dictionary<Tuple<DependencyObject, DependencyProperty>, DateTime>();

        // ������� ��� ����������� ���������, ����� ��������� �������, �� ��� ����� ��������� ������ ���������� ��������
        protected static Queue<Tuple<DependencyObject, DependencyProperty>> _updateQueue =
            new Queue<Tuple<DependencyObject, DependencyProperty>>();
        protected static HashSet<Tuple<DependencyObject, DependencyProperty>> _queuedKeys =
            new HashSet<Tuple<DependencyObject, DependencyProperty>>();

        private static DispatcherTimer DelayedUpdateTimer = new DispatcherTimer();

        static vMixControlTextField()
        {
            DelayedUpdateTimer.Interval = TimeSpan.FromSeconds(0.1);
            DelayedUpdateTimer.Tick += DelayedUpdateTimer_Tick;
            DelayedUpdateTimer.Start();
        }

        public override Hotkey[] GetHotkeys()
        {
            return new Classes.Hotkey[] { new Classes.Hotkey() { Name = "Focus" } };
        }

        public override void ExecuteHotkey(int index)
        {
            switch (index)
            {
                case 0:
                    IsFocused = false;
                    IsFocused = true;
                    break;
            }
            base.ExecuteHotkey(index);
        }

        private static void DelayedUpdateTimer_Tick(object sender, EventArgs e)
        {
            // ������� ��������� ������ ��� ���������, ������� ����� ����������
            var itemsToProcess = new List<Tuple<DependencyObject, DependencyProperty>>();

            lock (_pendingUpdates) // ������ �� �������������� ������� � ������� � �������
            {
                // �������� �� ��������� � ������� ��� ���������
                while (_updateQueue.Count > 0)
                {
                    var key = _updateQueue.Peek(); // ������� �� ������ �������, �� ������ ���

                    // ���������, ������ �� ���������� ������� � ������� ���������� ����������
                    if (_pendingUpdates.TryGetValue(key, out DateTime lastUpdateTime) && lastUpdateTime.AddSeconds(0.1) < DateTime.Now)
                    {
                        _updateQueue.Dequeue(); // ������� �� �������
                        _queuedKeys.Remove(key);
                        _pendingUpdates.Remove(key); // ������� �� �������
                        itemsToProcess.Add(key); // ��������� � ������ ��� ���������
                    }
                    else
                    {
                        // ���� ��� �� ������ ����� ��� ������� ��������, �� � ��� ����������� ����
                        break;
                    }
                }
            }

            // ������������ �������� ��� lock, ����� �� ����������� UI-����� �������
            foreach (var item in itemsToProcess)
            {
                try
                {
                    var exp = BindingOperations.GetMultiBindingExpression(item.Item1, item.Item2);
                    if (exp != null && exp.Status == BindingStatus.Active && exp.BindingExpressions.Count > 0)
                        exp.UpdateSource();
                }
                catch (Exception)
                {
                    // ����������� ������
                    //Console.WriteLine($"Error updating source: {ex.Message}");
                }
            }
        }

        internal bool _updating = false;
        internal string _text = "";

        public override string Type
        {
            get
            {
                return "Text Field";
            }
        }

        public vMixControlTextField()
        {
            //_paths.CollectionChanged += _paths_CollectionChanged;
        }

        internal override void OnStateSynced()
        {
            UpdateText(_paths);
        }

        internal virtual string MappedTextProperty { get { return "Text"; } }
        internal virtual string MappedImageProperty { get { return "Image"; } }

        protected bool _isLive = true;

        /// <summary>
        /// Sets and gets the IsLive property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsLive
        {
            get
            {
                return _isLive;
            }

            set
            {
                SetPropertyValue(ref _isLive, value, nameof(IsLive), isLive =>
                {
                    if (!isLive)
                    {
                        lock (_pendingUpdates)
                        {
                            _pendingUpdates.Clear(); // ������� �������
                            _updateQueue.Clear(); // ������� �������
                            _queuedKeys.Clear();
                        }
                    }

                    if (isLive)
                        _text = Text;

                    UpdateText(_paths);

                    Text = _text;
                });
            }
        }

        private bool _isTable = false;

        /// <summary>
        /// Sets and gets the IsTable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsTable
        {
            get
            {
                return _isTable;
            }

            set
            {
                SetPropertyValue(ref _isTable, value, nameof(IsTable), _ => UpdateText(_paths));
            }
        }

        private bool _isEditable = true;

        /// <summary>
        /// Sets and gets the IsEditable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsEditable
        {
            get => _isEditable;
            set => SetPropertyValue(ref _isEditable, value, nameof(IsEditable));
        }

        private bool _isMappedToGUID = true;

        /// <summary>
        /// Sets and gets the IsMappedToGUID property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMappedToGUID
        {
            get => _isMappedToGUID;
            set => SetPropertyValue(ref _isMappedToGUID, value, nameof(IsMappedToGUID));
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(vMixControlTextField), new PropertyMetadata("", InternalPropertyChanged));

        protected static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!((vMixControlTextField)d).IsLive)
                return;

            if (e.Property.Name == nameof(Text))
            {
                // ���������� Tuple ��� ���� ��� �������
                var key = Tuple.Create(d, e.Property);

                lock (_pendingUpdates) // ������ �� �������������� ������� � ������� � �������
                {
                    // ��������� ����� ���������� ��������� ��� ���� ���� DO/DP
                    _pendingUpdates[key] = DateTime.Now;

                    // ���� ����� �������� ��� ��� � �������, ��������� ���
                    if (_queuedKeys.Add(key))
                    {
                        _updateQueue.Enqueue(key);
                    }
                }
            }
        }

        internal virtual IMultiValueConverter ConverterSelector()
        {
            if (!IsTable)
                return new FirstValueConverter(def: _defaultValue);
            else
                return new StringsToStringConverter();
        }

        internal virtual void UpdateText(IList<Pair<string, string>> _paths)
        {
            if (!_isLive)
            {
                _text = Text;
                BindingOperations.ClearBinding(this, TextProperty);
                Text = _text;
                return;
            }

            if (!_updating)
            {
                _updating = true;
                _text = Text;
                BindingOperations.ClearBinding(this, TextProperty);
                Text = _text;
                MultiBinding binding = new MultiBinding
                {
                    Converter = ConverterSelector(),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.Default,
                    NotifyOnSourceUpdated = true,
                    NotifyOnTargetUpdated = true
                };
                //binding.Delay = 10; // ��� �������� �� ���������� � MultiBinding, ������ � Binding

                InputBase text = null;

                //update text
                if (_paths != null && State != null)
                    foreach (var item in _paths)
                    {
                        var input = (Input)GetValueByPath(State, string.Format("Inputs[{0}]", item.A));
                        if (input != null)
                        {
                            var val = input.Elements.Where(x => x.Name == item.B).FirstOrDefault();

                            if (val != null && !_isTable)
                            {
                                if (text == null)
                                    text = val;
                                else
                                {
                                    var prop = val.GetType().GetProperty(val is InputImage ? MappedImageProperty : MappedTextProperty);
                                    if (prop != null)
                                    {
                                        var iprop = text.GetType().GetProperty(val is InputImage ? MappedImageProperty : MappedTextProperty);
                                        if (iprop != null)
                                            prop.SetValue(val, iprop.GetValue(text));
                                    }
                                }
                            }

                            Binding b = new Binding(val is InputImage ? MappedImageProperty : MappedTextProperty)
                            {
                                Source = val,
                                Mode = BindingMode.TwoWay,
                                UpdateSourceTrigger = UpdateSourceTrigger.Default,
                                FallbackValue = _defaultValue,
                                TargetNullValue = _defaultValue
                            };

                            binding.Bindings.Add(b);
                            //PresentationTraceSources.SetTraceLevel(b, PresentationTraceLevel.High);
                        }
                    }

                BindingOperations.SetBinding(this, TextProperty, binding);
                //PresentationTraceSources.SetTraceLevel(binding, PresentationTraceLevel.High);

                if (text != null && !_isTable)
                {
                    var iprop = text.GetType().GetProperty(text is InputImage ? MappedImageProperty : MappedTextProperty);
                    if (iprop != null)
                        Text = (string)iprop.GetValue(text);
                }

                _updating = false;
            }
        }

        private ObservableCollection<Pair<string, string>> _paths = new ObservableCollection<Pair<string, string>>();

        /// <summary>
        /// Sets and gets the Paths property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Pair<string, string>> Paths
        {
            get => _paths;
            set => SetPropertyValue(ref _paths, value, nameof(Paths));
        }

        private bool _template = false;

        /// <summary>
        /// Sets and gets the Style property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Template
        {
            get => _template;
            set => SetPropertyValue(ref _template, value, nameof(Template));
        }

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        [RelayCommand]
        private void SelectPath()
        {
                        var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
                        {
                            Filter = "All files (*.*)|*.*"
                        };
                        var result = dialog.ShowDialog();
                        if (result.HasValue && result.Value)
                            Text = dialog.FileName;
                            }

        public override void AfterPropertiesChanged()
        {
            base.AfterPropertiesChanged();
            UpdateText(_paths);
        }

        protected override void Dispose(bool managed)
        {
            base.Dispose(managed);
        }

        public override void Update()
        {
            UpdateText(Paths);
            base.Update();
        }
    }
}

