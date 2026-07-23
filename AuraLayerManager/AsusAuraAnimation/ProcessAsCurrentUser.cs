using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AsusAuraAnimation
{
	// Token: 0x02000016 RID: 22
	public static class ProcessAsCurrentUser
	{
		// Token: 0x060001BC RID: 444
		[DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int WTSEnumerateSessions(IntPtr hServer, int reserved, int version, ref IntPtr sessionInfo, ref int count);

		// Token: 0x060001BD RID: 445
		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateProcessAsUserW", SetLastError = true)]
		private static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref ProcessAsCurrentUser.STARTUPINFO lpStartupInfo, out ProcessAsCurrentUser.PROCESS_INFORMATION lpProcessInformation);

		// Token: 0x060001BE RID: 446
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int WaitForSingleObject(IntPtr handle, int wait);

		// Token: 0x060001BF RID: 447
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GetExitCodeProcess(IntPtr hProcess, out uint exitCode);

		// Token: 0x060001C0 RID: 448
		[DllImport("wtsapi32.dll")]
		public static extern void WTSFreeMemory(IntPtr memory);

		// Token: 0x060001C1 RID: 449
		[DllImport("kernel32.dll")]
		private static extern uint WTSGetActiveConsoleSessionId();

		// Token: 0x060001C2 RID: 450
		[DllImport("wtsapi32.dll", SetLastError = true)]
		private static extern int WTSQueryUserToken(uint sessionId, out IntPtr Token);

		// Token: 0x060001C3 RID: 451
		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes, int ImpersonationLevel, int TokenType, out IntPtr phNewToken);

		// Token: 0x060001C4 RID: 452
		[DllImport("kernel32.dll")]
		private static extern int GetProcessId(IntPtr handle);

		// Token: 0x060001C5 RID: 453 RVA: 0x00014508 File Offset: 0x00012708
		public static int CreateProcessAsCurrentUser(string processExe, string commandLine)
		{
			IntPtr intPtr = 0;
			ProcessAsCurrentUser.STARTUPINFO startupinfo = default(ProcessAsCurrentUser.STARTUPINFO);
			ProcessAsCurrentUser.PROCESS_INFORMATION process_INFORMATION = default(ProcessAsCurrentUser.PROCESS_INFORMATION);
			LOGGER.DEBUG(string.Format("[CurrentUser] CreateProcessAsCurrentUser. processExe: " + processExe + " commandLine: " + commandLine, Array.Empty<object>()), Array.Empty<object>());
			IntPtr currentUserToken = ProcessAsCurrentUser.GetCurrentUserToken();
			bool flag = ProcessAsCurrentUser.DuplicateTokenEx(currentUserToken, 983551U, IntPtr.Zero, 2, 1, out intPtr);
			LOGGER.DEBUG(string.Format("[CurrentUser] duplicate: {0}", intPtr), Array.Empty<object>());
			if (flag)
			{
				flag = ProcessAsCurrentUser.CreateProcessAsUser(intPtr, processExe, commandLine, IntPtr.Zero, IntPtr.Zero, false, 134217728U, IntPtr.Zero, null, ref startupinfo, out process_INFORMATION);
				LOGGER.DEBUG(string.Format("[CurrentUser] CreateProcessAsUser result: {0} {1}", flag, Marshal.GetLastWin32Error()), Array.Empty<object>());
			}
			else
			{
				LOGGER.DEBUG(string.Format("[CurrentUser] DuplicateTokenEx result: {0} {1}", flag, Marshal.GetLastWin32Error()), Array.Empty<object>());
			}
			if (currentUserToken.ToInt32() != 0)
			{
				Marshal.Release(currentUserToken);
				LOGGER.DEBUG(string.Format("[CurrentUser] Released handle p: {0}", currentUserToken), Array.Empty<object>());
			}
			if (intPtr.ToInt32() != 0)
			{
				Marshal.Release(intPtr);
				LOGGER.DEBUG(string.Format("[CurrentUser] Released handle duplicate: {0}", intPtr), Array.Empty<object>());
			}
			int processId = ProcessAsCurrentUser.GetProcessId(process_INFORMATION.hProcess);
			LOGGER.DEBUG(string.Format("[CurrentUser] Create support process id: {0}", processId), Array.Empty<object>());
			return processId;
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0001467C File Offset: 0x0001287C
		public static int GetCurrentSessionId()
		{
			uint num = ProcessAsCurrentUser.WTSGetActiveConsoleSessionId();
			if (num == 4294967295U)
			{
				return -1;
			}
			return (int)num;
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x00014698 File Offset: 0x00012898
		public static bool IsUserLoggedOn()
		{
			List<ProcessAsCurrentUser.WTS_SESSION_INFO> list = ProcessAsCurrentUser.ListSessions();
			int num = 0;
			using (List<ProcessAsCurrentUser.WTS_SESSION_INFO>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.State == ProcessAsCurrentUser.ConnectionState.Active)
					{
						num++;
					}
				}
			}
			return num > 0;
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x000146F4 File Offset: 0x000128F4
		private static IntPtr GetCurrentUserToken()
		{
			List<ProcessAsCurrentUser.WTS_SESSION_INFO> list = ProcessAsCurrentUser.ListSessions();
			int num = 0;
			foreach (ProcessAsCurrentUser.WTS_SESSION_INFO wts_SESSION_INFO in list)
			{
				if (wts_SESSION_INFO.State == ProcessAsCurrentUser.ConnectionState.Active)
				{
					num = wts_SESSION_INFO.SessionID;
					break;
				}
			}
			if (num == 2147483647)
			{
				return IntPtr.Zero;
			}
			IntPtr result = 0;
			ProcessAsCurrentUser.WTSQueryUserToken((uint)num, out result);
			return result;
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x00014774 File Offset: 0x00012974
		public static List<ProcessAsCurrentUser.WTS_SESSION_INFO> ListSessions()
		{
			IntPtr zero = IntPtr.Zero;
			List<ProcessAsCurrentUser.WTS_SESSION_INFO> list = new List<ProcessAsCurrentUser.WTS_SESSION_INFO>();
			try
			{
				IntPtr zero2 = IntPtr.Zero;
				int num = 0;
				bool flag = ProcessAsCurrentUser.WTSEnumerateSessions(IntPtr.Zero, 0, 1, ref zero2, ref num) != 0;
				int num2 = Marshal.SizeOf(typeof(ProcessAsCurrentUser.WTS_SESSION_INFO));
				long num3 = (long)((int)zero2);
				if (flag)
				{
					for (int i = 0; i < num; i++)
					{
						ProcessAsCurrentUser.WTS_SESSION_INFO item = (ProcessAsCurrentUser.WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)num3, typeof(ProcessAsCurrentUser.WTS_SESSION_INFO));
						num3 += (long)num2;
						list.Add(item);
					}
					ProcessAsCurrentUser.WTSFreeMemory(zero2);
				}
			}
			catch (Exception ex)
			{
				LOGGER.DEBUG(ex.ToString(), Array.Empty<object>());
			}
			return list;
		}

		// Token: 0x040000DA RID: 218
		public const int INFINITE = -1;

		// Token: 0x040000DB RID: 219
		public const int WAIT_ABANDONED = 128;

		// Token: 0x040000DC RID: 220
		public const int WAIT_OBJECT_0 = 0;

		// Token: 0x040000DD RID: 221
		public const int WAIT_TIMEOUT = 258;

		// Token: 0x040000DE RID: 222
		public const int WAIT_FAILED = -1;

		// Token: 0x040000DF RID: 223
		private const int TokenImpersonation = 2;

		// Token: 0x040000E0 RID: 224
		private const int SecurityIdentification = 1;

		// Token: 0x040000E1 RID: 225
		private const int MAXIMUM_ALLOWED = 33554432;

		// Token: 0x040000E2 RID: 226
		private const int TOKEN_DUPLICATE = 2;

		// Token: 0x040000E3 RID: 227
		private const int TOKEN_QUERY = 8;

		// Token: 0x040000E4 RID: 228
		private const int TOKEN_IMPERSONATE = 4;

		// Token: 0x040000E5 RID: 229
		private const int TOKEN_ASSIGNPRIMARY = 1;

		// Token: 0x040000E6 RID: 230
		private const int TOKEN_ALLASCCESS = 983551;

		// Token: 0x040000E7 RID: 231
		private const int TOKEN_ALL_ACCESS = 983551;

		// Token: 0x040000E8 RID: 232
		private const int TOKEN_ASSIGN_PRIMARY = 1;

		// Token: 0x040000E9 RID: 233
		private const int TOKEN_ADJUST_PRIVILEGES = 32;

		// Token: 0x02000054 RID: 84
		public enum ConnectionState
		{
			// Token: 0x040001FE RID: 510
			Active,
			// Token: 0x040001FF RID: 511
			Connected,
			// Token: 0x04000200 RID: 512
			ConnectQuery,
			// Token: 0x04000201 RID: 513
			Shadowing,
			// Token: 0x04000202 RID: 514
			Disconnected,
			// Token: 0x04000203 RID: 515
			Idle,
			// Token: 0x04000204 RID: 516
			Listening,
			// Token: 0x04000205 RID: 517
			Reset,
			// Token: 0x04000206 RID: 518
			Down,
			// Token: 0x04000207 RID: 519
			Initializing
		}

		// Token: 0x02000055 RID: 85
		[StructLayout(LayoutKind.Sequential)]
		private class SECURITY_ATTRIBUTES
		{
			// Token: 0x04000208 RID: 520
			public int nLength;

			// Token: 0x04000209 RID: 521
			public IntPtr lpSecurityDescriptor;

			// Token: 0x0400020A RID: 522
			public int bInheritHandle;
		}

		// Token: 0x02000056 RID: 86
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct STARTUPINFO
		{
			// Token: 0x0400020B RID: 523
			public int cb;

			// Token: 0x0400020C RID: 524
			public string lpReserved;

			// Token: 0x0400020D RID: 525
			public string lpDesktop;

			// Token: 0x0400020E RID: 526
			public string lpTitle;

			// Token: 0x0400020F RID: 527
			public int dwX;

			// Token: 0x04000210 RID: 528
			public int dwY;

			// Token: 0x04000211 RID: 529
			public int dwXSize;

			// Token: 0x04000212 RID: 530
			public int dwYSize;

			// Token: 0x04000213 RID: 531
			public int dwXCountChars;

			// Token: 0x04000214 RID: 532
			public int dwYCountChars;

			// Token: 0x04000215 RID: 533
			public int dwFillAttribute;

			// Token: 0x04000216 RID: 534
			public int dwFlags;

			// Token: 0x04000217 RID: 535
			public short wShowWindow;

			// Token: 0x04000218 RID: 536
			public short cbReserved2;

			// Token: 0x04000219 RID: 537
			public IntPtr lpReserved2;

			// Token: 0x0400021A RID: 538
			public IntPtr hStdInput;

			// Token: 0x0400021B RID: 539
			public IntPtr hStdOutput;

			// Token: 0x0400021C RID: 540
			public IntPtr hStdError;
		}

		// Token: 0x02000057 RID: 87
		internal struct PROCESS_INFORMATION
		{
			// Token: 0x0400021D RID: 541
			public IntPtr hProcess;

			// Token: 0x0400021E RID: 542
			public IntPtr hThread;

			// Token: 0x0400021F RID: 543
			public int dwProcessId;

			// Token: 0x04000220 RID: 544
			public int dwThreadId;
		}

		// Token: 0x02000058 RID: 88
		private enum LOGON_TYPE
		{
			// Token: 0x04000222 RID: 546
			LOGON32_LOGON_INTERACTIVE = 2,
			// Token: 0x04000223 RID: 547
			LOGON32_LOGON_NETWORK,
			// Token: 0x04000224 RID: 548
			LOGON32_LOGON_BATCH,
			// Token: 0x04000225 RID: 549
			LOGON32_LOGON_SERVICE,
			// Token: 0x04000226 RID: 550
			LOGON32_LOGON_UNLOCK = 7,
			// Token: 0x04000227 RID: 551
			LOGON32_LOGON_NETWORK_CLEARTEXT,
			// Token: 0x04000228 RID: 552
			LOGON32_LOGON_NEW_CREDENTIALS
		}

		// Token: 0x02000059 RID: 89
		private enum LOGON_PROVIDER
		{
			// Token: 0x0400022A RID: 554
			LOGON32_PROVIDER_DEFAULT,
			// Token: 0x0400022B RID: 555
			LOGON32_PROVIDER_WINNT35,
			// Token: 0x0400022C RID: 556
			LOGON32_PROVIDER_WINNT40,
			// Token: 0x0400022D RID: 557
			LOGON32_PROVIDER_WINNT50
		}

		// Token: 0x0200005A RID: 90
		[Flags]
		private enum CreateProcessFlags : uint
		{
			// Token: 0x0400022F RID: 559
			CREATE_BREAKAWAY_FROM_JOB = 16777216U,
			// Token: 0x04000230 RID: 560
			CREATE_DEFAULT_ERROR_MODE = 67108864U,
			// Token: 0x04000231 RID: 561
			CREATE_NEW_CONSOLE = 16U,
			// Token: 0x04000232 RID: 562
			CREATE_NEW_PROCESS_GROUP = 512U,
			// Token: 0x04000233 RID: 563
			CREATE_NO_WINDOW = 134217728U,
			// Token: 0x04000234 RID: 564
			CREATE_PROTECTED_PROCESS = 262144U,
			// Token: 0x04000235 RID: 565
			CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 33554432U,
			// Token: 0x04000236 RID: 566
			CREATE_SEPARATE_WOW_VDM = 2048U,
			// Token: 0x04000237 RID: 567
			CREATE_SHARED_WOW_VDM = 4096U,
			// Token: 0x04000238 RID: 568
			CREATE_SUSPENDED = 4U,
			// Token: 0x04000239 RID: 569
			CREATE_UNICODE_ENVIRONMENT = 1024U,
			// Token: 0x0400023A RID: 570
			DEBUG_ONLY_THIS_PROCESS = 2U,
			// Token: 0x0400023B RID: 571
			DEBUG_PROCESS = 1U,
			// Token: 0x0400023C RID: 572
			DETACHED_PROCESS = 8U,
			// Token: 0x0400023D RID: 573
			EXTENDED_STARTUPINFO_PRESENT = 524288U,
			// Token: 0x0400023E RID: 574
			INHERIT_PARENT_AFFINITY = 65536U
		}

		// Token: 0x0200005B RID: 91
		public struct WTS_SESSION_INFO
		{
			// Token: 0x0400023F RID: 575
			public int SessionID;

			// Token: 0x04000240 RID: 576
			[MarshalAs(UnmanagedType.LPTStr)]
			public string WinStationName;

			// Token: 0x04000241 RID: 577
			public ProcessAsCurrentUser.ConnectionState State;
		}

		// Token: 0x0200005C RID: 92
		private enum SECURITY_IMPERSONATION_LEVEL
		{
			// Token: 0x04000243 RID: 579
			SecurityAnonymous,
			// Token: 0x04000244 RID: 580
			SecurityIdentification,
			// Token: 0x04000245 RID: 581
			SecurityImpersonation,
			// Token: 0x04000246 RID: 582
			SecurityDelegation
		}

		// Token: 0x0200005D RID: 93
		private enum TOKEN_TYPE
		{
			// Token: 0x04000248 RID: 584
			TokenPrimary = 1,
			// Token: 0x04000249 RID: 585
			TokenImpersonation
		}
	}
}
