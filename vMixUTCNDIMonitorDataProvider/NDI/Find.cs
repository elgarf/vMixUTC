using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace vMixUTCNDIMonitorDataProvider.NDI
{
    // The creation structure that is used when you are creating a finder
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct NDIlib_find_create_t
    {
        // Do we want to incluide the list of NDI sources that are running
        // on the local machine ?
        // If TRUE then local sources will be visible, if FALSE then they
        // will not.
        [MarshalAsAttribute(UnmanagedType.Bool)]
        public bool show_local_sources;

        // Which groups do you want to search in for sources. UTF-8 string
        public IntPtr p_groups;
    }


    public static class Find
    {
        // Create a new finder instance. This will return NULL if it fails.
        public static IntPtr NDIlib_find_create(ref NDIlib_find_create_t p_create_settings)
        {
            if (IntPtr.Size == 8)
                return NDIlib64_find_create(ref p_create_settings);
            else
                return NDIlib32_find_create(ref p_create_settings);
        }

        // This will destroy an existing finder instance.
        public static void NDIlib_find_destroy(IntPtr p_instance)
        {
            if (IntPtr.Size == 8)
                NDIlib64_find_destroy(p_instance);
            else
                NDIlib32_find_destroy(p_instance);
        }

        // This will recover the current set of located NDI sources. The string list is 
        // retained as a member of the instance (so you do not need to worry about freeing it)
        // and is valid until you call this function again. When the instance is destroyed
        // the pointer is no longer valid either.
        public static IntPtr NDIlib_find_get_sources(IntPtr p_instance, ref int p_no_sources, int timeout_in_ms)
        {
            if (IntPtr.Size == 8)
                return NDIlib64_find_get_sources(p_instance, ref p_no_sources, timeout_in_ms);
            else
                return NDIlib32_find_get_sources(p_instance, ref p_no_sources, timeout_in_ms);
        }


        #region pInvoke
        const string NDILib64Name = "Processing.NDI.Lib.x64.dll";
        const string NDILib32Name = "Processing.NDI.Lib.x86.dll";

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_find_create", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr NDIlib32_find_create(ref NDIlib_find_create_t p_create_settings);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_find_create", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr NDIlib64_find_create(ref NDIlib_find_create_t p_create_settings);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_find_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void NDIlib32_find_destroy(IntPtr p_instance);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_find_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void NDIlib64_find_destroy(IntPtr p_instance);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_find_get_sources", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NDIlib32_find_get_sources(IntPtr p_instance, ref int p_no_sources, int timeout_in_ms);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_find_get_sources", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NDIlib64_find_get_sources(IntPtr p_instance, ref int p_no_sources, int timeout_in_ms);

        #endregion
    }
}
