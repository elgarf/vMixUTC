using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using vMixController.PropertiesControls;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlList: vMixControlTextField
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

        private ObservableCollection<string> _items = new ObservableCollection<string>();

        /// <summary>
        /// Sets and gets the Items property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<string> Items
        {
            get
            {
                return _items;
            }

            set
            {
                if (_items == value)
                {
                    return;
                }

                _items = value;
                RaisePropertyChanged(ItemsPropertyName);
            }
        }

        public override UserControl[] GetPropertiesControls()
        {

            ListControl control = GetPropertyControl<ListControl>();
            control.Items.Clear();
            foreach (var item in Items)
            {
                control.Items.Add(new Classes.DummyStringProperty() { Value = item });
            }
            return base.GetPropertiesControls().Concat(new UserControl[] { control }).ToArray();
            //return base.GetPropertiesControls().Concat(new UserControl[] { control }).ToArray(); ;
        }

        public override void SetProperties(vMixControlSettingsViewModel viewModel)
        {
            base.SetProperties(viewModel);

            
        }

        public override void SetProperties(UserControl[] _controls)
        {
            base.SetProperties(_controls);

            Items.Clear();
            foreach (var item in (_controls.OfType<ListControl>().First()).Items)
                Items.Add(item.Value);
        }

    }
}
