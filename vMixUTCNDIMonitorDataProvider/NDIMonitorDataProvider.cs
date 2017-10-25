using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace vMixUTCNDIMonitorDataProvider
{
    public class NDIMonitorDataProvider : DependencyObject, vMixControllerDataProvider.IvMixDataProvider, IDisposable, INotifyPropertyChanged
    {
        private OnWidgetUI _ui;
        private static Random _random = new Random();
        private int _number;
        //private string _sourcePath;
        /// <summary>
        /// The <see cref="SourceName" /> property's name.
        /// </summary>
        public const string SourceNamePropertyName = "SourceName";

        private string _sourceName = "";

        /// <summary>
        /// Sets and gets the SourceName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SourceName
        {
            get
            {
                return _sourceName;
            }

            set
            {
                if (_sourceName == value)
                {
                    return;
                }

                _sourceName = value;
                Connect(_sourceName);
                RaisePropertyChanged(SourceNamePropertyName);
            }
        }

        private void RaisePropertyChanged(string sourceNamePropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(sourceNamePropertyName));
        }

        public WriteableBitmap VideoBitmap
        {
            private set;
            get;
        }

        public ObservableCollection<String> SourceNames
        {
            get { return (ObservableCollection<String>)GetValue(SourceNamesProperty); }
            set { SetValue(SourceNamesProperty, value); }
        }
        public static readonly DependencyProperty SourceNamesProperty =
            DependencyProperty.Register("SourceNames", typeof(ObservableCollection<String>), typeof(NDIMonitorDataProvider), new PropertyMetadata(new ObservableCollection<String>()));


        public System.Windows.UIElement CustomUI
        {
            get
            {
                return _ui;
            }
        }

        public bool IsProvidingCustomProperties
        {
            get
            {
                return false;
            }
        }

        public int Period
        {
            get;
            set;
        }

        public string[] Values
        {
            get
            {
                return new string[0];
            }
        }

        public List<object> GetProperties()
        {
            return new List<object>() { SourceName };
        }

        public void SetProperties(List<object> props)
        {
            if (props.Count > 0)
                SourceName = (string)props[0];
        }

        public void ShowProperties(System.Windows.Window owner)
        {
            return;
        }

        public void Dispose()
        {
            _timer.Stop();
            
            Disconnect();
            // we must free our unmanaged finder instance
            NDI.Find.NDIlib_find_destroy(_findInstancePtr);

            // Not required, but "correct". (see the SDK documentation)
            NDI.Common.NDIlib_destroy();
        }

        DispatcherTimer _timer = new DispatcherTimer();
        int _timerTicks = 0;
        public NDIMonitorDataProvider()
        {
            _number = _random.Next();
            string sourceName = SourceName;
            NDI.Common.NDIlib_initialize();
            InitFind();
            SourceName = sourceName;
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _ui = new OnWidgetUI() { DataContext = this };
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            string sourceName = SourceName;
            UpdateFindList();
            SourceName = sourceName;
            if (_recvInstancePtr != IntPtr.Zero || _timerTicks++ > 5)
            {
                _timer.Stop();
            }
        }


        #region NdiFind

        void InitFind()
        {
            // This will be IntPtr.Zero 99.999% of the time.
            // Could be one "MyGroup" or multiples "public,My Group,broadcast 42" etc.
            // Create a UTF-8 buffer from our string
            // Must use Marshal.FreeHGlobal() after use!
            // IntPtr groupsPtr = NDI.Common.StringToUtf8("public");
            IntPtr groupsPtr = IntPtr.Zero;

            // This is also optional.
            // The list of additional IP addresses that exist that we should query for 
            // sources on. For instance, if you want to find the sources on a remote machine
            // that is not on your local sub-net then you can put a comma seperated list of 
            // those IP addresses here and those sources will be available locally even though
            // they are not mDNS discoverable. An example might be "12.0.0.8,13.0.12.8".
            // When none is specified (IntPtr.Zero) the registry is used.
            // Create a UTF-8 buffer from our string
            // Must use Marshal.FreeHGlobal() after use!
            // IntPtr extraIpsPtr = NDI.Common.StringToUtf8("12.0.0.8,13.0.12.8")
            IntPtr extraIpsPtr = IntPtr.Zero;

            // how we want our find to operate
            NDI.NDIlib_find_create_t findDesc = new NDI.NDIlib_find_create_t()
            {
                // optional IntPtr to a UTF-8 string. See above.
                p_groups = groupsPtr,

                // also the ones on this computer - useful for debugging
                show_local_sources = true,

                // optional IntPtr to a UTF-8 string. See above.
                p_extra_ips = extraIpsPtr

            };

            // create our find instance
            _findInstancePtr = NDI.Find.NDIlib_find_create2(ref findDesc);

            // free our UTF-8 buffer if we created one
            if (groupsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(groupsPtr);
            }

            if (extraIpsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(extraIpsPtr);
            }

            // did it succeed?
            System.Diagnostics.Debug.Assert(_findInstancePtr != IntPtr.Zero, "Failed to create NDI find instance.");

            // attach it to our listbox
            //_ui.SourcesListBox.ItemsSource = SourceNames;

            // update the list
            UpdateFindList();
        }

        public void UpdateFindList()
        {
            int NumSources = 0;

            // ask for an update
            // timeout == 0, always return the full list
            // timeout > 0, wait timeout ms, then return 0 for no change or the total number of sources found
            IntPtr SourcesPtr = NDI.Find.NDIlib_find_get_current_sources(_findInstancePtr, ref NumSources);

            // if sources == 0, then there was no change, keep your list
            if (NumSources > 0)
            {
                // clear our list and dictionary
                SourceNames.Clear();
                _sources.Clear();

                // the size of an NDIlib_source_t, for pointer offsets
                int SourceSizeInBytes = Marshal.SizeOf(typeof(NDI.NDIlib_source_t));

                // convert each unmanaged ptr into a managed NDIlib_source_t
                for (int i = 0; i < NumSources; i++)
                {
                    // source ptr + (index * size of a source)
                    IntPtr p = IntPtr.Add(SourcesPtr, (i * SourceSizeInBytes));

                    // marshal it to a managed source and assign to our list
                    NDI.NDIlib_source_t src = (NDI.NDIlib_source_t)Marshal.PtrToStructure(p, typeof(NDI.NDIlib_source_t));

                    // .Net doesn't handle marshaling UTF-8 strings properly
                    String name = NDI.Common.Utf8ToString(src.p_ndi_name);

                    // add it to the list and dictionary
                    if (!_sources.ContainsKey(name) && !SourceNames.Contains(name))
                    {
                        _sources.Add(name, src);
                        SourceNames.Add(name);
                    }
                }
            }
        }

        #endregion NdiFind


        #region NdiReceive

        // connect to an NDI source in our Dictionary by name
        void Connect(String sourceName)
        {
            // just in case we're already connected
            Disconnect();

            // we need valid information to connect
            if (String.IsNullOrEmpty(sourceName) || !_sources.ContainsKey(sourceName))
            {
                return;
            }

            // find our new source
            NDI.NDIlib_source_t source = _sources[sourceName];

            // make a description of the receiver we want
            NDI.NDIlib_recv_create_t recvDescription = new NDI.NDIlib_recv_create_t()
            {
                // the source we selected
                source_to_connect_to = source,

                // we want BGRA frames for this example
                color_format = NDI.NDIlib_recv_color_format_e.NDIlib_recv_color_format_e_BGRX_BGRA,

                // we want full quality - for small previews or limited bandwidth, choose lowest
                bandwidth = NDI.NDIlib_recv_bandwidth_e.NDIlib_recv_bandwidth_highest
            };

            // create a new instance connected to this source
            _recvInstancePtr = NDI.Receive.NDIlib_recv_create(ref recvDescription);

            // did it work?
            System.Diagnostics.Debug.Assert(_recvInstancePtr != IntPtr.Zero, "Failed to create NDI receive instance.");

            if (_recvInstancePtr != IntPtr.Zero)
            {
                // We are now going to mark this source as being on program output for tally purposes (but not on preview)
                SetTallyIndicators(true, false);

                // start up a thread to receive on
                _receiveThread = new Thread(ReceiveThreadProc) { IsBackground = true, Name = "NdiExampleReceiveThread" };
                _receiveThread.Start();
            }
        }

        void Disconnect()
        {
            // in case we're connected, reset the tally indicators
            SetTallyIndicators(false, false);

            // check for a running thread
            if (_receiveThread != null)
            {
                // tell it to exit
                _exitThread = true;

                // wait for it to exit
                while (_receiveThread.IsAlive)
                    Thread.Sleep(100);
            }

            // reset thread defaults
            _receiveThread = null;
            _exitThread = false;

            // Destroy the receiver
            NDI.Receive.NDIlib_recv_destroy(_recvInstancePtr);

            // set it to a safe value
            _recvInstancePtr = IntPtr.Zero;
        }

        void SetTallyIndicators(bool onProgram, bool onPreview)
        {
            // we need to have a receive instance
            if (_recvInstancePtr != IntPtr.Zero)
            {
                // set up a state descriptor
                NDI.NDIlib_tally_t tallyState = new NDI.NDIlib_tally_t()
                {
                    on_program = onProgram,
                    on_preview = onPreview
                };

                // set it on the receiver instance
                NDI.Receive.NDIlib_recv_set_tally(_recvInstancePtr, ref tallyState);
            }
        }

        // the receive thread runs though this loop until told to exit
        void ReceiveThreadProc()
        {
            while (!_exitThread && _recvInstancePtr != IntPtr.Zero)
            {
                // The descriptors
                NDI.NDIlib_video_frame_t videoFrame = new NDI.NDIlib_video_frame_t();
                NDI.NDIlib_audio_frame_t audioFrame = new NDI.NDIlib_audio_frame_t();
                NDI.NDIlib_metadata_frame_t metadataFrame = new NDI.NDIlib_metadata_frame_t();

                switch (NDI.Receive.NDIlib_recv_capture(_recvInstancePtr, ref videoFrame, ref audioFrame, ref metadataFrame, 1000))
                {
                    // No data
                    case NDI.NDIlib_frame_type_e.NDIlib_frame_type_none:
                        // No data received
                        break;

                    // Video data
                    case NDI.NDIlib_frame_type_e.NDIlib_frame_type_video:

                        // this can occasionally happen when changing sources
                        if (videoFrame.p_data == IntPtr.Zero)
                        {
                            // alreays free received frames
                            NDI.Receive.NDIlib_recv_free_video(_recvInstancePtr, ref videoFrame);

                            break;
                        }

                        // get all our info so that we can free the frame
                        int yres = (int)videoFrame.yres;
                        int xres = (int)videoFrame.xres;

                        // quick and dirty aspect ratio correction for non-square pixels - SD 4:3, 16:9, etc.
                        double dpiX = 96.0 * (videoFrame.picture_aspect_ratio / ((double)xres / (double)yres));

                        int stride = (int)videoFrame.line_stride_in_bytes;
                        int bufferSize = yres * stride;

                        // We need to be on the UI thread to write to our bitmap
                        // Not very efficient, but this is just an example
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            // resize the writeable if needed
                            if (VideoBitmap == null ||
                                VideoBitmap.PixelWidth != xres ||
                                VideoBitmap.PixelHeight != yres ||
                                VideoBitmap.DpiX != dpiX)
                            {
                                VideoBitmap = new WriteableBitmap(xres, yres, dpiX, 96.0, PixelFormats.Pbgra32, null);
                                _ui.Preview.Source = VideoBitmap;
                            }

                            // update the writeable bitmap
                            VideoBitmap.WritePixels(new Int32Rect(0, 0, xres, yres), videoFrame.p_data, bufferSize, stride);

                            // free frames that were received AFTER use!
                            // This writepixels call is dispatched, so we must do it inside this scope.
                            NDI.Receive.NDIlib_recv_free_video(_recvInstancePtr, ref videoFrame);
                        }));

                        break;

                    // audio is beyond the scope of this example
                    /*case NDI.NDIlib_frame_type_e.NDIlib_frame_type_audio:

                        // if no audio, nothing to do
                        if (audioFrame.p_data == IntPtr.Zero || audioFrame.no_samples == 0)
                        {
                            // alreays free received frames
                            NDI.Receive.NDIlib_recv_free_audio(_recvInstancePtr, ref audioFrame);

                            break;
                        }

                        // if the audio format changed, we need to reconfigure the audio device
                        bool formatChanged = false;

                        // make sure our format has been created and matches the incomming audio
                        if (_waveFormat == null ||
                            _waveFormat.Channels != audioFrame.no_channels ||
                            _waveFormat.SampleRate != audioFrame.sample_rate)
                        {
                            // Create a wavformat that matches the incomming frames
                            _waveFormat = WaveFormat.CreateIeeeFloatWaveFormat((int)audioFrame.sample_rate, (int)audioFrame.no_channels);

                            formatChanged = true;
                        }

                        // set up our audio buffer if needed
                        if (_bufferedProvider == null || formatChanged)
                        {
                            _bufferedProvider = new BufferedWaveProvider(_waveFormat);
                            _bufferedProvider.DiscardOnBufferOverflow = true;
                        }

                        // set up our multiplexer used to mix down to 2 output channels)
                        if (_multiplexProvider == null || formatChanged)
                        {
                            _multiplexProvider = new MultiplexingWaveProvider(new List<IWaveProvider>() { _bufferedProvider }, 2);
                        }

                        // set up our audio output device
                        if (_wasapiOut == null || formatChanged)
                        {
                            // We can't guarantee audio sync or buffer fill, that's beyond the scope of this example.
                            // This is close enough to show that audio is received and converted correctly.
                            _wasapiOut = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 50);
                            _wasapiOut.Init(_multiplexProvider);
                            _wasapiOut.Play();
                        }

                        // we're working in bytes, so take the size of a 32 bit sample (float) into account
                        int sizeInBytes = (int)audioFrame.no_samples * (int)audioFrame.no_channels * sizeof(float);

                        // NAudio is expecting interleaved audio and NDI uses planar.
                        // create an interleaved frame and convert from the one we received
                        NDI.NDIlib_audio_frame_interleaved_32f_t interleavedFrame = new NDI.NDIlib_audio_frame_interleaved_32f_t()
                        {
                            sample_rate = audioFrame.sample_rate,
                            no_channels = audioFrame.no_channels,
                            no_samples = audioFrame.no_samples,
                            timecode = audioFrame.timecode
                        };

                        // we need a managed byte array to add to buffered provider
                        byte[] audBuffer = new byte[sizeInBytes];

                        // pin the byte[] and get a GC handle to it
                        // doing it this way saves an expensive Marshal.Alloc/Marshal.Copy/Marshal.Free later
                        // the data will only be moved once, during the fast interleave step that is required anyway
                        GCHandle handle = GCHandle.Alloc(audBuffer, GCHandleType.Pinned);

                        // access it by an IntPtr and use it for our interleaved audio buffer
                        interleavedFrame.p_data = handle.AddrOfPinnedObject();

                        // Convert from float planar to float interleaved audio
                        // There is a matching version of this that converts to interleaved 16 bit audio frames if you need 16 bit
                        NDI.Utilities.NDIlib_util_audio_to_interleaved_32f(ref audioFrame, ref interleavedFrame);

                        // release the pin on the byte[]
                        // never try to access p_data after the byte[] has been unpinned!
                        // that IntPtr will no longer be valid.
                        handle.Free();

                        // push the byte[] buffer into the bufferedProvider for output
                        _bufferedProvider.AddSamples(audBuffer, 0, sizeInBytes);

                        // free the frame that was received
                        NDI.Receive.NDIlib_recv_free_audio(_recvInstancePtr, ref audioFrame);

                        break;*/
                    // Metadata
                    case NDI.NDIlib_frame_type_e.NDIlib_frame_type_metadata:

                        // UTF-8 strings must be converted for use - length includes the terminating zero
                        //String metadata = Utf8ToString(metadataFrame.p_data, metadataFrame.length-1);

                        //System.Diagnostics.Debug.Print(metadata);

                        // free frames that were received
                        NDI.Receive.NDIlib_recv_free_metadata(_recvInstancePtr, ref metadataFrame);
                        break;
                }
            }
        }

        #endregion NdiReceive

        #region PrivateMembers

        // a pointer to our unmanaged NDI finder instance
        IntPtr _findInstancePtr = IntPtr.Zero;

        // a pointer to our unmanaged NDI receiver instance
        IntPtr _recvInstancePtr = IntPtr.Zero;

        // a thread to receive frames on so that the UI is still functional
        Thread _receiveThread = null;

        // a way to exit the thread safely
        bool _exitThread = false;

        // a map of names to sources
        Dictionary<String, NDI.NDIlib_source_t> _sources = new Dictionary<string, NDI.NDIlib_source_t>();

        public event PropertyChangedEventHandler PropertyChanged;

        // the NAudio related
        /*private WasapiOut _wasapiOut = null;
        private MultiplexingWaveProvider _multiplexProvider = null;
        private BufferedWaveProvider _bufferedProvider = null;

        // The last WaveFormat we used.
        // This may change over time, so remember how we are configured currently.
        private WaveFormat _waveFormat = null;*/

        #endregion PrivateMembers

    }
}
