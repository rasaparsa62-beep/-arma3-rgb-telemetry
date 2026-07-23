using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace AsusAuraAnimation
{
	// Token: 0x0200001D RID: 29
	public class SharedMemoryClient : IDisposable
	{
		// Token: 0x06000269 RID: 617 RVA: 0x00014E94 File Offset: 0x00013094
		private static string GetNamespacePrefix()
		{
			string result;
			try
			{
				result = ((Process.GetCurrentProcess().SessionId == 0) ? "Global\\" : "Local\\");
			}
			catch
			{
				result = "Global\\";
			}
			return result;
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x0600026A RID: 618 RVA: 0x00014ED8 File Offset: 0x000130D8
		private static string MmfName
		{
			get
			{
				return SharedMemoryClient.GetNamespacePrefix() + "LMCAP_FRAME";
			}
		}

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x0600026B RID: 619 RVA: 0x00014EE9 File Offset: 0x000130E9
		private static string EventName
		{
			get
			{
				return SharedMemoryClient.GetNamespacePrefix() + "LMCAP_FRAME_READY";
			}
		}

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x0600026C RID: 620 RVA: 0x00014EFA File Offset: 0x000130FA
		public uint LastFrameIndex
		{
			get
			{
				return this.lastFrameIndex;
			}
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x0600026D RID: 621 RVA: 0x00014F02 File Offset: 0x00013102
		public bool IsConnected
		{
			get
			{
				return this.pView != IntPtr.Zero;
			}
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x0600026E RID: 622 RVA: 0x00014F14 File Offset: 0x00013114
		// (set) Token: 0x0600026F RID: 623 RVA: 0x00014F1C File Offset: 0x0001311C
		public int Width { get; private set; }

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x06000270 RID: 624 RVA: 0x00014F25 File Offset: 0x00013125
		// (set) Token: 0x06000271 RID: 625 RVA: 0x00014F2D File Offset: 0x0001312D
		public int Height { get; private set; }

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000272 RID: 626 RVA: 0x00014F36 File Offset: 0x00013136
		// (set) Token: 0x06000273 RID: 627 RVA: 0x00014F3E File Offset: 0x0001313E
		public int Stride { get; private set; }

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000274 RID: 628 RVA: 0x00014F47 File Offset: 0x00013147
		// (set) Token: 0x06000275 RID: 629 RVA: 0x00014F4F File Offset: 0x0001314F
		public int OriginX { get; private set; }

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000276 RID: 630 RVA: 0x00014F58 File Offset: 0x00013158
		// (set) Token: 0x06000277 RID: 631 RVA: 0x00014F60 File Offset: 0x00013160
		public int OriginY { get; private set; }

		// Token: 0x06000278 RID: 632 RVA: 0x00014F6C File Offset: 0x0001316C
		public unsafe static bool CreateSharedMemory(int width, int height)
		{
			object obj = SharedMemoryClient.s_createLock;
			bool result;
			lock (obj)
			{
				if (SharedMemoryClient.s_hMapFile != IntPtr.Zero)
				{
					result = true;
				}
				else
				{
					int num = width * 4;
					int num2 = num * height;
					int num3 = 64 + num2 * 2;
					ShmNativeMethods.SECURITY_ATTRIBUTES security_ATTRIBUTES = SharedMemoryClient.CreateSecurityAttributes();
					SharedMemoryClient.s_hMapFile = ShmNativeMethods.CreateFileMapping(ShmNativeMethods.INVALID_HANDLE_VALUE, ref security_ATTRIBUTES, 4U, (uint)((long)num3 >> 32), (uint)((long)num3 & (long)((ulong)-1)), SharedMemoryClient.MmfName);
					if (SharedMemoryClient.s_hMapFile == IntPtr.Zero)
					{
						Console.Error.WriteLine("[SharedMemoryClient] CreateSharedMemory FAILED: " + SharedMemoryClient.MmfName + " err=" + Marshal.GetLastWin32Error().ToString());
						result = false;
					}
					else
					{
						IntPtr intPtr = ShmNativeMethods.MapViewOfFile(SharedMemoryClient.s_hMapFile, 6U, 0U, 0U, 64U);
						if (intPtr != IntPtr.Zero)
						{
							uint* ptr = (uint*)((void*)intPtr);
							*ptr = 1178815820U;
							ptr[1] = 1U;
							ptr[2] = (uint)width;
							ptr[3] = (uint)height;
							ptr[4] = (uint)num;
							ptr[5] = 0U;
							ptr[6] = 0U;
							ptr[7] = 0U;
							ptr[8] = 0U;
							ShmNativeMethods.UnmapViewOfFile(intPtr);
						}
						SharedMemoryClient.s_hEvent = ShmNativeMethods.CreateEvent(ref security_ATTRIBUTES, false, false, SharedMemoryClient.EventName);
						if (security_ATTRIBUTES.lpSecurityDescriptor != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(security_ATTRIBUTES.lpSecurityDescriptor);
						}
						result = true;
					}
				}
			}
			return result;
		}

		// Token: 0x06000279 RID: 633 RVA: 0x00015100 File Offset: 0x00013300
		public static void DestroySharedMemory()
		{
			object obj = SharedMemoryClient.s_createLock;
			lock (obj)
			{
				if (SharedMemoryClient.s_hEvent != IntPtr.Zero)
				{
					ShmNativeMethods.CloseHandle(SharedMemoryClient.s_hEvent);
					SharedMemoryClient.s_hEvent = IntPtr.Zero;
				}
				if (SharedMemoryClient.s_hMapFile != IntPtr.Zero)
				{
					ShmNativeMethods.CloseHandle(SharedMemoryClient.s_hMapFile);
					SharedMemoryClient.s_hMapFile = IntPtr.Zero;
				}
			}
		}

		// Token: 0x0600027A RID: 634 RVA: 0x00015188 File Offset: 0x00013388
		private static ShmNativeMethods.SECURITY_ATTRIBUTES CreateSecurityAttributes()
		{
			RawAcl rawAcl = new RawAcl(2, 2);
			SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
			rawAcl.InsertAce(0, new CommonAce(AceFlags.None, AceQualifier.AccessAllowed, 268435456, sid, false, null));
			SecurityIdentifier sid2 = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
			rawAcl.InsertAce(1, new CommonAce(AceFlags.None, AceQualifier.AccessAllowed, 268435456, sid2, false, null));
			RawSecurityDescriptor rawSecurityDescriptor = new RawSecurityDescriptor(ControlFlags.DiscretionaryAclPresent, null, null, null, rawAcl);
			byte[] array = new byte[rawSecurityDescriptor.BinaryLength];
			rawSecurityDescriptor.GetBinaryForm(array, 0);
			IntPtr intPtr = Marshal.AllocHGlobal(array.Length);
			Marshal.Copy(array, 0, intPtr, array.Length);
			ShmNativeMethods.SECURITY_ATTRIBUTES security_ATTRIBUTES = default(ShmNativeMethods.SECURITY_ATTRIBUTES);
			security_ATTRIBUTES.nLength = Marshal.SizeOf<ShmNativeMethods.SECURITY_ATTRIBUTES>(security_ATTRIBUTES);
			security_ATTRIBUTES.lpSecurityDescriptor = intPtr;
			security_ATTRIBUTES.bInheritHandle = false;
			return security_ATTRIBUTES;
		}

		// Token: 0x0600027B RID: 635 RVA: 0x00015238 File Offset: 0x00013438
		public unsafe bool TryConnect()
		{
			if (this.IsConnected)
			{
				return true;
			}
			this.hMapFile = ShmNativeMethods.OpenFileMapping(6U, false, SharedMemoryClient.MmfName);
			if (this.hMapFile == IntPtr.Zero)
			{
				Console.Error.WriteLine("[ScreenCapture] CaptureScreen2: TryConnect failed, lastErr=" + Marshal.GetLastWin32Error().ToString());
				return false;
			}
			this.pView = ShmNativeMethods.MapViewOfFile(this.hMapFile, 6U, 0U, 0U, 0U);
			if (this.pView == IntPtr.Zero)
			{
				ShmNativeMethods.CloseHandle(this.hMapFile);
				this.hMapFile = IntPtr.Zero;
				return false;
			}
			uint* ptr = (uint*)((void*)this.pView);
			if (*ptr != 1178815820U || ptr[1] != 1U)
			{
				this.Disconnect();
				return false;
			}
			this.Width = (int)ptr[2];
			this.Height = (int)ptr[3];
			this.Stride = (int)ptr[4];
			this.OriginX = (int)ptr[11];
			this.OriginY = (int)ptr[12];
			this.hEvent = ShmNativeMethods.OpenEvent(1048578U, false, SharedMemoryClient.EventName);
			return true;
		}

		// Token: 0x0600027C RID: 636 RVA: 0x00015354 File Offset: 0x00013554
		public unsafe bool WaitForFrame(int timeoutMs)
		{
			if (!this.IsConnected)
			{
				return false;
			}
			object accessLock = this._accessLock;
			lock (accessLock)
			{
				if (this.pView == IntPtr.Zero)
				{
					return false;
				}
				uint* ptr = (uint*)((void*)this.pView);
				uint num = ptr[6];
				if (num > 0U && num != this.lastFrameIndex)
				{
					this.lastFrameIndex = num;
					this.Width = (int)ptr[2];
					this.Height = (int)ptr[3];
					this.Stride = (int)ptr[4];
					this.OriginX = (int)ptr[11];
					this.OriginY = (int)ptr[12];
					this.UpdateClientReadTick();
					return true;
				}
			}
			int i = 0;
			int num2 = (this.hEvent != IntPtr.Zero) ? Math.Min(33, timeoutMs) : Math.Min(50, timeoutMs);
			while (i < timeoutMs)
			{
				if (this.hEvent != IntPtr.Zero)
				{
					ShmNativeMethods.WaitForSingleObject(this.hEvent, (uint)num2);
				}
				else
				{
					Thread.Sleep(num2);
				}
				accessLock = this._accessLock;
				lock (accessLock)
				{
					if (this.pView == IntPtr.Zero)
					{
						return false;
					}
					uint* ptr2 = (uint*)((void*)this.pView);
					uint num3 = ptr2[6];
					if (num3 > 0U && num3 != this.lastFrameIndex)
					{
						this.lastFrameIndex = num3;
						this.Width = (int)ptr2[2];
						this.Height = (int)ptr2[3];
						this.Stride = (int)ptr2[4];
						this.OriginX = (int)ptr2[11];
						this.OriginY = (int)ptr2[12];
						this.UpdateClientReadTick();
						return true;
					}
				}
				i += num2;
			}
			return false;
		}

		// Token: 0x0600027D RID: 637 RVA: 0x0001555C File Offset: 0x0001375C
		public unsafe uint GetCaptureState()
		{
			object accessLock = this._accessLock;
			uint result;
			lock (accessLock)
			{
				if (this.pView == IntPtr.Zero)
				{
					result = 0U;
				}
				else
				{
					uint* ptr = (uint*)((void*)this.pView);
					result = ptr[8];
				}
			}
			return result;
		}

		// Token: 0x0600027E RID: 638 RVA: 0x000155C4 File Offset: 0x000137C4
		public unsafe IntPtr GetActiveBufferPtr()
		{
			object accessLock = this._accessLock;
			IntPtr result;
			lock (accessLock)
			{
				if (this.pView == IntPtr.Zero)
				{
					result = IntPtr.Zero;
				}
				else
				{
					uint* ptr = (uint*)((void*)this.pView);
					uint num = ptr[7];
					int num2 = this.Stride * this.Height;
					int offset = (int)(64U + num * (uint)num2);
					result = IntPtr.Add(this.pView, offset);
				}
			}
			return result;
		}

		// Token: 0x0600027F RID: 639 RVA: 0x00015658 File Offset: 0x00013858
		public unsafe void CopyRect(int srcX, int srcY, int copyWidth, int copyHeight, IntPtr destPtr, int destStride)
		{
			object accessLock = this._accessLock;
			lock (accessLock)
			{
				if (!(this.pView == IntPtr.Zero))
				{
					if (this.Width > 0 && this.Height > 0 && this.Stride > 0)
					{
						uint* ptr = (uint*)((void*)this.pView);
						if (ptr[6] != 0U)
						{
							uint num = ptr[7];
							if (num <= 1U)
							{
								int num2 = this.Stride * this.Height;
								int offset = (int)(64U + num * (uint)num2);
								IntPtr value = IntPtr.Add(this.pView, offset);
								if (srcX < 0)
								{
									srcX = 0;
								}
								if (srcY < 0)
								{
									srcY = 0;
								}
								if (srcX < this.Width && srcY < this.Height)
								{
									if (srcX + copyWidth > this.Width)
									{
										copyWidth = this.Width - srcX;
									}
									if (srcY + copyHeight > this.Height)
									{
										copyHeight = this.Height - srcY;
									}
									if (copyWidth > 0 && copyHeight > 0)
									{
										int stride = this.Stride;
										int num3 = copyWidth * 4;
										byte* ptr2 = (byte*)((byte*)((void*)value) + srcY * stride) + srcX * 4;
										byte* ptr3 = (byte*)((void*)destPtr);
										for (int i = 0; i < copyHeight; i++)
										{
											Buffer.MemoryCopy((void*)ptr2, (void*)ptr3, (long)destStride, (long)num3);
											ptr2 += stride;
											ptr3 += destStride;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000280 RID: 640 RVA: 0x000157D8 File Offset: 0x000139D8
		public unsafe void SampleRect(int srcX, int srcY, int srcWidth, int srcHeight, IntPtr destPtr, int destStride, int destWidth, int destHeight)
		{
			object accessLock = this._accessLock;
			lock (accessLock)
			{
				if (!(this.pView == IntPtr.Zero))
				{
					if (this.Width > 0 && this.Height > 0 && this.Stride > 0)
					{
						uint* ptr = (uint*)((void*)this.pView);
						if (ptr[6] != 0U)
						{
							uint num = ptr[7];
							if (num <= 1U)
							{
								int num2 = this.Stride * this.Height;
								int offset = (int)(64U + num * (uint)num2);
								IntPtr value = IntPtr.Add(this.pView, offset);
								if (srcX < 0)
								{
									srcX = 0;
								}
								if (srcY < 0)
								{
									srcY = 0;
								}
								if (srcX < this.Width && srcY < this.Height)
								{
									if (srcX + srcWidth > this.Width)
									{
										srcWidth = this.Width - srcX;
									}
									if (srcY + srcHeight > this.Height)
									{
										srcHeight = this.Height - srcY;
									}
									if (srcWidth > 0 && srcHeight > 0)
									{
										int stride = this.Stride;
										byte* ptr2 = (byte*)((byte*)((void*)value) + srcY * stride) + srcX * 4;
										byte* ptr3 = (byte*)((void*)destPtr);
										if (destWidth == srcWidth && destHeight == srcHeight)
										{
											for (int i = 0; i < srcHeight; i++)
											{
												Buffer.MemoryCopy((void*)(ptr2 + i * stride), (void*)(ptr3 + i * destStride), (long)destStride, (long)(srcWidth * 4));
											}
										}
										else
										{
											for (int j = 0; j < destHeight; j++)
											{
												int num3 = j * srcHeight / destHeight;
												byte* ptr4 = ptr2 + num3 * stride;
												byte* ptr5 = ptr3 + j * destStride;
												for (int k = 0; k < destWidth; k++)
												{
													int num4 = k * srcWidth / destWidth;
													*(int*)(ptr5 + k * 4) = (int)(*(uint*)(ptr4 + num4 * 4));
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000281 RID: 641 RVA: 0x000159BC File Offset: 0x00013BBC
		private unsafe void UpdateClientReadTick()
		{
			object accessLock = this._accessLock;
			lock (accessLock)
			{
				if (!(this.pView == IntPtr.Zero))
				{
					long* ptr = (long*)((byte*)((void*)this.pView) + 36);
					*ptr = (long)Environment.TickCount;
				}
			}
		}

		// Token: 0x06000282 RID: 642 RVA: 0x00015A24 File Offset: 0x00013C24
		public void Disconnect()
		{
			object accessLock = this._accessLock;
			lock (accessLock)
			{
				if (this.pView != IntPtr.Zero)
				{
					ShmNativeMethods.UnmapViewOfFile(this.pView);
					this.pView = IntPtr.Zero;
				}
			}
			if (this.hMapFile != IntPtr.Zero)
			{
				ShmNativeMethods.CloseHandle(this.hMapFile);
				this.hMapFile = IntPtr.Zero;
			}
			if (this.hEvent != IntPtr.Zero)
			{
				ShmNativeMethods.CloseHandle(this.hEvent);
				this.hEvent = IntPtr.Zero;
			}
			this.Width = 0;
			this.Height = 0;
			this.Stride = 0;
		}

		// Token: 0x06000283 RID: 643 RVA: 0x00015AF0 File Offset: 0x00013CF0
		public void Dispose()
		{
			if (this.disposed)
			{
				return;
			}
			this.disposed = true;
			this.Disconnect();
		}

		// Token: 0x04000105 RID: 261
		private const string MMF_BASE_NAME = "LMCAP_FRAME";

		// Token: 0x04000106 RID: 262
		private const string EVENT_BASE_NAME = "LMCAP_FRAME_READY";

		// Token: 0x04000107 RID: 263
		private static IntPtr s_hMapFile = IntPtr.Zero;

		// Token: 0x04000108 RID: 264
		private static IntPtr s_hEvent = IntPtr.Zero;

		// Token: 0x04000109 RID: 265
		private static readonly object s_createLock = new object();

		// Token: 0x0400010A RID: 266
		private IntPtr hMapFile = IntPtr.Zero;

		// Token: 0x0400010B RID: 267
		private IntPtr pView = IntPtr.Zero;

		// Token: 0x0400010C RID: 268
		private IntPtr hEvent = IntPtr.Zero;

		// Token: 0x0400010D RID: 269
		private int mappedSize;

		// Token: 0x0400010E RID: 270
		private bool disposed;

		// Token: 0x0400010F RID: 271
		private uint lastFrameIndex;

		// Token: 0x04000110 RID: 272
		private readonly object _accessLock = new object();
	}
}
