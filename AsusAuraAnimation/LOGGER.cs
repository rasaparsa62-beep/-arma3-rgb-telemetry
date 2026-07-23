using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;

namespace AsusAuraAnimation
{
	// Token: 0x0200001A RID: 26
	public class LOGGER
	{
		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000257 RID: 599 RVA: 0x00014828 File Offset: 0x00012A28
		public static string FileLogPath
		{
			get
			{
				return LOGGER._fileLogPath;
			}
		}

		// Token: 0x06000258 RID: 600 RVA: 0x00014830 File Offset: 0x00012A30
		public static void EnableFileLog()
		{
			object fileLock = LOGGER._fileLock;
			lock (fileLock)
			{
				if (LOGGER._fileWriter == null)
				{
					string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ASUS\\ARMOURY CRATE Diagnosis\\LightingService");
					Directory.CreateDirectory(text);
					LOGGER._fileLogPath = Path.Combine(text, string.Format("LayerManager_tester_{0:yyyyMMdd_HHmmss}.log", DateTime.Now));
					LOGGER._fileWriter = new StreamWriter(LOGGER._fileLogPath, false, Encoding.UTF8)
					{
						AutoFlush = true
					};
					LOGGER._fileWriter.WriteLine(string.Format("{0:HH:mm:ss.fff} [LOGGER] File log enabled: {1}", DateTime.Now, LOGGER._fileLogPath));
				}
			}
		}

		// Token: 0x06000259 RID: 601 RVA: 0x000148EC File Offset: 0x00012AEC
		public static void DisableFileLog()
		{
			object fileLock = LOGGER._fileLock;
			lock (fileLock)
			{
				if (LOGGER._fileWriter != null)
				{
					try
					{
						LOGGER._fileWriter.Dispose();
					}
					catch
					{
					}
					LOGGER._fileWriter = null;
				}
			}
		}

		// Token: 0x0600025A RID: 602 RVA: 0x00014950 File Offset: 0x00012B50
		private static void WriteToFile(string level, string msg)
		{
			StreamWriter fileWriter = LOGGER._fileWriter;
			if (fileWriter == null)
			{
				return;
			}
			object fileLock = LOGGER._fileLock;
			lock (fileLock)
			{
				try
				{
					fileWriter.WriteLine(string.Format("{0:HH:mm:ss.fff} [{1}] {2} {3}", new object[]
					{
						DateTime.Now,
						Thread.CurrentThread.ManagedThreadId,
						level,
						msg
					}));
				}
				catch
				{
				}
			}
		}

		// Token: 0x0600025B RID: 603 RVA: 0x000149E0 File Offset: 0x00012BE0
		public static void INIT(string logname = "LayerManager")
		{
			Trace.WriteLine("[LayerManager] LOGGER init");
			LOGGER.logger = LogManager.GetLogger(logname);
			LOGGER.IsInited = true;
			LOGGER.EnforceLogLimits();
		}

		// Token: 0x0600025C RID: 604 RVA: 0x00014A04 File Offset: 0x00012C04
		public static void EnforceLogLimits()
		{
			try
			{
				if (!(DateTime.Today == LOGGER._lastCleanupDate))
				{
					LOGGER._lastCleanupDate = DateTime.Today;
					string text = "C:\\ProgramData\\ASUS\\ARMOURY CRATE Diagnosis\\LightingService";
					if (Directory.Exists(text))
					{
						LOGGER.EnforceLogLimit(text, "LayerManager-*.*", 10);
						LOGGER.EnforceLogLimit(text, "LM_Support-*.*", 10);
						LOGGER.EnforceLogLimit(text, "LayerManager_tester*.*", 2);
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x0600025D RID: 605 RVA: 0x00014A7C File Offset: 0x00012C7C
		private static void EnforceLogLimit(string directory, string pattern, int maxCount)
		{
			FileInfo[] array = (from f in Directory.GetFiles(directory, pattern)
			select new FileInfo(f) into f
			orderby f.LastWriteTimeUtc descending
			select f).ToArray<FileInfo>();
			if (array.Length <= maxCount)
			{
				return;
			}
			for (int i = maxCount; i < array.Length; i++)
			{
				try
				{
					array[i].Delete();
				}
				catch
				{
				}
			}
		}

		// Token: 0x0600025E RID: 606 RVA: 0x00014B14 File Offset: 0x00012D14
		public static void DEBUG(string msg, params object[] args)
		{
			if (!LOGGER.IsInited)
			{
				LOGGER.INIT("LayerManager");
			}
			string text = string.Format(msg, args);
			Trace.WriteLine("[LayerManager] " + text);
			if (LOGGER.logger.IsDebugEnabled)
			{
				LOGGER.logger.Debug(text);
			}
			LOGGER.WriteToFile("DEBUG", text);
		}

		// Token: 0x0600025F RID: 607 RVA: 0x00014B6C File Offset: 0x00012D6C
		public static void INFO(string msg, params object[] args)
		{
			if (!LOGGER.IsInited)
			{
				LOGGER.INIT("LayerManager");
			}
			string text = string.Format(msg, args);
			Trace.WriteLine("[LayerManager] " + text);
			if (LOGGER.logger.IsInfoEnabled)
			{
				LOGGER.logger.Info(text);
			}
			LOGGER.WriteToFile("INFO", text);
		}

		// Token: 0x06000260 RID: 608 RVA: 0x00014BC4 File Offset: 0x00012DC4
		public static void WARN(string msg, params object[] args)
		{
			if (!LOGGER.IsInited)
			{
				LOGGER.INIT("LayerManager");
			}
			string text = string.Format(msg, args);
			Trace.WriteLine("[LayerManager] " + text);
			if (LOGGER.logger.IsWarnEnabled)
			{
				LOGGER.logger.Warn(text);
			}
			LOGGER.WriteToFile("WARN", text);
		}

		// Token: 0x06000261 RID: 609 RVA: 0x00014C1C File Offset: 0x00012E1C
		public static void ERROR(string msg, params object[] args)
		{
			if (!LOGGER.IsInited)
			{
				LOGGER.INIT("LayerManager");
			}
			string text = string.Format(msg, args);
			Trace.WriteLine("[LayerManager] " + text);
			if (LOGGER.logger.IsErrorEnabled)
			{
				LOGGER.logger.Error(text);
			}
			LOGGER.WriteToFile("ERROR", text);
		}

		// Token: 0x06000262 RID: 610 RVA: 0x00014C74 File Offset: 0x00012E74
		public static void FATAL(string msg, params object[] args)
		{
			if (!LOGGER.IsInited)
			{
				LOGGER.INIT("LayerManager");
			}
			string text = string.Format(msg, args);
			Trace.WriteLine("[LayerManager] " + text);
			if (LOGGER.logger.IsFatalEnabled)
			{
				LOGGER.logger.Fatal(text);
			}
			LOGGER.WriteToFile("FATAL", text);
		}

		// Token: 0x040000EA RID: 234
		private static ILog logger;

		// Token: 0x040000EB RID: 235
		private static bool IsInited = false;

		// Token: 0x040000EC RID: 236
		private static StreamWriter _fileWriter;

		// Token: 0x040000ED RID: 237
		private static readonly object _fileLock = new object();

		// Token: 0x040000EE RID: 238
		private static string _fileLogPath;

		// Token: 0x040000EF RID: 239
		private static DateTime _lastCleanupDate = DateTime.MinValue;
	}
}
