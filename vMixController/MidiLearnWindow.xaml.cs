using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace vMixController
{
    /// <summary>
    /// Логика взаимодействия для MidiLearnWindow.xaml
    /// </summary>
    public partial class MidiLearnWindow : Window
    {
        private Melanchall.DryWetMidi.Devices.InputDevice _device;//Sanford.Multimedia.Midi.InputDevice _device;
        private bool _doNotDispose;

        public Widgets.MidiInterfaceKey Key { get; set; }

        public MidiLearnWindow()
        {
            InitializeComponent();
            Topmost = Application.Current?.MainWindow?.Topmost ?? false;
            Activate();
        }

        public MidiLearnWindow(Melanchall.DryWetMidi.Devices.InputDevice device, bool doNotDispose = true)
        {
            InitializeComponent();
            _doNotDispose = doNotDispose;
            if (device != null)
            {
                _device = device;
                //_device.ChannelMessageReceived += Device_ChannelMessageReceived;
                //_device.Reset();
                if (!_device.IsListeningForEvents)
                    _device.StartEventsListening();
                _device.EventReceived += _device_EventReceived;
            }
        }

        private void _device_EventReceived(object sender, Melanchall.DryWetMidi.Devices.MidiEventReceivedEventArgs e)
        {
            Debug.WriteLine(e.Event.ToString());
            int A = -1;
            int B = -1;
            switch (e.Event.EventType)
            {
                case Melanchall.DryWetMidi.Core.MidiEventType.NoteOff:
                case Melanchall.DryWetMidi.Core.MidiEventType.NoteOn:
                case Melanchall.DryWetMidi.Core.MidiEventType.NoteAftertouch:
                    var note = (Melanchall.DryWetMidi.Core.NoteEvent)e.Event;
                    A = note.Channel;
                    B = note.NoteNumber;
                    break;
                case Melanchall.DryWetMidi.Core.MidiEventType.ControlChange:
                    var control = (Melanchall.DryWetMidi.Core.ControlChangeEvent)e.Event;
                    A = control.Channel;
                    B = control.ControlNumber;
                    break;
                case Melanchall.DryWetMidi.Core.MidiEventType.ProgramChange:
                    var pc = (Melanchall.DryWetMidi.Core.ChannelEvent)e.Event;
                    A = pc.Channel;
                    B = -1;
                    break;
                case Melanchall.DryWetMidi.Core.MidiEventType.PitchBend:
                    var pb = (Melanchall.DryWetMidi.Core.PitchBendEvent)e.Event;
                    A = pb.Channel;
                    break;
                default:
                    //B = mm.;
                    break;
            }
            Dispatcher.Invoke(() => Key = new Widgets.MidiInterfaceKey() { A = A, B = B, D = e.Event.EventType });
            try
            {
                Dispatcher.Invoke(() => DialogResult = true);
            }
            catch (Exception) { }
            //throw new NotImplementedException();
        }

        private void Device_ChannelMessageReceived(object sender, Sanford.Multimedia.Midi.ChannelMessageEventArgs e)
        {
            //Data1 = control

            //Key = new Widgets.MidiInterfaceKey() { A = e.Message.MidiChannel, B = e.Message.Data1, D = e.Message.Command };
            try
            {
                DialogResult = true;
            }
            catch (Exception) { }
        }



        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_device != null)
            {
                //_device.ChannelMessageReceived -= Device_ChannelMessageReceived;
                _device.EventReceived -= _device_EventReceived;
                if (!_doNotDispose)
                {
                    _device.Reset();
                    _device.Dispose();
                }
            }
        }
    }
}
