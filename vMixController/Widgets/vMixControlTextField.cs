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

        internal bool _updating = false;

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

        internal virtual string MappedProperty { get { return "Text"; } }

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
            if (e.Property.Name == "Text")
            {

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

        internal virtual void UpdateText(IList<Pair<int, string>> _paths)
        {
            if (!_updating)
            {
                _updating = true;

                BindingOperations.ClearBinding(this, TextProperty);
                MultiBinding binding = new MultiBinding();
                if (!IsTable)
                    binding.Converter = new FirstValueConverter();
                else
                    binding.Converter = new TableConverter();
                binding.Mode = BindingMode.TwoWay;

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
                                    var prop = val.GetType().GetProperty(MappedProperty);
                                    if (prop != null)
                                    {
                                        var iprop = text.GetType().GetProperty(MappedProperty);
                                        if (iprop != null)
                                            prop.SetValue(val, iprop.GetValue(text));
                                    }
                                }
                            }

                            Binding b = new Binding(MappedProperty);
                            b.Source = val;
                            b.Mode = BindingMode.TwoWay;
                            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                            binding.Bindings.Add(b);
                        }
                    }

                BindingOperations.SetBinding(this, TextProperty, binding);

                if (text != null && !_isTable)
                {
                    var iprop = text.GetType().GetProperty(MappedProperty);
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

        private ObservableCollection<Pair<int, string>> _paths = new ObservableCollection<Pair<int, string>>();

        /// <summary>
        /// Sets and gets the Paths property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Pair<int, string>> Paths
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
                control.Titles.Add(new Pair<int, string>(item.A, item.B));

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

        public override void SetProperties(UserControl[] _controls)
        {
            base.SetProperties(_controls);

            _paths.Clear();
            foreach (var item in (_controls.OfType<TitleMappingControl>().First()).Titles)
                _paths.Add(new Pair<int, string>(item.A, item.B));

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
