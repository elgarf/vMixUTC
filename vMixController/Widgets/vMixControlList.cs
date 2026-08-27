using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using vMixController.Classes;
using vMixController.Converters;
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
                return "List";
            }
        }

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
                SetValue(ItemsProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<string>), typeof(vMixControlList), new PropertyMetadata(new ObservableCollection<string>()));


        /// <summary>
        /// Sets and gets the SelectedIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return (int)GetValue(SelectedIndexProperty);
            }

            set
            {
                SetValue(SelectedIndexProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(vMixControlList), new PropertyMetadata(-1));

        public Triple<string, string, bool> DataSource { get; set; }

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        public override void Update()
        {
            base.Update();
            UpdateBinding();
        }

        internal override IMultiValueConverter ConverterSelector()
        {
            if (!IsTable)
                return new FirstValueConverter(true);
            else
                return new StringsToStringConverter(true);
        }

        public override void AfterPropertiesChanged()
        {
            var tb = BindingOperations.GetBindingBase(this, TextProperty);
            BindingOperations.ClearBinding(this, TextProperty);
            UpdateBinding();

            if (tb != null)
                BindingOperations.SetBinding(this, TextProperty, tb);

            base.AfterPropertiesChanged();
        }

        private void UpdateBinding()
        {
            BindingOperations.ClearBinding(this, ItemsProperty);
            if (DataSource == null || !DataSource.C) return;
            Binding b = new Binding(DataSource.B)
            {
                Converter = new StringToCollectionConverter(),
                Source = Singleton<SharedData>.Instance.GetDataSource(DataSource.A),
                NotifyOnTargetUpdated = true,
                NotifyOnSourceUpdated = true
            };
            BindingOperations.SetBinding(this, ItemsProperty, b);
            
            
        }

        public vMixControlList()
        {
            Items = new ObservableCollection<string>();
            DataSource = new Triple<string, string, bool>();
        }

    }
}
