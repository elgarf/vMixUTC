using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using vMixController.Classes;
using System.Windows.Controls;
using vMixController.Extensions;
using GalaSoft.MvvmLight.CommandWpf;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.ComponentModel;
using System.Threading;
using vMixController.PropertiesControls;

namespace vMixController.Widgets
{
    [Serializable]
    public class vMixControlTimecode : vMixControl
    {
        public override string Type => "Timecode";
        public override int MaxCount => 1;

        //private string _lastExecuted = null;
        [NonSerialized]
        private SMPTEReader _reader = new SMPTEReader(1, 2);
        [NonSerialized]
        private DispatcherTimer _worker;
        [NonSerialized]
        private WasapiCapture _capture;
        [NonSerialized]
        private DateTime _lastFrameReceived;

        /// <summary>
        /// The <see cref="Events" /> property's name.
        /// </summary>
        public const string EventsPropertyName = "Events";

        private ObservableCollection<Pair<DateTime, string>> _events = new ObservableCollection<Pair<DateTime, string>>();

        /// <summary>
        /// Sets and gets the Events property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<Pair<DateTime, string>> Events
        {
            get
            {
                return _events;
            }

            set
            {
                if (_events == value)
                {
                    return;
                }

                _events = value;
                RaisePropertyChanged(EventsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FrameRate" /> property's name.
        /// </summary>
        public const string FrameRatePropertyName = "FrameRate";

        private int _framerate = 30;

        /// <summary>
        /// Sets and gets the FrameRate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int FrameRate
        {
            get
            {
                return _framerate;
            }

            set
            {
                if (_framerate == value)
                {
                    return;
                }

                _framerate = value;
                RaisePropertyChanged(FrameRatePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="AudioDevice" /> property's name.
        /// </summary>
        public const string AudioDevicePropertyName = "AudioDevice";

        private string _audioDevice = "";

        /// <summary>
        /// Sets and gets the AudioDevice property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string AudioDevice
        {
            get
            {
                return _audioDevice;
            }

            set
            {
                if (_audioDevice == value)
                {
                    return;
                }

                _audioDevice = value;
                RaisePropertyChanged(AudioDevicePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="NextEventAt" /> property's name.
        /// </summary>
        public const string NextEvetnAtPropertyName = "NextEventAt";

        private string _nextEventAt = "";

        private bool _cancelled = false;

        /// <summary>
        /// Sets and gets the NextEvetnAt property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string NextEventAt
        {
            get
            {
                return _nextEventAt;
            }

            set
            {
                if (_nextEventAt == value)
                {
                    return;
                }

                _nextEventAt = value;
                RaisePropertyChanged(NextEvetnAtPropertyName);
            }
        }

        public vMixControlTimecode()
        {
            _worker = new DispatcherTimer(DispatcherPriority.Input);
            _worker.Interval = TimeSpan.FromSeconds(0.5 / FrameRate);
            _worker.Tick += _worker_Tick;
            _worker.Start();

            //_worker.DoWork += Worker_DoWork;
            _lastFrameReceived = DateTime.Now;
        }

        private void _worker_Tick(object sender, EventArgs e)
        {
            /*var currentFrame = FramesToTimecode(TimecodeToFrames(_timecode, FrameRate) + (int)((DateTime.Now - _lastFrameReceived).TotalSeconds * FrameRate), FrameRate);
            foreach (var item in _events)
            {
                //prevent executing one item twice
                if (item.A.Hour == _timecode.Hours && item.A.Minute == _timecode.Minutes && item.A.Second == _timecode.Seconds && item.A.Millisecond == _timecode.Frame)
                {
                    //_lastExecuted = item.A.ToShortDateString() + item.B;
                    Messenger.Default.Send(item.B);
                }

            }
            NextEventAt = currentFrame.ToString();*/
        }



        public override UserControl[] GetPropertiesControls()
        {
            //_timer.Stop();


            /*
            _capture = new WasapiCapture(ep[0]);
            _capture.WaveFormat = new WaveFormat(44100, 8, 2);
            //lc.WaveFormat = new NAudio.Wave.WaveFormat(44100, 8, 2);
            _capture.DataAvailable += Lc_DataAvailable;
            _capture.StartRecording();*/

            /*_cancelled = true;
            if (_capture?.CaptureState == CaptureState.Capturing)
            {
                _capture?.StopRecording();
                _capture.Dispose();
            }*/


            MMDeviceEnumerator e = new MMDeviceEnumerator();
            var ep = e.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            ComboBoxControl mmi = GetPropertyControl<ComboBoxControl>();
            mmi.Title = Extensions.LocalizationManager.Get("Audio Device");
            mmi.Items = ep.Select(x => x.FriendlyName);
            mmi.Value = _audioDevice;

            var ctrl = GetPropertyControl<PropertiesControls.SchedulerControl>();
            ctrl.Events.Clear();
            foreach (var item in _events)
            {
                ctrl.Events.Add(item);
            }
            return base.GetPropertiesControls().Union(new UserControl[] { mmi, ctrl }).ToArray();
        }

        private void Lc_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            if (e.BytesRecorded == 0) return;

            byte[] downSampled = new byte[e.BytesRecorded / 2];
            for (int i = 0; i < e.BytesRecorded / 2; i++)
            {
                downSampled[i] = e.Buffer[i * 2 + 0];
            }
            _reader.ProcessFrame(downSampled, e.BytesRecorded / 2  - (e.BytesRecorded / 2) % 2);
            NextEventAt = _reader.Timecode;
            //NextEventAt = now.ToString();

        }


    public override void SetProperties(UserControl[] _controls)
        {
            MMDeviceEnumerator e = new MMDeviceEnumerator();
            var ep = e.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            base.SetProperties(_controls);
            var ctrl = _controls.OfType<PropertiesControls.SchedulerControl>().First();
            var mmi = _controls.OfType<PropertiesControls.ComboBoxControl>().First();

            AudioDevice = (string)mmi.Value;

            _cancelled = true;
            var device = ep.Where(x => x.FriendlyName == AudioDevice).FirstOrDefault();
            if (device != null)
            {
                _cancelled = false;
                _capture = new WasapiCapture(device);
                _capture.WaveFormat = new WaveFormat(44100, 8, 2);

                //lc.WaveFormat = new NAudio.Wave.WaveFormat(44100, 8, 2);
                _capture.DataAvailable += Lc_DataAvailable;
                _capture.StartRecording();
            }


            _events.Clear();
            foreach (var item in ctrl.Events.OrderBy(x => x.A.Ticks))
            {
                _events.Add(item);
            }
        }

        protected override void Dispose(bool managed)
        {
            if (_disposed) return;

            if (managed)
            {
                _cancelled = true;

                //_worker.CancelAsync();

                //_worker?.Join(500);
                _worker.Stop();
                if (_capture?.CaptureState == CaptureState.Capturing)
                    _capture?.StopRecording();

                base.Dispose(managed);
                GC.SuppressFinalize(this);
            }
        }
    }
}
