using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace vMixUTCNDIMonitorDataProvider.NDI
{
    public static class Common
    {
        // This is not actually required, but will start and end the libraries which might get
        // you slightly better performance in some cases. In general it is more "correct" to 
        // call these although it is not required. There is no way to call these that would have
        // an adverse impact on anything (even calling destroy before you've deleted all your
        // objects).
        public static bool NDIlib_initialize()
        {
            if (IntPtr.Size == 8)
                return NDIlib64_initialize();
            else
                return NDIlib32_initialize();
        }

        public static void NDIlib_destroy()
        {
            if (IntPtr.Size == 8)
                NDIlib64_destroy();
            else
                NDIlib32_destroy();
        }

        // Recover whether the current CPU in the system is capable of running NDILib. Currently
        // NDILib requires SSE4.1 https://en.wikipedia.org/wiki/SSE4 Creating devices when your 
        // CPU is not capable will return IntPtr.Zero and not crash. This function is provided to help
        // understand why they cannot be created or warn users before they run.
        public static bool NDIlib_is_supported_CPU()
        {
            if (IntPtr.Size == 8)
                return NDIlib64_is_supported_CPU();
            else
                return NDIlib32_is_supported_CPU();
        }

        // This REQUIRES you to use Marshal.FreeHGlobal() on the returned pointer!
        public static IntPtr StringToUtf8(String managedString)
        {
            int len = Encoding.UTF8.GetByteCount(managedString);

            byte[] buffer = new byte[len + 1];

            Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);

            IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);

            Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);

            return nativeUtf8;
        }

        // this version will also return the length of the utf8 string
        // This REQUIRES you to use Marshal.FreeHGlobal() on the returned pointer!
        public static IntPtr StringToUtf8(String managedString, out int utf8Length)
        {
            utf8Length = Encoding.UTF8.GetByteCount(managedString);

            byte[] buffer = new byte[utf8Length + 1];

            Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);

            IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);

            Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);

            return nativeUtf8;
        }

        // Length is optional, but recommended
        // This is all potentially dangerous
        public static string Utf8ToString(IntPtr nativeUtf8, uint? length = null)
        {
            uint len = 0;

            if (length.HasValue)
            {
                len = length.Value;
            }
            else
            {
                // try to find the terminator
                while (Marshal.ReadByte(nativeUtf8, (int)len) != 0)
                {
                    ++len;
                }
            }

            byte[] buffer = new byte[len];

            Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(buffer);
        }

        #region pInvoke
        const string NDILib64Name = "Processing.NDI.Lib.x64.dll";
        const string NDILib32Name = "Processing.NDI.Lib.x86.dll";

        [DllImportAttribute(NDILib32Name, EntryPoint = "NDIlib_initialize", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NDIlib32_initialize();
        [DllImportAttribute(NDILib64Name, EntryPoint = "NDIlib_initialize", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NDIlib64_initialize();

        [DllImportAttribute(NDILib32Name, EntryPoint = "NDIlib_destroy", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib32_destroy();
        [DllImportAttribute(NDILib64Name, EntryPoint = "NDIlib_destroy", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NDIlib64_destroy();

        [DllImportAttribute(NDILib32Name, EntryPoint = "NDIlib_is_supported_CPU", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NDIlib32_is_supported_CPU();
        [DllImportAttribute(NDILib64Name, EntryPoint = "NDIlib_is_supported_CPU", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NDIlib64_is_supported_CPU();

        #endregion
    }

    // An enumeration to specify the type of a packet returned by the functions
    public enum NDIlib_frame_type_e
    {
        NDIlib_frame_type_none = 0,
        NDIlib_frame_type_video = 1,
        NDIlib_frame_type_audio = 2,
        NDIlib_frame_type_metadata = 3,
    }


    public enum NDIlib_FourCC_type_e : uint
    {
        NDIlib_FourCC_type_UYVY = 0x59565955U,
        NDIlib_FourCC_type_BGRA = 0x41524742U
    }


    // This is a descriptor of a NDI source available on the network.
    [StructLayout(LayoutKind.Sequential)]
    public struct NDIlib_source_t
    {	// A UTF8 string that provides a user readable name for this source.
        // This can be used for serialization, etc... and comprises the machine
        // name and the source name on that machine. In the form
        //		MACHINE_NAME (NDI_SOURCE_NAME)
        public IntPtr p_ndi_name;

        // A UTF8 string that provides the actual IP address and port number.
        public IntPtr p_ip_address;
    }


    // This describes a video frame
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct NDIlib_video_frame_t
    {
        // The resolution of this frame
        public uint xres;
        public uint yres;

        // What FourCC this is with. This can be two values
        public NDIlib_FourCC_type_e FourCC;

        // What is the frame-rate of this frame.
        // For instance NTSC is 30000,1001 = 30000/1001 = 29.97fps
        public uint frame_rate_N;
        public uint frame_rate_D;

        // What is the picture aspect ratio of this frame.
        // For instance 16.0/9.0 = 1.778 is 16:9 video
        public float picture_aspect_ratio;

        // Is this a fielded frame, or is it progressive
        [MarshalAsAttribute(UnmanagedType.Bool)]
        public bool is_progressive;

        // The timecode of this frame in 10ns intervals
        public long timecode;

        // The video data itself
        public IntPtr p_data;

        // The inter line stride of the video data, in bytes.
        public uint line_stride_in_bytes;
    }


    // This describes an video frame
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct NDIlib_audio_frame_t
    {
        // The sample-rate of this buffer
        public uint sample_rate;

        // The number of audio channels
        public uint no_channels;

        // The number of audio samples per channel
        public uint no_samples;

        // The timecode of this frame in 10ns intervals
        public long timecode;

        // The audio data, float[]
        public IntPtr p_data;

        // The inter channel stride of the audio channels, in bytes
        public uint channel_stride_in_bytes;
    }


    // The data description for meta data
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct NDIlib_metadata_frame_t
    {
        // The length of the metadata in UTF8 characters.This includes the NULL terminating character.
        public uint length;

        // The timecode of this frame in 10ns intervals
        public long timecode;

        // The meta data as a UTF8 XML string. This is a NULL terminated string.
        public IntPtr p_data;
    }


    // Tally structures
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct NDIlib_tally_t
    {
        // Is this currently on program output
        [MarshalAsAttribute(UnmanagedType.Bool)]
        public bool on_program;

        // Is this currently on preview output
        [MarshalAsAttribute(UnmanagedType.Bool)]
        public bool on_preview;
    }

}
