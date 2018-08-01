using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using vMixAPI;
using vMixController.Classes;
using vMixController.Converters;
using vMixController.Extensions;
using vMixController.PropertiesControls;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlTextField : vMixControl
    {

        private static Queue<Triple<DependencyObject, DependencyProperty, DateTime>> DelayedUpdate = new Queue<Triple<DependencyObject, DependencyProperty, DateTime>>();
        private static DispatcherTimer DelayedUpdateTimer = new DispatcherTimer();

        static vMixControlTextField()
        {
            //Отложенное обновление биндинга
            DelayedUpdateTimer.Interval = TimeSpan.FromSeconds(0.1);
            DelayedUpdateTimer.Tick += DelayedUpdateTimer_Tick;
            DelayedUpdateTimer.Start();
        }

        private static void DelayedUpdateTimer_Tick(object sender, EventArgs e)
        {
            while (DelayedUpdate.Count > 0 && DelayedUpdate.Peek().C.AddSeconds(0.1) < DateTime.Now)
            {
                var t = DelayedUpdate.Dequeue();
                try
                {
                    var exp = BindingOperations.GetMultiBindingExpression(t.A, t.B);
                    if (exp != null && exp.Status == BindingStatus.Active && exp.BindingExpressions.Count > 0)
                        exp.UpdateSource();
                }
                catch (Exception) { }
            }
        }

        internal bool _updating = false;
        internal string _text = "";

        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Text Field");
            }
        }

        public vMixControlTextField()
        {
            _paths.CollectionChanged += _paths_CollectionChanged;
        }

        internal override void OnStateUpdated()
        {
            UpdateText(_paths);
        }

        private void _paths_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            /*if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    UpdateText((sender as IList).OfType<Pair<int, string>>().ToList());*/
        }

        internal virtual string MappedTextProperty { get { return "Text"; } }

        internal virtual string MappedImageProperty { get { return "Image"; } }

        /// <summary>
        /// The <see cref="IsLive" /> property's name.
        /// </summary>
        public const string IsLivePropertyName = "IsLive";

        private bool _isLive = true;

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

                if (!value && DelayedUpdate.Count > 0)
                    DelayedUpdate.Clear();

                _isLive = value;

                if (value)
                    _text = Text;

                UpdateText(_paths);

                Text = _text;

                RaisePropertyChanged(IsLivePropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsTable" /> property's name.
        /// </summary>
        public const string IsTablePropertyName = "IsTable";

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
                RaisePropertyChanged(IsTablePropertyName);
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(vMixControlTextField), new PropertyMetadata("", InternalPropertyChanged));

        private static void InternalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!((vMixControlTextField)d).IsLive)
                return;
            if (e.Property.Name == "Text")
            {
                try
                {
                    var exp = BindingOperations.GetMultiBindingExpression(d, TextProperty);
                    if (exp != null && exp.Status == BindingStatus.Active)
                    {
                        DelayedUpdate.Enqueue(new Triple<DependencyObject, DependencyProperty, DateTime>() { A = d, B = e.Property, C = DateTime.Now });
                        //DelayedUpdateTimer.Start();
                        //exp.UpdateTarget();
                        //exp.UpdateSource();
                        /*foreach (var item in exp.BindingExpressions.OfType<BindingExpression>())
                        {
                            if (item.DataItem is InputBase && (item.DataItem as InputBase).Type == "TXT" && (item.DataItem as InputText).Text == (string)e.OldValue)
                                (item.DataItem as InputText).Text = (string)e.NewValue;
                            if (item.DataItem is InputBase && (item.DataItem as InputBase).Type == "IMG" && (item.DataItem as InputImage).Image == (string)e.OldValue)
                                (item.DataItem as InputImage).Image = (string)e.NewValue;
                            
                        }*/
                    }
                    //BindingOperations.SetBinding(d, TextProperty, bnd);
                }
                catch (Exception) { }
                //(d as vMixControlTextField).Update();
                ///TODO: Non defined behavior, rewrite
                /*var obj = (vMixControlTextField)d;
                if (obj._updating) return;
                var text = e.NewValue as string;
                if (obj.Paths != null && obj.State != null)
                    foreach (var item in obj.Paths)
                    {
                        var input = (Input)obj.GetValueByPath(obj.State, string.Format("Inputs[{0}]", item.A));
                        if (input != null)
                        {
                            var val = input.Elements.Where(x => (x is InputBase) && (x as InputBase).Name == item.B).FirstOrDefault();
                            if (val != null)
                            {
                                var prop = val.GetType().GetProperty(((vMixControlTextField)d).MappedProperty);
                                if (prop != null)
                                    prop.SetValue(val, text);
                            }
                        }
                    }*/
            }
        }

        internal virtual IMultiValueConverter ConverterSelector()
        {
            if (!IsTable)
                return new FirstValueConverter();
            else
                return new TableConverter();
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

                BindingOperations.ClearBinding(this, TextProperty);
                MultiBinding binding = new MultiBinding();

                binding.Converter = ConverterSelector();

                binding.Mode = BindingMode.TwoWay;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.Default;
                binding.NotifyOnSourceUpdated = true;
                binding.NotifyOnTargetUpdated = true;
                //binding.Delay = 10;


                InputBase text = null;

                //update text
                if (_paths != null && State != null)
                    foreach (var item in _paths)
                    {
                        var input = (Input)GetValueByPath(State, string.Format("Inputs[{0}]", item.A));
                        if (input != null)
                        {
                            var val = input.Elements.Where(x => (x is InputBase) && (x as InputBase).Name == item.B).FirstOrDefault();

                            if (val != null && val is InputBase && !_isTable)
                            {
                                if (text == null)
                                    text = (val as InputBase);
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

                            Binding b = new Binding(val is InputImage ? MappedImageProperty : MappedTextProperty);
                            b.Source = val;
                            b.Mode = BindingMode.TwoWay;
                            b.UpdateSourceTrigger = UpdateSourceTrigger.Default;
                            binding.Bindings.Add(b);
                        }
                    }

                BindingOperations.SetBinding(this, TextProperty, binding);

                if (text != null && !_isTable)
                {
                    var iprop = text.GetType().GetProperty(text is InputImage ? MappedImageProperty : MappedTextProperty);
                    if (iprop != null)
                        Text = (string)iprop.GetValue(text);
                }

                _updating = false;
            }
        }

        /// <summary>
        /// The <see cref="Paths" /> property's name.
        /// </summary>
        public const string PathsPropertyName = "Paths";

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
                RaisePropertyChanged(PathsPropertyName);
            }
        }

        public override UserControl[] GetPropertiesControls()
        {
            var control = GetPropertyControl<TitleMappingControl>();
            control.Titles.Clear();
            foreach (var item in _paths)
                control.Titles.Add(new Pair<string, string>(item.A, item.B));

            var control1 = GetPropertyControl<BoolControl>();
            control1.Visibility = Visibility.Visible;
            control1.Value = IsTable;
            control1.Title = LocalizationManager.Get("Table");

            return base.GetPropertiesControls().Concat(new UserControl[] { control1, control }).ToArray();
        }

        public override void SetProperties(vMixControlSettingsViewModel viewModel)
        {
            base.SetProperties(viewModel);


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
                        var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
                        dialog.Filter = "All files (*.*)|*.*";
                        var result = dialog.ShowDialog();
                        if (result.HasValue && result.Value)
                            Text = dialog.FileName;
                    }));
            }
        }

        public override void SetProperties(UserControl[] _controls)
        {
            base.SetProperties(_controls);

            _paths.Clear();
            foreach (var item in (_controls.OfType<TitleMappingControl>().First()).Titles)
                _paths.Add(new Pair<string, string>(item.A, item.B));

            _isTable = _controls.OfType<BoolControl>().First().Value;

            UpdateText(_paths);
        }

        public override void Update()
        {
            UpdateText(Paths);
            base.Update();
        }
    }
}
