using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;
using vMixAPI;
using vMixController.Classes;
using vMixController.PropertiesControls;
using vMixController.ViewModel;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlContainer: vMixWidget
    {

        
        Dictionary<int, UserControl[]> _propIndex = new Dictionary<int, UserControl[]>();

        public override string Type
        {
            get
            {
                return Extensions.LocalizationManager.Get("Container");
            }
        }

        public override double Width
        {
            get
            {
                return base.Width;
            }

            set
            {
                foreach (var item in _controls)
                    item.Width = value - 2;
                base.Width = value;
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

        /// <summary>
        /// The <see cref="Controls" /> property's name.
        /// </summary>
        public const string ControlsPropertyName = "Controls";

        private ObservableCollection<vMixWidget> _controls = new ObservableCollection<vMixWidget>();

        /// <summary>
        /// Sets and gets the Controls property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<vMixWidget> Controls
        {
            get
            {
                return _controls;
            }

            set
            {
                if (_controls == value)
                {
                    return;
                }

                _controls = value;
                RaisePropertyChanged(ControlsPropertyName);
            }
        }

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

        public override UserControl[] GetPropertiesControls()
        {

            var _filepath = GetPropertyControl<FilePathControl>();
            _filepath.Filter = "vMix Title Controller|*.vmc";
            _filepath.Value = null;

            _propIndex.Clear();
            List<UserControl> _props = new List<UserControl>();
            for (int i = 0; i < _controls.Count; i++)
            {
                var _ctrls = _controls[i].GetPropertiesControls();
                _propIndex.Add(i, _ctrls);
                var lbl = GetPropertyControl<LabelControl>();
                lbl.Title = _controls[i].Name;
                _props.Add(lbl);
                foreach (var ctrl in _ctrls)
                    ctrl.Margin = new System.Windows.Thickness(8, 0, 0, 0);
                _props.AddRange(_ctrls);
            }

            return base.GetPropertiesControls().Concat(new UserControl[] { _filepath }).Concat(_props).ToArray();
        }

        public override void SetProperties(vMixWidgetSettingsViewModel viewModel)
        {
            base.SetProperties(viewModel);

            MainWindowSettings _tmp;
            var fp = (viewModel.WidgetPropertiesControls.OfType<FilePathControl>().First()).Value;

            if (!string.IsNullOrWhiteSpace(fp))
            {
                _controls.Clear();
                foreach (var item in Utils.LoadController(fp, null, out _tmp).OrderBy(x=>x.Top))
                {
                    item.Width = Width - 2;
                    item.State = State;
                    item.IsCaptionVisible = false;
                    item.Locked = true;
                    _controls.Add(item);
                }
            }
            else
            {
                for (int i = 0; i < _controls.Count; i++)
                {
                    /*_controls[i].Color = Color;
                    _controls[i].BorderColor = BorderColor;*/
                    if (_propIndex.Count > i)
                    _controls[i].SetProperties(_propIndex[i]);
                    _propIndex[i] = null;
                    //_controls[i].SetProperties(_propIndex[i]);
                }
            }
        }

        protected override void Dispose(bool managed)
        {
            base.Dispose(managed);
        }
    }
}
