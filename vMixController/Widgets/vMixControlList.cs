using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using vMixController.Classes;
using vMixController.Converters;
using vMixController.PropertiesControls;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlList : vMixControlTextField
    {
        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("List");
            }
        }
        /// <summary>
        /// The <see cref="Items" /> property's name.
        /// </summary>
        public const string ItemsPropertyName = "Items";

        //private ObservableCollection<string> _items = new ObservableCollection<string>();

        /// <summary>
        /// Sets and gets the Items property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> Items
        {
            get
            {
                return (ObservableCollection<string>)GetValue(ItemsProperty);
            }

            set
            {
                /*if (_items == value)
                {
                    return;
                }*/

                //_items = value;
                SetValue(ItemsProperty, value);
                //RaisePropertyChanged(ItemsPropertyName);
            }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<string>), typeof(vMixControlTextField), new PropertyMetadata(null));

        public Triple<string, string, bool> DataSource { get; set; }

        public override UserControl[] GetPropertiesControls()
        {
            var dataSource = GetPropertyControl<DataSourceControl>();
            dataSource.Update();
            if (DataSource != null)
            {
                dataSource.DataSource = DataSource.A;
                dataSource.DataProperty = DataSource.B;
                dataSource.Active = DataSource.C;
            }


            ListControl itemsList = GetPropertyControl<ListControl>();
            BindingOperations.ClearAllBindings(itemsList);
            var binding = new Binding("Active")
            {
                Converter = new NKristek.Wpf.Converters.BoolToInverseVisibilityConverter(),
                Source = dataSource
            };
            BindingOperations.SetBinding(itemsList, ListControl.VisibilityProperty, binding);

            itemsList.Items.Clear();
            if (Items != null)
                foreach (var item in Items)
                {
                    itemsList.Items.Add(new Classes.DummyStringProperty() { Value = item });
                }



            return base.GetPropertiesControls().Concat(new UserControl[] { itemsList, dataSource }).ToArray();
            //return base.GetPropertiesControls().Concat(new UserControl[] { control }).ToArray(); ;
        }

        public override void Update()
        {
            base.Update();
            UpdateBinding();
        }

        public override void SetProperties(vMixWidgetSettingsViewModel viewModel)
        {
            base.SetProperties(viewModel);


        }

        internal override IMultiValueConverter ConverterSelector()
        {
            if (!IsTable)
                return new FirstValueConverter(true);
            else
                return new StringsToStringConverter(true);
        }

        public override void SetProperties(UserControl[] _controls)
        {
            var tb = BindingOperations.GetBindingBase(this, TextProperty);
            BindingOperations.ClearBinding(this, TextProperty);

            if (Items == null) Items = new ObservableCollection<string>();

            //Update Items instead of clearing
            var newItems = (_controls.OfType<ListControl>().First()).Items;
            for (int i = 0; i < Math.Max(Items.Count, newItems.Count); i++)
            {
                if (i < Math.Min(Items.Count, newItems.Count))
                {
                    if (Items[i] != newItems[i].Value)
                        Items[i] = newItems[i].Value;
                }
                else if (Items.Count < newItems.Count)
                    Items.Add(newItems[i].Value);
            }
            while (Items.Count > newItems.Count)
                Items.RemoveAt(Items.Count - 1);

            //UpdateBinding();

            DataSource = new Triple<string, string, bool>
            {
                A = _controls.OfType<DataSourceControl>().First().DataSource,
                B = _controls.OfType<DataSourceControl>().First().DataProperty,
                C = _controls.OfType<DataSourceControl>().First().Active
            };
            UpdateBinding();

            if (tb != null)
                BindingOperations.SetBinding(this, TextProperty, tb);

            base.SetProperties(_controls);
        }

        private void UpdateBinding()
        {
            BindingOperations.ClearBinding(this, ItemsProperty);
            if (DataSource == null || !DataSource.C) return;
            Binding b = new Binding(DataSource.B)
            {
                Converter = new StringToCollectionConverter(),
                Source = Singleton<SharedData>.Instance.GetDataSource(DataSource.A)
            };
            BindingOperations.SetBinding(this, ItemsProperty, b);
        }

        public vMixControlList()
        {
            Items = new ObservableCollection<string>();
        }
    }
}
