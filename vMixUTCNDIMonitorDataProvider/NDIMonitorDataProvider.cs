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

            // how we want our find to operate
            NDI.NDIlib_find_create_t findDesc = new NDI.NDIlib_find_create_t()
            {
                // Needs an IntPtr to a UTF-8 string
                p_groups = groupsPtr,

                // also the ones on this computer - useful for debugging
                show_local_sources = true
            };

            // create our find instance
            _findInstancePtr = NDI.Find.NDIlib_find_create(ref findDesc);

            // free our UTF-8 buffer if we created one
            if (groupsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(groupsPtr);
            }

            // did it succeed?
            System.Diagnostics.Debug.Assert(_findInstancePtr != IntPtr.Zero, "Failed to create NDI find instance.");

            // attach it to our listbox
            //SourcesListBox.ItemsSource = SourceNames;
            // update the list
            UpdateFindList();
        }

        public void UpdateFindList()
        {
            string _src = _sourceName;
            int NumSources = 0;

            // ask for an update
            // timeout == 0, always return the full list
            // timeout > 0, wait timeout ms, then return 0 for no change or the total number of sources found
            IntPtr SourcesPtr = NDI.Find.NDIlib_find_get_sources(_findInstancePtr, ref NumSources, 0);

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
            SourceName = _src;
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
                prefer_UYVY = false,

                // we want full quality - for small previews or limited bandwidth, choose lowest
                bandwidth = NDI.NDIlib_recv_bandwidth_e.NDIlib_recv_bandwidth_lowest
            };

            // create a new instance connected to this source
            _recvInstancePtr = NDI.Receive.NDIlib_recv_create(ref recvDescription);

            // did it work?
            System.Diagnostics.Debug.Assert(_recvInstancePtr != IntPtr.Zero, "Failed to create NDI receive instance.");

            if (_recvInstancePtr != IntPtr.Zero)
            {
                // We are now going to mark this source as being on program output for tally purposes (but not on preview)
                SetTallyIndicators(false, true);

                // start up a thread to receive on
                _receiveThread = new Thread(ReceiveThreadProc) { IsBackground = true, Name = string.Format("NdiMonitor{0}", _number) };
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
                        if (videoFrame.p_data != IntPtr.Zero)
                        {
                            // get all our info so that we can free the frame
                            int yres = (int)videoFrame.yres;
                            int xres = (int)videoFrame.xres;

                            // quick and dirty aspect ratio correction for non-square pixels - SD 4:3, 16:9, etc.
                            double dpiX = 96.0 * (videoFrame.picture_aspect_ratio / ((double)xres / (double)yres));

                            int stride = (int)videoFrame.line_stride_in_bytes;
                            int bufferSize = yres * stride;

                            // copy the bitmap out
                            Byte[] buffer = new Byte[bufferSize];
                            Marshal.Copy(videoFrame.p_data, buffer, 0, bufferSize);

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
                                    //VideoSurface.Source = VideoBitmap;
                                }

                                // update the writeable bitmap
                                VideoBitmap.WritePixels(new Int32Rect(0, 0, xres, yres), buffer, stride, 0, 0);
                                _ui.UpdatePreview(VideoBitmap);
                            }));
                        }

                        // free frames that were received
                        NDI.Receive.NDIlib_recv_free_video(_recvInstancePtr, ref videoFrame);
                        break;

                    // audio is beyond the scope of this example
                    case NDI.NDIlib_frame_type_e.NDIlib_frame_type_audio:

                        // free frames that were received
                        NDI.Receive.NDIlib_recv_free_audio(_recvInstancePtr, ref audioFrame);
                        break;

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

        #endregion PrivateMembers


    }
}
