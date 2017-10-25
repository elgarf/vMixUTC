using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace vMixUTCNDIMonitorDataProvider.NDI
{
    // This describes an audio frame
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct NDIlib_audio_frame_interleaved_16s_t
    {
        // The sample-rate of this buffer
        public uint sample_rate;

        // The number of audio channels
        public uint no_channels;

        // The number of audio samples per channel
        public uint no_samples;

        // The timecode of this frame in 100ns intervals
        public long timecode;

        // The audio reference level in dB. This specifies how many dB above the reference level (+4dBU) is the full range of 16 bit audio. 
        // If you do not understand this and want to just use numbers :
        //		-	If you are sending audio, specify +0dB. Most common applications produce audio at reference level.
        //		-	If receiving audio, specify +20dB. This means that the full 16 bit range corresponds to professional level audio with 20dB of headroom. Note that
        //			if you are writing it into a file it might sound soft because you have 20dB of headroom before clipping.
        public uint reference_level;

        // The audio data, interleaved 16bpp
        public IntPtr p_data;
    };

    // This describes an audio frame
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct NDIlib_audio_frame_interleaved_32f_t
    {
        // The sample-rate of this buffer
        public uint sample_rate;

        // The number of audio channels
        public uint no_channels;

        // The number of audio samples per channel
        public uint no_samples;

        // The timecode of this frame in 100ns intervals
        public long timecode;

        // The audio data, interleaved 32bpp
        public IntPtr p_data;
    };

    [SuppressUnmanagedCodeSecurity]
    public static class Utilities
    {
        // This will add an audio frame
        public static void NDIlib_util_send_send_audio_interleaved_16s(IntPtr p_instance, ref NDIlib_audio_frame_interleaved_16s_t p_audio_data)
        {
            if (IntPtr.Size == 8)
                NDIlib64_util_send_send_audio_interleaved_16s(p_instance, ref p_audio_data);
            else
                NDIlib32_util_send_send_audio_interleaved_16s(p_instance, ref p_audio_data);
        }

        // Convert an planar floating point audio buffer into a interleaved short audio buffer. 
        // IMPORTANT : You must allocate the space for the samples in the destination to allow for your own memory management.
        public static void NDIlib_util_audio_to_interleaved_16s(ref NDIlib_audio_frame_t p_audio_src, ref NDIlib_audio_frame_interleaved_16s_t p_audio_dest)
        {
            if (IntPtr.Size == 8)
                NDIlib64_util_audio_to_interleaved_16s(ref p_audio_src, ref p_audio_dest);
            else
                NDIlib32_util_audio_to_interleaved_16s(ref p_audio_src, ref p_audio_dest);
        }

        // Convert an interleaved short audio buffer audio buffer into a planar floating point one. 
        // IMPORTANT : You must allocate the space for the samples in the destination to allow for your own memory management.
        public static void NDIlib_util_audio_from_interleaved_16s(ref NDIlib_audio_frame_interleaved_16s_t p_audio_src, ref NDIlib_audio_frame_t p_audio_dest)
        {
            if (IntPtr.Size == 8)
                NDIlib64_util_audio_from_interleaved_16s(ref p_audio_src, ref p_audio_dest);
            else
                NDIlib32_util_audio_from_interleaved_16s(ref p_audio_src, ref p_audio_dest);
        }

        // Convert a planar floating point audio buffer into a interleaved floating point audio buffer. 
        // IMPORTANT : You must allocate the space for the samples in the destination to allow for your own memory management.
        public static void NDIlib_util_audio_to_interleaved_32f(ref NDIlib_audio_frame_t p_audio_src, ref NDIlib_audio_frame_interleaved_32f_t p_audio_dest)
        {
            if (IntPtr.Size == 8)
                NDIlib64_util_audio_to_interleaved_32f(ref p_audio_src, ref p_audio_dest);
            else
                NDIlib32_util_audio_to_interleaved_32f(ref p_audio_src, ref p_audio_dest);
        }

        // Convert an interleaved floating point audio buffer into a planar floating point one. 
        // IMPORTANT : You must allocate the space for the samples in the destination to allow for your own memory management.
        public static void NDIlib_util_audio_from_interleaved_32f(ref NDIlib_audio_frame_interleaved_32f_t p_audio_src, ref NDIlib_audio_frame_t p_audio_dest)
        {
            if (IntPtr.Size == 8)
                NDIlib64_util_audio_from_interleaved_32f(ref p_audio_src, ref p_audio_dest);
            else
                NDIlib32_util_audio_from_interleaved_32f(ref p_audio_src, ref p_audio_dest);
        }

        // This will add an audio frame
        public static void NDIlib_util_send_send_audio_interleaved_32f(IntPtr p_instance, ref NDIlib_audio_frame_interleaved_32f_t p_audio_data)
        {
            if (IntPtr.Size == 8)
                NDIlib64_util_send_send_audio_interleaved_32f(p_instance, ref p_audio_data);
            else
                NDIlib32_util_send_send_audio_interleaved_32f(p_instance, ref p_audio_data);
        }

        #region pInvoke
        const string NDILib64Name = "Processing.NDI.Lib.x64.dll";
        const string NDILib32Name = "Processing.NDI.Lib.x86.dll";

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_util_send_send_audio_interleaved_16s", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib32_util_send_send_audio_interleaved_16s(
                        IntPtr p_instance,                                      // The library instance
                        ref NDIlib_audio_frame_interleaved_16s_t p_audio_data); // The audio datato send
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_util_send_send_audio_interleaved_16s", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib64_util_send_send_audio_interleaved_16s(
                        IntPtr p_instance,                                      // The library instance
                        ref NDIlib_audio_frame_interleaved_16s_t p_audio_data); // The audio datato send

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_util_audio_to_interleaved_16s", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib32_util_audio_to_interleaved_16s(
                        ref NDIlib_audio_frame_t p_audio_src,   // The source audio
                        ref NDIlib_audio_frame_interleaved_16s_t p_audio_dest); // The audio destination
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_util_audio_to_interleaved_16s", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib64_util_audio_to_interleaved_16s(
                        ref NDIlib_audio_frame_t p_audio_src,                   // The source audio
                        ref NDIlib_audio_frame_interleaved_16s_t p_audio_dest); // The audio destination

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_util_audio_from_interleaved_16s", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib32_util_audio_from_interleaved_16s(
                        ref NDIlib_audio_frame_interleaved_16s_t p_audio_src,   // The source audio
                        ref NDIlib_audio_frame_t p_audio_dest);                 // The audio destination
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_util_audio_from_interleaved_16s", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib64_util_audio_from_interleaved_16s(
                        ref NDIlib_audio_frame_interleaved_16s_t p_audio_src,   // The source audio
                        ref NDIlib_audio_frame_t p_audio_dest);                 // The audio destination

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_util_audio_to_interleaved_32f", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib32_util_audio_to_interleaved_32f(
                        ref NDIlib_audio_frame_t p_audio_src,   // The source audio
                        ref NDIlib_audio_frame_interleaved_32f_t p_audio_dest); // The audio destination
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_util_audio_to_interleaved_32f", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib64_util_audio_to_interleaved_32f(
                        ref NDIlib_audio_frame_t p_audio_src,                   // The source audio
                        ref NDIlib_audio_frame_interleaved_32f_t p_audio_dest); // The audio destination

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_util_audio_from_interleaved_32f", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib32_util_audio_from_interleaved_32f(
                        ref NDIlib_audio_frame_interleaved_32f_t p_audio_src,   // The source audio
                        ref NDIlib_audio_frame_t p_audio_dest);                 // The audio destination
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_util_audio_from_interleaved_32f", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib64_util_audio_from_interleaved_32f(
                        ref NDIlib_audio_frame_interleaved_32f_t p_audio_src,   // The source audio
                        ref NDIlib_audio_frame_t p_audio_dest);                 // The audio destination

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_util_send_send_audio_interleaved_32f", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib32_util_send_send_audio_interleaved_32f(
                        IntPtr p_instance,                                      // The library instance
                        ref NDIlib_audio_frame_interleaved_32f_t p_audio_data); // The audio datato send
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_util_send_send_audio_interleaved_32f", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern NDIlib_frame_type_e NDIlib64_util_send_send_audio_interleaved_32f(
                        IntPtr p_instance,                                      // The library instance
                        ref NDIlib_audio_frame_interleaved_32f_t p_audio_data); // The audio datato send

        #endregion
    }
}
