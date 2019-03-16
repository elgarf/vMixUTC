using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace NewTek
{
	[SuppressUnmanagedCodeSecurity]
	public static partial class NDIlib
	{
		// This describes an audio frame
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct audio_frame_interleaved_16s_t
		{
			// The sample-rate of this buffer
			public int	sample_rate;

			// The number of audio channels
			public int	no_channels;

			// The number of audio samples per channel
			public int	no_samples;

			// The timecode of this frame in 100ns intervals
			public Int64	timecode;

			// The audio reference level in dB. This specifies how many dB above the reference level (+4dBU) is the full range of 16 bit audio.
			// If you do not understand this and want to just use numbers :
			//		-	If you are sending audio, specify +0dB. Most common applications produce audio at reference level.
			//		-	If receiving audio, specify +20dB. This means that the full 16 bit range corresponds to professional level audio with 20dB of headroom. Note that
			//			if you are writing it into a file it might sound soft because you have 20dB of headroom before clipping.
			public int	reference_level;

			// The audio data, interleaved 16bpp
			public IntPtr	p_data;
		}

		// This describes an audio frame
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct audio_frame_interleaved_32f_t
		{
			// The sample-rate of this buffer
			public int	sample_rate;

			// The number of audio channels
			public int	no_channels;

			// The number of audio samples per channel
			public int	no_samples;

			// The timecode of this frame in 100ns intervals
			public Int64	timecode;

			// The audio data, interleaved 32bpp
			public IntPtr	p_data;
		}

		// This will add an audio frame in 16bpp
		public static void util_send_send_audio_interleaved_16s(IntPtr p_instance, ref audio_frame_interleaved_16s_t p_audio_data)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.util_send_send_audio_interleaved_16s_64( p_instance, ref p_audio_data);
			else
				 UnsafeNativeMethods.util_send_send_audio_interleaved_16s_32( p_instance, ref p_audio_data);
		}

		// This will add an audio frame interleaved floating point
		public static void util_send_send_audio_interleaved_32f(IntPtr p_instance, ref audio_frame_interleaved_32f_t p_audio_data)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.util_send_send_audio_interleaved_32f_64( p_instance, ref p_audio_data);
			else
				 UnsafeNativeMethods.util_send_send_audio_interleaved_32f_32( p_instance, ref p_audio_data);
		}

		public static void util_audio_to_interleaved_16s_v2(ref audio_frame_v2_t p_src, ref audio_frame_interleaved_16s_t p_dst)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.util_audio_to_interleaved_16s_v2_64(ref p_src, ref p_dst);
			else
				 UnsafeNativeMethods.util_audio_to_interleaved_16s_v2_32(ref p_src, ref p_dst);
		}

		public static void util_audio_from_interleaved_16s_v2(ref audio_frame_interleaved_16s_t p_src, ref audio_frame_v2_t p_dst)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.util_audio_from_interleaved_16s_v2_64(ref p_src, ref p_dst);
			else
				 UnsafeNativeMethods.util_audio_from_interleaved_16s_v2_32(ref p_src, ref p_dst);
		}

		public static void util_audio_to_interleaved_32f_v2(ref audio_frame_v2_t p_src, ref audio_frame_interleaved_32f_t p_dst)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.util_audio_to_interleaved_32f_v2_64(ref p_src, ref p_dst);
			else
				 UnsafeNativeMethods.util_audio_to_interleaved_32f_v2_32(ref p_src, ref p_dst);
		}

		public static void util_audio_from_interleaved_32f_v2(ref audio_frame_interleaved_32f_t p_src, ref audio_frame_v2_t p_dst)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.util_audio_from_interleaved_32f_v2_64(ref p_src, ref p_dst);
			else
				 UnsafeNativeMethods.util_audio_from_interleaved_32f_v2_32(ref p_src, ref p_dst);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static partial class UnsafeNativeMethods
		{
			// util_send_send_audio_interleaved_16s 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_util_send_send_audio_interleaved_16s", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void util_send_send_audio_interleaved_16s_64(IntPtr p_instance, ref audio_frame_interleaved_16s_t p_audio_data);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_util_send_send_audio_interleaved_16s", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void util_send_send_audio_interleaved_16s_32(IntPtr p_instance, ref audio_frame_interleaved_16s_t p_audio_data);

			// util_send_send_audio_interleaved_32f 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_util_send_send_audio_interleaved_32f", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void util_send_send_audio_interleaved_32f_64(IntPtr p_instance, ref audio_frame_interleaved_32f_t p_audio_data);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_util_send_send_audio_interleaved_32f", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void util_send_send_audio_interleaved_32f_32(IntPtr p_instance, ref audio_frame_interleaved_32f_t p_audio_data);

			// util_audio_to_interleaved_16s_v2 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_util_audio_to_interleaved_16s_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void util_audio_to_interleaved_16s_v2_64(ref audio_frame_v2_t p_src, ref audio_frame_interleaved_16s_t p_dst);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_util_audio_to_interleaved_16s_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void util_audio_to_interleaved_16s_v2_32(ref audio_frame_v2_t p_src, ref audio_frame_interleaved_16s_t p_dst);

			// util_audio_from_interleaved_16s_v2 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_util_audio_from_interleaved_16s_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void util_audio_from_interleaved_16s_v2_64(ref audio_frame_interleaved_16s_t p_src, ref audio_frame_v2_t p_dst);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_util_audio_from_interleaved_16s_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void util_audio_from_interleaved_16s_v2_32(ref audio_frame_interleaved_16s_t p_src, ref audio_frame_v2_t p_dst);

			// util_audio_to_interleaved_32f_v2 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_util_audio_to_interleaved_32f_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void util_audio_to_interleaved_32f_v2_64(ref audio_frame_v2_t p_src, ref audio_frame_interleaved_32f_t p_dst);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_util_audio_to_interleaved_32f_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void util_audio_to_interleaved_32f_v2_32(ref audio_frame_v2_t p_src, ref audio_frame_interleaved_32f_t p_dst);

			// util_audio_from_interleaved_32f_v2 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_util_audio_from_interleaved_32f_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void util_audio_from_interleaved_32f_v2_64(ref audio_frame_interleaved_32f_t p_src, ref audio_frame_v2_t p_dst);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_util_audio_from_interleaved_32f_v2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void util_audio_from_interleaved_32f_v2_32(ref audio_frame_interleaved_32f_t p_src, ref audio_frame_v2_t p_dst);

		} // UnsafeNativeMethods

	} // class NDIlib

} // namespace NewTek

