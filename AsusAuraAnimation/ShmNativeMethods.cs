using System;
using System.Runtime.InteropServices;

namespace AsusAuraAnimation
{
	// Token: 0x0200001E RID: 30
	internal static class ShmNativeMethods
	{
		// Token: 0x06000286 RID: 646
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr CreateFileMapping(IntPtr hFile, ref ShmNativeMethods.SECURITY_ATTRIBUTES lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

		// Token: 0x06000287 RID: 647
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr CreateEvent(ref ShmNativeMethods.SECURITY_ATTRIBUTES lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

		// Token: 0x06000288 RID: 648
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr OpenFileMapping(uint dwDesiredAccess, bool bInheritHandle, string lpName);

		// Token: 0x06000289 RID: 649
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

		// Token: 0x0600028A RID: 650
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

		// Token: 0x0600028B RID: 651
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CloseHandle(IntPtr hObject);

		// Token: 0x0600028C RID: 652
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr OpenEvent(uint dwDesiredAccess, bool bInheritHandle, string lpName);

		// Token: 0x0600028D RID: 653
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

		// Token: 0x0600028E RID: 654
		[DllImport("user32.dll")]
		public static extern int GetSystemMetrics(int nIndex);

		// Token: 0x04000116 RID: 278
		public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

		// Token: 0x04000117 RID: 279
		public const uint PAGE_READWRITE = 4U;

		// Token: 0x04000118 RID: 280
		public const uint FILE_MAP_READ = 4U;

		// Token: 0x04000119 RID: 281
		public const uint FILE_MAP_WRITE = 2U;

		// Token: 0x0400011A RID: 282
		public const uint WAIT_OBJECT_0 = 0U;

		// Token: 0x0400011B RID: 283
		public const uint WAIT_TIMEOUT = 258U;

		// Token: 0x0400011C RID: 284
		public const uint SYNCHRONIZE = 1048576U;

		// Token: 0x0400011D RID: 285
		public const uint EVENT_MODIFY_STATE = 2U;

		// Token: 0x0400011E RID: 286
		public const int SM_XVIRTUALSCREEN = 76;

		// Token: 0x0400011F RID: 287
		public const int SM_YVIRTUALSCREEN = 77;

		// Token: 0x04000120 RID: 288
		public const int SM_CXVIRTUALSCREEN = 78;

		// Token: 0x04000121 RID: 289
		public const int SM_CYVIRTUALSCREEN = 79;

		// Token: 0x0200005F RID: 95
		public struct SECURITY_ATTRIBUTES
		{
			// Token: 0x0400024D RID: 589
			public int nLength;

			// Token: 0x0400024E RID: 590
			public IntPtr lpSecurityDescriptor;

			// Token: 0x0400024F RID: 591
			[MarshalAs(UnmanagedType.Bool)]
			public bool bInheritHandle;
		}
	}
}
