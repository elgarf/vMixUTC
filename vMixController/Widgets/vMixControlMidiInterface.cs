using GalaSoft.MvvmLight.Messaging;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Serialization;
using vMixController.Classes;
using vMixController.Classes.Scripting;
using vMixController.Extensions;
using vMixController.Messages;

namespace vMixController.Widgets
{
    public class vMixControlMidiInterface : vMixControl
    {
        private static Dictionary<int, Melanchall.DryWetMidi.Multimedia.InputDevice> _openedDevices = new Dictionary<int, Melanchall.DryWetMidi.Multimedia.InputDevice>();
        private static Dictionary<int, int> _deviceRefCounts = new Dictionary<int, int>();
        private int _subscribedDeviceIndex = -1;

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
                RaisePropertyChanged(nameof(Midis));
            }
        }

        //private static string[] _midiDevices = null;
        public static string[] MidiDevices
        {
            get
            {

                return Melanchall.DryWetMidi.Multimedia.InputDevice.GetAll().Select(x => x.Name).ToArray();
            }
        }

        [XmlIgnore]
        public Melanchall.DryWetMidi.Multimedia.InputDevice Device { get; set; }

        private int _maxMIDIValue = 127;

        /// <summary>
        /// Sets and gets the MaxMIDIValue property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int MaxMIDIValue
        {
            get
            {
                return _maxMIDIValue;
            }

            set
            {
                if (_maxMIDIValue == value)
                {
                    return;
                }

                _maxMIDIValue = value;
                RaisePropertyChanged(nameof(MaxMIDIValue));
            }
        }

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

                if (Device != null)
                    Device.EventReceived -= Device_EventReceived;

                _deviceCaps = value;
                RaisePropertyChanged(nameof(DeviceCaps));
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

                Device = CreateDeviceByName(_midiDeviceName);

                if (Device != null)
                {
                    var idx = Array.IndexOf(MidiDevices, _midiDeviceName);
                    SubscribeToDevice(Device, idx);
                }
            }
        }

        private void Device_EventReceived(object sender, Melanchall.DryWetMidi.Multimedia.MidiEventReceivedEventArgs e)
        {

            Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in Midis)
                {
                    switch (e.Event.EventType)
                    {
                        case Melanchall.DryWetMidi.Core.MidiEventType.NoteOff:
                        case Melanchall.DryWetMidi.Core.MidiEventType.NoteOn:
                        case Melanchall.DryWetMidi.Core.MidiEventType.NoteAftertouch:
                            var note = (e.Event as Melanchall.DryWetMidi.Core.NoteEvent);
                            if (item.D != e.Event.EventType) break;
                            if (e.Event.EventType == Melanchall.DryWetMidi.Core.MidiEventType.NoteOn && note.Velocity == 0)
                                break;
                            if (note.Channel == item.A && item.B == note.NoteNumber)
                                Messenger.Default.Send(new HotkeyLinkMessage() { Link = item.C, Parameter = ScriptExecutionDispatchRuntime.CreateOutgoingParameter((byte)note.Velocity) });
                            break;
                        case Melanchall.DryWetMidi.Core.MidiEventType.ControlChange:
                            var cc = (e.Event as Melanchall.DryWetMidi.Core.ControlChangeEvent);
                            if (cc.Channel == item.A && item.B == cc.ControlNumber)
                                Messenger.Default.Send(new HotkeyLinkMessage() { Link = item.C, Parameter = ScriptExecutionDispatchRuntime.CreateOutgoingParameter((byte)cc.ControlValue) });
                            break;
                        case Melanchall.DryWetMidi.Core.MidiEventType.ProgramChange:
                            var pc = (e.Event as Melanchall.DryWetMidi.Core.ProgramChangeEvent);
                            if (pc.Channel == item.A)
                                Messenger.Default.Send(new HotkeyLinkMessage() { Link = item.C, Parameter = ScriptExecutionDispatchRuntime.CreateOutgoingParameter((byte)pc.ProgramNumber) });
                            break;
                        case Melanchall.DryWetMidi.Core.MidiEventType.PitchBend:
                            var pb = (e.Event as Melanchall.DryWetMidi.Core.PitchBendEvent);
                            if (pb.Channel == item.A)
                                Messenger.Default.Send(new HotkeyLinkMessage() { Link = item.C, Parameter = ScriptExecutionDispatchRuntime.CreateOutgoingParameter((byte)pb.PitchValue) });
                            break;
                        default:

                            break;
                    }
                }
            }));
        }

        private Melanchall.DryWetMidi.Multimedia.InputDevice CreateDeviceByName(string name)
        {
            try
            {
                var deviceNumber = MidiDevices.Select((obj, idx) => new { obj, idx }).Where(x => x.obj == name).FirstOrDefault();

                if (deviceNumber != null)
                    if (Device?.Name != name)
                    {
                        if (_openedDevices.ContainsKey(deviceNumber.idx))
                            return _openedDevices[deviceNumber.idx];
                        var device = Melanchall.DryWetMidi.Multimedia.InputDevice.GetByIndex(deviceNumber.idx);
                        _openedDevices[deviceNumber.idx] = device;
                        return device;
                    }
                    else
                        return Device;
                return null;
            }
            catch (Exception)
            {
                return Device;
            }
        }

        private void SubscribeToDevice(Melanchall.DryWetMidi.Multimedia.InputDevice device, int index)
        {
            if (device == null) return;
            device.EventReceived -= Device_EventReceived;
            device.EventReceived += Device_EventReceived;
            if (_subscribedDeviceIndex != index)
            {
                if (_subscribedDeviceIndex >= 0)
                {
                    if (_deviceRefCounts.TryGetValue(_subscribedDeviceIndex, out var old))
                    {
                        if (old <= 1)
                            _deviceRefCounts.Remove(_subscribedDeviceIndex);
                        else
                            _deviceRefCounts[_subscribedDeviceIndex] = old - 1;
                    }
                }
                _subscribedDeviceIndex = index;
                _deviceRefCounts[index] = _deviceRefCounts.TryGetValue(index, out var cur) ? cur + 1 : 1;
            }
            if (!device.IsListeningForEvents)
                device.StartEventsListening();
        }

        public vMixControlMidiInterface()
        {
            Learn = () =>
            {

                var wnd = new MidiLearnWindow(Device = CreateDeviceByName(DeviceCaps));
                var result = wnd.ShowDialog();
                if (result ?? true)
                {
                    var k = wnd.Key;
                    wnd.Close();
                    return k;
                }

                return null;
            };
        }

        public override void BeforePropertiesChanged()
        {
            Device = CreateDeviceByName(_midiDeviceName);
            if (Device != null)
            {
                var idx = Array.IndexOf(MidiDevices, _midiDeviceName);
                SubscribeToDevice(Device, idx);
            }
            base.BeforePropertiesChanged();
        }

        [XmlIgnore]
        public Func<MidiInterfaceKey> Learn { get; set; }

        public override void AfterPropertiesChanged()
        {
            MidiDeviceName = DeviceCaps;
            base.AfterPropertiesChanged();
        }

        protected override void Dispose(bool managed)
        {
            if (_disposed) return;

            if (managed)
            {
                if (Device != null)
                {
                    Device.EventReceived -= Device_EventReceived;
                    if (_subscribedDeviceIndex >= 0 && _deviceRefCounts.TryGetValue(_subscribedDeviceIndex, out var c))
                    {
                        if (c <= 1)
                        {
                            _deviceRefCounts.Remove(_subscribedDeviceIndex);
                            if (Device.IsListeningForEvents)
                                Device.StopEventsListening();
                        }
                        else
                            _deviceRefCounts[_subscribedDeviceIndex] = c - 1;
                    }
                }
                base.Dispose(managed);
                GC.SuppressFinalize(this);
            }
        }
    }

    [Serializable]
    public class MidiInterfaceKey : Quadriple<int, int, string, Melanchall.DryWetMidi.Core.MidiEventType>, ICloneable
    {
        new public object Clone()
        {
            return new MidiInterfaceKey() { A = (int)A.Copy(), B = (int)B.Copy(), C = (string)C.Copy(), D = (Melanchall.DryWetMidi.Core.MidiEventType)D.Copy() };
        }
    }
}
