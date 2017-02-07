using GalaSoft.MvvmLight.Messaging;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Serialization;
using vMixController.Classes;
using vMixController.PropertiesControls;

namespace vMixController.Widgets
{
    public class vMixControlMidiInterface : vMixControl
    {

        /// <summary>
        /// The <see cref="Midis" /> property's name.
        /// </summary>
        public const string MidisPropertyName = "Midis";

        private ObservableCollection<MidiInterfaceKey> _midis = new ObservableCollection<MidiInterfaceKey>();

        /// <summary>
        /// Sets and gets the Midis property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<MidiInterfaceKey> Midis
        {
            get
            {
                return _midis;
            }

            set
            {
                if (_midis == value)
                {
                    return;
                }

                _midis = value;
                RaisePropertyChanged(MidisPropertyName);
            }
        }

        private static string[] _midiDevices = null;
        public static string[] MidiDevices
        {
            get
            {
                if (_midiDevices == null)
                {
                    _midiDevices = new string[InputDevice.DeviceCount];
                    for (int i = 0; i < _midiDevices.Length; i++)
                        _midiDevices[i] = InputDevice.GetDeviceCapabilities(i).name;
                }

                return _midiDevices;
            }
        }

        [XmlIgnore]
        public InputDevice Device { get; set; }


        /// <summary>
            /// The <see cref="DeviceCaps" /> property's name.
            /// </summary>
        public const string DeviceCapsPropertyName = "DeviceCaps";

        private string _deviceCaps = "";

        /// <summary>
        /// Sets and gets the DeviceCaps property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string DeviceCaps
        {
            get
            {
                return _deviceCaps;
            }

            set
            {
                if (_deviceCaps == value)
                {
                    return;
                }

                _deviceCaps = value;
                RaisePropertyChanged(DeviceCapsPropertyName);
            }
        }

        public override string Type
        {
            get
            {
                return "MIDI Device";
            }
        }

        string _midiDeviceName;
        public string MidiDeviceName
        {
            get { return _midiDeviceName; }
            set
            {
                if (_midiDeviceName == value) return;
                _midiDeviceName = value;
                if (Device != null)
                {
                    Device.Close();
                    Device.Dispose();
                }

                Device = CreateDeviceByName(_midiDeviceName);

                if (Device != null)
                {
                    Device.Reset();
                    Device.StartRecording();
                    Device.ChannelMessageReceived += Device_ChannelMessageReceived;

                }
            }
        }

        private void Device_ChannelMessageReceived(object sender, ChannelMessageEventArgs e)
        {
            foreach (var item in Midis)
            {
                if (e.Message.MidiChannel == item.A && e.Message.Data1 == item.B && e.Message.Command == item.D)
                    Messenger.Default.Send<string>(item.C);
            }
        }

        private InputDevice CreateDeviceByName(string name)
        {
            var deviceNumber = MidiDevices.Select((obj, idx) => new { obj = obj, idx = idx }).Where(x => x.obj == name).FirstOrDefault();
            if (deviceNumber != null)
                return new InputDevice(deviceNumber.idx);
            return null;
        }

        public vMixControlMidiInterface()
        {

        }

        public override UserControl[] GetPropertiesControls()
        {

            if (Device != null)
            {
                Device.Reset();
                Device.Dispose();
                Device = null;
            }

            var midiDeviceSelector = new ComboBox();
            midiDeviceSelector.ItemsSource = MidiDevices;
            midiDeviceSelector.SelectedValue = Device;

            Binding bnd = new Binding("DeviceCaps");
            bnd.Source = this;
            BindingOperations.SetBinding(midiDeviceSelector, ComboBox.SelectedValueProperty, bnd);

            var midiMappingCtrl = GetPropertyControl<MidiMappingControl>();
            midiMappingCtrl.LearnFunction = Learn;


            midiMappingCtrl.Midis.Clear();
            foreach (var item in Midis)
            {
                midiMappingCtrl.Midis.Add(item);
            }
            /*Binding bnd = new Binding("Device");
            bnd.Source = this;
            BindingOperations.SetBinding(midiDeviceSelector, ComboBox.SelectedValueProperty, bnd);*/
            return base.GetPropertiesControls().Union(new UserControl[] { new UserControl() { Tag = "DeviceSelector", Content = midiDeviceSelector }, midiMappingCtrl }).ToArray();
        }

        private MidiInterfaceKey Learn()
        {

            var wnd = new MidiLearnWindow(CreateDeviceByName(DeviceCaps));
            var result = wnd.ShowDialog();
            if (result ?? true)
            {
                var k = wnd.Key;
                wnd.Close();
                return k;
            }

            return null;
            //throw new NotImplementedException();
        }

        public override void SetProperties(UserControl[] _controls)
        {
            MidiDeviceName = (string)(_controls.Where(x => (string)x.Tag == "DeviceSelector").First().Content as ComboBox).SelectedValue;
            var ctrl = _controls.OfType<MidiMappingControl>().First();
            Midis.Clear();
            foreach (var item in ctrl.Midis)
            {
                Midis.Add(item);
            }
            base.SetProperties(_controls);
        }

        public override void Dispose()
        {
            if (Device != null)
            {
                Device.Reset();
                Device.Dispose();
            }
            
            base.Dispose();
        }
    }

    public class MidiInterfaceKey : Quadriple<int, int, string, Sanford.Multimedia.Midi.ChannelCommand>
    {

    }
}
