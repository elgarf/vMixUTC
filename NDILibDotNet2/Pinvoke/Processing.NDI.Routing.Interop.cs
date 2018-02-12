using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace NewTek
{
	[SuppressUnmanagedCodeSecurity]
	public static partial class NDIlib
	{
		// The creation structure that is used when you are creating a sender
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct routing_create_t
		{
			// The name of the NDI source to create. This is a NULL terminated UTF8 string.
			public IntPtr	p_ndi_name;

			// What groups should this source be part of
			public IntPtr	p_groups;
		}

		// Create an NDI routing source
		public static IntPtr routing_create(ref routing_create_t p_create_settings)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.routing_create_64(ref p_create_settings);
			else
				return  UnsafeNativeMethods.routing_create_32(ref p_create_settings);
		}

		// Destroy and NDI routing source
		public static void routing_destroy(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				 UnsafeNativeMethods.routing_destroy_64( p_instance);
			else
				 UnsafeNativeMethods.routing_destroy_32( p_instance);
		}

		// Change the routing of this source to another destination
		public static bool routing_change(IntPtr p_instance, ref source_t p_source)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.routing_change_64( p_instance, ref p_source);
			else
				return  UnsafeNativeMethods.routing_change_32( p_instance, ref p_source);
		}

		// Change the routing of this source to another destination
		public static bool routing_clear(IntPtr p_instance)
		{
			if (IntPtr.Size == 8)
				return  UnsafeNativeMethods.routing_clear_64( p_instance);
			else
				return  UnsafeNativeMethods.routing_clear_32( p_instance);
		}

		[SuppressUnmanagedCodeSecurity]
		internal static partial class UnsafeNativeMethods
		{
			// routing_create 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_routing_create", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr routing_create_64(ref routing_create_t p_create_settings);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_routing_create", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern IntPtr routing_create_32(ref routing_create_t p_create_settings);

			// routing_destroy 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_routing_destroy", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void routing_destroy_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_routing_destroy", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			internal static extern void routing_destroy_32(IntPtr p_instance);

			// routing_change 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_routing_change", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool routing_change_64(IntPtr p_instance, ref source_t p_source);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_routing_change", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool routing_change_32(IntPtr p_instance, ref source_t p_source);

			// routing_clear 
			[DllImport("Processing.NDI.Lib.x64.dll", EntryPoint = "NDIlib_routing_clear", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool routing_clear_64(IntPtr p_instance);
			[DllImport("Processing.NDI.Lib.x86.dll", EntryPoint = "NDIlib_routing_clear", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAsAttribute(UnmanagedType.U1)]
			internal static extern bool routing_clear_32(IntPtr p_instance);

		} // UnsafeNativeMethods

	} // class NDIlib

} // namespace NewTek

