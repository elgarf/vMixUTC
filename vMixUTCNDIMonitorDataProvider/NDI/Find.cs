using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
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
        [MarshalAsAttribute(UnmanagedType.U1)]
        public bool show_local_sources;

        // Which groups do you want to search in for sources. UTF-8 string
        public IntPtr p_groups;

        // The list of additional IP addresses that exist that we should query for 
        // sources on. For instance, if you want to find the sources on a remote machine
        // that is not on your local sub-net then you can put a comma seperated list of 
        // those IP addresses here and those sources will be available locally even though
        // they are not mDNS discoverable. An example might be "12.0.0.8,13.0.12.8".
        // When none is specified (IntPtr.Zero) the registry is used.
        // UTF-8 string
        public IntPtr p_extra_ips;
    }

    [SuppressUnmanagedCodeSecurity]
    public static class Find
    {
        // Create a new finder instance. This will return NULL if it fails.
        [Obsolete("NDIlib_find_create is obsolete, please use NDIlib_find_create2.", true)]
        public static IntPtr NDIlib_find_create(ref NDIlib_find_create_t p_create_settings)
        {
            return IntPtr.Zero;
        }

        // Create a new finder instance. This will return NULL if it fails.
        public static IntPtr NDIlib_find_create2(ref NDIlib_find_create_t p_create_settings)
        {
            if (IntPtr.Size == 8)
                return NDIlib64_find_create2(ref p_create_settings);
            else
                return NDIlib32_find_create2(ref p_create_settings);
        }

        // This will destroy an existing finder instance.
        public static void NDIlib_find_destroy(IntPtr p_instance)
        {
            if (IntPtr.Size == 8)
                NDIlib64_find_destroy(p_instance);
            else
                NDIlib32_find_destroy(p_instance);
        }

        // This will allow you to wait until the number of online sources have changed.
        public static bool NDIlib_find_wait_for_sources(IntPtr p_instance, int timeout_in_ms)
        {
            if (IntPtr.Size == 8)
                return NDIlib64_find_wait_for_sources(p_instance, timeout_in_ms);
            else
                return NDIlib32_find_wait_for_sources(p_instance, timeout_in_ms);
        }

        // This function will recover the current set of sources (i.e. the ones that exist right this second).
        public static IntPtr NDIlib_find_get_current_sources(IntPtr p_instance, ref int p_no_sources)
        {
            if (IntPtr.Size == 8)
                return NDIlib64_find_get_current_sources(p_instance, ref p_no_sources);
            else
                return NDIlib32_find_get_current_sources(p_instance, ref p_no_sources);
        }

        [Obsolete("NDIlib_find_get_sources is obsolete, please read comments.", true)]
        public static IntPtr NDIlib_find_get_sources(IntPtr p_instance, ref int p_no_sources, int timeout_in_ms)
        {
            // This function is basically exactly the following and has been removed.
            //if (timeout_in_ms == 0 || NDIlib_find_wait_for_sources(timeout_in_ms)) 
            //    return NDIlib_find_get_current_sources(p_instance, p_no_sources);

            return IntPtr.Zero;
        }

        #region pInvoke
        const string NDILib64Name = "Processing.NDI.Lib.x64.dll";
        const string NDILib32Name = "Processing.NDI.Lib.x86.dll";

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_find_create2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr NDIlib32_find_create2(ref NDIlib_find_create_t p_create_settings);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_find_create2", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr NDIlib64_find_create2(ref NDIlib_find_create_t p_create_settings);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_find_destroy", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NDIlib32_find_destroy(IntPtr p_instance);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_find_destroy", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NDIlib64_find_destroy(IntPtr p_instance);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_find_wait_for_sources", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool NDIlib32_find_wait_for_sources(IntPtr p_instance, int timeout_in_ms);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_find_wait_for_sources", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool NDIlib64_find_wait_for_sources(IntPtr p_instance, int timeout_in_ms);

        [DllImport(NDILib32Name, EntryPoint = "NDIlib_find_get_current_sources", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NDIlib32_find_get_current_sources(IntPtr p_instance, ref int p_no_sources);
        [DllImport(NDILib64Name, EntryPoint = "NDIlib_find_get_current_sources", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NDIlib64_find_get_current_sources(IntPtr p_instance, ref int p_no_sources);

        #endregion
    }
}
