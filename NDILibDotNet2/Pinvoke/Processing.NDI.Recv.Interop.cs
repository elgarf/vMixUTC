using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace NewTek
{
	[SuppressUnmanagedCodeSecurity]
	public static partial class NDIlib
	{
		public enum recv_bandwidth_e
		{
			// Receive metadata.
			recv_bandwidth_metadata_only = -10,

			// Receive metadata audio.
			recv_bandwidth_audio_only = 10,

			// Receive metadata audio video at a lower bandwidth and resolution.
			recv_bandwidth_lowest = 0,

			// Receive metadata audio video at full resolution.
			recv_bandwidth_highest = 100
		}

		public enum recv_color_format_e
		{
			// No alpha channel: BGRX Alpha channel: BGRA
			recv_color_format_BGRX_BGRA = 0,

			// No alpha channel: UYVY Alpha channel: BGRA
			recv_color_format_UYVY_BGRA = 1,

			// No alpha channel: RGBX Alpha channel: RGBA
			recv_color_format_RGBX_RGBA = 2,

			// No alpha channel: UYVY Alpha channel: RGBA
			recv_color_format_UYVY_RGBA = 3,

			// Read the SDK documentation to understand the pros and cons of this format.
			recv_color_format_fastest = 100
		}

		// The creation structure that is used when you are creating a receiver
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct recv_create_t
		{
			// The source that you wish to connect to.
			public source_t	source_to_connect_to;

			// Your preference of color space. See above.
			public recv_color_format_e	color_format;

			// The bandwidth setting that you wish to use for this video source. Bandwidth
			// controlled by changing both the compression level and the resolution of the source.
			// A good use for low bandwidth is working on WIFI connections.
			public recv_bandwidth_e	bandwidth;

			// When this flag is FALSE, all video that you receive will be progressive. For sources
			// that provide fields, this is de-interlaced on the receiving side (because we cannot change
			// what the up-stream source was actually rendering. This is provided as a convenience to
			// down-stream sources that do not wish to understand fielded video. There is almost no
			// performance impact of using this function.
			[MarshalAsAttribute(UnmanagedType.U1)]
			public bool	allow_video_fields;
		}

		// This allows you determine the current performance levels of the receiving to be able to detect whether frames have been dropped
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct recv_performance_t
		{
			// The number of video frames
			public Int64	video_frames;

			// The number of audio frames
			public Int64	audio_frames;

			// The number of metadata frames
			public Int64	metadata_frames;
		}

		// Get the current queue depths
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct recv_queue_t
		{
			// The number of video frames
			public int	video_frames;

			// The number of audio frames
			public int	audio_frames;

			// The number of metadata frames
			public int	metadata_frames;
		}

		// In order to get the duration
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct recv_recording_time_t
		{
			// The number of actual video frames recorded.
			public Int64	no_frames;

			// The starting time and current largest time of the record, in UTC time, at 100ns unit intervals. This allows you to know the record
			// time irrespective of frame-rate. For instance, last_time - start_time woudl give you the recording length in 100ns intervals.
			public Int64 start_time,	last_time;
		}

		//**************************************************************************************************************************
		// Create a new receiver instance. This will return NULL if it fails.
		public static IntPtr recv_create_v2(ref recv_create_t p_create_settings)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_create_v2_64(ref p_create_settings);
			else
				return  UnsafeNativeMethods.recv_create_v2_32(ref p_create_settings);
		}

		// For legacy reasons I called this the wrong thing. For backwards compatability.
		[Obsolete("recv_create2 is obsolete.", false)]
		public static IntPtr recv_create2(ref recv_create_t p_create_settings)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_create2_64(ref p_create_settings);
			else
				return  UnsafeNativeMethods.recv_create2_32(ref p_create_settings);
		}

		// This function is deprecated, please use NDIlib_recv_create_v2 if you can. Using this function will continue to work, and be
		// supported for backwards compatibility. This version sets bandwidth to highest and allow fields to true.
		[Obsolete("recv_create is obsolete.", false)]
		public static IntPtr recv_create(ref recv_create_t p_create_settings)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_create_64(ref p_create_settings);
			else
				return  UnsafeNativeMethods.recv_create_32(ref p_create_settings);
		}

		// This will destroy an existing receiver instance.
		public static void recv_destroy(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.recv_destroy_64( p_instance);
			else
				 UnsafeNativeMethods.recv_destroy_32( p_instance);
		}

		// This will allow you to receive video, audio and metadata frames.
		// Any of the buffers can be NULL, in which case data of that type
		// will not be captured in this call. This call can be called simultaneously
		// on separate threads, so it is entirely possible to receive audio, video, metadata
		// all on separate threads. This function will return NDIlib_frame_type_none if no
		// data is received within the specified timeout and NDIlib_frame_type_error if the connection is lost.
		// Buffers captured with this must be freed with the appropriate free function below.
		[Obsolete("recv_capture is obsolete.", false)]
		public static frame_type_e recv_capture(IntPtr p_instance, ref video_frame_t p_video_data, ref audio_frame_t p_audio_data, ref metadata_frame_t p_metadata, UInt32 timeout_in_ms)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_capture_64( p_instance, ref p_video_data, ref p_audio_data, ref p_metadata,  timeout_in_ms);
			else
				return  UnsafeNativeMethods.recv_capture_32( p_instance, ref p_video_data, ref p_audio_data, ref p_metadata,  timeout_in_ms);
		}

		public static frame_type_e recv_capture_v2(IntPtr p_instance, ref video_frame_v2_t p_video_data, ref audio_frame_v2_t p_audio_data, ref metadata_frame_t p_metadata, UInt32 timeout_in_ms)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_capture_v2_64( p_instance, ref p_video_data, ref p_audio_data, ref p_metadata,  timeout_in_ms);
			else
				return  UnsafeNativeMethods.recv_capture_v2_32( p_instance, ref p_video_data, ref p_audio_data, ref p_metadata,  timeout_in_ms);
		}

		// Free the buffers returned by capture for video
		[Obsolete("recv_free_video is obsolete.", false)]
		public static void recv_free_video(IntPtr p_instance, ref video_frame_t p_video_data)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.recv_free_video_64( p_instance, ref p_video_data);
			else
				 UnsafeNativeMethods.recv_free_video_32( p_instance, ref p_video_data);
		}

		public static void recv_free_video_v2(IntPtr p_instance, ref video_frame_v2_t p_video_data)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.recv_free_video_v2_64( p_instance, ref p_video_data);
			else
				 UnsafeNativeMethods.recv_free_video_v2_32( p_instance, ref p_video_data);
		}

		// Free the buffers returned by capture for audio
		[Obsolete("recv_free_audio is obsolete.", false)]
		public static void recv_free_audio(IntPtr p_instance, ref audio_frame_t p_audio_data)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.recv_free_audio_64( p_instance, ref p_audio_data);
			else
				 UnsafeNativeMethods.recv_free_audio_32( p_instance, ref p_audio_data);
		}

		public static void recv_free_audio_v2(IntPtr p_instance, ref audio_frame_v2_t p_audio_data)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.recv_free_audio_v2_64( p_instance, ref p_audio_data);
			else
				 UnsafeNativeMethods.recv_free_audio_v2_32( p_instance, ref p_audio_data);
		}

		// Free the buffers returned by capture for metadata
		public static void recv_free_metadata(IntPtr p_instance, ref metadata_frame_t p_metadata)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.recv_free_metadata_64( p_instance, ref p_metadata);
			else
				 UnsafeNativeMethods.recv_free_metadata_32( p_instance, ref p_metadata);
		}

		// This will free a string that was allocated and returned by NDIlib_recv (for instance the NDIlib_recv_get_web_control) function.
		public static void recv_free_string(IntPtr p_instance, IntPtr p_string)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.recv_free_string_64( p_instance,  p_string);
			else
				 UnsafeNativeMethods.recv_free_string_32( p_instance,  p_string);
		}

		// This function will send a meta message to the source that we are connected too. This returns FALSE if we are
		// not currently connected to anything.
		public static bool recv_send_metadata(IntPtr p_instance, ref metadata_frame_t p_metadata)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_send_metadata_64( p_instance, ref p_metadata);
			else
				return  UnsafeNativeMethods.recv_send_metadata_32( p_instance, ref p_metadata);
		}

		// Set the up-stream tally notifications. This returns FALSE if we are not currently connected to anything. That
		// said, the moment that we do connect to something it will automatically be sent the tally state.
		public static bool recv_set_tally(IntPtr p_instance, ref tally_t p_tally)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_set_tally_64( p_instance, ref p_tally);
			else
				return  UnsafeNativeMethods.recv_set_tally_32( p_instance, ref p_tally);
		}

		// Get the current performance structures. This can be used to determine if you have been calling NDIlib_recv_capture fast
		// enough, or if your processing of data is not keeping up with real-time. The total structure will give you the total frame
		// counts received, the dropped structure will tell you how many frames have been dropped. Either of these could be NULL.
		public static void recv_get_performance(IntPtr p_instance, ref recv_performance_t p_total, ref recv_performance_t p_dropped)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.recv_get_performance_64( p_instance, ref p_total, ref p_dropped);
			else
				 UnsafeNativeMethods.recv_get_performance_32( p_instance, ref p_total, ref p_dropped);
		}

		// This will allow you to determine the current queue depth for all of the frame sources at any time.
		public static void recv_get_queue(IntPtr p_instance, ref recv_queue_t p_total)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.recv_get_queue_64( p_instance, ref p_total);
			else
				 UnsafeNativeMethods.recv_get_queue_32( p_instance, ref p_total);
		}

		// Connection based metadata is data that is sent automatically each time a new connection is received. You queue all of these
		// up and they are sent on each connection. To reset them you need to clear them all and set them up again.
		public static void recv_clear_connection_metadata(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.recv_clear_connection_metadata_64( p_instance);
			else
				 UnsafeNativeMethods.recv_clear_connection_metadata_32( p_instance);
		}

		// Add a connection metadata string to the list of what is sent on each new connection. If someone is already connected then
		// this string will be sent to them immediately.
		public static void recv_add_connection_metadata(IntPtr p_instance, ref metadata_frame_t p_metadata)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.recv_add_connection_metadata_64( p_instance, ref p_metadata);
			else
				 UnsafeNativeMethods.recv_add_connection_metadata_32( p_instance, ref p_metadata);
		}

		// Is this receiver currently connected to a source on the other end, or has the source not yet been found or is no longe ronline.
		// This will normally return 0 or 1
		public static int recv_get_no_connections(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_get_no_connections_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_get_no_connections_32( p_instance);
		}

		// Has this receiver got PTZ control. Note that it might take a second or two after the connection for this value to be set.
		// To avoid the need to poll this function, you can know when the value of this function might have changed when the
		// NDILib_recv_capture* call would return NDIlib_frame_type_status_change
		public static bool recv_ptz_is_supported(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_is_supported_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_ptz_is_supported_32( p_instance);
		}

		// Has this receiver got recording control. Note that it might take a second or two after the connection for this value to be set.
		// To avoid the need to poll this function, you can know when the value of this function might have changed when the
		// NDILib_recv_capture* call would return NDIlib_frame_type_status_change
		public static bool recv_recording_is_supported(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_recording_is_supported_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_recording_is_supported_32( p_instance);
		}

		// Get the URL that might be used for configuration of this input. Note that it might take a second or two after the connection for
		// this value to be set. This function will return NULL if there is no web control user interface. You should call NDIlib_recv_free_string
		// to free the string that is returned by this function. The returned value will be a fully formed URL, for instamce "http://10.28.1.192/configuration/"
		// To avoid the need to poll this function, you can know when the value of this function might have changed when the
		// NDILib_recv_capture* call would return NDIlib_frame_type_status_change
		public static IntPtr recv_get_web_control(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_get_web_control_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_get_web_control_32( p_instance);
		}

		// PTZ Controls
		// Zoom to an absolute value.
		// zoom_value = 0.0 (zoomed in) ... 1.0 (zoomed out)
		public static bool recv_ptz_zoom(IntPtr p_instance, float zoom_value)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_zoom_64( p_instance,  zoom_value);
			else
				return  UnsafeNativeMethods.recv_ptz_zoom_32( p_instance,  zoom_value);
		}

		// Zoom at a particular speed
		// zoom_speed = -1.0 (zoom outwards) ... +1.0 (zoom inwards)
		public static bool recv_ptz_zoom_speed(IntPtr p_instance, float zoom_speed)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_zoom_speed_64( p_instance,  zoom_speed);
			else
				return  UnsafeNativeMethods.recv_ptz_zoom_speed_32( p_instance,  zoom_speed);
		}

		// Set the pan and tilt to an absolute value
		// pan_value  = -1.0 (left) ... 0.0 (centred) ... +1.0 (right)
		// tilt_value = -1.0 (bottom) ... 0.0 (centred) ... +1.0 (top)
		public static bool recv_ptz_pan_tilt(IntPtr p_instance, float pan_value, float tilt_value)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_pan_tilt_64( p_instance,  pan_value,  tilt_value);
			else
				return  UnsafeNativeMethods.recv_ptz_pan_tilt_32( p_instance,  pan_value,  tilt_value);
		}

		// Set the pan and tilt direction and speed
		// pan_speed = -1.0 (moving left) ... 0.0 (stopped) ... +1.0 (moving right)
		// tilt_speed = -1.0 (down) ... 0.0 (stopped) ... +1.0 (moving up)
		public static bool recv_ptz_pan_tilt_speed(IntPtr p_instance, float pan_speed, float tilt_speed)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_pan_tilt_speed_64( p_instance,  pan_speed,  tilt_speed);
			else
				return  UnsafeNativeMethods.recv_ptz_pan_tilt_speed_32( p_instance,  pan_speed,  tilt_speed);
		}

		// Store the current position, focus, etc... as a preset.
		// preset_no = 0 ... 99
		public static bool recv_ptz_store_preset(IntPtr p_instance, int preset_no)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_store_preset_64( p_instance,  preset_no);
			else
				return  UnsafeNativeMethods.recv_ptz_store_preset_32( p_instance,  preset_no);
		}

		// Recall a preset, including position, focus, etc...
		// preset_no = 0 ... 99
		// speed = 0.0(as slow as possible) ... 1.0(as fast as possible) The speed at which to move to the new preset
		public static bool recv_ptz_recall_preset(IntPtr p_instance, int preset_no, float speed)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_recall_preset_64( p_instance,  preset_no,  speed);
			else
				return  UnsafeNativeMethods.recv_ptz_recall_preset_32( p_instance,  preset_no,  speed);
		}

		// Put the camera in auto-focus
		public static bool recv_ptz_auto_focus(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_auto_focus_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_ptz_auto_focus_32( p_instance);
		}

		// Focus to an absolute value.
		// focus_value = 0.0 (focussed to infinity) ... 1.0 (focussed as close as possible)
		public static bool recv_ptz_focus(IntPtr p_instance, float focus_value)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_focus_64( p_instance,  focus_value);
			else
				return  UnsafeNativeMethods.recv_ptz_focus_32( p_instance,  focus_value);
		}

		// Focus at a particular speed
		// focus_speed = -1.0 (focus outwards) ... +1.0 (focus inwards)
		public static bool recv_ptz_focus_speed(IntPtr p_instance, float focus_speed)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_focus_speed_64( p_instance,  focus_speed);
			else
				return  UnsafeNativeMethods.recv_ptz_focus_speed_32( p_instance,  focus_speed);
		}

		// Put the camera in auto white balance moce
		public static bool recv_ptz_white_balance_auto(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_white_balance_auto_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_ptz_white_balance_auto_32( p_instance);
		}

		// Put the camera in indoor white balance
		public static bool recv_ptz_white_balance_indoor(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_white_balance_indoor_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_ptz_white_balance_indoor_32( p_instance);
		}

		// Put the camera in indoor white balance
		public static bool recv_ptz_white_balance_outdoor(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_white_balance_outdoor_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_ptz_white_balance_outdoor_32( p_instance);
		}

		// Use the current brightness to automatically set the current white balance
		public static bool recv_ptz_white_balance_oneshot(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_white_balance_oneshot_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_ptz_white_balance_oneshot_32( p_instance);
		}

		// Set the manual camera white balance using the R, B values
		// red = 0.0(not red) ... 1.0(very red)
		// blue = 0.0(not blue) ... 1.0(very blue)
		public static bool recv_ptz_white_balance_manual(IntPtr p_instance, float red, float blue)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_white_balance_manual_64( p_instance,  red,  blue);
			else
				return  UnsafeNativeMethods.recv_ptz_white_balance_manual_32( p_instance,  red,  blue);
		}

		// Put the camera in auto-exposure mode
		public static bool recv_ptz_exposure_auto(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_exposure_auto_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_ptz_exposure_auto_32( p_instance);
		}

		// Manually set the camera exposure
		// exposure_level = 0.0(dark) ... 1.0(light)
		public static bool recv_ptz_exposure_manual(IntPtr p_instance, float exposure_level)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_ptz_exposure_manual_64( p_instance,  exposure_level);
			else
				return  UnsafeNativeMethods.recv_ptz_exposure_manual_32( p_instance,  exposure_level);
		}

		// Recording control
		// This will start recording.If the recorder was already recording then the message is ignored.A filename is passed in as a �hint�.Since the recorder might
		// already be recording(or might not allow complete flexibility over its filename), the filename might or might not be used.If the filename is empty, or
		// not present, a name will be chosen automatically. If you do not with to provide a filename hint you can simply pass NULL.
		public static bool recv_recording_start(IntPtr p_instance, IntPtr p_filename_hint)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_recording_start_64( p_instance,  p_filename_hint);
			else
				return  UnsafeNativeMethods.recv_recording_start_32( p_instance,  p_filename_hint);
		}

		// Stop recording.
		public static bool recv_recording_stop(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_recording_stop_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_recording_stop_32( p_instance);
		}

		// This will control the audio level for the recording.dB is specified in decibels relative to the reference level of the source. Not all recording sources support
		// controlling audio levels.For instance, a digital audio device would not be able to avoid clipping on sources already at the wrong level, thus
		// might not support this message.
		public static bool recv_recording_set_audio_level(IntPtr p_instance, float level_dB)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_recording_set_audio_level_64( p_instance,  level_dB);
			else
				return  UnsafeNativeMethods.recv_recording_set_audio_level_32( p_instance,  level_dB);
		}

		// This will determine if the source is currently recording. It will return true while recording is in progress and false when it is not. Because there is
		// one recorded and multiple people might be connected to it, there is a chance that it is recording which was initiated by someone else.
		public static bool recv_recording_is_recording(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_recording_is_recording_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_recording_is_recording_32( p_instance);
		}

		// Get the current filename for recording. When this is set it will return a non-NULL value which is owned by you and freed using NDIlib_recv_free_string.
		// If a file was already being recorded by another client, the massage will contain the name of that file. The filename contains a UNC path (when one is available)
		// to the recorded file, and can be used to access the file on your local machine for playback.  If a UNC path is not available, then this will represent the local
		// filename. This will remain valid even after the file has stopped being recorded until the next file is started.
		public static IntPtr recv_recording_get_filename(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_recording_get_filename_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_recording_get_filename_32( p_instance);
		}

		// This will tell you whether there was a recording error and what that string is. When this is set it will return a non-NULL value which is owned by you and
		// freed using NDIlib_recv_free_string. When there is no error it will return NULL.
		public static IntPtr recv_recording_get_error(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_recording_get_error_64( p_instance);
			else
				return  UnsafeNativeMethods.recv_recording_get_error_32( p_instance);
		}

		// Get the current recording times. These remain
		public static bool recv_recording_get_times(IntPtr p_instance, ref recv_recording_time_t p_times)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.recv_recording_get_times_64( p_instance, ref p_times);
			else
				return  UnsafeNativeMethods.recv_recording_get_times_32( p_instance, ref p_times);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static partial class UnsafeNativeMethods
		{
			// recv_create_v2 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_create_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr recv_create_v2_64(ref recv_create_t p_create_settings);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_create_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr recv_create_v2_32(ref recv_create_t p_create_settings);

			// recv_create2 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_create2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr recv_create2_64(ref recv_create_t p_create_settings);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_create2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr recv_create2_32(ref recv_create_t p_create_settings);

			// recv_create 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_create", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr recv_create_64(ref recv_create_t p_create_settings);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_create", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr recv_create_32(ref recv_create_t p_create_settings);

			// recv_destroy 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_destroy", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_destroy_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_destroy", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_destroy_32(IntPtr p_instance);

			// recv_capture 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_capture", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern frame_type_e recv_capture_64(IntPtr p_instance, ref video_frame_t p_video_data, ref audio_frame_t p_audio_data, ref metadata_frame_t p_metadata, UInt32 timeout_in_ms);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_capture", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern frame_type_e recv_capture_32(IntPtr p_instance, ref video_frame_t p_video_data, ref audio_frame_t p_audio_data, ref metadata_frame_t p_metadata, UInt32 timeout_in_ms);

			// recv_capture_v2 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_capture_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern frame_type_e recv_capture_v2_64(IntPtr p_instance, ref video_frame_v2_t p_video_data, ref audio_frame_v2_t p_audio_data, ref metadata_frame_t p_metadata, UInt32 timeout_in_ms);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_capture_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern frame_type_e recv_capture_v2_32(IntPtr p_instance, ref video_frame_v2_t p_video_data, ref audio_frame_v2_t p_audio_data, ref metadata_frame_t p_metadata, UInt32 timeout_in_ms);

			// recv_free_video 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_free_video", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_free_video_64(IntPtr p_instance, ref video_frame_t p_video_data);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_free_video", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_free_video_32(IntPtr p_instance, ref video_frame_t p_video_data);

			// recv_free_video_v2 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_free_video_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_free_video_v2_64(IntPtr p_instance, ref video_frame_v2_t p_video_data);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_free_video_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_free_video_v2_32(IntPtr p_instance, ref video_frame_v2_t p_video_data);

			// recv_free_audio 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_free_audio", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_free_audio_64(IntPtr p_instance, ref audio_frame_t p_audio_data);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_free_audio", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_free_audio_32(IntPtr p_instance, ref audio_frame_t p_audio_data);

			// recv_free_audio_v2 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_free_audio_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_free_audio_v2_64(IntPtr p_instance, ref audio_frame_v2_t p_audio_data);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_free_audio_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_free_audio_v2_32(IntPtr p_instance, ref audio_frame_v2_t p_audio_data);

			// recv_free_metadata 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_free_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_free_metadata_64(IntPtr p_instance, ref metadata_frame_t p_metadata);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_free_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_free_metadata_32(IntPtr p_instance, ref metadata_frame_t p_metadata);

			// recv_free_string 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_free_string", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_free_string_64(IntPtr p_instance, IntPtr p_string);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_free_string", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_free_string_32(IntPtr p_instance, IntPtr p_string);

			// recv_send_metadata 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_send_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_send_metadata_64(IntPtr p_instance, ref metadata_frame_t p_metadata);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_send_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_send_metadata_32(IntPtr p_instance, ref metadata_frame_t p_metadata);

			// recv_set_tally 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_set_tally", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_set_tally_64(IntPtr p_instance, ref tally_t p_tally);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_set_tally", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_set_tally_32(IntPtr p_instance, ref tally_t p_tally);

			// recv_get_performance 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_get_performance", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_get_performance_64(IntPtr p_instance, ref recv_performance_t p_total, ref recv_performance_t p_dropped);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_get_performance", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_get_performance_32(IntPtr p_instance, ref recv_performance_t p_total, ref recv_performance_t p_dropped);

			// recv_get_queue 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_get_queue", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_get_queue_64(IntPtr p_instance, ref recv_queue_t p_total);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_get_queue", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_get_queue_32(IntPtr p_instance, ref recv_queue_t p_total);

			// recv_clear_connection_metadata 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_clear_connection_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_clear_connection_metadata_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_clear_connection_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_clear_connection_metadata_32(IntPtr p_instance);

			// recv_add_connection_metadata 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_add_connection_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_add_connection_metadata_64(IntPtr p_instance, ref metadata_frame_t p_metadata);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_add_connection_metadata", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void recv_add_connection_metadata_32(IntPtr p_instance, ref metadata_frame_t p_metadata);

			// recv_get_no_connections 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_get_no_connections", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern int recv_get_no_connections_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_get_no_connections", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern int recv_get_no_connections_32(IntPtr p_instance);

			// recv_ptz_is_supported 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_is_supported", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_is_supported_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_is_supported", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_is_supported_32(IntPtr p_instance);

			// recv_recording_is_supported 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_recording_is_supported", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_recording_is_supported_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_recording_is_supported", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_recording_is_supported_32(IntPtr p_instance);

			// recv_get_web_control 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_get_web_control", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr recv_get_web_control_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_get_web_control", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr recv_get_web_control_32(IntPtr p_instance);

			// recv_ptz_zoom 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_zoom", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_zoom_64(IntPtr p_instance, float zoom_value);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_zoom", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_zoom_32(IntPtr p_instance, float zoom_value);

			// recv_ptz_zoom_speed 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_zoom_speed", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_zoom_speed_64(IntPtr p_instance, float zoom_speed);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_zoom_speed", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_zoom_speed_32(IntPtr p_instance, float zoom_speed);

			// recv_ptz_pan_tilt 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_pan_tilt", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_pan_tilt_64(IntPtr p_instance, float pan_value, float tilt_value);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_pan_tilt", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_pan_tilt_32(IntPtr p_instance, float pan_value, float tilt_value);

			// recv_ptz_pan_tilt_speed 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_pan_tilt_speed", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_pan_tilt_speed_64(IntPtr p_instance, float pan_speed, float tilt_speed);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_pan_tilt_speed", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_pan_tilt_speed_32(IntPtr p_instance, float pan_speed, float tilt_speed);

			// recv_ptz_store_preset 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_store_preset", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_store_preset_64(IntPtr p_instance, int preset_no);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_store_preset", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_store_preset_32(IntPtr p_instance, int preset_no);

			// recv_ptz_recall_preset 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_recall_preset", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_recall_preset_64(IntPtr p_instance, int preset_no, float speed);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_recall_preset", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_recall_preset_32(IntPtr p_instance, int preset_no, float speed);

			// recv_ptz_auto_focus 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_auto_focus", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_auto_focus_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_auto_focus", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_auto_focus_32(IntPtr p_instance);

			// recv_ptz_focus 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_focus", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_focus_64(IntPtr p_instance, float focus_value);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_focus", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_focus_32(IntPtr p_instance, float focus_value);

			// recv_ptz_focus_speed 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_focus_speed", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_focus_speed_64(IntPtr p_instance, float focus_speed);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_focus_speed", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_focus_speed_32(IntPtr p_instance, float focus_speed);

			// recv_ptz_white_balance_auto 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_white_balance_auto", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_white_balance_auto_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_white_balance_auto", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_white_balance_auto_32(IntPtr p_instance);

			// recv_ptz_white_balance_indoor 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_white_balance_indoor", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_white_balance_indoor_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_white_balance_indoor", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_white_balance_indoor_32(IntPtr p_instance);

			// recv_ptz_white_balance_outdoor 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_white_balance_outdoor", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_white_balance_outdoor_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_white_balance_outdoor", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_white_balance_outdoor_32(IntPtr p_instance);

			// recv_ptz_white_balance_oneshot 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_white_balance_oneshot", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_white_balance_oneshot_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_white_balance_oneshot", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_white_balance_oneshot_32(IntPtr p_instance);

			// recv_ptz_white_balance_manual 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_white_balance_manual", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_white_balance_manual_64(IntPtr p_instance, float red, float blue);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_white_balance_manual", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_white_balance_manual_32(IntPtr p_instance, float red, float blue);

			// recv_ptz_exposure_auto 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_exposure_auto", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_exposure_auto_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_exposure_auto", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_exposure_auto_32(IntPtr p_instance);

			// recv_ptz_exposure_manual 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_ptz_exposure_manual", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_exposure_manual_64(IntPtr p_instance, float exposure_level);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_ptz_exposure_manual", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_ptz_exposure_manual_32(IntPtr p_instance, float exposure_level);

			// recv_recording_start 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_recording_start", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_recording_start_64(IntPtr p_instance, IntPtr p_filename_hint);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_recording_start", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_recording_start_32(IntPtr p_instance, IntPtr p_filename_hint);

			// recv_recording_stop 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_recording_stop", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_recording_stop_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_recording_stop", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_recording_stop_32(IntPtr p_instance);

			// recv_recording_set_audio_level 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_recording_set_audio_level", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_recording_set_audio_level_64(IntPtr p_instance, float level_dB);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_recording_set_audio_level", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_recording_set_audio_level_32(IntPtr p_instance, float level_dB);

			// recv_recording_is_recording 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_recording_is_recording", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_recording_is_recording_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_recording_is_recording", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_recording_is_recording_32(IntPtr p_instance);

			// recv_recording_get_filename 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_recording_get_filename", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr recv_recording_get_filename_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_recording_get_filename", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr recv_recording_get_filename_32(IntPtr p_instance);

			// recv_recording_get_error 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_recording_get_error", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr recv_recording_get_error_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_recording_get_error", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr recv_recording_get_error_32(IntPtr p_instance);

			// recv_recording_get_times 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_recv_recording_get_times", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_recording_get_times_64(IntPtr p_instance, ref recv_recording_time_t p_times);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_recv_recording_get_times", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool recv_recording_get_times_32(IntPtr p_instance, ref recv_recording_time_t p_times);

		} // UnsafeNativeMethods

	} // class NDIlib

} // namespace NewTek

