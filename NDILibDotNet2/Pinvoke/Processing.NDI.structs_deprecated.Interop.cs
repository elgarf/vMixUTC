using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace NewTek
{
	[SuppressUnmanagedCodeSecurity]
	public static partial class NDIlib
	{
		// This describes a video frame
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct video_frame_t
		{
			// The resolution of this frame
			public int xres,	yres;

			// What FourCC this is with. This can be two values
			public FourCC_type_e	FourCC;

			// What is the frame-rate of this frame.
			// For instance NTSC is 30000,1001 = 30000/1001 = 29.97fps
			public int frame_rate_N,	frame_rate_D;

			// What is the picture aspect ratio of this frame.
			// For instance 16.0/9.0 = 1.778 is 16:9 video
			public float	picture_aspect_ratio;

			// Is this a fielded frame, or is it progressive
			public frame_format_type_e	frame_format_type;

			// The timecode of this frame in 100ns intervals
			public Int64	timecode;

			// The video data itself
			public IntPtr	p_data;

			// The inter line stride of the video data, in bytes.
			public int	line_stride_in_bytes;
		}

		// This describes an audio frame
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct audio_frame_t
		{
			// The sample-rate of this buffer
			public int	sample_rate;

			// The number of audio channels
			public int	no_channels;

			// The number of audio samples per channel
			public int	no_samples;

			// The timecode of this frame in 100ns intervals
			public Int64	timecode;

			// The audio data
			public IntPtr	p_data;

			// The inter channel stride of the audio channels, in bytes
			public int	channel_stride_in_bytes;
		}

	} // class NDIlib

} // namespace NewTek

