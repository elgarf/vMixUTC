using System;
using System.Collections.Generic;
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
        private Sanford.Multimedia.Midi.InputDevice _device;
        private bool _doNotDispose;

        public Widgets.MidiInterfaceKey Key { get; set; }

        public MidiLearnWindow()
        {
            InitializeComponent();
            Topmost = Application.Current?.MainWindow?.Topmost ?? false;
            Activate();
        }

        public MidiLearnWindow(Sanford.Multimedia.Midi.InputDevice device, bool doNotDispose = true)
        {
            InitializeComponent();
            _doNotDispose = doNotDispose;
            if (device != null)
            {
                _device = device;
                _device.ChannelMessageReceived += Device_ChannelMessageReceived;
                _device.Reset();
                _device.StartRecording();
            }
        }

        private void Device_ChannelMessageReceived(object sender, Sanford.Multimedia.Midi.ChannelMessageEventArgs e)
        {
            //Data1 = control
            Key = new Widgets.MidiInterfaceKey() { A = e.Message.MidiChannel, B = e.Message.Data1, D = e.Message.Command };
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
                _device.ChannelMessageReceived -= Device_ChannelMessageReceived;
                if (!_doNotDispose)
                {
                    _device.Reset();
                    _device.Dispose();
                }
            }
        }
    }
}
