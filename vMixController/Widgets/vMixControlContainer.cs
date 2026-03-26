using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlContainer: vMixControl
    {

        
        Dictionary<int, UserControl[]> _propIndex = new Dictionary<int, UserControl[]>();

        public override string Type
        {
            get
            {
                return "Container";
            }
        }

        public override bool Locked
        {
            get
            {
                return base.Locked;
            }

            set
            {
                foreach (var item in _controls)
                    item.Locked = value;
                base.Locked = value;
            }
        }

        [XmlIgnore]
        public override State State
        {
            get
            {
                return base.State;
            }

            set
            {
                foreach (var item in _controls)
                    item.State = value;
                base.State = value;
            }
        }

        private ObservableCollection<vMixControl> _controls = new ObservableCollection<vMixControl>();

        /// <summary>
        /// Sets and gets the Controls property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<vMixControl> Controls
        {
            get => _controls;
            set => SetPropertyValue(ref _controls, value, nameof(Controls));
        }

        public string FilePath { get; set; }

        public vMixControlContainer()
        {

        }

        public override Hotkey[] GetHotkeys()
        {
            return new Classes.Hotkey[] { };
        }

        public override void ExecuteHotkey(int index)
        {
            base.ExecuteHotkey(index);
        }

        public override void BeforePropertiesChanged()
        {
            base.BeforePropertiesChanged();
        }

        public override void AfterPropertiesChanged()
        {
            base.AfterPropertiesChanged();
            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                _controls.Clear();
                var loaded = Utils.LoadController(FilePath, null, out MainWindowSettings _tmp).OrderBy(x => x.Top);
                var minx = loaded.Select(x => x.Left).Min();
                var miny = loaded.Select(x => x.Top).Min();
                var w = loaded.Select(x => x.Left + x.Width).Max();
                Width = w - minx + 8;
                foreach (var item in loaded)
                {
                    //item.Width = Width - 2;
                    item.State = State;
                    item.Left -= minx;
                    item.Top -= miny;
                    //item.IsCaptionVisible = false;
                    item.Locked = false;
                    _controls.Add(item);
                }
            }
            FilePath = null;
        }

        protected override void Dispose(bool managed)
        {
            base.Dispose(managed);
        }
    }
}
