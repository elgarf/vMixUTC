using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace vMixUTCNDIMonitorDataProvider.NDI
{
    // thr current options for receive quality
    public enum NDIlib_recv_bandwidth_e : uint
    {
        NDIlib_recv_bandwidth_lowest = 0,
        NDIlib_recv_bandwidth_highest = 100			// Default
    };

    // The creation structure that is used when you are creating a receiver
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct NDIlib_recv_create_t
    {
        // The source that you wish to connect to.
        public NDIlib_source_t source_to_connect_to;

        // What color space is your preference ?
        //
        //	prefer_UYVY == true
        //		No Alpha channel   : UYVY
        //		With Alpha channel : BGRA
        //
        //	prefer_UYVY == false 
        //		No Alpha channel   : BGRA
        //		With Alpha channel : BGRA
        [MarshalAsAttribute(UnmanagedType.Bool)]
        public bool prefer_UYVY;

        // The bandwidth setting that you wish to use for this video source. Bandwidth
        // controlled by changing both the compression level and the resolution of the source.
        // A good use for low bandwidth is working on WIFI connections or small previews.
        public NDIlib_recv_bandwidth_e bandwidth;
    }

    // This allows you determine the current performance levels of the receiving to be able to detect whether frames have been dropped
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct NDIlib_recv_performance_t
    {
        // The number of video frames
        public long m_video_frames;

        // The number of audio frames
        public long m_audio_frames;

        // The number of meta-data frames
        public long m_metadata_frames;
    }

    // Get the current queue depths
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct NDIlib_recv_queue_t
    {   // The number of video frames
        public long m_video_frames;

        // The number of audio frames
        public long m_audio_frames;

        // The number of metadata frames
        public long m_metadata_frames;
    };

    public static class Receive
    {
        // Create a new receiver instance. This will return null if it fails.
        public static IntPtr NDIlib_recv_create(ref NDIlib_recv_create_t p_create_settings)
        {
            if (IntPtr.Size == 8)
                return NDIlib64_recv_create(ref p_create_settings);
            else
                return NDIlib32_recv_create(ref p_create_settings);
        }

        // This will destroy an existing receiver instance.
        public static void NDIlib_recv_destroy(IntPtr p_instance)
        {
            if (IntPtr.Size == 8)
                NDIlib64_recv_destroy(p_instance);
            else
                NDIlib32_recv_destroy(p_instance);
        }

        // This will allow you to receive video, audio and meta-data frames.
        // Any of the buffers can be NULL, in which case data of that type
        // will not be captured in this call. This call can be called simultaneouslt
        // on seperate threads, so it is entirely possible to receive audio, video, metadata
        // all on seperate threads. This function will return NDIlib_frame_type_none if no
        // data is received within the specified timeout. Buffers captured with this must
        // be freed with the appropriate free function below.
        public static NDIlib_frame_type_e NDIlib_recv_capture(
                        IntPtr p_instance,                          // The library instance
                        ref NDIlib_video_frame_t p_video_data,      // The video data received (can be null)
                        ref NDIlib_audio_frame_t p_audio_data,		// The audio data received (can be null)
                        ref NDIlib_metadata_frame_t p_meta_data,    // The meta data data received (can be null)
                        uint timeout_in_ms)				            // The ammount of time in milliseconds to wait for data.
        {
            if (IntPtr.Size == 8)
                return NDIlib64_recv_capture(p_instance, ref p_video_data, ref p_audio_data, ref p_meta_data, timeout_in_ms);
            else
                return NDIlib32_recv_capture(p_instance, ref p_video_data, ref p_audio_data, ref p_meta_data, timeout_in_ms);
        }

        // same, but only asks for video
        public static NDIlib_frame_type_e NDIlib_recv_capture(
                        IntPtr p_instance,                          // The library instance
                        ref NDIlib_video_frame_t p_video_data,      // The video data received (can be null)
                        uint timeout_in_ms)				            // The ammount of time in milliseconds to wait for data.
        {
            if (IntPtr.Size == 8)
                return NDIlib64_recv_capture(p_instance, ref p_video_data, IntPtr.Zero, IntPtr.Zero, timeout_in_ms);
            else
                return NDIlib32_recv_capture(p_instance, ref p_video_data, IntPtr.Zero, IntPtr.Zero, timeout_in_ms);
        }

        // same, but only asks for audio
        public static NDIlib_frame_type_e NDIlib_recv_capture(
                        IntPtr p_instance,                          // The library instance
                        ref NDIlib_audio_frame_t p_audio_data,      // The video data received (can be null)
                        uint timeout_in_ms)				            // The ammount of time in milliseconds to wait for data.
        {
            if (IntPtr.Size == 8)
                return NDIlib64_recv_capture(p_instance, IntPtr.Zero, ref p_audio_data, IntPtr.Zero, timeout_in_ms);
            else
                return NDIlib32_recv_capture(p_instance, IntPtr.Zero, ref p_audio_data, IntPtr.Zero, timeout_in_ms);
        }

        // same, but only asks for metadata
        public static NDIlib_frame_type_e NDIlib_recv_capture(
                        IntPtr p_instance,                          // The library instance
                        ref NDIlib_metadata_frame_t p_meta_data,    // The meta data data received (can be null)
                        uint timeout_in_ms)				            // The ammount of time in milliseconds to wait for data.
        {
            if (IntPtr.Size == 8)
                return NDIlib64_recv_capture(p_instance, IntPtr.Zero, IntPtr.Zero, ref p_meta_data, timeout_in_ms);
            else
                return NDIlib32_recv_capture(p_instance, IntPtr.Zero, IntPtr.Zero, ref p_meta_data, timeout_in_ms);
        }

        // Free the buffers returned by capture for video
        public static void NDIlib_recv_free_video(IntPtr p_instance, ref NDIlib_video_frame_t p_video_data)
        {
            if (IntPtr.Size == 8)
                NDIlib64_recv_free_video(p_instance, ref p_video_data);
            else
                NDIlib32_recv_free_video(p_instance, ref p_video_data);
        }

        // Free the buffers returned by capture for audio
        public static void NDIlib_recv_free_audio(IntPtr p_instance, ref NDIlib_audio_frame_t p_audio_data)
        {
            if (IntPtr.Size == 8)
                NDIlib64_recv_free_audio(p_instance, ref p_audio_data);
            else
                NDIlib32_recv_free_audio(p_instance, ref p_audio_data);
        }

        // Free the buffers returned by capture for meta-data
        public static void NDIlib_recv_free_metadata(IntPtr p_instance, ref NDIlib_metadata_frame_t p_meta_data)
        {
            if (IntPtr.Size == 8)
                NDIlib64_recv_free_metadata(p_instance, ref p_meta_data);
            else
                NDIlib32_recv_free_metadata(p_instance, ref p_meta_data);
        }

        // This function will send a meta message to the source that we are connected too. This returns FALSE if we are
        // not currently connected to anything.
        public static bool NDIlib_recv_send_metadata(IntPtr p_instance, ref NDIlib_metadata_frame_t p_meta_data)
        {
            if (IntPtr.Size == 8)
                return NDIlib64_recv_send_metadata(p_instance, ref p_meta_data);
            else
                return NDIlib32_recv_send_metadata(p_instance, ref p_meta_data);
        }

        // Set the up-stream tally notifications. This returns FALSE if we are not currently connected to anything. That
        // said, the moment that we do connect to something it will automatically be sent the tally state.
        public static bool NDIlib_recv_set_tally(IntPtr p_instance, ref NDIlib_tally_t p_tally)
        {
            if (IntPtr.Size == 8)
                return NDIlib64_recv_set_tally(p_instance, ref p_tally);
            else
                return NDIlib32_recv_set_tally(p_instance, ref p_tally);
        }

        // Get the current performance structures. This can be used to determine if you have been calling NDIlib_recv_capture fast
        // enough, or if your processing of data is not keeping up with real-time. The total structure will give you the total frame
        // counts received, the dropped structure will tell you how many frames have been dropped. Either of these could be null.
        public static void NDIlib_recv_get_performance(IntPtr p_instance, ref NDIlib_recv_performance_t p_total, ref NDIlib_recv_performance_t p_dropped)
        {
            if (IntPtr.Size == 8)
                NDIlib64_recv_get_performance(p_instance, ref p_total, ref p_dropped);
            else
                NDIlib32_recv_get_performance(p_instance, ref p_total, ref p_dropped);
        }

        // This will allow you to determine the current queue depth for all of the frame sources at any time.
        public static void NDIlib_recv_get_queue(IntPtr p_instance, ref NDIlib_recv_queue_t p_total)
        {
            if (IntPtr.Size == 8)
                NDIlib64_recv_get_queue(p_instance, ref p_total);
            else
                NDIlib32_recv_get_queue(p_instance, ref p_total);
        }

        // Connection based metadata is data that is sent automatically each time a new connection is received. You queue all of these
        // up and they are sent on each connection. To reset them you need to clear them all and set them up again.
        public static void NDIlib_recv_clear_connection_metadata(IntPtr p_instance)
        {
            if (IntPtr.Size == 8)
                NDIlib64_recv_clear_connection_metadata(p_instance);
            else
                NDIlib32_recv_clear_connection_metadata(p_instance);
        }

        // Add a connection metadata string to the list of what is sent on each new connection. If someone is already connected then
        // this string will be sent to them immediately.
        public static void NDIlib_recv_add_connection_metadata(IntPtr p_instance, ref NDIlib_metadata_frame_t p_metadata)
        {
            if (IntPtr.Size == 8)
                NDIlib64_recv_add_connection_metadata(p_instance, ref p_metadata);
            else
                NDIlib64_recv_add_connection_metadata(p_instance, ref p_metadata);
        }

        #region pInvoke
        const string NDILib64Name = "Processing.NDI.Lib.x64.dll";
        const string NDILib32Name = "Processing.NDI.Lib.x86.dll";

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_create2", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr NDIlib32_recv_create(ref NDIlib_recv_create_t p_create_settings);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_create2", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr NDIlib64_recv_create(ref NDIlib_recv_create_t p_create_settings);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_destroy", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib32_recv_destroy(IntPtr p_instance);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_destroy", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib64_recv_destroy(IntPtr p_instance);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_capture", CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib32_recv_capture(
                        IntPtr p_instance,                          // The library instance
                        ref NDIlib_video_frame_t p_video_data,      // The video data received (can be null)
                        ref NDIlib_audio_frame_t p_audio_data,		// The audio data received (can be null)
                        ref NDIlib_metadata_frame_t p_meta_data,    // The meta data data received (can be null)
                        uint timeout_in_ms);				        // The ammount of time in milliseconds to wait for data.
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_capture", CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib64_recv_capture(
                        IntPtr p_instance,                          // The library instance
                        ref NDIlib_video_frame_t p_video_data,      // The video data received (can be null)
                        ref NDIlib_audio_frame_t p_audio_data,		// The audio data received (can be null)
                        ref NDIlib_metadata_frame_t p_meta_data,    // The meta data data received (can be null)
                        uint timeout_in_ms);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_capture", CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib32_recv_capture(
                        IntPtr p_instance,                          // The library instance
                        ref NDIlib_video_frame_t p_video_data,      // The video data received (can be null)
                        IntPtr p_audio_data,		                // The audio data received (can be null)
                        IntPtr p_meta_data,                         // The meta data data received (can be null)
                        uint timeout_in_ms);				        // The ammount of time in milliseconds to wait for data.
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_capture", CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib64_recv_capture(
                        IntPtr p_instance,                          // The library instance
                        ref NDIlib_video_frame_t p_video_data,      // The video data received (can be null)
                        IntPtr p_audio_data,		                // The audio data received (can be null)
                        IntPtr p_meta_data,                         // The meta data data received (can be null)
                        uint timeout_in_ms);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_capture", CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib32_recv_capture(
                        IntPtr p_instance,                          // The library instance
                        IntPtr p_video_data,                        // The video data received (can be null)
                        ref NDIlib_audio_frame_t p_audio_data,      // The audio data received (can be null)
                        IntPtr p_meta_data,                         // The meta data data received (can be null)
                        uint timeout_in_ms);				        // The ammount of time in milliseconds to wait for data.
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_capture", CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib64_recv_capture(
                        IntPtr p_instance,                          // The library instance
                        IntPtr p_video_data,                        // The video data received (can be null)
                        ref NDIlib_audio_frame_t p_audio_data,      // The audio data received (can be null)
                        IntPtr p_meta_data,                         // The meta data data received (can be null)
                        uint timeout_in_ms);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_capture", CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib32_recv_capture(
                        IntPtr p_instance,                          // The library instance
                        IntPtr p_video_data,                        // The video data received (can be null)
                        IntPtr p_audio_data,		                // The audio data received (can be null)
                        ref NDIlib_metadata_frame_t p_meta_data,    // The meta data data received (can be null)
                        uint timeout_in_ms);				        // The ammount of time in milliseconds to wait for data.
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_capture", CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib64_recv_capture(
                        IntPtr p_instance,                          // The library instance
                        IntPtr p_video_data,                        // The video data received (can be null)
                        IntPtr p_audio_data,		                // The audio data received (can be null)
                        ref NDIlib_metadata_frame_t p_meta_data,    // The meta data data received (can be null)
                        uint timeout_in_ms);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_free_video", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib32_recv_free_video(IntPtr p_instance, ref NDIlib_video_frame_t p_video_data);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_free_video", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib64_recv_free_video(IntPtr p_instance, ref NDIlib_video_frame_t p_video_data);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_free_audio", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib32_recv_free_audio(IntPtr p_instance, ref NDIlib_audio_frame_t p_audio_data);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_free_audio", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib64_recv_free_audio(IntPtr p_instance, ref NDIlib_audio_frame_t p_audio_data);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_free_metadata", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib32_recv_free_metadata(IntPtr p_instance, ref NDIlib_metadata_frame_t p_meta_data);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_free_metadata", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib64_recv_free_metadata(IntPtr p_instance, ref NDIlib_metadata_frame_t p_meta_data);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_send_metadata", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool NDIlib32_recv_send_metadata(IntPtr p_instance, ref NDIlib_metadata_frame_t p_meta_data);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_send_metadata", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool NDIlib64_recv_send_metadata(IntPtr p_instance, ref NDIlib_metadata_frame_t p_meta_data);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_set_tally", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool NDIlib32_recv_set_tally(IntPtr p_instance, ref NDIlib_tally_t p_tally);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_set_tally", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool NDIlib64_recv_set_tally(IntPtr p_instance, ref NDIlib_tally_t p_tally);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_get_performance", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib32_recv_get_performance(IntPtr p_instance, ref NDIlib_recv_performance_t p_total, ref NDIlib_recv_performance_t p_dropped);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_get_performance", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib64_recv_get_performance(IntPtr p_instance, ref NDIlib_recv_performance_t p_total, ref NDIlib_recv_performance_t p_dropped);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_get_queue", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib32_recv_get_queue(IntPtr p_instance, ref NDIlib_recv_queue_t p_total);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_get_queue", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib64_recv_get_queue(IntPtr p_instance, ref NDIlib_recv_queue_t p_total);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_clear_connection_metadata", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib32_recv_clear_connection_metadata(IntPtr p_instance);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_clear_connection_metadata", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib64_recv_clear_connection_metadata(IntPtr p_instance);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_recv_add_connection_metadata", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib32_recv_add_connection_metadata(IntPtr p_instance, ref NDIlib_metadata_frame_t p_metadata);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_recv_add_connection_metadata", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib64_recv_add_connection_metadata(IntPtr p_instance, ref NDIlib_metadata_frame_t p_metadata);

        #endregion
    }
}
