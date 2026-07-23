using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace AsusAuraAnimation
{
	// Token: 0x02000020 RID: 32
	internal class Util
	{
		// Token: 0x06000293 RID: 659 RVA: 0x00015E9C File Offset: 0x0001409C
		public static float GetProcessCpuUsage(int processId)
		{
			float result;
			try
			{
				int num = Process.GetCurrentProcess().Id;
				if (processId != 0)
				{
					num = processId;
				}
				LOGGER.DEBUG("[ScreenCapture] GetProcessCpuUsage {0}", new object[]
				{
					num
				});
				float num2 = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName, true)
				{
					InstanceName = num.ToString()
				}.NextValue();
				LOGGER.DEBUG("[ScreenCapture] Current CPU usage for id: {0}  ==> {1}%", new object[]
				{
					num,
					num2
				});
				result = num2;
			}
			catch (Exception ex)
			{
				LOGGER.DEBUG("[ScreenCapture] GetProcessCpuUsage exception: " + ex.ToString(), Array.Empty<object>());
				result = 0f;
			}
			return result;
		}

		// Token: 0x06000294 RID: 660 RVA: 0x00015F5C File Offset: 0x0001415C
		public static Bitmap CropAndScaleBitmap(Bitmap sourceBitmap, Rectangle sourceRectangle, Size targetSize)
		{
			Bitmap bitmap = new Bitmap(targetSize.Width, targetSize.Height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.DrawImage(sourceBitmap, new Rectangle(0, 0, targetSize.Width, targetSize.Height), sourceRectangle, GraphicsUnit.Pixel);
			}
			return bitmap;
		}

		// Token: 0x06000295 RID: 661 RVA: 0x00015FC0 File Offset: 0x000141C0
		public static bool IsX86Build()
		{
			return IntPtr.Size == 4;
		}

		// Token: 0x06000296 RID: 662 RVA: 0x00015FCC File Offset: 0x000141CC
		public static string GetAppVersion(string softwareName)
		{
			string result = string.Empty;
			try
			{
				using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall"))
				{
					if (registryKey != null)
					{
						foreach (string name in registryKey.GetSubKeyNames())
						{
							using (RegistryKey registryKey2 = registryKey.OpenSubKey(name))
							{
								if (string.Equals(registryKey2.GetValue("DisplayName") as string, softwareName, StringComparison.OrdinalIgnoreCase))
								{
									result = (registryKey2.GetValue("DisplayVersion") as string);
									break;
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				LOGGER.DEBUG("[Util] GetAppVersion exception: " + ex.ToString(), Array.Empty<object>());
			}
			return result;
		}

		// Token: 0x06000297 RID: 663 RVA: 0x000160A8 File Offset: 0x000142A8
		public static DateTime GetCurrentBuildTime()
		{
			return Util.GetBuildTime(Assembly.GetExecutingAssembly());
		}

		// Token: 0x06000298 RID: 664 RVA: 0x000160B4 File Offset: 0x000142B4
		private static DateTime GetBuildTime(Assembly assembly)
		{
			string empty = string.Empty;
			DateTime result = DateTime.MinValue;
			try
			{
				result = File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
			}
			catch (Exception ex)
			{
				LOGGER.DEBUG("[Util] GetBuildTime exception: " + ex.ToString(), Array.Empty<object>());
			}
			return result;
		}
	}
}
