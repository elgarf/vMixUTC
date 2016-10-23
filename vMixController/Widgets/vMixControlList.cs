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
            var control2 = GetPropertyControl<DataSourceControl>();
            control2.Update();
            if (DataSource != null)
            {
                control2.DataSource = DataSource.A;
                control2.DataProperty = DataSource.B;
                control2.Active = DataSource.C;
            }


            ListControl control = GetPropertyControl<ListControl>();
            BindingOperations.ClearAllBindings(control);
            var binding = new Binding("Active");
            binding.Converter = new NonVisibilityConverter();
            binding.Source = control2;
            BindingOperations.SetBinding(control, ListControl.VisibilityProperty, binding);

            control.Items.Clear();
            if (Items != null)
                foreach (var item in Items)
                {
                    control.Items.Add(new Classes.DummyStringProperty() { Value = item });
                }
            return base.GetPropertiesControls().Concat(new UserControl[] { control, control2 }).ToArray();
            //return base.GetPropertiesControls().Concat(new UserControl[] { control }).ToArray(); ;
        }

        BackgroundWorker _bgWorker = new BackgroundWorker();
        //string _previousText;
        public override void ShadowUpdate()
        {
            base.ShadowUpdate();
            /*if (DataSource != null && DataSource.C && !_bgWorker.IsBusy)
            {

                _previousText = Text;
                _bgWorker.WorkerReportsProgress = true;
                _bgWorker.DoWork += (sender, bgwp) => {
                    var result = new string[5];
                    int i = 0;
                    int n = 0;

                    var data = (IList<string>)bgwp.Argument;
                    foreach (var item in data)
                    {
                        result[i++] = item;
                        if (i >= 5)
                        {
                            n++;
                            i = 0;
                            (sender as BackgroundWorker).ReportProgress(n, result);
                        }
                    }
                    bgwp.Result = null;
                };
                _bgWorker.ProgressChanged += (sender, pce) =>
                {
                    foreach (var item in (string[])pce.UserState)
                    {
                        Items.Add(item);
                    }
                };
                _bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    Text = _previousText;
                };
                Items.Clear();
                Text = _previousText;
                _bgWorker.RunWorkerAsync(Singleton<SharedData>.Instance.GetData(DataSource.A, DataSource.B));
            
            }*/
        }

        public override void Update()
        {
            base.Update();
            UpdateBinding();
        }

        public override void SetProperties(vMixControlSettingsViewModel viewModel)
        {
            base.SetProperties(viewModel);


        }

        public override void SetProperties(UserControl[] _controls)
        {
            var tb = BindingOperations.GetBindingBase(this, TextProperty);
            BindingOperations.ClearBinding(this, TextProperty);

            if (Items == null) Items = new ObservableCollection<string>();
            Items.Clear();
            foreach (var item in (_controls.OfType<ListControl>().First()).Items)
                Items.Add(item.Value);
            UpdateBinding();

            DataSource = new Triple<string, string, bool>();
            DataSource.A = _controls.OfType<DataSourceControl>().First().DataSource;
            DataSource.B = _controls.OfType<DataSourceControl>().First().DataProperty;
            DataSource.C = _controls.OfType<DataSourceControl>().First().Active;
            UpdateBinding();

            if (tb != null)
                BindingOperations.SetBinding(this, TextProperty, tb);

            base.SetProperties(_controls);
        }

        private void UpdateBinding()
        {
            BindingOperations.ClearBinding(this, ItemsProperty);
            if (DataSource == null || !DataSource.C) return;
            Binding b = new Binding(DataSource.B);
            b.Converter = new StringToCollectionConverter();
            b.Source = Singleton<SharedData>.Instance.GetDataSource(DataSource.A);
            BindingOperations.SetBinding(this, ItemsProperty, b);
        }

        public vMixControlList()
        {
            Items = new ObservableCollection<string>();
        }
    }
}
