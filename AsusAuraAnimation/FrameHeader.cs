using System;
using System.Runtime.InteropServices;

namespace AsusAuraAnimation
{
	// Token: 0x0200001C RID: 28
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FrameHeader
	{
		// Token: 0x040000F7 RID: 247
		public uint Magic;

		// Token: 0x040000F8 RID: 248
		public uint Version;

		// Token: 0x040000F9 RID: 249
		public uint Width;

		// Token: 0x040000FA RID: 250
		public uint Height;

		// Token: 0x040000FB RID: 251
		public uint Stride;

		// Token: 0x040000FC RID: 252
		public uint Format;

		// Token: 0x040000FD RID: 253
		public uint FrameIndex;

		// Token: 0x040000FE RID: 254
		public uint ActiveBuffer;

		// Token: 0x040000FF RID: 255
		public uint CaptureState;

		// Token: 0x04000100 RID: 256
		public long LastClientReadTick;

		// Token: 0x04000101 RID: 257
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
		public byte[] Reserved;

		// Token: 0x04000102 RID: 258
		public const uint MAGIC_VALUE = 1178815820U;

		// Token: 0x04000103 RID: 259
		public const uint CURRENT_VERSION = 1U;

		// Token: 0x04000104 RID: 260
		public const int SIZE = 64;
	}
}
