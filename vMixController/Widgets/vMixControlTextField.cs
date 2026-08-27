using GalaSoft.MvvmLight.CommandWpf;
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
    public class vMixControlTextField : vMixControl
    {
        protected string _defaultValue = null;

        // Изменяем Queue на Dictionary для отслеживания последних обновлений
        // Ключ: (DependencyObject, DependencyProperty), Значение: Время последнего обновления
        protected static Dictionary<Tuple<DependencyObject, DependencyProperty>, DateTime> _pendingUpdates =
            new Dictionary<Tuple<DependencyObject, DependencyProperty>, DateTime>();

        // Очередь для фактической обработки, чтобы сохранить порядок, но она будет содержать только уникальные элементы
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
            // Создаем временный список для элементов, которые нужно обработать
            var itemsToProcess = new List<Tuple<DependencyObject, DependencyProperty>>();

            lock (_pendingUpdates) // Защита от одновременного доступа к словарю и очереди
            {
                // Проходим по элементам в очереди для обработки
                var skipped = 0;
                while (_updateQueue.Count > 0 && skipped < _updateQueue.Count)
                {
                    var key = _updateQueue.Peek(); // Смотрим на первый элемент, не удаляя его

                    // Проверяем, прошло ли достаточно времени с момента последнего обновления
                    if (_pendingUpdates.TryGetValue(key, out DateTime lastUpdateTime) && lastUpdateTime.AddSeconds(0.1) < DateTime.Now)
                    {
                        _updateQueue.Dequeue(); // Удаляем из очереди
                        _queuedKeys.Remove(key);
                        _pendingUpdates.Remove(key); // Удаляем из словаря
                        itemsToProcess.Add(key); // Добавляем в список для обработки
                        skipped = 0;
                    }
                    else
                    {
                        // Если еще не пришло время, перемещаем в конец очереди, не блокируя остальные
                        _updateQueue.Enqueue(_updateQueue.Dequeue());
                        skipped++;
                    }
                }
            }

            // Обрабатываем элементы вне lock, чтобы не блокировать UI-поток надолго
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
                    // Логирование ошибки
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
                if (_isLive == value)
                {
                    return;
                }

                if (!value)
                {
                    lock (_pendingUpdates)
                    {
                        _pendingUpdates.Clear(); // Очищаем словарь
                        _updateQueue.Clear(); // Очищаем очередь
                        _queuedKeys.Clear();
                    }
                }

                _isLive = value;

                if (value)
                    _text = Text;

                UpdateText(_paths);

                Text = _text;

                RaisePropertyChanged(nameof(IsLive));
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
                if (_isTable == value)
                {
                    return;
                }

                _isTable = value;
                UpdateText(_paths);
                RaisePropertyChanged(nameof(IsTable));
            }
        }

        private bool _isEditable = true;

        /// <summary>
        /// Sets and gets the IsEditable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsEditable
        {
            get
            {
                return _isEditable;
            }

            set
            {
                if (_isEditable == value)
                {
                    return;
                }

                _isEditable = value;
                RaisePropertyChanged(nameof(IsEditable));
            }
        }

        private bool _isMappedToGUID = true;

        /// <summary>
        /// Sets and gets the IsMappedToGUID property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMappedToGUID
        {
            get
            {
                return _isMappedToGUID;
            }

            set
            {
                if (_isMappedToGUID == value)
                {
                    return;
                }

                _isMappedToGUID = value;
                RaisePropertyChanged(nameof(IsMappedToGUID));
            }
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
                // Используем Tuple как ключ для словаря
                var key = Tuple.Create(d, e.Property);

                lock (_pendingUpdates) // Защита от одновременного доступа к словарю и очереди
                {
                    // Обновляем время последнего изменения для этой пары DO/DP
                    _pendingUpdates[key] = DateTime.Now;

                    // Если этого элемента еще нет в очереди, добавляем его
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
                //binding.Delay = 10; // Это свойство не существует в MultiBinding, только в Binding

                InputBase text = null;

                //update text
                if (_paths != null && State != null)
                    foreach (var item in _paths)
                    {
                        var input = (Input)GetValueByPath(State, string.Format("Inputs[{0}]", item.A));
                        if (input != null)
                        {
                            var val = input.Elements.Where(x => (x is InputBase) && x.Name == item.B).FirstOrDefault();

                            if (val != null && val is InputBase && !_isTable)
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
            get
            {
                return _paths;
            }

            set
            {
                if (_paths == value)
                {
                    return;
                }

                _paths = value;
                RaisePropertyChanged(nameof(Paths));
            }
        }

        private bool _template = false;

        /// <summary>
        /// Sets and gets the Style property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Template
        {
            get
            {
                return _template;
            }

            set
            {
                if (_template == value)
                {
                    return;
                }

                _template = value;
                RaisePropertyChanged(nameof(Template));
            }
        }

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        [NonSerialized]
        private RelayCommand _selectPathCommand;

        /// <summary>
        /// Gets the SelectPathCommand.
        /// </summary>
        public RelayCommand SelectPathCommand
        {
            get
            {
                return _selectPathCommand
                    ?? (_selectPathCommand = new RelayCommand(
                    () =>
                    {
                        var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
                        {
                            Filter = "All files (*.*)|*.*"
                        };
                        var result = dialog.ShowDialog();
                        if (result.HasValue && result.Value)
                            Text = dialog.FileName;
                    }));
            }
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
