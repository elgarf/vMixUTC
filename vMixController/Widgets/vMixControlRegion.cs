using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Controls;

namespace vMixController.Widgets
{
    public partial class vMixControlRegion: vMixControl
    {
        public vMixControlRegion()
        {
            ZIndex = -1;
        }

        public override string Type
        {
            get
            {
                return "Region";
            }
        }

        public override bool IsResizeableVertical => true;

        private string _Text = "";

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Text
        {
            get => _Text;
            set => SetPropertyValue(ref _Text, value, nameof(Text));
        }

        private bool _sticky = false;

        /// <summary>
        /// Sets and gets the Magnet property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Sticky
        {
            get => _sticky;
            set => SetPropertyValue(ref _sticky, value, nameof(Sticky));
        }

        private bool _isEditable = false;

        /// <summary>
        /// Sets and gets the Magnet property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsEditable
        {
            get => _isEditable;
            set => SetPropertyValue(ref _isEditable, value, nameof(IsEditable));
        }

        [RelayCommand]
        private void MouseDoubleClick(object p)
        {
            IsEditable = true;
        }

        

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        public override void AfterPropertiesChanged()
        {
            base.AfterPropertiesChanged();
        }

        public override void Update()
        {
            Height++;
            Height--;
            base.Update();
        }
    }
}

