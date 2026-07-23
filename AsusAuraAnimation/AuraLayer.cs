using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using AsusAuraColorGenerator;
using NAudio.Wave;

namespace AsusAuraAnimation
{
	// Token: 0x0200000E RID: 14
	[Guid("180ef1ca-0f09-3b54-981b-62fe421800be")]
	[ClassInterface(ClassInterfaceType.None)]
	public class AuraLayer : IAuraLayer, IDisposable
	{
		// Token: 0x060000A1 RID: 161
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

		// Token: 0x060000A2 RID: 162
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetProcessHeap();

		// Token: 0x060000A3 RID: 163 RVA: 0x000066F0 File Offset: 0x000048F0
		public static bool IsFileLocked(string filePath)
		{
			try
			{
				using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				{
					fileStream.Close();
				}
			}
			catch (IOException)
			{
				return true;
			}
			return false;
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x00006740 File Offset: 0x00004940
		private void PrepareDRM()
		{
			try
			{
				LOGGER.DEBUG("[DRM] PrepareDRM()", Array.Empty<object>());
				string path = Path.GetPathRoot(Environment.SystemDirectory).ToString();
				string[] source = new string[]
				{
					Path.Combine(path, "Program Files\\ASUS\\ARMOURY CRATE Service\\ACStorePlugin\\ACStoreFileManager_x86.dll"),
					Path.Combine(path, "Program Files\\ASUS\\ARMOURY CRATE Lite Service\\ACStorePlugin\\") + "ACStoreFileManager_x86.dll",
					Path.Combine(path, "Program Files\\ASUS\\ARMOURY CRATE SE Service\\ACStorePlugin\\ACStoreFileManager_x86.dll")
				};
				this.existingPath = source.FirstOrDefault((string p) => File.Exists(p));
				if (this.existingPath != null)
				{
					LOGGER.DEBUG("[DRM] PrepareDRM() found dll in: " + this.existingPath, Array.Empty<object>());
				}
				AuraLayer.initialize = AuraLayer.DllLoader.LoadFunction<AuraLayer.ACFM_InitializeDelegate>(this.existingPath, "Initialize");
				AuraLayer.InitializeWithPath = AuraLayer.DllLoader.LoadFunction<AuraLayer.ACFM_InitializeWithPathDelegate>(this.existingPath, "InitializeWithPath");
				AuraLayer.GetFileStreamCSharp = AuraLayer.DllLoader.LoadFunction<AuraLayer.GetFileStreamCSharpDelegate>(this.existingPath, "GetFileStreamCSharp");
				AuraLayer.GetFileSizeCSharp = AuraLayer.DllLoader.LoadFunction<AuraLayer.GetFileSizeCSharpDelegate>(this.existingPath, "GetFileSizeCSharp");
				AuraLayer.GetFileReadedSizeCSharp = AuraLayer.DllLoader.LoadFunction<AuraLayer.GetFileReadedSizeCSharpDelegate>(this.existingPath, "GetFileReadedSizeCSharp");
				AuraLayer.ReadStreamCSharp = AuraLayer.DllLoader.LoadFunction<AuraLayer.ReadStreamCSharpDelegate>(this.existingPath, "ReadStreamCSharp");
				AuraLayer.ReleaseStreamDataCSharp = AuraLayer.DllLoader.LoadFunction<AuraLayer.ReleaseStreamDataCSharpDelegate>(this.existingPath, "ReleaseStreamDataCSharp");
				AuraLayer.DestroyStreamCSharp = AuraLayer.DllLoader.LoadFunction<AuraLayer.DestroyStreamCSharpDelegate>(this.existingPath, "DestroyStreamCSharp");
			}
			catch (Exception ex)
			{
				LOGGER.DEBUG("[DRM] PrepareDRM() Exception: " + ex.ToString(), Array.Empty<object>());
			}
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x000068D8 File Offset: 0x00004AD8
		private void DRM_Init(string DRMString = null)
		{
			this.PrepareDRM();
			if (DRMString == null)
			{
				AuraLayer.initialize();
				return;
			}
			string text;
			string text2;
			string text3;
			string text4;
			if (!this.DRMStringtoAddress(DRMString, out text, out text2, out text3, out text4))
			{
				return;
			}
			LOGGER.DEBUG("[DRM] ACFM_InitializeWithPath({0})", new object[]
			{
				text
			});
			if (text == "DEFAULT")
			{
				AuraLayer.initialize();
				return;
			}
			AuraLayer.InitializeWithPath(text);
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00006945 File Offset: 0x00004B45
		private string GetDRMString(string input)
		{
			if (input.StartsWith("DRM:"))
			{
				return input.Substring("DRM:".Length);
			}
			return null;
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x00006968 File Offset: 0x00004B68
		private bool DRMStringtoAddress(string DRMString, out string userpath, out string user_id, out string content_id, out string path)
		{
			bool result;
			try
			{
				string[] array = DRMString.Split(new char[]
				{
					';'
				});
				if (array.Length != 4)
				{
					userpath = "";
					user_id = "";
					content_id = "";
					path = "";
					result = false;
				}
				else
				{
					userpath = string.Concat(new string[]
					{
						array[0]
					});
					user_id = string.Concat(new string[]
					{
						array[1]
					});
					content_id = string.Concat(new string[]
					{
						array[2]
					});
					path = string.Concat(new string[]
					{
						array[3]
					});
					result = true;
				}
			}
			catch (Exception ex)
			{
				LOGGER.DEBUG("[DRM] DRMStringtoAddress exception: " + ex.ToString(), Array.Empty<object>());
				userpath = "";
				user_id = "";
				content_id = "";
				path = "";
				result = false;
			}
			return result;
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00006A54 File Offset: 0x00004C54
		private Image DRMStringToImage(string DRMString)
		{
			Image result = null;
			IntPtr zero = IntPtr.Zero;
			string text;
			string text2;
			string text3;
			string text4;
			if (!this.DRMStringtoAddress(DRMString, out text, out text2, out text3, out text4))
			{
				return null;
			}
			LOGGER.DEBUG("[DRM] DRMStringToImage user_id = {0},  content_id = {1}, path = {2}", new object[]
			{
				text2,
				text3,
				text4
			});
			try
			{
				int num = AuraLayer.GetFileStreamCSharp(text2, text3, text4, ref zero);
				if (num == 0)
				{
					int num2 = AuraLayer.GetFileSizeCSharp(zero);
					LOGGER.DEBUG("[DRM] GetFileSizeCSharp total size = {0}", new object[]
					{
						num2
					});
					byte[] array = new byte[num2];
					int num3 = 0;
					for (;;)
					{
						int num4 = 0;
						IntPtr zero2 = IntPtr.Zero;
						if (AuraLayer.ReadStreamCSharp(zero, ref zero2, ref num4, true) != 0)
						{
							break;
						}
						Marshal.Copy(zero2, array, num3, num4);
						AuraLayer.ReleaseStreamDataCSharp(ref zero2);
						num3 += num4;
						int num5 = AuraLayer.GetFileSizeCSharp(zero);
						int num6 = AuraLayer.GetFileReadedSizeCSharp(zero);
						if (num5 <= num6)
						{
							goto IL_F4;
						}
					}
					LOGGER.DEBUG("[DRM] Error occurred: read stream fail", Array.Empty<object>());
					IL_F4:
					result = Image.FromStream(new MemoryStream(array));
				}
				else
				{
					LOGGER.DEBUG("[DRM] Error occurred: get file stream fail " + num.ToString(), Array.Empty<object>());
				}
				AuraLayer.DestroyStreamCSharp(zero);
				LOGGER.DEBUG("[DRM] DestroyStreamCSharp {0}", new object[]
				{
					zero.ToInt32()
				});
			}
			catch (Exception ex)
			{
				LOGGER.DEBUG("[DRM] DRMStringToImage()" + ex.ToString(), Array.Empty<object>());
			}
			return result;
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x060000A9 RID: 169 RVA: 0x00006BE8 File Offset: 0x00004DE8
		// (set) Token: 0x060000AA RID: 170 RVA: 0x00006BF0 File Offset: 0x00004DF0
		public int GradientFactor
		{
			get
			{
				return this._gradientFactor;
			}
			set
			{
				this._gradientFactor = value;
				LOGGER.DEBUG("[GradientColor] set factor = " + this._gradientFactor.ToString(), Array.Empty<object>());
				if (this.colorpoints.Count<AuraLayer.ColorPoint>() > 0)
				{
					Bitmap bitmap = this.Gradient2D(this.gradientW, this.gradientH, this.colorpoints);
					if (bitmap.Width > this.frameWidth && bitmap.Height > this.frameHeight)
					{
						this.StaticBitmap = this.GenerateBitmapFromGradientBitmap(bitmap, this.frameWidth, this.frameHeight);
						return;
					}
					this.StaticBitmap = new Bitmap(this.frameWidth, this.frameHeight);
					using (Graphics graphics = Graphics.FromImage(this.StaticBitmap))
					{
						graphics.DrawImage(bitmap, new Rectangle(0, 0, this.StaticBitmap.Width, this.StaticBitmap.Height), new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
						graphics.Flush();
					}
				}
			}
		}

		// Token: 0x060000AB RID: 171 RVA: 0x00006D00 File Offset: 0x00004F00
		public void InitGradientLayer(int gw, int gh, bool repeat = true, int offset_x = 0, int offset_y = 0, bool isFullLayer = true, int width = 21, int height = 8)
		{
			this.init(repeat, offset_x, offset_y, isFullLayer, width, height);
			this.gradientW = gw;
			this.gradientH = gh;
			this.isGradientLayer = true;
			this.isStaticFrame = true;
		}

		// Token: 0x060000AC RID: 172 RVA: 0x00006D30 File Offset: 0x00004F30
		public bool AddGradientColorPoint(int x, int y, int r, int g, int b)
		{
			LOGGER.DEBUG("[GradientColor] AddGradientColorPoint {0}, {1}, {2}, {3}, {4}", new object[]
			{
				x,
				y,
				r,
				g,
				b
			});
			if (x < 0 || x > this.gradientW)
			{
				return false;
			}
			if (y < 0 || y > this.gradientH)
			{
				return false;
			}
			this.colorpoints.Add(new AuraLayer.ColorPoint(new Vector((double)x, (double)y), Color.FromArgb(255, r, g, b)));
			Bitmap grabmp = this.Gradient2D(this.gradientW, this.gradientH, this.colorpoints);
			this.StaticBitmap = this.GenerateBitmapFromGradientBitmap(grabmp, this.frameWidth, this.frameHeight);
			return true;
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00006DF8 File Offset: 0x00004FF8
		public void ClearGradientColorPoint()
		{
			this.colorpoints.Clear();
			using (Graphics graphics = Graphics.FromImage(this.StaticBitmap))
			{
				graphics.Clear(Color.FromArgb(255, 0, 0, 0));
				graphics.Flush();
			}
		}

		// Token: 0x060000AE RID: 174 RVA: 0x00006E54 File Offset: 0x00005054
		private double Distance(Vector v1, Vector v2)
		{
			return Vector.Subtract(v1, v2).LengthSquared;
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00006E70 File Offset: 0x00005070
		private Bitmap Gradient2D(int Width, int Height, List<AuraLayer.ColorPoint> list)
		{
			LOGGER.DEBUG(string.Concat(new string[]
			{
				"[GradientColor]  Gradient2D: w = ",
				Width.ToString(),
				" h = ",
				Height.ToString(),
				" factor = ",
				this._gradientFactor.ToString()
			}), Array.Empty<object>());
			this.Distance(new Vector(0.0, 0.0), new Vector((double)(Width / 2), (double)(Height / 2)));
			Bitmap bitmap = new Bitmap(Width, Height);
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			byte[] array = new byte[bitmapData.Stride * bitmapData.Height];
			Marshal.Copy(bitmapData.Scan0, array, 0, array.Length);
			bitmap.UnlockBits(bitmapData);
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					Dictionary<AuraLayer.ColorPoint, double> dictionary = new Dictionary<AuraLayer.ColorPoint, double>();
					double num = 0.0;
					AuraLayer.ColorPoint colorPoint = list[0];
					double num2 = this.Distance(list[0].point, new Vector((double)i, (double)j));
					foreach (AuraLayer.ColorPoint colorPoint2 in list)
					{
						double num3 = this.Distance(colorPoint2.point, new Vector((double)i, (double)j));
						if (num3 < num2)
						{
							num2 = num3;
						}
						dictionary.Add(colorPoint2, num3);
						num += num3;
					}
					List<AuraLayer.ColorPoint> list2 = dictionary.Keys.ToList<AuraLayer.ColorPoint>();
					double num4 = num - dictionary.Min((KeyValuePair<AuraLayer.ColorPoint, double> m) => m.Value);
					double num5 = 0.0;
					foreach (AuraLayer.ColorPoint key in list2)
					{
						double num6 = Math.Pow((num - dictionary[key]) / num4, (double)this._gradientFactor);
						num5 += num6;
						dictionary[key] = num6;
					}
					double num7 = 0.0;
					double num8 = 0.0;
					double num9 = 0.0;
					foreach (KeyValuePair<AuraLayer.ColorPoint, double> keyValuePair in dictionary)
					{
						double num10 = keyValuePair.Value / num5;
						num7 += (double)keyValuePair.Key.color.R * num10;
						num8 += (double)keyValuePair.Key.color.G * num10;
						num9 += (double)keyValuePair.Key.color.B * num10;
					}
					int num11 = ((int)num7 >= 0) ? ((int)num7) : 0;
					int num12 = ((int)num8 >= 0) ? ((int)num8) : 0;
					int num13 = ((int)num9 >= 0) ? ((int)num9) : 0;
					int num14 = j * bitmapData.Stride + i * 4;
					array[num14] = (byte)num13;
					array[num14 + 1] = (byte)num12;
					array[num14 + 2] = (byte)num11;
					array[num14 + 3] = byte.MaxValue;
				}
			}
			BitmapData bitmapData2 = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			Marshal.Copy(array, 0, bitmapData2.Scan0, array.Length);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00007214 File Offset: 0x00005414
		private Bitmap GenerateBitmapFromGradientBitmap(Bitmap grabmp, int w, int h)
		{
			if (w > 100 || h > 100)
			{
				Bitmap bitmap = new Bitmap(w, h);
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.DrawImage(grabmp, new Rectangle(0, 0, bitmap.Width, bitmap.Height), new Rectangle(0, 0, grabmp.Width, grabmp.Height), GraphicsUnit.Pixel);
					graphics.Flush();
				}
				return bitmap;
			}
			Bitmap bitmap2 = new Bitmap(w, h);
			BitmapData bitmapData = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			int stride = bitmapData.Stride;
			byte[] array = new byte[bitmapData.Stride * bitmapData.Height];
			for (int i = 0; i < h; i++)
			{
				for (int j = 0; j < w; j++)
				{
					Color avgColorOfPartialRegion = this.GetAvgColorOfPartialRegion(grabmp, w, h, j, i);
					int num = i * stride + j * 4;
					array[num] = avgColorOfPartialRegion.B;
					array[num + 1] = avgColorOfPartialRegion.G;
					array[num + 2] = avgColorOfPartialRegion.R;
					array[num + 3] = byte.MaxValue;
				}
			}
			Marshal.Copy(array, 0, bitmapData.Scan0, array.Length);
			bitmap2.UnlockBits(bitmapData);
			return bitmap2;
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x0000735C File Offset: 0x0000555C
		private Color GetAvgColor(Bitmap sourceBitmap, int start_x, int start_y, int width, int height)
		{
			BitmapData bitmapData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			byte[] array = new byte[bitmapData.Stride * bitmapData.Height];
			Marshal.Copy(bitmapData.Scan0, array, 0, array.Length);
			sourceBitmap.UnlockBits(bitmapData);
			int stride = bitmapData.Stride;
			int[] array2 = new int[3];
			for (int i = start_y; i < start_y + height; i++)
			{
				for (int j = start_x; j < start_x + width; j++)
				{
					for (int k = 0; k < 3; k++)
					{
						int num = i * stride + j * 4 + k;
						array2[k] += (int)array[num];
					}
				}
			}
			int blue = array2[0] / (width * height);
			int green = array2[1] / (width * height);
			return Color.FromArgb(array2[2] / (width * height), green, blue);
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x00007440 File Offset: 0x00005640
		private Color GetAvgColorOfPartialRegion(Bitmap bmp, int total_row, int total_col, int row, int col)
		{
			int width = bmp.Width;
			int height = bmp.Height;
			int num = width / total_row;
			int num2 = height / total_col;
			return this.GetAvgColor(bmp, row * num, col * num2, num, num2);
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x00007472 File Offset: 0x00005672
		// (set) Token: 0x060000B4 RID: 180 RVA: 0x0000747A File Offset: 0x0000567A
		private Bitmap layerMaskBitmap { get; set; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x060000B5 RID: 181 RVA: 0x00007483 File Offset: 0x00005683
		// (set) Token: 0x060000B6 RID: 182 RVA: 0x0000748B File Offset: 0x0000568B
		public Color layermaskColor { get; set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x060000B7 RID: 183 RVA: 0x00007494 File Offset: 0x00005694
		// (set) Token: 0x060000B8 RID: 184 RVA: 0x0000749C File Offset: 0x0000569C
		public int layeroffset_x { get; set; }

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x060000B9 RID: 185 RVA: 0x000074A5 File Offset: 0x000056A5
		// (set) Token: 0x060000BA RID: 186 RVA: 0x000074AD File Offset: 0x000056AD
		public int layeroffset_y { get; set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060000BB RID: 187 RVA: 0x000074B6 File Offset: 0x000056B6
		// (set) Token: 0x060000BC RID: 188 RVA: 0x000074BE File Offset: 0x000056BE
		public bool repeatable { get; set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060000BD RID: 189 RVA: 0x000074C7 File Offset: 0x000056C7
		// (set) Token: 0x060000BE RID: 190 RVA: 0x000074CF File Offset: 0x000056CF
		public int startGlobalFrameID { get; set; }

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060000BF RID: 191 RVA: 0x000074D8 File Offset: 0x000056D8
		// (set) Token: 0x060000C0 RID: 192 RVA: 0x000074E0 File Offset: 0x000056E0
		public int frameWidth { get; set; }

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060000C1 RID: 193 RVA: 0x000074E9 File Offset: 0x000056E9
		// (set) Token: 0x060000C2 RID: 194 RVA: 0x000074F1 File Offset: 0x000056F1
		public int frameHeight { get; set; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000C3 RID: 195 RVA: 0x000074FA File Offset: 0x000056FA
		// (set) Token: 0x060000C4 RID: 196 RVA: 0x00007502 File Offset: 0x00005702
		public bool isStaticFrame { get; set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060000C5 RID: 197 RVA: 0x0000750B File Offset: 0x0000570B
		// (set) Token: 0x060000C6 RID: 198 RVA: 0x00007513 File Offset: 0x00005713
		public Bitmap StaticBitmap { get; set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060000C7 RID: 199 RVA: 0x0000751C File Offset: 0x0000571C
		// (set) Token: 0x060000C8 RID: 200 RVA: 0x00007524 File Offset: 0x00005724
		public bool isMusicLayer { get; set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x060000C9 RID: 201 RVA: 0x0000752D File Offset: 0x0000572D
		// (set) Token: 0x060000CA RID: 202 RVA: 0x00007535 File Offset: 0x00005735
		public bool isTextLayer { get; set; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060000CB RID: 203 RVA: 0x0000753E File Offset: 0x0000573E
		// (set) Token: 0x060000CC RID: 204 RVA: 0x00007546 File Offset: 0x00005746
		public int timeLength { get; set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060000CD RID: 205 RVA: 0x0000754F File Offset: 0x0000574F
		// (set) Token: 0x060000CE RID: 206 RVA: 0x00007557 File Offset: 0x00005757
		public int timeStart { get; set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060000CF RID: 207 RVA: 0x00007560 File Offset: 0x00005760
		// (set) Token: 0x060000D0 RID: 208 RVA: 0x00007568 File Offset: 0x00005768
		public bool Afterglow { get; set; }

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000D1 RID: 209 RVA: 0x00007571 File Offset: 0x00005771
		// (set) Token: 0x060000D2 RID: 210 RVA: 0x00007579 File Offset: 0x00005779
		public bool ApplyMatrix { get; set; }

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000D3 RID: 211 RVA: 0x00007582 File Offset: 0x00005782
		// (set) Token: 0x060000D4 RID: 212 RVA: 0x0000758A File Offset: 0x0000578A
		public float AnimationSpeedRatio
		{
			get
			{
				return this._AnimationSpeedRatio;
			}
			set
			{
				this._AnimationSpeedRatio = value;
				if (value > 100f)
				{
					this._AnimationSpeedRatio = 100f;
				}
				if (value <= 0f)
				{
					this._AnimationSpeedRatio = 1f;
				}
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000D5 RID: 213 RVA: 0x000075B9 File Offset: 0x000057B9
		// (set) Token: 0x060000D6 RID: 214 RVA: 0x000075C1 File Offset: 0x000057C1
		public int RotationDegree
		{
			get
			{
				return this._rotationDegree;
			}
			set
			{
				this._rotationDegree = value;
				if (value > 359)
				{
					this._rotationDegree = 0;
				}
				if (value < 0)
				{
					this._rotationDegree = 0;
				}
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000D7 RID: 215 RVA: 0x000075E4 File Offset: 0x000057E4
		// (set) Token: 0x060000D8 RID: 216 RVA: 0x000075EC File Offset: 0x000057EC
		public bool IsShowAsMatrixDefault { get; set; }

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060000D9 RID: 217 RVA: 0x000075F5 File Offset: 0x000057F5
		// (set) Token: 0x060000DA RID: 218 RVA: 0x000075FD File Offset: 0x000057FD
		public int MatrixDirection { get; set; }

		// Token: 0x060000DB RID: 219 RVA: 0x00007606 File Offset: 0x00005806
		public int GetAnimationLength()
		{
			return this.imageInfo.AnimationLength;
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00007614 File Offset: 0x00005814
		public AuraLayer(AuraAnimation parent)
		{
			this.parentAnimation = parent;
		}

		// Token: 0x060000DD RID: 221 RVA: 0x000076EC File Offset: 0x000058EC
		public void init(bool repeat = true, int offset_x = 0, int offset_y = 0, bool isFullLayer = true, int width = 21, int height = 8)
		{
			if (width <= 0)
			{
				width = 1;
			}
			if (height <= 0)
			{
				height = 1;
			}
			this.frameWidth = width;
			this.frameHeight = height;
			this.layermaskColor = Color.Black;
			this.isStaticFrame = false;
			this.isMusicLayer = false;
			this.isTextLayer = false;
			this.isScreenCaptureLayer = false;
			this.Afterglow = false;
			this.RotationDegree = 0;
			this.ApplyMatrix = false;
			this._audioEffectStrength = 0;
			this.startGlobalFrameID = 0;
			this._DecayEffect = 0f;
			this._StrobingBeatThreadhold = 0;
			this._StrobingBeatStartFreq = 0;
			this._StrobingBeatEndFreq = 10;
			this.StrobingBeatUseMax = false;
			if (isFullLayer)
			{
				this.frameWidth = this.parentAnimation.GetWidth();
				this.frameHeight = this.parentAnimation.GetHeight();
			}
			this.layeroffset_x = offset_x;
			this.layeroffset_y = offset_y;
			this.repeatable = repeat;
			this.frameList = new List<Bitmap>(1000);
			this.imageInfo = default(AuraLayer.ImageInfo);
			this.imageInfo.AnimationLength = 0;
			this.layerMaskBitmap = new Bitmap(this.frameWidth, this.frameHeight);
			Graphics graphics = Graphics.FromImage(this.layerMaskBitmap);
			graphics.Clear(Color.White);
			graphics.Dispose();
			this.CropframeList = new List<Bitmap>(1000);
			this.StaticBitmap = new Bitmap(this.frameWidth, this.frameHeight);
			this.CropRect = new Rectangle(0, 0, this.frameWidth, this.frameHeight);
			this.imageInfo.IsMatrixDefault = false;
			this.MatrixDirection = this.parentAnimation.MatrixDirection;
			LOGGER.DEBUG(string.Concat(new string[]
			{
				"[AuraLayer] Layer init x = ",
				offset_x.ToString(),
				" y = ",
				offset_y.ToString(),
				" w = ",
				this.frameWidth.ToString(),
				" h = ",
				this.frameHeight.ToString()
			}), Array.Empty<object>());
		}

		// Token: 0x060000DE RID: 222 RVA: 0x000078E8 File Offset: 0x00005AE8
		public void UpdateStaticeFrame()
		{
			if (this.isMusicLayer)
			{
				this.UpdateMusicFrame();
				return;
			}
			if (this.isTextLayer)
			{
				this.UpdateTextFrame();
				return;
			}
			if (this.isScreenCaptureLayer)
			{
				this.UpdateScreenCaptureFrame();
				return;
			}
			if (this.isSlashLightingLayer)
			{
				this.UpdateSlashLightingFrame();
			}
		}

		// Token: 0x060000DF RID: 223 RVA: 0x00007928 File Offset: 0x00005B28
		private void DrawStaticBitmap(Bitmap frame)
		{
			using (Graphics graphics = Graphics.FromImage(this.StaticBitmap))
			{
				graphics.DrawImage(frame, new Rectangle(0, 0, this.StaticBitmap.Width, this.StaticBitmap.Height), new Rectangle(0, 0, frame.Width, frame.Height), GraphicsUnit.Pixel);
				graphics.Flush();
			}
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x0000799C File Offset: 0x00005B9C
		public void SetCrop(int x, int y, int w, int h)
		{
			LOGGER.DEBUG(string.Concat(new string[]
			{
				"[Picture] SetCrop(",
				x.ToString(),
				",",
				y.ToString(),
				",",
				w.ToString(),
				",",
				h.ToString(),
				")"
			}), Array.Empty<object>());
			this.CropRect = new Rectangle(x, y, w, h);
			this.ContructCropframeList();
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x00007A25 File Offset: 0x00005C25
		public Rectangle GetCropRect()
		{
			return this.CropRect;
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x00007A30 File Offset: 0x00005C30
		public bool IsCropped()
		{
			return this.CropRect.X != 0 || this.CropRect.Y != 0 || this.CropRect.Width != this.frameWidth || this.CropRect.Height != this.frameHeight;
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x00007A80 File Offset: 0x00005C80
		public void ClearCrop()
		{
			this.SetCrop(0, 0, this.frameWidth, this.frameHeight);
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060000E4 RID: 228 RVA: 0x00007A96 File Offset: 0x00005C96
		// (set) Token: 0x060000E5 RID: 229 RVA: 0x00007A9E File Offset: 0x00005C9E
		public bool IsShowCropPreview { get; set; }

		// Token: 0x060000E6 RID: 230 RVA: 0x00007AA8 File Offset: 0x00005CA8
		private void ContructCropframeList()
		{
			if (!this.IsCropped())
			{
				return;
			}
			this.CropframeList.Clear();
			for (int i = 0; i < this.frameList.Count<Bitmap>(); i++)
			{
				if (this.imageInfo.IsMatrixDefault)
				{
					Bitmap image = this.RevertMatrixDefaultContent(this.frameList[i]);
					int num = this.parentAnimation.GetMatrixParameterSlashHeight() + (this.parentAnimation.GetMatrixParameterWidth() - 1) / 2 - (this.parentAnimation.GetMatrixParameterWidth() + this.parentAnimation.GetMatrixParameterWidth() % 2) / 2 + this.parentAnimation.GetMatrixParameterWidth() / 2;
					if (this.parentAnimation.GetMatrixParameterWidth() == 68 && this.parentAnimation.GetMatrixParameterHeight() == 28 && this.parentAnimation.GetMatrixParameterSlashHeight() == 36)
					{
						num = 74;
					}
					int num2 = num * this.frameWidth / this.parentAnimation.GetMatrixParameterWidth();
					int num3 = this.parentAnimation.GetMatrixParameterSlashHeight() * this.frameHeight / this.parentAnimation.GetMatrixParameterHeight();
					double num4 = (double)this.imageInfo.Width / (double)num2;
					double num5 = (double)this.imageInfo.Height / (double)num3;
					if (num4 <= num5)
					{
						Convert.ToInt32(num3 * this.imageInfo.Width / this.imageInfo.Height);
					}
					if (num5 <= num4)
					{
						Convert.ToInt32(num2 * this.imageInfo.Height / this.imageInfo.Width);
					}
					Bitmap bitmap = new Bitmap(num2, num3);
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						graphics.Clear(Color.FromArgb(0, 0, 0, 0));
						Rectangle srcRect = new Rectangle(this.CropRect.X, this.CropRect.Y, this.CropRect.Width, this.CropRect.Height);
						Rectangle destRect = new Rectangle(this.CropRect.X, this.CropRect.Y, this.CropRect.Width, this.CropRect.Height);
						graphics.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
					}
					this.CropframeList.Add(this.ConvertMatrixDefaultContent(bitmap));
				}
				else
				{
					Bitmap image2 = this.frameList[i];
					Bitmap bitmap2 = new Bitmap(this.frameWidth, this.frameHeight);
					using (Graphics graphics2 = Graphics.FromImage(bitmap2))
					{
						graphics2.Clear(Color.FromArgb(0, 0, 0, 0));
						Rectangle srcRect2 = new Rectangle(this.CropRect.X, this.CropRect.Y, this.CropRect.Width, this.CropRect.Height);
						Rectangle destRect2 = new Rectangle(this.CropRect.X, this.CropRect.Y, this.CropRect.Width, this.CropRect.Height);
						graphics2.DrawImage(image2, destRect2, srcRect2, GraphicsUnit.Pixel);
					}
					this.CropframeList.Add(bitmap2);
				}
			}
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x00007DC4 File Offset: 0x00005FC4
		public void deInit()
		{
			this.analyzer = null;
			this.discardCap = true;
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x00007DD4 File Offset: 0x00005FD4
		~AuraLayer()
		{
			LOGGER.DEBUG("[AuraLayer] ~AuraLayer()+", Array.Empty<object>());
			if (this.DoCapture)
			{
				this.discardCap = true;
				AuraLayer.DecrementAndScheduleKill();
			}
			this.frameList.Clear();
			LOGGER.DEBUG("[AuraLayer] ~AuraLayer()-", Array.Empty<object>());
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x00007E38 File Offset: 0x00006038
		public int GetFrameCount()
		{
			return this.frameList.Count<Bitmap>();
		}

		// Token: 0x060000EA RID: 234 RVA: 0x00007E45 File Offset: 0x00006045
		public Bitmap GetFrameAt(int index)
		{
			return this.frameList[index];
		}

		// Token: 0x060000EB RID: 235 RVA: 0x00007E54 File Offset: 0x00006054
		private Bitmap ConvertMatrixDefaultContent(Bitmap originBitmap)
		{
			int num = this.frameWidth * 3;
			int num2 = this.frameHeight * 3;
			int num3 = this.parentAnimation.GetMatrixParameterSlashHeight() * this.frameHeight / this.parentAnimation.GetMatrixParameterHeight();
			Bitmap bitmap = new Bitmap(num, num2);
			this.imageInfo.default_Width = originBitmap.Width;
			this.imageInfo.default_Height = originBitmap.Height;
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					int num4 = j - this.frameHeight + (i - this.frameWidth + 1) / 2;
					int num5 = j - this.frameHeight - (i - this.frameWidth) / 2 + ((this.parentAnimation.GetMatrixParameterWidth() + this.parentAnimation.GetMatrixParameterWidth() % 2) / 2 - 1);
					if (num4 >= 0 && num4 < this.imageInfo.default_Width && num5 >= 0 && num5 < this.imageInfo.default_Height)
					{
						if (this.MatrixDirection != 0)
						{
							bitmap.SetPixel(num - i - 1, j, originBitmap.GetPixel(this.imageInfo.default_Width - num4 - 1, num5));
						}
						else
						{
							bitmap.SetPixel(i, j, originBitmap.GetPixel(num4, num5));
						}
					}
				}
			}
			return bitmap;
		}

		// Token: 0x060000EC RID: 236 RVA: 0x00007F9C File Offset: 0x0000619C
		private Bitmap RevertMatrixDefaultContent(Bitmap defaultBitmap)
		{
			int num = this.frameWidth * 3;
			int num2 = this.frameHeight * 3;
			int num3 = this.parentAnimation.GetMatrixParameterSlashHeight() * this.frameHeight / this.parentAnimation.GetMatrixParameterHeight();
			Bitmap bitmap = new Bitmap(this.imageInfo.default_Width, this.imageInfo.default_Height);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					int num4 = j - this.frameHeight + (i - this.frameWidth + 1) / 2;
					int num5 = j - this.frameHeight - (i - this.frameWidth) / 2 + ((this.parentAnimation.GetMatrixParameterWidth() + this.parentAnimation.GetMatrixParameterWidth() % 2) / 2 - 1);
					if (num4 >= 0 && num4 < this.imageInfo.default_Width && num5 >= 0 && num5 < this.imageInfo.default_Height)
					{
						if (this.MatrixDirection != 0)
						{
							bitmap.SetPixel(this.imageInfo.default_Width - num4 - 1, num5, defaultBitmap.GetPixel(num - i - 1, j));
						}
						else
						{
							bitmap.SetPixel(num4, num5, defaultBitmap.GetPixel(i, j));
						}
					}
				}
			}
			return bitmap;
		}

		// Token: 0x060000ED RID: 237 RVA: 0x000080D5 File Offset: 0x000062D5
		public void OpenPicture(string path)
		{
			this.OpenPicture(path, false, false);
		}

		// Token: 0x060000EE RID: 238 RVA: 0x000080E0 File Offset: 0x000062E0
		public void OpenPicture(string path, bool matrixLayout)
		{
			this.OpenPicture(path, matrixLayout, false);
		}

		// Token: 0x060000EF RID: 239 RVA: 0x000080EC File Offset: 0x000062EC
		public void OpenPicture(string path, bool matrixLayout, bool isloaddefault)
		{
			LOGGER.DEBUG(string.Concat(new string[]
			{
				"[Picture] OpenPicture(",
				path,
				", ",
				matrixLayout.ToString(),
				", ",
				isloaddefault.ToString(),
				")"
			}), Array.Empty<object>());
			this.imageInfo.IsMatrixDefault = isloaddefault;
			try
			{
				string drmstring = this.GetDRMString(path);
				Image image;
				if (drmstring == null)
				{
					image = Image.FromFile(path);
				}
				else
				{
					if (!this.IsDRMInited)
					{
						LOGGER.DEBUG("[Picture] OpenPicture: before DRM_init", Array.Empty<object>());
						this.DRM_Init(drmstring);
						LOGGER.DEBUG("[Picture] OpenPicture: after DRM_init", Array.Empty<object>());
						this.IsDRMInited = true;
					}
					image = this.DRMStringToImage(drmstring);
				}
				this.imageInfo.Height = image.Height;
				this.imageInfo.Width = image.Width;
				double num = (double)image.Width / (double)this.frameWidth;
				double num2 = (double)image.Height / (double)this.frameHeight;
				this.imageInfo.newWidth = ((num > num2) ? this.frameWidth : Convert.ToInt32(this.frameHeight * image.Width / image.Height));
				this.imageInfo.newHeight = ((num2 > num) ? this.frameHeight : Convert.ToInt32(this.frameWidth * image.Height / image.Width));
				if (matrixLayout && !isloaddefault)
				{
					if (this.imageInfo.newWidth == this.frameWidth)
					{
						this.imageInfo.newHeight = this.imageInfo.newHeight * 2 / 3;
					}
					else if (this.imageInfo.newHeight == this.frameHeight)
					{
						this.imageInfo.newWidth = this.imageInfo.newWidth * 3 / 2;
					}
					this.ApplyMatrix = matrixLayout;
				}
				LOGGER.DEBUG(string.Concat(new string[]
				{
					"[Picture] OPEN PIC w=",
					image.Width.ToString(),
					" PIC h=",
					image.Height.ToString(),
					" Frame w=",
					this.frameWidth.ToString(),
					" Frame h=",
					this.frameHeight.ToString(),
					" New w=",
					this.imageInfo.newWidth.ToString(),
					" New h=",
					this.imageInfo.newHeight.ToString()
				}), Array.Empty<object>());
				if (image.RawFormat.Equals(ImageFormat.Gif))
				{
					if (ImageAnimator.CanAnimate(image))
					{
						LOGGER.DEBUG("[Picture] Frame Count = " + image.GetFrameCount(FrameDimension.Time).ToString(), Array.Empty<object>());
						this.imageInfo.FrameCount = image.GetFrameCount(FrameDimension.Time);
						byte[] value = image.GetPropertyItem(20736).Value;
						this.imageInfo.FrameDelay = new int[this.imageInfo.FrameCount];
						this.imageInfo.AnimationLength = 0;
						for (int i = 0; i < this.imageInfo.FrameCount; i++)
						{
							this.imageInfo.FrameDelay[i] = BitConverter.ToInt32(value, 4 * i) * 10;
							if (this.imageInfo.FrameDelay[i] == 0)
							{
								this.imageInfo.FrameDelay[i] = 100;
							}
							this.imageInfo.AnimationLength = this.imageInfo.AnimationLength + this.imageInfo.FrameDelay[i];
						}
						this.imageInfo.IsAnimated = true;
						this.imageInfo.IsLooped = (BitConverter.ToInt16(image.GetPropertyItem(20737).Value, 0) != 1);
						for (int j = 0; j < this.imageInfo.FrameCount; j++)
						{
							image.SelectActiveFrame(FrameDimension.Time, j);
							if (isloaddefault)
							{
								this.IsShowAsMatrixDefault = true;
								int num3 = this.parentAnimation.GetMatrixParameterSlashHeight() + (this.parentAnimation.GetMatrixParameterWidth() - 1) / 2 - (this.parentAnimation.GetMatrixParameterWidth() + this.parentAnimation.GetMatrixParameterWidth() % 2) / 2 + this.parentAnimation.GetMatrixParameterWidth() / 2;
								if (this.parentAnimation.GetMatrixParameterWidth() == 68 && this.parentAnimation.GetMatrixParameterHeight() == 28 && this.parentAnimation.GetMatrixParameterSlashHeight() == 36)
								{
									num3 = 74;
								}
								int num4 = num3 * this.frameWidth / this.parentAnimation.GetMatrixParameterWidth();
								int num5 = this.parentAnimation.GetMatrixParameterSlashHeight() * this.frameHeight / this.parentAnimation.GetMatrixParameterHeight();
								double num6 = (double)image.Width / (double)num4;
								double num7 = (double)image.Height / (double)num5;
								int num8 = (num6 > num7) ? num4 : Convert.ToInt32(num5 * image.Width / image.Height);
								int num9 = (num7 > num6) ? num5 : Convert.ToInt32(num4 * image.Height / image.Width);
								Bitmap bitmap = new Bitmap(num4, num5);
								using (Graphics graphics = Graphics.FromImage(bitmap))
								{
									graphics.Clear(Color.FromArgb(0, 0, 0, 0));
									if (matrixLayout)
									{
										graphics.DrawImage(image, num4 / 2 - num8 / 2, num5 / 2 - num9 / 2, num8, num9);
									}
									else
									{
										graphics.DrawImage(image, 0, 0, image.Width, image.Height);
									}
								}
								this.frameList.Add(this.ConvertMatrixDefaultContent(bitmap));
							}
							else
							{
								Bitmap bitmap2 = new Bitmap(this.frameWidth, this.frameHeight);
								using (Graphics graphics2 = Graphics.FromImage(bitmap2))
								{
									graphics2.Clear(Color.FromArgb(0, 0, 0, 0));
									graphics2.DrawImage(image, this.frameWidth / 2 - this.imageInfo.newWidth / 2, this.frameHeight / 2 - this.imageInfo.newHeight / 2, this.imageInfo.newWidth, this.imageInfo.newHeight);
								}
								this.frameList.Add(bitmap2);
							}
						}
					}
					else
					{
						this.imageInfo.FrameCount = 1;
						this.imageInfo.FrameDelay = new int[1];
						this.imageInfo.FrameDelay[0] = 1;
						this.imageInfo.AnimationLength = 1;
						this.imageInfo.IsAnimated = false;
						this.imageInfo.IsLooped = false;
						if (isloaddefault)
						{
							this.IsShowAsMatrixDefault = true;
							int num10 = this.parentAnimation.GetMatrixParameterSlashHeight() + (this.parentAnimation.GetMatrixParameterWidth() - 1) / 2 - (this.parentAnimation.GetMatrixParameterWidth() + this.parentAnimation.GetMatrixParameterWidth() % 2) / 2 + this.parentAnimation.GetMatrixParameterWidth() / 2;
							if (this.parentAnimation.GetMatrixParameterWidth() == 68 && this.parentAnimation.GetMatrixParameterHeight() == 28 && this.parentAnimation.GetMatrixParameterSlashHeight() == 36)
							{
								num10 = 74;
							}
							int num11 = num10 * this.frameWidth / this.parentAnimation.GetMatrixParameterWidth();
							int num12 = this.parentAnimation.GetMatrixParameterSlashHeight() * this.frameHeight / this.parentAnimation.GetMatrixParameterHeight();
							double num13 = (double)image.Width / (double)num11;
							double num14 = (double)image.Height / (double)num12;
							int num15 = (num13 > num14) ? num11 : Convert.ToInt32(num12 * image.Width / image.Height);
							int num16 = (num14 > num13) ? num12 : Convert.ToInt32(num11 * image.Height / image.Width);
							Bitmap bitmap3 = new Bitmap(num11, num12);
							using (Graphics graphics3 = Graphics.FromImage(bitmap3))
							{
								graphics3.Clear(Color.FromArgb(0, 0, 0, 0));
								graphics3.Clear(Color.FromArgb(0, 0, 0, 0));
								if (matrixLayout)
								{
									graphics3.DrawImage(image, num11 / 2 - num15 / 2, num12 / 2 - num16 / 2, num15, num16);
								}
								else
								{
									graphics3.DrawImage(image, 0, 0, image.Width, image.Height);
								}
							}
							this.frameList.Add(this.ConvertMatrixDefaultContent(bitmap3));
						}
						else
						{
							Bitmap bitmap4 = new Bitmap(this.frameWidth, this.frameHeight);
							using (Graphics graphics4 = Graphics.FromImage(bitmap4))
							{
								graphics4.Clear(Color.FromArgb(0, 0, 0, 0));
								graphics4.DrawImage(image, this.frameWidth / 2 - this.imageInfo.newWidth / 2, this.frameHeight / 2 - this.imageInfo.newHeight / 2, this.imageInfo.newWidth, this.imageInfo.newHeight);
							}
							this.frameList.Add(bitmap4);
						}
					}
				}
				else
				{
					this.imageInfo.FrameCount = 1;
					this.imageInfo.FrameDelay = new int[1];
					this.imageInfo.FrameDelay[0] = 1;
					this.imageInfo.AnimationLength = 1;
					this.imageInfo.IsAnimated = false;
					this.imageInfo.IsLooped = false;
					if (isloaddefault)
					{
						this.IsShowAsMatrixDefault = true;
						int num17 = this.parentAnimation.GetMatrixParameterSlashHeight() + (this.parentAnimation.GetMatrixParameterWidth() - 1) / 2 - (this.parentAnimation.GetMatrixParameterWidth() + this.parentAnimation.GetMatrixParameterWidth() % 2) / 2 + this.parentAnimation.GetMatrixParameterWidth() / 2;
						if (this.parentAnimation.GetMatrixParameterWidth() == 68 && this.parentAnimation.GetMatrixParameterHeight() == 28 && this.parentAnimation.GetMatrixParameterSlashHeight() == 36)
						{
							num17 = 74;
						}
						int num18 = num17 * this.frameWidth / this.parentAnimation.GetMatrixParameterWidth();
						int num19 = this.parentAnimation.GetMatrixParameterSlashHeight() * this.frameHeight / this.parentAnimation.GetMatrixParameterHeight();
						double num20 = (double)image.Width / (double)num18;
						double num21 = (double)image.Height / (double)num19;
						int num22 = (num20 > num21) ? num18 : Convert.ToInt32(num19 * image.Width / image.Height);
						int num23 = (num21 > num20) ? num19 : Convert.ToInt32(num18 * image.Height / image.Width);
						Bitmap bitmap5 = new Bitmap(num18, num19);
						using (Graphics graphics5 = Graphics.FromImage(bitmap5))
						{
							graphics5.Clear(Color.FromArgb(0, 0, 0, 0));
							graphics5.Clear(Color.FromArgb(0, 0, 0, 0));
							if (matrixLayout)
							{
								graphics5.DrawImage(image, num18 / 2 - num22 / 2, num19 / 2 - num23 / 2, num22, num23);
							}
							else
							{
								graphics5.DrawImage(image, 0, 0, image.Width, image.Height);
							}
						}
						this.frameList.Add(this.ConvertMatrixDefaultContent(bitmap5));
					}
					else
					{
						Bitmap bitmap6 = new Bitmap(this.frameWidth, this.frameHeight);
						using (Graphics graphics6 = Graphics.FromImage(bitmap6))
						{
							graphics6.Clear(Color.FromArgb(0, 0, 0, 0));
							graphics6.DrawImage(image, this.frameWidth / 2 - this.imageInfo.newWidth / 2, this.frameHeight / 2 - this.imageInfo.newHeight / 2, this.imageInfo.newWidth, this.imageInfo.newHeight);
						}
						this.frameList.Add(bitmap6);
					}
				}
				image.Dispose();
			}
			catch (Exception ex)
			{
				LOGGER.DEBUG("[Picture] OpenPicture exception: " + ex.ToString(), Array.Empty<object>());
			}
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x00008CF0 File Offset: 0x00006EF0
		public Bitmap GetPictureFrame(int timeFromStart)
		{
			int num = timeFromStart;
			if (timeFromStart > this.timeLength)
			{
				return null;
			}
			if (this.IsCropped())
			{
				if (this.CropframeList.Count<Bitmap>() == 0)
				{
					return null;
				}
			}
			else if (this.frameList.Count<Bitmap>() == 0)
			{
				return null;
			}
			if (!this.imageInfo.IsAnimated)
			{
				if (!this.IsCropped())
				{
					return this.frameList[0];
				}
				if (this.IsShowCropPreview)
				{
					return this.DrawCropPreview(0);
				}
				return this.CropframeList[0];
			}
			else
			{
				if ((float)timeFromStart >= (float)this.imageInfo.AnimationLength * this._AnimationSpeedRatio)
				{
					if (!this.imageInfo.IsLooped)
					{
						if (!this.IsCropped())
						{
							return this.frameList[this.frameList.Count<Bitmap>() - 1];
						}
						if (this.IsShowCropPreview)
						{
							return this.DrawCropPreview(this.CropframeList.Count<Bitmap>() - 1);
						}
						return this.CropframeList[this.CropframeList.Count<Bitmap>() - 1];
					}
					else
					{
						num = timeFromStart % (int)((float)this.imageInfo.AnimationLength * this._AnimationSpeedRatio);
					}
				}
				float num2 = 0f;
				if (this.IsCropped())
				{
					int i = 0;
					while (i < this.CropframeList.Count<Bitmap>())
					{
						num2 += (float)this.imageInfo.FrameDelay[i] * this._AnimationSpeedRatio;
						if (num2 >= (float)num)
						{
							if (this.IsShowCropPreview)
							{
								return this.DrawCropPreview(i);
							}
							return this.CropframeList[i];
						}
						else
						{
							i++;
						}
					}
				}
				else
				{
					for (int j = 0; j < this.frameList.Count<Bitmap>(); j++)
					{
						num2 += (float)this.imageInfo.FrameDelay[j] * this._AnimationSpeedRatio;
						if (num2 >= (float)num)
						{
							return this.frameList[j];
						}
					}
				}
				if (!this.IsCropped())
				{
					return this.frameList[0];
				}
				if (this.IsShowCropPreview)
				{
					return this.DrawCropPreview(0);
				}
				return this.CropframeList[0];
			}
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x00008ED4 File Offset: 0x000070D4
		private Bitmap DrawCropPreview(int index)
		{
			Bitmap bitmap = AuraAnimation.Brightness(this.frameList[index], -75);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.DrawImage(this.CropframeList[index], new Rectangle(0, 0, bitmap.Width, bitmap.Height));
				graphics.Flush();
			}
			return bitmap;
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00008F44 File Offset: 0x00007144
		public void AddGif(string path)
		{
			try
			{
				int num = 0;
				Image image = Image.FromFile(path);
				double num2 = (double)image.Width / (double)this.frameWidth;
				double num3 = (double)image.Height / (double)this.frameHeight;
				if (num2 <= num3)
				{
					Convert.ToInt32(this.frameHeight * image.Width / image.Height);
				}
				else
				{
					int frameWidth = this.frameWidth;
				}
				if (num3 <= num2)
				{
					Convert.ToInt32(this.frameWidth * image.Height / image.Width);
				}
				else
				{
					int frameHeight = this.frameHeight;
				}
				if (image.RawFormat.Equals(ImageFormat.Gif))
				{
					num = image.GetFrameCount(FrameDimension.Time);
					LOGGER.DEBUG("[Picture] GIF frame count = " + num.ToString(), Array.Empty<object>());
					this.imageInfo.FrameCount = image.GetFrameCount(FrameDimension.Time);
					byte[] value = image.GetPropertyItem(20736).Value;
					this.imageInfo.FrameDelay = new int[this.imageInfo.FrameCount];
					this.imageInfo.AnimationLength = 0;
					for (int i = 0; i < this.imageInfo.FrameCount; i++)
					{
						this.imageInfo.FrameDelay[i] = BitConverter.ToInt32(value, 4 * i) * 10;
						if (this.imageInfo.FrameDelay[i] == 0)
						{
							this.imageInfo.FrameDelay[i] = 100;
						}
						this.imageInfo.AnimationLength = this.imageInfo.AnimationLength + this.imageInfo.FrameDelay[i];
					}
					this.imageInfo.IsAnimated = true;
					this.imageInfo.IsLooped = (BitConverter.ToInt16(image.GetPropertyItem(20737).Value, 0) != 1);
				}
				else
				{
					LOGGER.DEBUG("[Picture] Not gif file", Array.Empty<object>());
					this.imageInfo.FrameCount = 1;
					this.imageInfo.FrameDelay = new int[1];
					this.imageInfo.FrameDelay[0] = 1;
					this.imageInfo.AnimationLength = 1;
					this.imageInfo.IsAnimated = false;
					this.imageInfo.IsLooped = false;
					num = 1;
				}
				for (int j = 0; j < num; j++)
				{
					if (image.RawFormat.Equals(ImageFormat.Gif))
					{
						image.SelectActiveFrame(FrameDimension.Time, j);
					}
					int x = image.Width / 2;
					int y = image.Height / 2;
					((Bitmap)image).GetPixel(x, y);
					Bitmap bitmap = new Bitmap(this.frameWidth, this.frameHeight);
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						graphics.Clear(Color.Black);
						if (this.frameHeight < 5 || this.frameWidth < 14)
						{
							Bitmap image2 = new Bitmap(this.frameWidth * 2, this.frameHeight * 2);
							using (Graphics graphics2 = Graphics.FromImage(image2))
							{
								graphics2.Clear(Color.Black);
								graphics2.DrawImage(image, 0, 0, this.frameWidth * 2, this.frameHeight * 2);
								graphics2.Flush();
							}
							graphics.DrawImage(image2, 0, 0, this.frameWidth, this.frameHeight);
						}
						else
						{
							graphics.DrawImage(image, 0, 0, this.frameWidth, this.frameHeight);
						}
						graphics.Flush();
					}
					int x2 = bitmap.Width / 2;
					int y2 = bitmap.Height / 2;
					bitmap.GetPixel(x2, y2);
					this.frameList.Add(bitmap);
				}
				image.Dispose();
			}
			catch (Exception ex)
			{
				LOGGER.DEBUG(ex.ToString(), Array.Empty<object>());
			}
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x00009324 File Offset: 0x00007524
		public void AddStaticPicture(string path, int framecount)
		{
			Image image = Image.FromFile(path);
			this.imageInfo.FrameCount = framecount;
			this.imageInfo.FrameDelay = new int[this.imageInfo.FrameCount];
			this.imageInfo.AnimationLength = 0;
			for (int i = 0; i < this.imageInfo.FrameCount; i++)
			{
				this.imageInfo.FrameDelay[i] = 100;
				this.imageInfo.AnimationLength = this.imageInfo.AnimationLength + this.imageInfo.FrameDelay[i];
			}
			this.imageInfo.IsAnimated = false;
			this.imageInfo.IsLooped = false;
			for (int j = 0; j < framecount; j++)
			{
				this.imageInfo.FrameDelay[j] = 100;
				this.imageInfo.AnimationLength = this.imageInfo.AnimationLength + this.imageInfo.FrameDelay[j];
				Bitmap bitmap = new Bitmap(this.frameWidth, this.frameHeight);
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.Clear(Color.Black);
					graphics.DrawImage(image, 0, 0, this.frameWidth, this.frameHeight);
				}
				this.frameList.Add(bitmap);
			}
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00009468 File Offset: 0x00007668
		public void InitTextLayer(bool repeat = true, int offset_x = 0, int offset_y = 0, bool isFullLayer = true, int width = 21, int height = 8)
		{
			this.init(repeat, offset_x, offset_y, isFullLayer, width, height);
			this.isStaticFrame = true;
			this.isTextLayer = true;
			this.TextUpdateCount = 0;
			this.TextList = new List<AuraLayer.TextContent>(100);
			this.localDate = DateTime.Now;
			this.SysPower = default(AuraLayer.SYSTEM_POWER_STATUS);
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x000094C0 File Offset: 0x000076C0
		public void AddTextContent(string Content, bool Slide, bool Revert, int Offset_x, int Offset_y, int Width, int Height, string Fontname, int Fontsize, int Speed, bool IsAntiAlias, bool Border, Color TextColor)
		{
			this.AddTextContent_withAlignment(Content, Slide, Revert, Offset_x, Offset_y, Width, Height, Fontname, Fontsize, Speed, IsAntiAlias, Border, TextColor, 0, 0);
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x000094EC File Offset: 0x000076EC
		public void AddTextContent_withAlignment(string Content, bool Slide, bool Revert, int Offset_x, int Offset_y, int Width, int Height, string Fontname, int Fontsize, int Speed, bool IsAntiAlias, bool Border, Color TextColor, int Halign, int Valign)
		{
			LOGGER.DEBUG(string.Concat(new string[]
			{
				"[Text] Add String:",
				Content,
				" Font name:\"",
				Fontname,
				"\" Font size:",
				Fontsize.ToString()
			}), Array.Empty<object>());
			if (Content == null || Content.Length == 0)
			{
				LOGGER.DEBUG("[Text] AddTextContent(): Not avail string content!", Array.Empty<object>());
				return;
			}
			AuraLayer.TextContent item = new AuraLayer.TextContent
			{
				content = Content,
				bSlide = Slide,
				bRevert = Revert,
				offset_x = Offset_x,
				offset_y = Offset_y,
				width = Width,
				height = Height,
				fontname = Fontname,
				fontsize = Fontsize,
				speed = Speed,
				isAntiAlias = IsAntiAlias,
				bBorder = Border,
				color = TextColor,
				h_align = Halign,
				v_align = Valign
			};
			this.TextList.Add(item);
			if (Content.Contains("[@"))
			{
				this.hasTextPattern = true;
			}
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x000095EF File Offset: 0x000077EF
		public void ClearTextContent()
		{
			this.TextList.Clear();
			this.TextUpdateCount = 0;
			this.hasTextPattern = false;
		}

		// Token: 0x060000F8 RID: 248
		[DllImport("kernel32")]
		private static extern void GetSystemPowerStatus(ref AuraLayer.SYSTEM_POWER_STATUS lpSystemPowerStatus);

		// Token: 0x060000F9 RID: 249 RVA: 0x0000960A File Offset: 0x0000780A
		private void UpdateSpecialStringState()
		{
			if (!this.hasTextPattern)
			{
				return;
			}
			this.localDate = DateTime.Now;
			AuraLayer.GetSystemPowerStatus(ref this.SysPower);
		}

		// Token: 0x060000FA RID: 250 RVA: 0x0000962C File Offset: 0x0000782C
		private string ConvertSpecialString(string str)
		{
			if (!this.hasTextPattern)
			{
				return str;
			}
			string arg = this.localDate.Hour.ToString();
			string arg2 = "AM";
			if (this.localDate.Hour >= 13 && this.localDate.Hour <= 23)
			{
				arg = (this.localDate.Hour - 12).ToString();
				arg2 = "PM";
			}
			if (this.localDate.Hour == 12)
			{
				arg2 = "PM";
			}
			str = str.Replace("[@YEAR]", string.Format("{0:0000}", this.localDate.Year));
			str = str.Replace("[@MONTH]", string.Format("{0:00}", this.localDate.Month));
			str = str.Replace("[@DAY]", string.Format("{0:00}", this.localDate.Day));
			str = str.Replace("[@HOUR]", string.Format("{0:00}", this.localDate.Hour));
			str = str.Replace("[@12HOUR]", string.Format("{0:00}", arg));
			str = str.Replace("[@AMPM]", string.Format("{0:00}", arg2));
			str = str.Replace("[@MINUTE]", string.Format("{0:00}", this.localDate.Minute));
			str = str.Replace("[@SECOND]", string.Format("{0:00}", this.localDate.Second));
			str = str.Replace("[@BATLVL]", this.SysPower.BatteryLifePercent.ToString(CultureInfo.InvariantCulture));
			if (this.localDate.Millisecond >= 0 && this.localDate.Millisecond < 800)
			{
				str = str.Replace("[@IND]", ":");
			}
			else
			{
				str = str.Replace("[@IND]", " ");
			}
			return str;
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00009830 File Offset: 0x00007A30
		private static int GCD(int m, int n)
		{
			if (m < n)
			{
				int num = n;
				n = m;
				m = num;
			}
			while (n != 0)
			{
				int num2 = m % n;
				m = n;
				n = num2;
			}
			return m;
		}

		// Token: 0x060000FC RID: 252 RVA: 0x0000984C File Offset: 0x00007A4C
		public int GetTextUpdateCount()
		{
			int num = 1;
			using (Graphics graphics = Graphics.FromImage(this.StaticBitmap))
			{
				foreach (AuraLayer.TextContent textContent in this.TextList)
				{
					SizeF sizeF = graphics.MeasureString(this.ConvertSpecialString(textContent.content), new Font(textContent.fontname, (float)textContent.fontsize));
					int num2 = sizeF.ToSize().Width + 4;
					int height = sizeF.ToSize().Height;
					int num3;
					if (textContent.bSlide)
					{
						if (textContent.speed > 0 && textContent.speed <= 5)
						{
							num3 = (textContent.width + num2) / (1 + textContent.speed);
						}
						else if (textContent.speed < 0 && textContent.speed >= -5)
						{
							num3 = (textContent.width + num2) * (1 - textContent.speed);
						}
						else
						{
							num3 = textContent.width + num2;
						}
					}
					else
					{
						num3 = 1;
					}
					num = num * num3 / AuraLayer.GCD(num, num3);
				}
			}
			return num;
		}

		// Token: 0x060000FD RID: 253 RVA: 0x00009988 File Offset: 0x00007B88
		public void UpdateTextFrame()
		{
			this.UpdateSpecialStringState();
			Bitmap image = (Bitmap)this.StaticBitmap.Clone();
			using (Graphics graphics = Graphics.FromImage(this.StaticBitmap))
			{
				graphics.Clear(Color.FromArgb(0, 0, 0, 0));
				using (Graphics graphics2 = Graphics.FromImage(image))
				{
					graphics2.Clear(Color.FromArgb(0, 0, 0, 0));
					foreach (AuraLayer.TextContent textContent in this.TextList)
					{
						SizeF sizeF = graphics2.MeasureString(this.ConvertSpecialString(textContent.content), new Font(textContent.fontname, (float)textContent.fontsize));
						int num = sizeF.ToSize().Width + 4;
						int height = sizeF.ToSize().Height;
						RectangleF layoutRectangle;
						if (textContent.bSlide)
						{
							int num2;
							if (textContent.speed > 0 && textContent.speed <= 5)
							{
								num2 = (1 + textContent.speed) * this.TextUpdateCount % (textContent.width + num);
							}
							else if (textContent.speed < 0 && textContent.speed >= -5)
							{
								num2 = this.TextUpdateCount / (1 - textContent.speed) % (textContent.width + num);
							}
							else
							{
								num2 = this.TextUpdateCount % (textContent.width + num);
							}
							layoutRectangle = new RectangleF((float)(textContent.offset_x + textContent.width - num2), (float)textContent.offset_y, (float)num, (float)textContent.height);
						}
						else if (num > textContent.width)
						{
							layoutRectangle = new RectangleF((float)(textContent.offset_x - (num - textContent.width) / 2), (float)textContent.offset_y, (float)num, (float)textContent.height);
						}
						else
						{
							layoutRectangle = new RectangleF((float)textContent.offset_x, (float)textContent.offset_y, (float)textContent.width, (float)textContent.height);
						}
						graphics2.SmoothingMode = SmoothingMode.HighSpeed;
						graphics2.InterpolationMode = InterpolationMode.Low;
						graphics2.PixelOffsetMode = PixelOffsetMode.Default;
						if (textContent.isAntiAlias)
						{
							graphics2.TextRenderingHint = TextRenderingHint.AntiAlias;
						}
						else
						{
							graphics2.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
						}
						StringFormat stringFormat = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.NoClip)
						{
							Alignment = StringAlignment.Center,
							LineAlignment = StringAlignment.Center,
							Trimming = StringTrimming.None
						};
						if (textContent.h_align == 1)
						{
							stringFormat.Alignment = StringAlignment.Near;
						}
						if (textContent.h_align == 2)
						{
							stringFormat.Alignment = StringAlignment.Far;
						}
						if (textContent.v_align == 1)
						{
							stringFormat.LineAlignment = StringAlignment.Near;
						}
						if (textContent.v_align == 2)
						{
							stringFormat.LineAlignment = StringAlignment.Far;
						}
						if (textContent.bBorder)
						{
							Pen pen = new Pen(Color.FromArgb(128, 180, 180, 180), 1f);
							Rectangle rect = new Rectangle(textContent.offset_x, textContent.offset_y, textContent.width, textContent.height);
							graphics2.DrawRectangle(pen, rect);
						}
						if (textContent.bRevert)
						{
							GraphicsState gstate = graphics2.Save();
							graphics2.ResetTransform();
							graphics2.RotateTransform(180f);
							graphics2.TranslateTransform((float)(this.frameWidth + textContent.offset_x - (this.frameWidth - (textContent.offset_x + textContent.width))), (float)(this.frameHeight + textContent.offset_y - (this.frameHeight - (textContent.offset_y + textContent.height))), MatrixOrder.Append);
							graphics2.DrawString(this.ConvertSpecialString(textContent.content), new Font(textContent.fontname, (float)textContent.fontsize), new SolidBrush(textContent.color), layoutRectangle, stringFormat);
							graphics2.Restore(gstate);
						}
						else
						{
							graphics2.DrawString(this.ConvertSpecialString(textContent.content), new Font(textContent.fontname, (float)textContent.fontsize), new SolidBrush(textContent.color), layoutRectangle, stringFormat);
						}
						graphics2.Flush();
						Rectangle rectangle = new Rectangle(textContent.offset_x, textContent.offset_y, textContent.width, textContent.height);
						graphics.DrawImage(image, rectangle, rectangle, GraphicsUnit.Pixel);
					}
				}
			}
			this.TextUpdateCount++;
		}

		// Token: 0x060000FE RID: 254 RVA: 0x00009E00 File Offset: 0x00008000
		public void AddSlideText(string text, string fontName, int fontSize, bool bReverted)
		{
			int num;
			using (Graphics graphics = Graphics.FromImage(new Bitmap(this.frameWidth, this.frameHeight)))
			{
				num = graphics.MeasureString(text, new Font(fontName, (float)fontSize)).ToSize().Width + 4;
				LOGGER.DEBUG("[Text] text=" + text + " expecWidth=" + num.ToString(), Array.Empty<object>());
			}
			for (int i = 0; i < this.frameWidth + num; i++)
			{
				Bitmap bitmap = new Bitmap(this.frameWidth, this.frameHeight);
				RectangleF layoutRectangle = new RectangleF((float)(this.frameWidth - i), 0f, (float)num, (float)this.frameHeight);
				using (Graphics graphics2 = Graphics.FromImage(bitmap))
				{
					graphics2.SmoothingMode = SmoothingMode.HighSpeed;
					graphics2.InterpolationMode = InterpolationMode.Low;
					graphics2.PixelOffsetMode = PixelOffsetMode.Default;
					graphics2.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
					StringFormat format = new StringFormat
					{
						Alignment = StringAlignment.Near,
						LineAlignment = StringAlignment.Near
					};
					if (bReverted)
					{
						GraphicsState gstate = graphics2.Save();
						graphics2.ResetTransform();
						graphics2.RotateTransform(180f);
						graphics2.TranslateTransform((float)(this.frameWidth / 2), (float)(this.frameHeight / 2), MatrixOrder.Append);
						graphics2.DrawString(text, new Font(fontName, (float)fontSize), Brushes.White, layoutRectangle, format);
						graphics2.Restore(gstate);
					}
					else
					{
						graphics2.DrawString(text, new Font(fontName, (float)fontSize), Brushes.White, layoutRectangle, format);
					}
					graphics2.Flush();
				}
				this.frameList.Add(bitmap);
			}
		}

		// Token: 0x060000FF RID: 255 RVA: 0x00009FB8 File Offset: 0x000081B8
		public void MaskFromStaticPicture(string path)
		{
			Image original = Image.FromFile(path);
			this.layerMaskBitmap = new Bitmap(original, this.frameWidth, this.frameHeight);
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000100 RID: 256 RVA: 0x00009FE4 File Offset: 0x000081E4
		// (set) Token: 0x06000101 RID: 257 RVA: 0x00009FEC File Offset: 0x000081EC
		public int AudioEffectStrength
		{
			get
			{
				return this._audioEffectStrength;
			}
			set
			{
				this._audioEffectStrength = value;
				if (value < -100)
				{
					this._audioEffectStrength = -100;
				}
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000102 RID: 258 RVA: 0x0000A002 File Offset: 0x00008202
		// (set) Token: 0x06000103 RID: 259 RVA: 0x0000A014 File Offset: 0x00008214
		public int DecayEffect
		{
			get
			{
				return (int)(this._DecayEffect * 100f);
			}
			set
			{
				this._DecayEffect = (float)value / 100f;
				if (value > 99)
				{
					this._DecayEffect = 0.9f;
				}
				if (value < 0)
				{
					this._DecayEffect = 0f;
				}
				if (this._DecayEffect == 0f)
				{
					this.Afterglow = false;
					return;
				}
				this.Afterglow = true;
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000104 RID: 260 RVA: 0x0000A06A File Offset: 0x0000826A
		// (set) Token: 0x06000105 RID: 261 RVA: 0x0000A072 File Offset: 0x00008272
		public int StrobingBeatThreadhold
		{
			get
			{
				return this._StrobingBeatThreadhold;
			}
			set
			{
				this._StrobingBeatThreadhold = value;
				if (value > 99)
				{
					this._StrobingBeatThreadhold = 99;
				}
				if (value < 0)
				{
					this._StrobingBeatThreadhold = 0;
				}
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000106 RID: 262 RVA: 0x0000A093 File Offset: 0x00008293
		// (set) Token: 0x06000107 RID: 263 RVA: 0x0000A09B File Offset: 0x0000829B
		public int StrobingBeatStartFreq
		{
			get
			{
				return this._StrobingBeatStartFreq;
			}
			set
			{
				this._StrobingBeatStartFreq = value;
				if (value > 9)
				{
					this._StrobingBeatStartFreq = 9;
				}
				if (value < 0)
				{
					this._StrobingBeatStartFreq = 0;
				}
				if (this._StrobingBeatStartFreq >= this._StrobingBeatEndFreq)
				{
					this._StrobingBeatStartFreq = this._StrobingBeatEndFreq - 1;
				}
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000108 RID: 264 RVA: 0x0000A0D8 File Offset: 0x000082D8
		// (set) Token: 0x06000109 RID: 265 RVA: 0x0000A0E0 File Offset: 0x000082E0
		public int StrobingBeatEndFreq
		{
			get
			{
				return this._StrobingBeatEndFreq;
			}
			set
			{
				this._StrobingBeatEndFreq = value;
				if (value > 10)
				{
					this._StrobingBeatEndFreq = 10;
				}
				if (value < 1)
				{
					this._StrobingBeatEndFreq = 1;
				}
				if (this._StrobingBeatEndFreq <= this._StrobingBeatStartFreq)
				{
					this._StrobingBeatStartFreq++;
				}
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x0600010A RID: 266 RVA: 0x0000A11D File Offset: 0x0000831D
		// (set) Token: 0x0600010B RID: 267 RVA: 0x0000A125 File Offset: 0x00008325
		public bool StrobingBeatUseMax { get; set; }

		// Token: 0x0600010C RID: 268 RVA: 0x0000A12E File Offset: 0x0000832E
		public void InitMusicLayer(bool repeat = true, int offset_x = 0, int offset_y = 0, bool isFullLayer = true, int width = 21, int height = 8)
		{
			this.init(repeat, offset_x, offset_y, isFullLayer, width, height);
			this.isMusicLayer = true;
			this.isStaticFrame = true;
			this.analyzer = new AudioAnalyzer();
			this.SpectrumType = 0;
			this.analyzer.Init();
		}

		// Token: 0x0600010D RID: 269 RVA: 0x0000A16A File Offset: 0x0000836A
		public void EnableMusicLayer(bool isEnable, int audioDevID)
		{
			if ((isEnable && this.IsAnalyzerEnabled) || (!isEnable && !this.IsAnalyzerEnabled))
			{
				return;
			}
			this.analyzer.Enable(isEnable, audioDevID);
			this.IsAnalyzerEnabled = isEnable;
		}

		// Token: 0x0600010E RID: 270 RVA: 0x0000A197 File Offset: 0x00008397
		public void EnableMusicLayer2(bool isEnable, string audioDevName)
		{
			if ((isEnable && this.IsAnalyzerEnabled) || (!isEnable && !this.IsAnalyzerEnabled))
			{
				return;
			}
			this.analyzer.Enable2(isEnable, audioDevName);
			this.IsAnalyzerEnabled = isEnable;
		}

		// Token: 0x0600010F RID: 271 RVA: 0x0000A1C4 File Offset: 0x000083C4
		public void SetAudioBassCompressRatio(int ratio)
		{
			this.analyzer.BassCompressRatio = ratio;
		}

		// Token: 0x06000110 RID: 272 RVA: 0x0000A1D2 File Offset: 0x000083D2
		public void SetAudioTrebleCompressRatio(int ratio)
		{
			this.analyzer.TrebleCompressRatio = ratio;
		}

		// Token: 0x06000111 RID: 273 RVA: 0x0000A1E0 File Offset: 0x000083E0
		public string GetAudioDeviceList()
		{
			return this.analyzer.GetAudioDeviceList();
		}

		// Token: 0x06000112 RID: 274 RVA: 0x0000A1ED File Offset: 0x000083ED
		public int GetAudioDeviceCount()
		{
			return this.analyzer.GetAudioDeviceCount();
		}

		// Token: 0x06000113 RID: 275 RVA: 0x0000A1FA File Offset: 0x000083FA
		public int GetMusicSPectrumTypeCount()
		{
			return 150;
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000114 RID: 276 RVA: 0x0000A201 File Offset: 0x00008401
		// (set) Token: 0x06000115 RID: 277 RVA: 0x0000A20C File Offset: 0x0000840C
		public int SpectrumType
		{
			get
			{
				return this._SpectrumType;
			}
			set
			{
				this._SpectrumType = value;
				if (value == 1)
				{
					this.analyzer.SetLines((this.frameHeight > 1) ? (this.frameHeight / 2) : this.frameHeight);
					return;
				}
				if (value == 2)
				{
					this.analyzer.SetLines((this.frameWidth - 5) / 4);
					return;
				}
				if (value == 3)
				{
					this.analyzer.SetLines((this.frameWidth - 5) / 4);
					return;
				}
				if (value == 4)
				{
					this.analyzer.SetLines(10);
					return;
				}
				if (value == 5)
				{
					this.analyzer.SetLines(this.frameWidth);
					return;
				}
				if (value == 6)
				{
					this.analyzer.SetLines((this.frameHeight > 2) ? (this.frameHeight / 3) : this.frameHeight);
					return;
				}
				if (value == 7)
				{
					this.analyzer.SetLines((this.frameHeight > 1) ? (this.frameHeight / 2) : this.frameHeight);
					return;
				}
				if (value == 10 || value == 11)
				{
					this.analyzer.SetLines(this.frameWidth);
					return;
				}
				if (value == 12 || value == 13)
				{
					this.analyzer.SetLines(this.frameHeight);
					return;
				}
				if (value == 14 || value == 15)
				{
					this.analyzer.SetLines((this.frameWidth > 1) ? (this.frameWidth / 2) : this.frameWidth);
					return;
				}
				if (value == 16 || value == 17)
				{
					this.analyzer.SetLines((this.frameHeight > 1) ? (this.frameHeight / 2) : this.frameHeight);
					return;
				}
				if (value == 110 || value == 111)
				{
					this.analyzer.SetLines(this.frameWidth);
					return;
				}
				if (value == 112 || value == 113)
				{
					this.analyzer.SetLines(this.frameHeight);
					return;
				}
				if (value == 114 || value == 115)
				{
					this.analyzer.SetLines((this.frameWidth > 1) ? (this.frameWidth / 2) : this.frameWidth);
					return;
				}
				if (value == 116 || value == 117)
				{
					this.analyzer.SetLines((this.frameHeight > 1) ? (this.frameHeight / 2) : this.frameHeight);
					return;
				}
				if (value == 20 || value == 21 || value == 22 || value == 23)
				{
					this.analyzer.SetLines(1);
					return;
				}
				if (value == 30)
				{
					this.analyzer.SetLines(this.frameWidth);
					return;
				}
				if (value == 31)
				{
					this.analyzer.SetLines(this.frameHeight);
					return;
				}
				if (value == 32 || value == 33)
				{
					this.analyzer.SetLines(1);
					return;
				}
				if (value == 34)
				{
					this.analyzer.SetLines(this.frameHeight);
					return;
				}
				if (value == 130)
				{
					this.analyzer.SetLines(this.frameWidth);
					return;
				}
				if (value == 131)
				{
					this.analyzer.SetLines(this.frameHeight);
					return;
				}
				if (value == 132 || value == 133)
				{
					this.analyzer.SetLines(1);
					return;
				}
				if (value == 40)
				{
					this.analyzer.SetLines(1);
					return;
				}
				if (value == 41)
				{
					this.analyzer.SetLines(10);
					return;
				}
				if (value == 50 || value == 51)
				{
					this.analyzer.SetLines(this.frameWidth);
					return;
				}
				if (value == 52 || value == 53)
				{
					this.analyzer.SetLines(this.frameHeight);
					return;
				}
				if (value == 54 || value == 55 || value == 56 || value == 57)
				{
					this.analyzer.SetLines(1);
					return;
				}
				if (value == 60)
				{
					this.analyzer.SetLines((this.frameWidth > 1) ? (this.frameWidth / 2) : this.frameWidth);
					return;
				}
				if (value == 61)
				{
					this.analyzer.SetLines(1);
					return;
				}
				if (value == 70)
				{
					this.analyzer.SetLines(this.frameWidth);
					return;
				}
				if (value == 71)
				{
					this.analyzer.SetLines(this.frameHeight);
					return;
				}
				if (value == 80 || value == 81)
				{
					this.analyzer.SetLines(this.frameWidth);
					return;
				}
				if (value == 82 || value == 83)
				{
					this.analyzer.SetLines(this.frameHeight);
					return;
				}
				if (value == 84 || value == 85 || value == 86 || value == 87)
				{
					this.analyzer.SetLines(1);
					return;
				}
				if (value == 90)
				{
					this.analyzer.SetLines(8);
					return;
				}
				if (value == 91)
				{
					this.analyzer.SetLines(16);
					return;
				}
				if (value == 200 || value == 201 || value == 202 || value == 203 || value == 204)
				{
					this.analyzer.SetLines(2);
					return;
				}
				if (value == 250 || value == 251 || value == 252 || value == 253 || value == 254)
				{
					this.analyzer.SetLines(2);
					return;
				}
				this.analyzer.SetLines((this.frameWidth > 1) ? (this.frameWidth / 2) : this.frameWidth);
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x06000116 RID: 278 RVA: 0x0000A6E5 File Offset: 0x000088E5
		// (set) Token: 0x06000117 RID: 279 RVA: 0x0000A6ED File Offset: 0x000088ED
		public bool ShowBleading
		{
			get
			{
				return this._ShowBeading;
			}
			set
			{
				this._ShowBeading = value;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x06000118 RID: 280 RVA: 0x0000A6F6 File Offset: 0x000088F6
		// (set) Token: 0x06000119 RID: 281 RVA: 0x0000A6FE File Offset: 0x000088FE
		public int BleadingSpeed
		{
			get
			{
				return this.MS_DecreaseCount;
			}
			set
			{
				if (value < 1)
				{
					this.MS_DecreaseCount = 1;
				}
				if (value > 50)
				{
					this.MS_DecreaseCount = 50;
					return;
				}
				this.MS_DecreaseCount = value;
			}
		}

		// Token: 0x0600011A RID: 282 RVA: 0x0000A720 File Offset: 0x00008920
		private void CaculateBeading()
		{
			int num = this.analyzer._spectrumdata.Count<byte>();
			if (this.MaxSpectrum.Length < num)
			{
				this.MaxSpectrum = new int[num];
			}
			for (int i = 0; i < num; i++)
			{
				this.MaxSpectrum[i] -= this.MS_DecreaseCount;
				if (this.MaxSpectrum[i] <= 0)
				{
					this.MaxSpectrum[i] = 0;
				}
				if ((int)this.analyzer._spectrumdata[i] > this.MaxSpectrum[i])
				{
					this.MaxSpectrum[i] = (int)(this.analyzer._spectrumdata[i] + 10) % 256;
				}
			}
		}

		// Token: 0x0600011B RID: 283 RVA: 0x0000A7C8 File Offset: 0x000089C8
		public void UpdateMusicFrame()
		{
			int num = 2048;
			RectangleF[] array = new RectangleF[num];
			if (this.analyzer == null)
			{
				return;
			}
			this.analyzer.UpdateAudioData(this._audioEffectStrength);
			if (this.analyzer._spectrumdata.Count<byte>() == 0)
			{
				return;
			}
			this.CaculateBeading();
			this.lastBitmap = (Bitmap)this.StaticBitmap.Clone();
			using (Graphics graphics = Graphics.FromImage(this.StaticBitmap))
			{
				graphics.Clear(Color.FromArgb(0, 0, 0, 0));
				int spectrumType = this.SpectrumType;
				LinearGradientBrush brush;
				if (spectrumType <= 117)
				{
					switch (spectrumType)
					{
					case 1:
						break;
					case 2:
						num = (this.frameWidth - 5) / 4;
						for (int i = 0; i < num; i++)
						{
							int num2 = (4 * i + 6) / 2 + 3 + ((this.frameHeight >= 28) ? (this.frameHeight - 28) : 0);
							array[2 * i] = new RectangleF((float)(6 + i * 4 - 1), (float)(num2 - num2 * (int)this.analyzer._spectrumdata[i] / 255), 2f, (float)(num2 * (int)this.analyzer._spectrumdata[i] / 255));
							array[2 * i + 1] = new RectangleF((float)(6 + i * 4 + 1), (float)((this.analyzer._spectrumdata[i] == 0) ? num2 : (num2 - num2 * (int)this.analyzer._spectrumdata[i] / 255 - 1)), 1f, (float)((num2 * (int)this.analyzer._spectrumdata[i] / 255 + 1 < 2) ? 0 : (num2 * (int)this.analyzer._spectrumdata[i] / 255 + 1)));
						}
						brush = new LinearGradientBrush(new System.Drawing.Point(this.frameWidth / 2, this.frameHeight), new System.Drawing.Point(this.frameWidth / 2, 0), Color.FromArgb(255, 50, 50, 50), Color.FromArgb(255, 255, 255, 255));
						graphics.FillRectangles(brush, array);
						goto IL_68C5;
					case 3:
						num = (this.frameWidth - 5) / 4;
						for (int j = 0; j < num; j++)
						{
							int num3 = (4 * j + 6) / 2 + 3;
							array[2 * j] = new RectangleF((float)(6 + j * 4 - 1), (float)(10 - num3 * (int)this.analyzer._spectrumdata[j] / 255 / 2), 3f, (float)(num3 * (int)this.analyzer._spectrumdata[j] / 255));
							array[2 * j + 1] = new RectangleF((float)(6 + j * 4), (float)(10 - num3 * (int)this.analyzer._spectrumdata[j] / 255 / 2), 1f, (float)((this.analyzer._spectrumdata[j] == 0) ? 0 : (num3 * (int)this.analyzer._spectrumdata[j] / 255 + 1)));
						}
						brush = new LinearGradientBrush(new System.Drawing.Point(this.frameWidth / 2, this.frameHeight), new System.Drawing.Point(this.frameWidth / 2, 0), Color.FromArgb(255, 50, 50, 50), Color.FromArgb(255, 255, 255, 255));
						graphics.FillRectangles(brush, array);
						goto IL_68C5;
					case 4:
					{
						byte b = Math.Max(Math.Max(this.analyzer._spectrumdata[0], this.analyzer._spectrumdata[1]), Math.Max(this.analyzer._spectrumdata[2], this.analyzer._spectrumdata[3]));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[9] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[9] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[9] / byte.MaxValue))), new RectangleF(0f, 0f, (float)this.frameWidth, (float)this.frameHeight));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[8] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[8] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[8] / byte.MaxValue))), new RectangleF(2f, 2f, (float)(this.frameWidth - 5), 24f));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[7] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[7] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[7] / byte.MaxValue))), new RectangleF(10f, 4f, (float)(this.frameWidth - 16), 20f));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[6] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[6] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[6] / byte.MaxValue))), new RectangleF(18f, 6f, (float)(this.frameWidth - 27), 16f));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[5] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[5] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[5] / byte.MaxValue))), new RectangleF(26f, 8f, (float)(this.frameWidth - 38), 12f));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[4] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[4] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[4] / byte.MaxValue))), new RectangleF(34f, 10f, (float)(this.frameWidth - 49), 8f));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * b / byte.MaxValue), (int)(byte.MaxValue * b / byte.MaxValue), (int)(byte.MaxValue * b / byte.MaxValue))), new RectangleF(42f, 12f, (float)(this.frameWidth - 60), (float)(this.frameHeight - 24)));
						RectangleF[] array2 = new RectangleF[this.frameWidth];
						for (int k = 0; k < this.frameWidth - 60; k++)
						{
							array2[k] = new RectangleF((float)(42 + k), (float)(12 + k / 2), 1f, 2f);
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[4] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[4] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[4] / byte.MaxValue))), array2);
						for (int l = 0; l < this.frameWidth - 52; l++)
						{
							array2[l] = new RectangleF((float)(34 + l), (float)(10 + l / 2), 1f, 2f);
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[5] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[5] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[5] / byte.MaxValue))), array2);
						for (int m = 0; m < this.frameWidth - 44; m++)
						{
							array2[m] = new RectangleF((float)(26 + m), (float)(8 + m / 2), 1f, 2f);
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[6] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[6] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[6] / byte.MaxValue))), array2);
						for (int n = 0; n < this.frameWidth - 36; n++)
						{
							array2[n] = new RectangleF((float)(18 + n), (float)(6 + n / 2), 1f, 2f);
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[7] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[7] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[7] / byte.MaxValue))), array2);
						for (int num4 = 0; num4 < this.frameWidth - 28; num4++)
						{
							array2[num4] = new RectangleF((float)(10 + num4), (float)(4 + num4 / 2), 1f, 2f);
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[8] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[8] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[8] / byte.MaxValue))), array2);
						for (int num5 = 0; num5 < this.frameWidth - 5; num5++)
						{
							array2[num5] = new RectangleF((float)(2 + num5), (float)(2 + num5 / 2), 1f, (float)this.frameHeight);
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[9] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[9] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[9] / byte.MaxValue))), array2);
						goto IL_68C5;
					}
					case 5:
						num = this.frameWidth;
						graphics.DrawImage(this.lastBitmap, new RectangleF(0f, 1f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height));
						for (int num6 = 0; num6 < num; num6++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num6], (int)this.analyzer._spectrumdata[num6], (int)this.analyzer._spectrumdata[num6])), new RectangleF((float)num6, 0f, 1f, 1f));
						}
						goto IL_68C5;
					case 6:
						if (this.frameHeight > 2)
						{
							num = this.frameHeight / 3;
						}
						else
						{
							num = this.frameHeight;
						}
						for (int num7 = 0; num7 < num; num7++)
						{
							int num8 = (this.frameWidth * (int)this.analyzer._spectrumdata[num7] / 255 % 2 == 1) ? (this.frameWidth * (int)this.analyzer._spectrumdata[num7] / 255) : (this.frameWidth * (int)this.analyzer._spectrumdata[num7] / 255 - 1);
							if (num8 < 1)
							{
								num8 = 1;
							}
							array[2 * num7] = new RectangleF((float)(this.frameWidth - num8), (float)(num7 * this.frameHeight / num), (float)num8, 1f);
							array[2 * num7 + 1] = new RectangleF((float)(this.frameWidth - num8 + 1), (float)(num7 * this.frameHeight / num + 1), (float)(num8 - 1), 1f);
						}
						brush = new LinearGradientBrush(new System.Drawing.Point(this.frameWidth, this.frameHeight / 2), new System.Drawing.Point(0, this.frameHeight / 2), Color.FromArgb(255, 50, 50, 50), Color.FromArgb(255, 255, 255, 255));
						graphics.FillRectangles(brush, array);
						goto IL_68C5;
					case 7:
						if (this.frameHeight > 1)
						{
							num = this.frameHeight / 2;
						}
						else
						{
							num = this.frameHeight;
						}
						for (int num9 = 0; num9 < num; num9++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[num - num9 - 1] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[num - num9 - 1] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[num - num9 - 1] / byte.MaxValue))), new RectangleF((float)num9, (float)num9, (float)(this.frameWidth - 2 * num9), (float)(this.frameHeight - 2 * num9)));
						}
						goto IL_68C5;
					case 8:
						if (this.frameHeight > 2)
						{
							num = this.frameHeight / 3;
						}
						else
						{
							num = this.frameHeight;
						}
						for (int num10 = 0; num10 < num; num10++)
						{
							int num11 = (this.frameWidth * (int)this.analyzer._spectrumdata[num10] / 255 % 2 == 1) ? (this.frameWidth * (int)this.analyzer._spectrumdata[num10] / 255) : (this.frameWidth * (int)this.analyzer._spectrumdata[num10] / 255 - 1);
							if (num11 < 1)
							{
								num11 = 1;
							}
							array[2 * num10] = new RectangleF(0f, (float)(num10 * this.frameHeight / num), (float)num11, 1f);
							array[2 * num10 + 1] = new RectangleF(0f, (float)(num10 * this.frameHeight / num + 1), (float)(num11 - 1), 1f);
						}
						brush = new LinearGradientBrush(new System.Drawing.Point(0, this.frameHeight / 2), new System.Drawing.Point(this.frameWidth, this.frameHeight / 2), Color.FromArgb(255, 50, 50, 50), Color.FromArgb(255, 255, 255, 255));
						graphics.FillRectangles(brush, array);
						goto IL_68C5;
					case 9:
					{
						byte b2 = Math.Max(Math.Max(this.analyzer._spectrumdata[0], this.analyzer._spectrumdata[1]), Math.Max(this.analyzer._spectrumdata[2], this.analyzer._spectrumdata[3]));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[9] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[9] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[9] / byte.MaxValue))), new RectangleF(0f, 0f, (float)this.frameWidth, (float)this.frameHeight));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[8] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[8] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[8] / byte.MaxValue))), new RectangleF(2f, 2f, (float)(this.frameWidth - 5), (float)(this.frameHeight - 4)));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[7] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[7] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[7] / byte.MaxValue))), new RectangleF(10f, 4f, (float)(this.frameWidth - 16), (float)(this.frameHeight - 8)));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[6] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[6] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[6] / byte.MaxValue))), new RectangleF(18f, 6f, (float)(this.frameWidth - 27), (float)(this.frameHeight - 12)));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[5] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[5] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[5] / byte.MaxValue))), new RectangleF(26f, 8f, (float)(this.frameWidth - 38), (float)(this.frameHeight - 16)));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[4] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[4] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[4] / byte.MaxValue))), new RectangleF(34f, 10f, (float)(this.frameWidth - 49), (float)(this.frameHeight - 20)));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * b2 / byte.MaxValue), (int)(byte.MaxValue * b2 / byte.MaxValue), (int)(byte.MaxValue * b2 / byte.MaxValue))), new RectangleF(42f, 12f, (float)(this.frameWidth - 60), (float)(this.frameHeight - 24)));
						RectangleF[] array3 = new RectangleF[this.frameWidth];
						for (int num12 = 0; num12 < this.frameWidth - 62; num12++)
						{
							array3[num12] = new RectangleF((float)(42 + num12), (float)(12 + num12 / 2), 1f, 2f);
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[4] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[4] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[4] / byte.MaxValue))), array3);
						for (int num13 = 0; num13 < this.frameWidth - 54; num13++)
						{
							array3[num13] = new RectangleF((float)(34 + num13), (float)(10 + num13 / 2), 1f, 2f);
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[5] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[5] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[5] / byte.MaxValue))), array3);
						for (int num14 = 0; num14 < this.frameWidth - 46; num14++)
						{
							array3[num14] = new RectangleF((float)(26 + num14), (float)(8 + num14 / 2), 1f, 2f);
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[6] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[6] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[6] / byte.MaxValue))), array3);
						for (int num15 = 0; num15 < this.frameWidth - 38; num15++)
						{
							array3[num15] = new RectangleF((float)(18 + num15), (float)(6 + num15 / 2), 1f, 2f);
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[7] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[7] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[7] / byte.MaxValue))), array3);
						for (int num16 = 0; num16 < this.frameWidth - 30; num16++)
						{
							array3[num16] = new RectangleF((float)(10 + num16), (float)(4 + num16 / 2), 1f, 2f);
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[8] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[8] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[8] / byte.MaxValue))), array3);
						for (int num17 = 0; num17 < this.frameWidth - 7; num17++)
						{
							array3[num17] = new RectangleF((float)(2 + num17), (float)(2 + num17 / 2), 1f, (float)this.frameHeight);
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[9] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[9] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[9] / byte.MaxValue))), array3);
						goto IL_68C5;
					}
					case 10:
						num = this.frameWidth;
						for (int num18 = 0; num18 < num; num18++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num18], (int)this.analyzer._spectrumdata[num18], (int)this.analyzer._spectrumdata[num18])), new RectangleF((float)(num18 * this.frameWidth / num), 0f, (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num18] / 255)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num18], this.MaxSpectrum[num18], this.MaxSpectrum[num18])), new RectangleF((float)(num18 * this.frameWidth / num), (float)(this.frameHeight * this.MaxSpectrum[num18] / 255), (float)(this.frameWidth / num), 1f));
							}
						}
						for (int num19 = 0; num19 < num; num19++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num19], (int)this.analyzer._spectrumdata[num19], (int)this.analyzer._spectrumdata[num19])), new RectangleF((float)num19, 0f, 1f, 1f));
						}
						goto IL_68C5;
					case 11:
						num = this.frameWidth;
						for (int num20 = 0; num20 < num; num20++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num20], (int)this.analyzer._spectrumdata[num20], (int)this.analyzer._spectrumdata[num20])), new RectangleF((float)(num20 * this.frameWidth / num), (float)(this.frameHeight - this.frameHeight * (int)this.analyzer._spectrumdata[num20] / 255), (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num20] / 255)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num20], this.MaxSpectrum[num20], this.MaxSpectrum[num20])), new RectangleF((float)(num20 * this.frameWidth / num), (float)(this.frameHeight - this.frameHeight * this.MaxSpectrum[num20] / 255), (float)(this.frameWidth / num), 1f));
							}
						}
						for (int num21 = 0; num21 < num; num21++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num21], (int)this.analyzer._spectrumdata[num21], (int)this.analyzer._spectrumdata[num21])), new RectangleF((float)num21, (float)(this.frameHeight - 1), 1f, 1f));
						}
						goto IL_68C5;
					case 12:
						num = this.frameHeight;
						for (int num22 = 0; num22 < num; num22++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num22], (int)this.analyzer._spectrumdata[num22], (int)this.analyzer._spectrumdata[num22])), new RectangleF(0f, (float)(num22 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num22] / 255), (float)(this.frameHeight / num)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num22], this.MaxSpectrum[num22], this.MaxSpectrum[num22])), new RectangleF((float)(this.frameWidth * this.MaxSpectrum[num22] / 255), (float)(num22 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
							}
						}
						for (int num23 = 0; num23 < num; num23++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num23], (int)this.analyzer._spectrumdata[num23], (int)this.analyzer._spectrumdata[num23])), new RectangleF(0f, (float)(num23 * this.frameHeight / num), 1f, 1f));
						}
						goto IL_68C5;
					case 13:
						num = this.frameHeight;
						for (int num24 = 0; num24 < num; num24++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num24], (int)this.analyzer._spectrumdata[num24], (int)this.analyzer._spectrumdata[num24])), new RectangleF((float)(this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num24] / 255), (float)(num24 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num24] / 255), (float)(this.frameHeight / num)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num24], this.MaxSpectrum[num24], this.MaxSpectrum[num24])), new RectangleF((float)(this.frameWidth - this.frameWidth * this.MaxSpectrum[num24] / 255), (float)(num24 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
							}
						}
						for (int num25 = 0; num25 < num; num25++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num25], (int)this.analyzer._spectrumdata[num25], (int)this.analyzer._spectrumdata[num25])), new RectangleF((float)(this.frameWidth - 1), (float)(num25 * this.frameHeight / num), 1f, 1f));
						}
						goto IL_68C5;
					case 14:
						if (this.frameWidth > 1)
						{
							num = this.frameWidth / 2;
						}
						else
						{
							num = this.frameWidth;
						}
						for (int num26 = 0; num26 < num; num26++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num26], (int)this.analyzer._spectrumdata[num26], (int)this.analyzer._spectrumdata[num26])), new RectangleF((float)(num26 * this.frameWidth / num), 0f, (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num26] / 255)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num26], this.MaxSpectrum[num26], this.MaxSpectrum[num26])), new RectangleF((float)(num26 * this.frameWidth / num), (float)(this.frameHeight * this.MaxSpectrum[num26] / 255), (float)(this.frameWidth / num), 1f));
							}
						}
						goto IL_68C5;
					case 15:
						if (this.frameWidth > 1)
						{
							num = this.frameWidth / 2;
						}
						else
						{
							num = this.frameWidth;
						}
						for (int num27 = 0; num27 < num; num27++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num27], (int)this.analyzer._spectrumdata[num27], (int)this.analyzer._spectrumdata[num27])), new RectangleF((float)(num27 * this.frameWidth / num), (float)(this.frameHeight - this.frameHeight * (int)this.analyzer._spectrumdata[num27] / 255), (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num27] / 255)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num27], this.MaxSpectrum[num27], this.MaxSpectrum[num27])), new RectangleF((float)(num27 * this.frameWidth / num), (float)(this.frameHeight - this.frameHeight * this.MaxSpectrum[num27] / 255), (float)(this.frameWidth / num), 1f));
							}
						}
						for (int num28 = 0; num28 < num; num28++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num28], (int)this.analyzer._spectrumdata[num28], (int)this.analyzer._spectrumdata[num28])), new RectangleF((float)num28, (float)(this.frameHeight - 1), 1f, 1f));
						}
						goto IL_68C5;
					case 16:
						num = this.frameHeight / 2;
						for (int num29 = 0; num29 < num; num29++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num29], (int)this.analyzer._spectrumdata[num29], (int)this.analyzer._spectrumdata[num29])), new RectangleF(0f, (float)(num29 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num29] / 255), (float)(this.frameHeight / num)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num29], this.MaxSpectrum[num29], this.MaxSpectrum[num29])), new RectangleF((float)(this.frameWidth * this.MaxSpectrum[num29] / 255), (float)(num29 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
							}
						}
						goto IL_68C5;
					case 17:
						num = this.frameHeight / 2;
						for (int num30 = 0; num30 < num; num30++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num30], (int)this.analyzer._spectrumdata[num30], (int)this.analyzer._spectrumdata[num30])), new RectangleF((float)(this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num30] / 255), (float)(num30 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num30] / 255), (float)(this.frameHeight / num)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num30], this.MaxSpectrum[num30], this.MaxSpectrum[num30])), new RectangleF((float)(this.frameWidth - this.frameWidth * this.MaxSpectrum[num30] / 255), (float)(num30 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
							}
						}
						goto IL_68C5;
					case 18:
					case 19:
					case 24:
					case 25:
					case 26:
					case 27:
					case 28:
					case 29:
					case 35:
					case 36:
					case 37:
					case 38:
					case 39:
					case 42:
					case 43:
					case 44:
					case 45:
					case 46:
					case 47:
					case 48:
					case 49:
					case 58:
					case 59:
					case 62:
					case 63:
					case 64:
					case 65:
					case 66:
					case 67:
					case 68:
					case 69:
					case 72:
					case 73:
					case 74:
					case 75:
					case 76:
					case 77:
					case 78:
					case 79:
					case 88:
					case 89:
						goto IL_67FB;
					case 20:
						num = 1;
						for (int num31 = 0; num31 < num; num31++)
						{
							array[num31] = new RectangleF((float)(num31 * this.frameWidth / num), 0f, (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num31] / 255));
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), array);
						if (this._ShowBeading)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[0], this.MaxSpectrum[0], this.MaxSpectrum[0])), new RectangleF(0f, (float)(this.frameHeight * this.MaxSpectrum[0] / 255), (float)(this.frameWidth / num), 1f));
						}
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), new RectangleF(0f, 0f, (float)this.frameWidth, 1f));
						goto IL_68C5;
					case 21:
						num = 1;
						for (int num32 = 0; num32 < num; num32++)
						{
							array[num32] = new RectangleF((float)(num32 * this.frameWidth / num), (float)(this.frameHeight - this.frameHeight * (int)this.analyzer._spectrumdata[num32] / 255), (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num32] / 255));
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), array);
						if (this._ShowBeading)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[0], this.MaxSpectrum[0], this.MaxSpectrum[0])), new RectangleF(0f, (float)(this.frameHeight - this.frameHeight * this.MaxSpectrum[0] / 255), (float)(this.frameWidth / num), 1f));
						}
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), new RectangleF(0f, (float)(this.frameHeight - 1), (float)this.frameWidth, 1f));
						goto IL_68C5;
					case 22:
						num = 1;
						for (int num33 = 0; num33 < num; num33++)
						{
							array[num33] = new RectangleF(0f, (float)(num33 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num33] / 255), (float)(this.frameHeight / num));
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), array);
						if (this._ShowBeading)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[0], this.MaxSpectrum[0], this.MaxSpectrum[0])), new RectangleF((float)(this.frameWidth * this.MaxSpectrum[0] / 255), 0f, 1f, (float)(this.frameHeight / num)));
						}
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), new RectangleF(0f, 0f, 1f, (float)this.frameHeight));
						goto IL_68C5;
					case 23:
						num = 1;
						for (int num34 = 0; num34 < num; num34++)
						{
							array[num34] = new RectangleF((float)(this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num34] / 255), (float)(num34 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num34] / 255), (float)(this.frameHeight / num));
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), array);
						if (this._ShowBeading)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[0], this.MaxSpectrum[0], this.MaxSpectrum[0])), new RectangleF((float)(this.frameWidth - this.frameWidth * this.MaxSpectrum[0] / 255), 0f, 1f, (float)(this.frameHeight / num)));
						}
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), new RectangleF(0f, (float)(this.frameWidth - 1), 1f, (float)this.frameHeight));
						goto IL_68C5;
					case 30:
						num = this.frameWidth;
						if (this.frameHeight < 3)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), new RectangleF(0f, 0f, (float)this.frameWidth, (float)this.frameHeight));
							goto IL_68C5;
						}
						for (int num35 = 0; num35 < num; num35++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num35], (int)this.analyzer._spectrumdata[num35], (int)this.analyzer._spectrumdata[num35])), new RectangleF((float)(num35 * this.frameWidth / num), (float)((this.frameHeight - this.frameHeight * (int)this.analyzer._spectrumdata[num35] / 255) / 2), (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num35] / 255)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num35], this.MaxSpectrum[num35], this.MaxSpectrum[num35])), new RectangleF((float)(num35 * this.frameWidth / num), (float)((this.frameHeight - this.frameHeight * this.MaxSpectrum[num35] / 255) / 2), (float)(this.frameWidth / num), 1f));
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num35], this.MaxSpectrum[num35], this.MaxSpectrum[num35])), new RectangleF((float)(num35 * this.frameWidth / num), (float)((this.frameHeight - this.frameHeight * this.MaxSpectrum[num35] / 255) / 2 + this.frameHeight * this.MaxSpectrum[num35] / 255), (float)(this.frameWidth / num), 1f));
							}
						}
						goto IL_68C5;
					case 31:
						num = this.frameHeight;
						if (this.frameWidth < 3)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), new RectangleF(0f, 0f, (float)this.frameWidth, (float)this.frameHeight));
							goto IL_68C5;
						}
						for (int num36 = 0; num36 < num; num36++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num36], (int)this.analyzer._spectrumdata[num36], (int)this.analyzer._spectrumdata[num36])), new RectangleF((float)((this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num36] / 255) / 2), (float)(num36 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num36] / 255), (float)(this.frameHeight / num)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num36], this.MaxSpectrum[num36], this.MaxSpectrum[num36])), new RectangleF((float)((this.frameWidth - this.frameWidth * this.MaxSpectrum[num36] / 255) / 2), (float)(num36 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num36], this.MaxSpectrum[num36], this.MaxSpectrum[num36])), new RectangleF((float)((this.frameWidth - this.frameWidth * this.MaxSpectrum[num36] / 255) / 2 + this.frameWidth * this.MaxSpectrum[num36] / 255), (float)(num36 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
							}
						}
						goto IL_68C5;
					case 32:
						num = 1;
						for (int num37 = 0; num37 < num; num37++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num37], (int)this.analyzer._spectrumdata[num37], (int)this.analyzer._spectrumdata[num37])), new RectangleF((float)(num37 * this.frameWidth / num), (float)((this.frameHeight - this.frameHeight * (int)this.analyzer._spectrumdata[num37] / 255) / 2), (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num37] / 255)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num37], this.MaxSpectrum[num37], this.MaxSpectrum[num37])), new RectangleF((float)(num37 * this.frameWidth / num), (float)((this.frameHeight - this.frameHeight * this.MaxSpectrum[num37] / 255) / 2), (float)(this.frameWidth / num), 1f));
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num37], this.MaxSpectrum[num37], this.MaxSpectrum[num37])), new RectangleF((float)(num37 * this.frameWidth / num), (float)((this.frameHeight - this.frameHeight * this.MaxSpectrum[num37] / 255) / 2 + this.frameHeight * this.MaxSpectrum[num37] / 255), (float)(this.frameWidth / num), 1f));
							}
						}
						goto IL_68C5;
					case 33:
						num = 1;
						for (int num38 = 0; num38 < num; num38++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num38], (int)this.analyzer._spectrumdata[num38], (int)this.analyzer._spectrumdata[num38])), new RectangleF((float)((this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num38] / 255) / 2), (float)(num38 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num38] / 255), (float)(this.frameHeight / num)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num38], this.MaxSpectrum[num38], this.MaxSpectrum[num38])), new RectangleF((float)((this.frameWidth - this.frameWidth * this.MaxSpectrum[num38] / 255) / 2), (float)(num38 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num38], this.MaxSpectrum[num38], this.MaxSpectrum[num38])), new RectangleF((float)((this.frameWidth - this.frameWidth * this.MaxSpectrum[num38] / 255) / 2 + this.frameWidth * this.MaxSpectrum[num38] / 255), (float)(num38 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
							}
						}
						goto IL_68C5;
					case 34:
						num = this.frameHeight;
						if (this.frameWidth < 3)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), new RectangleF(0f, 0f, (float)this.frameWidth, (float)this.frameHeight));
							goto IL_68C5;
						}
						for (int num39 = 0; num39 < num; num39++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num39], (int)this.analyzer._spectrumdata[num39], (int)this.analyzer._spectrumdata[num39])), new RectangleF(0f, (float)(num39 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num39] / 255 / 2), (float)(this.frameHeight / num)));
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num39], (int)this.analyzer._spectrumdata[num39], (int)this.analyzer._spectrumdata[num39])), new RectangleF((float)(this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num39] / 255 / 2), (float)(num39 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num39] / 255 / 2), (float)(this.frameHeight / num)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num39], this.MaxSpectrum[num39], this.MaxSpectrum[num39])), new RectangleF((float)(this.frameWidth * this.MaxSpectrum[num39] / 255 / 2), (float)(num39 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num39], this.MaxSpectrum[num39], this.MaxSpectrum[num39])), new RectangleF((float)(this.frameWidth - this.frameWidth * this.MaxSpectrum[num39] / 255 / 2), (float)(num39 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
							}
						}
						goto IL_68C5;
					case 40:
					{
						array[0] = new RectangleF(0f, 0f, (float)this.frameWidth, (float)this.frameHeight);
						int num40 = (int)this.analyzer._spectrumdata[0];
						if (num40 < this._StrobingBeatThreadhold * 255 / 100)
						{
							num40 = 0;
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, num40, num40, num40)), array);
						goto IL_68C5;
					}
					case 41:
					{
						array[0] = new RectangleF(0f, 0f, (float)this.frameWidth, (float)this.frameHeight);
						int num41 = 0;
						for (int num42 = this._StrobingBeatStartFreq; num42 < this._StrobingBeatEndFreq; num42++)
						{
							if (this.StrobingBeatUseMax)
							{
								num41 = Math.Max((int)this.analyzer._spectrumdata[num42], num41);
							}
							else
							{
								num41 += (int)this.analyzer._spectrumdata[num42];
							}
						}
						if (!this.StrobingBeatUseMax)
						{
							num41 /= this._StrobingBeatEndFreq - this._StrobingBeatStartFreq;
						}
						if (num41 < this._StrobingBeatThreadhold * 255 / 100)
						{
							num41 = 0;
						}
						graphics.FillRectangles(new SolidBrush(Color.FromArgb(255, num41, num41, num41)), array);
						goto IL_68C5;
					}
					case 50:
						num = this.frameWidth;
						graphics.DrawImage(this.lastBitmap, new RectangleF(0f, 1f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height));
						for (int num43 = 0; num43 < num; num43++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num43], (int)this.analyzer._spectrumdata[num43], (int)this.analyzer._spectrumdata[num43])), new RectangleF((float)num43, 0f, 1f, 1f));
						}
						goto IL_68C5;
					case 51:
						num = this.frameWidth;
						graphics.DrawImage(this.lastBitmap, new RectangleF(0f, -1f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height));
						for (int num44 = 0; num44 < num; num44++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num44], (int)this.analyzer._spectrumdata[num44], (int)this.analyzer._spectrumdata[num44])), new RectangleF((float)num44, (float)(this.frameHeight - 1), 1f, 1f));
						}
						goto IL_68C5;
					case 52:
						num = this.frameHeight;
						graphics.DrawImage(this.lastBitmap, new RectangleF(1f, 0f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height));
						for (int num45 = 0; num45 < num; num45++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num45], (int)this.analyzer._spectrumdata[num45], (int)this.analyzer._spectrumdata[num45])), new RectangleF(0f, (float)num45, 1f, 1f));
						}
						goto IL_68C5;
					case 53:
						num = this.frameHeight;
						graphics.DrawImage(this.lastBitmap, new RectangleF(-1f, 0f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height));
						for (int num46 = 0; num46 < num; num46++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num46], (int)this.analyzer._spectrumdata[num46], (int)this.analyzer._spectrumdata[num46])), new RectangleF((float)(this.frameWidth - 1), (float)num46, 1f, 1f));
						}
						goto IL_68C5;
					case 54:
						graphics.DrawImage(this.lastBitmap, new RectangleF(0f, 1f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), new RectangleF(0f, 0f, (float)this.frameWidth, 1f));
						goto IL_68C5;
					case 55:
						graphics.DrawImage(this.lastBitmap, new RectangleF(0f, -1f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), new RectangleF(0f, (float)(this.frameHeight - 1), (float)this.frameWidth, 1f));
						goto IL_68C5;
					case 56:
						graphics.DrawImage(this.lastBitmap, new RectangleF(1f, 0f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), new RectangleF(0f, 0f, 1f, (float)this.frameHeight));
						goto IL_68C5;
					case 57:
						graphics.DrawImage(this.lastBitmap, new RectangleF(-1f, 0f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height));
						graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), new RectangleF((float)(this.frameWidth - 1), 0f, 1f, (float)this.frameHeight));
						goto IL_68C5;
					case 60:
					{
						if (this.frameWidth > 1)
						{
							num = this.frameWidth / 2;
						}
						else
						{
							num = this.frameWidth;
						}
						Random random = new Random();
						for (int num47 = 0; num47 < num; num47++)
						{
							array[num47] = new RectangleF((float)(num47 * this.frameWidth / num + random.Next() % 2), (float)(random.Next() % this.frameHeight), 1f, 1f);
							if (this.analyzer._spectrumdata[num47] != 0)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), array[num47]);
							}
							else
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 0, 0)), array[num47]);
							}
						}
						goto IL_68C5;
					}
					case 61:
					{
						Random random2 = new Random();
						for (int num48 = 0; num48 < this.frameWidth / 2; num48++)
						{
							array[num48] = new RectangleF((float)(num48 * 2 + random2.Next() % 2), (float)(random2.Next() % this.frameHeight), 1f, 1f);
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0], (int)this.analyzer._spectrumdata[0])), array[num48]);
						}
						goto IL_68C5;
					}
					case 70:
						num = this.frameWidth;
						for (int num49 = 0; num49 < num; num49++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num49], (int)this.analyzer._spectrumdata[num49], (int)this.analyzer._spectrumdata[num49])), new RectangleF((float)(num49 * this.frameWidth / num), 0f, (float)(this.frameWidth / num), (float)this.frameHeight));
						}
						goto IL_68C5;
					case 71:
						num = this.frameHeight;
						for (int num50 = 0; num50 < num; num50++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num50], (int)this.analyzer._spectrumdata[num50], (int)this.analyzer._spectrumdata[num50])), new RectangleF(0f, (float)(num50 * this.frameHeight / num), (float)this.frameWidth, (float)(this.frameHeight / num)));
						}
						goto IL_68C5;
					case 80:
						num = this.frameWidth;
						for (int num51 = 0; num51 < num; num51++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), new RectangleF((float)(num51 * this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num51] / 255), (float)(this.frameWidth / num), 1f));
						}
						goto IL_68C5;
					case 81:
						num = this.frameWidth;
						for (int num52 = 0; num52 < num; num52++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), new RectangleF((float)(num52 * this.frameWidth / num), (float)(this.frameHeight - this.frameHeight * (int)this.analyzer._spectrumdata[num52] / 255 - 1), (float)(this.frameWidth / num), 1f));
						}
						goto IL_68C5;
					case 82:
						num = this.frameHeight;
						for (int num53 = 0; num53 < num; num53++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), new RectangleF((float)(this.frameWidth * (int)this.analyzer._spectrumdata[num53] / 255), (float)(num53 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
						}
						goto IL_68C5;
					case 83:
						num = this.frameHeight;
						for (int num54 = 0; num54 < num; num54++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), new RectangleF((float)(this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num54] / 255 - 1), (float)(num54 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
						}
						goto IL_68C5;
					case 84:
						num = 1;
						for (int num55 = 0; num55 < num; num55++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), new RectangleF((float)(num55 * this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num55] / 255), (float)this.frameWidth, 1f));
						}
						goto IL_68C5;
					case 85:
						num = 1;
						for (int num56 = 0; num56 < num; num56++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), new RectangleF((float)(num56 * this.frameWidth / num), (float)(this.frameHeight - this.frameHeight * (int)this.analyzer._spectrumdata[num56] / 255), (float)this.frameWidth, 1f));
						}
						goto IL_68C5;
					case 86:
						num = 1;
						for (int num57 = 0; num57 < num; num57++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), new RectangleF((float)(this.frameWidth * (int)this.analyzer._spectrumdata[num57] / 255), (float)(num57 * this.frameHeight / num), 1f, (float)this.frameHeight));
						}
						goto IL_68C5;
					case 87:
						num = 1;
						for (int num58 = 0; num58 < num; num58++)
						{
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), new RectangleF((float)(this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num58] / 255), (float)(num58 * this.frameHeight / num), 1f, (float)this.frameHeight));
						}
						goto IL_68C5;
					case 90:
					{
						int num59 = 74;
						int num60 = 60;
						int num61 = 68;
						int num62 = 34;
						Bitmap bitmap = new Bitmap(num59, num60);
						SolidBrush[] array4 = new SolidBrush[8];
						for (int num63 = 0; num63 < 8; num63++)
						{
							array4[num63] = new SolidBrush(Color.FromArgb(255, (int)(byte.MaxValue * this.analyzer._spectrumdata[num63] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[num63] / byte.MaxValue), (int)(byte.MaxValue * this.analyzer._spectrumdata[num63] / byte.MaxValue)));
						}
						using (Graphics graphics2 = Graphics.FromImage(bitmap))
						{
							graphics2.FillPolygon(array4[0], new System.Drawing.Point[]
							{
								new System.Drawing.Point(0, 33),
								new System.Drawing.Point(67, 33),
								new System.Drawing.Point(51, 48),
								new System.Drawing.Point(14, 48)
							});
							graphics2.FillPolygon(array4[1], new System.Drawing.Point[]
							{
								new System.Drawing.Point(2, 34),
								new System.Drawing.Point(65, 34),
								new System.Drawing.Point(51, 47),
								new System.Drawing.Point(14, 47)
							});
							graphics2.FillPolygon(array4[2], new System.Drawing.Point[]
							{
								new System.Drawing.Point(4, 35),
								new System.Drawing.Point(63, 35),
								new System.Drawing.Point(51, 46),
								new System.Drawing.Point(14, 46)
							});
							graphics2.FillPolygon(array4[3], new System.Drawing.Point[]
							{
								new System.Drawing.Point(6, 36),
								new System.Drawing.Point(60, 36),
								new System.Drawing.Point(51, 45),
								new System.Drawing.Point(14, 45)
							});
							graphics2.FillPolygon(array4[4], new System.Drawing.Point[]
							{
								new System.Drawing.Point(8, 37),
								new System.Drawing.Point(58, 37),
								new System.Drawing.Point(51, 44),
								new System.Drawing.Point(14, 44)
							});
							graphics2.FillPolygon(array4[5], new System.Drawing.Point[]
							{
								new System.Drawing.Point(10, 38),
								new System.Drawing.Point(56, 38),
								new System.Drawing.Point(50, 43),
								new System.Drawing.Point(14, 43)
							});
							graphics2.FillPolygon(array4[6], new System.Drawing.Point[]
							{
								new System.Drawing.Point(12, 39),
								new System.Drawing.Point(54, 39),
								new System.Drawing.Point(50, 42),
								new System.Drawing.Point(14, 42)
							});
							graphics2.FillPolygon(array4[7], new System.Drawing.Point[]
							{
								new System.Drawing.Point(14, 40),
								new System.Drawing.Point(51, 40),
								new System.Drawing.Point(49, 41),
								new System.Drawing.Point(14, 41)
							});
							graphics2.Flush();
						}
						for (int num64 = 0; num64 < num61; num64++)
						{
							for (int num65 = 0; num65 < num62; num65++)
							{
								int num66 = num65 + (num64 + 1) / 2;
								int num67 = num65 - num64 / 2 + (num61 + num61 % 2) / 2 - 1;
								if (num66 >= 0 && num66 < num59 && num67 >= 0 && num67 < num60)
								{
									this.StaticBitmap.SetPixel(num64, num65, bitmap.GetPixel(num66, num67));
								}
							}
						}
						goto IL_68C5;
					}
					case 91:
						for (int num68 = 0; num68 < 11; num68++)
						{
							int num69 = 15;
							array[2 * num68] = new RectangleF((float)(num68 * 4), (float)(num68 * 2 + num69 - num69 * (int)this.analyzer._spectrumdata[num68] / 255), 2f, (float)(num69 * (int)this.analyzer._spectrumdata[num68] / 255 + 1));
							array[2 * num68 + 1] = new RectangleF((float)(num68 * 4 + 2), (float)(num68 * 2 + num69 - num69 * (int)this.analyzer._spectrumdata[num68] / 255 + 1), 1f, (float)(num69 * (int)this.analyzer._spectrumdata[num68] / 255));
						}
						for (int num70 = 11; num70 < 16; num70++)
						{
							int num71 = 27 - num70;
							array[2 * num70] = new RectangleF((float)(num70 * 4), (float)(num70 * 2 + num71 - num71 * (int)this.analyzer._spectrumdata[num70] / 255), 2f, (float)(num71 * (int)this.analyzer._spectrumdata[num70] / 255 + 1));
							array[2 * num70 + 1] = new RectangleF((float)(num70 * 4 + 2), (float)(num70 * 2 + num71 - num71 * (int)this.analyzer._spectrumdata[num70] / 255 + 1), 1f, (float)(num71 * (int)this.analyzer._spectrumdata[num70] / 255));
						}
						brush = new LinearGradientBrush(new System.Drawing.Point(this.frameWidth, this.frameHeight), new System.Drawing.Point(0, 0), Color.FromArgb(255, 50, 50, 50), Color.FromArgb(255, 255, 255, 255));
						graphics.FillRectangles(brush, array);
						goto IL_68C5;
					default:
						switch (spectrumType)
						{
						case 110:
							num = this.frameWidth;
							for (int num72 = 0; num72 < num; num72++)
							{
								int num73 = num - num72 - 1;
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num72], (int)this.analyzer._spectrumdata[num72], (int)this.analyzer._spectrumdata[num72])), new RectangleF((float)(num73 * this.frameWidth / num), 0f, (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num72] / 255)));
								if (this._ShowBeading)
								{
									graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num72], this.MaxSpectrum[num72], this.MaxSpectrum[num72])), new RectangleF((float)(num73 * this.frameWidth / num), (float)(this.frameHeight * this.MaxSpectrum[num72] / 255), (float)(this.frameWidth / num), 1f));
								}
							}
							for (int num74 = 0; num74 < num; num74++)
							{
								int num75 = num - num74 - 1;
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num74], (int)this.analyzer._spectrumdata[num74], (int)this.analyzer._spectrumdata[num74])), new RectangleF((float)num75, 0f, 1f, 1f));
							}
							goto IL_68C5;
						case 111:
							num = this.frameWidth;
							for (int num76 = 0; num76 < num; num76++)
							{
								int num77 = num - num76 - 1;
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num76], (int)this.analyzer._spectrumdata[num76], (int)this.analyzer._spectrumdata[num76])), new RectangleF((float)(num77 * this.frameWidth / num), (float)(this.frameHeight - this.frameHeight * (int)this.analyzer._spectrumdata[num76] / 255), (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num76] / 255)));
								if (this._ShowBeading)
								{
									graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num76], this.MaxSpectrum[num76], this.MaxSpectrum[num76])), new RectangleF((float)(num77 * this.frameWidth / num), (float)(this.frameHeight - this.frameHeight * this.MaxSpectrum[num76] / 255), (float)(this.frameWidth / num), 1f));
								}
							}
							for (int num78 = 0; num78 < num; num78++)
							{
								int num79 = num - num78 - 1;
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num78], (int)this.analyzer._spectrumdata[num78], (int)this.analyzer._spectrumdata[num78])), new RectangleF((float)num79, (float)(this.frameHeight - 1), 1f, 1f));
							}
							goto IL_68C5;
						case 112:
							num = this.frameHeight;
							for (int num80 = 0; num80 < num; num80++)
							{
								int num81 = num - num80 - 1;
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num80], (int)this.analyzer._spectrumdata[num80], (int)this.analyzer._spectrumdata[num80])), new RectangleF(0f, (float)(num81 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num80] / 255), (float)(this.frameHeight / num)));
								if (this._ShowBeading)
								{
									graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num80], this.MaxSpectrum[num80], this.MaxSpectrum[num80])), new RectangleF((float)(this.frameWidth * this.MaxSpectrum[num80] / 255), (float)(num81 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
								}
							}
							for (int num82 = 0; num82 < num; num82++)
							{
								int num83 = num - num82 - 1;
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num82], (int)this.analyzer._spectrumdata[num82], (int)this.analyzer._spectrumdata[num82])), new RectangleF(0f, (float)(num83 * this.frameHeight / num), 1f, 1f));
							}
							goto IL_68C5;
						case 113:
							num = this.frameHeight;
							for (int num84 = 0; num84 < num; num84++)
							{
								int num85 = num - num84 - 1;
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num84], (int)this.analyzer._spectrumdata[num84], (int)this.analyzer._spectrumdata[num84])), new RectangleF((float)(this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num84] / 255), (float)(num85 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num84] / 255), (float)(this.frameHeight / num)));
								if (this._ShowBeading)
								{
									graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num84], this.MaxSpectrum[num84], this.MaxSpectrum[num84])), new RectangleF((float)(this.frameWidth - this.frameWidth * this.MaxSpectrum[num84] / 255), (float)(num85 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
								}
							}
							for (int num86 = 0; num86 < num; num86++)
							{
								int num87 = num - num86 - 1;
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num86], (int)this.analyzer._spectrumdata[num86], (int)this.analyzer._spectrumdata[num86])), new RectangleF((float)(this.frameWidth - 1), (float)(num87 * this.frameHeight / num), 1f, 1f));
							}
							goto IL_68C5;
						case 114:
							if (this.frameWidth > 1)
							{
								num = this.frameWidth / 2;
							}
							else
							{
								num = this.frameWidth;
							}
							for (int num88 = 0; num88 < num; num88++)
							{
								int num89 = num - num88 - 1;
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num88], (int)this.analyzer._spectrumdata[num88], (int)this.analyzer._spectrumdata[num88])), new RectangleF((float)(num89 * this.frameWidth / num), 0f, (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num88] / 255)));
								if (this._ShowBeading)
								{
									graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num88], this.MaxSpectrum[num88], this.MaxSpectrum[num88])), new RectangleF((float)(num89 * this.frameWidth / num), (float)(this.frameHeight * this.MaxSpectrum[num88] / 255), (float)(this.frameWidth / num), 1f));
								}
							}
							goto IL_68C5;
						case 115:
							if (this.frameWidth > 1)
							{
								num = this.frameWidth / 2;
							}
							else
							{
								num = this.frameWidth;
							}
							for (int num90 = 0; num90 < num; num90++)
							{
								int num91 = num - num90 - 1;
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num90], (int)this.analyzer._spectrumdata[num90], (int)this.analyzer._spectrumdata[num90])), new RectangleF((float)(num91 * this.frameWidth / num), (float)(this.frameHeight - this.frameHeight * (int)this.analyzer._spectrumdata[num90] / 255), (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num90] / 255)));
								if (this._ShowBeading)
								{
									graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num90], this.MaxSpectrum[num90], this.MaxSpectrum[num90])), new RectangleF((float)(num91 * this.frameWidth / num), (float)(this.frameHeight - this.frameHeight * this.MaxSpectrum[num90] / 255), (float)(this.frameWidth / num), 1f));
								}
							}
							goto IL_68C5;
						case 116:
							num = this.frameHeight / 2;
							for (int num92 = 0; num92 < num; num92++)
							{
								int num93 = num - num92 - 1;
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num92], (int)this.analyzer._spectrumdata[num92], (int)this.analyzer._spectrumdata[num92])), new RectangleF(0f, (float)(num93 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num92] / 255), (float)(this.frameHeight / num)));
								if (this._ShowBeading)
								{
									graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num92], this.MaxSpectrum[num92], this.MaxSpectrum[num92])), new RectangleF((float)(this.frameWidth * this.MaxSpectrum[num92] / 255), (float)(num93 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
								}
							}
							goto IL_68C5;
						case 117:
							if (this.frameHeight > 1)
							{
								num = this.frameHeight / 2;
							}
							else
							{
								num = this.frameHeight;
							}
							for (int num94 = 0; num94 < num; num94++)
							{
								int num95 = num - num94 - 1;
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num94], (int)this.analyzer._spectrumdata[num94], (int)this.analyzer._spectrumdata[num94])), new RectangleF((float)(this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num94] / 255), (float)(num95 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num94] / 255), (float)(this.frameHeight / num)));
								if (this._ShowBeading)
								{
									graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num94], this.MaxSpectrum[num94], this.MaxSpectrum[num94])), new RectangleF((float)(this.frameWidth - this.frameWidth * this.MaxSpectrum[num94] / 255), (float)(num95 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
								}
							}
							goto IL_68C5;
						default:
							goto IL_67FB;
						}
						break;
					}
				}
				else
				{
					switch (spectrumType)
					{
					case 130:
						num = this.frameWidth;
						for (int num96 = 0; num96 < num; num96++)
						{
							int num97 = num - num96 - 1;
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num96], (int)this.analyzer._spectrumdata[num96], (int)this.analyzer._spectrumdata[num96])), new RectangleF((float)(num97 * this.frameWidth / num), (float)((this.frameHeight - this.frameHeight * (int)this.analyzer._spectrumdata[num96] / 255) / 2), (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num96] / 256)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num96], this.MaxSpectrum[num96], this.MaxSpectrum[num96])), new RectangleF((float)(num97 * this.frameWidth / num), (float)((this.frameHeight - this.frameHeight * this.MaxSpectrum[num96] / 255) / 2), (float)(this.frameWidth / num), 1f));
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num96], this.MaxSpectrum[num96], this.MaxSpectrum[num96])), new RectangleF((float)(num97 * this.frameWidth / num), (float)((this.frameHeight - this.frameHeight * this.MaxSpectrum[num96] / 255) / 2 + this.frameHeight * this.MaxSpectrum[num96] / 255), (float)(this.frameWidth / num), 1f));
							}
						}
						goto IL_68C5;
					case 131:
						num = this.frameHeight;
						for (int num98 = 0; num98 < num; num98++)
						{
							int num99 = num - num98 - 1;
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num98], (int)this.analyzer._spectrumdata[num98], (int)this.analyzer._spectrumdata[num98])), new RectangleF((float)((this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num98] / 255) / 2), (float)(num99 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num98] / 255), (float)(this.frameHeight / num)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num98], this.MaxSpectrum[num98], this.MaxSpectrum[num98])), new RectangleF((float)((this.frameWidth - this.frameWidth * this.MaxSpectrum[num98] / 255) / 2), (float)(num99 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num98], this.MaxSpectrum[num98], this.MaxSpectrum[num98])), new RectangleF((float)((this.frameWidth - this.frameWidth * this.MaxSpectrum[num98] / 255) / 2 + this.frameWidth * this.MaxSpectrum[num98] / 255), (float)(num99 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
							}
						}
						goto IL_68C5;
					case 132:
						num = 1;
						for (int num100 = 0; num100 < num; num100++)
						{
							int num101 = num - num100 - 1;
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num100], (int)this.analyzer._spectrumdata[num100], (int)this.analyzer._spectrumdata[num100])), new RectangleF((float)(num101 * this.frameWidth / num), (float)((this.frameHeight - this.frameHeight * (int)this.analyzer._spectrumdata[num100] / 255) / 2), (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num100] / 255)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num100], this.MaxSpectrum[num100], this.MaxSpectrum[num100])), new RectangleF((float)(num101 * this.frameWidth / num), (float)((this.frameHeight - this.frameHeight * this.MaxSpectrum[num100] / 255) / 2), (float)(this.frameWidth / num), 1f));
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num100], this.MaxSpectrum[num100], this.MaxSpectrum[num100])), new RectangleF((float)(num101 * this.frameWidth / num), (float)((this.frameHeight - this.frameHeight * this.MaxSpectrum[num100] / 255) / 2 + this.frameHeight * this.MaxSpectrum[num100] / 255), (float)(this.frameWidth / num), 1f));
							}
						}
						goto IL_68C5;
					case 133:
						num = 1;
						for (int num102 = 0; num102 < num; num102++)
						{
							int num103 = num - num102 - 1;
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.analyzer._spectrumdata[num102], (int)this.analyzer._spectrumdata[num102], (int)this.analyzer._spectrumdata[num102])), new RectangleF((float)((this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num102] / 255) / 2), (float)(num103 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num102] / 255), (float)(this.frameHeight / num)));
							if (this._ShowBeading)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num102], this.MaxSpectrum[num102], this.MaxSpectrum[num102])), new RectangleF((float)((this.frameWidth - this.frameWidth * this.MaxSpectrum[num102] / 255) / 2), (float)(num103 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, this.MaxSpectrum[num102], this.MaxSpectrum[num102], this.MaxSpectrum[num102])), new RectangleF((float)((this.frameWidth - this.frameWidth * this.MaxSpectrum[num102] / 255) / 2 + this.frameWidth * this.MaxSpectrum[num102] / 255), (float)(num103 * this.frameHeight / num), 1f, (float)(this.frameHeight / num)));
							}
						}
						goto IL_68C5;
					default:
					{
						switch (spectrumType)
						{
						case 200:
							break;
						case 201:
						case 202:
							goto IL_496E;
						case 203:
							goto IL_4ED4;
						default:
							switch (spectrumType)
							{
							case 250:
								break;
							case 251:
							case 252:
								goto IL_496E;
							case 253:
								goto IL_4ED4;
							default:
								goto IL_67FB;
							}
							break;
						}
						double stabilizedPosition = this.GetStabilizedPosition(this.analyzer._spectrumdata);
						if (this.IsAllyCase(this.SpectrumType, this.frameWidth, this.frameHeight))
						{
							if (this.AllyBitmap == null)
							{
								if (this.frameWidth == 4)
								{
									this.AllyBitmap = new Bitmap(2, 2);
								}
								else
								{
									this.AllyBitmap = new Bitmap(4, 2);
								}
							}
							this.AllyBitmapClone = (this.AllyBitmap.Clone() as Bitmap);
							using (Graphics graphics3 = Graphics.FromImage(this.AllyBitmap))
							{
								graphics3.Clear(Color.Black);
								graphics3.DrawImage(this.AllyBitmapClone, new RectangleF(0f, 1f, (float)this.AllyBitmap.Width, (float)this.AllyBitmap.Height));
								if (this.analyzer._spectrumdata[0] != 0 || this.analyzer._spectrumdata[1] != 0)
								{
									byte b3 = (byte)(255.0 * Math.Pow((double)(this.analyzer._spectrumdata[0] + this.analyzer._spectrumdata[1] / 2) / 255.0, 0.5));
									using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(255, (int)b3, (int)b3, (int)b3)))
									{
										graphics3.FillRectangle(solidBrush, new RectangleF((float)((int)((double)this.AllyBitmap.Width * stabilizedPosition)), 0f, 1f, 1f));
									}
								}
							}
							if (this.frameWidth == 4)
							{
								this.StaticBitmap.SetPixel(0, 0, this.AllyBitmap.GetPixel(0, 1));
								this.StaticBitmap.SetPixel(1, 0, this.AllyBitmap.GetPixel(0, 0));
								this.StaticBitmap.SetPixel(2, 0, this.AllyBitmap.GetPixel(1, 0));
								this.StaticBitmap.SetPixel(3, 0, this.AllyBitmap.GetPixel(1, 1));
							}
							if (this.frameWidth == 8)
							{
								this.StaticBitmap.SetPixel(0, 0, this.AllyBitmap.GetPixel(0, 1));
								this.StaticBitmap.SetPixel(1, 0, this.AllyBitmap.GetPixel(0, 0));
								this.StaticBitmap.SetPixel(2, 0, this.AllyBitmap.GetPixel(1, 0));
								this.StaticBitmap.SetPixel(3, 0, this.AllyBitmap.GetPixel(1, 1));
								this.StaticBitmap.SetPixel(4, 0, this.AllyBitmap.GetPixel(2, 1));
								this.StaticBitmap.SetPixel(5, 0, this.AllyBitmap.GetPixel(2, 0));
								this.StaticBitmap.SetPixel(6, 0, this.AllyBitmap.GetPixel(3, 0));
								this.StaticBitmap.SetPixel(7, 0, this.AllyBitmap.GetPixel(3, 1));
								goto IL_68C5;
							}
							goto IL_68C5;
						}
						else
						{
							graphics.DrawImage(this.lastBitmap, new RectangleF(0f, 1f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height));
							if (this.analyzer._spectrumdata[0] == 0 && this.analyzer._spectrumdata[1] == 0)
							{
								goto IL_68C5;
							}
							byte b4 = (byte)(255.0 * Math.Pow((double)(this.analyzer._spectrumdata[0] + this.analyzer._spectrumdata[1] / 2) / 255.0, 0.5));
							using (SolidBrush solidBrush2 = new SolidBrush(Color.FromArgb(255, (int)b4, (int)b4, (int)b4)))
							{
								graphics.FillRectangle(solidBrush2, new RectangleF((float)((int)((double)this.frameWidth * stabilizedPosition)), 0f, 1f, 1f));
								goto IL_68C5;
							}
						}
						IL_496E:
						double stabilizedPosition2 = this.GetStabilizedPosition(this.analyzer._spectrumdata);
						if (this.IsAllyCase(this.SpectrumType, this.frameWidth, this.frameHeight))
						{
							if (this.AllyBitmap == null)
							{
								if (this.frameWidth == 4)
								{
									this.AllyBitmap = new Bitmap(2, 2);
								}
								else
								{
									this.AllyBitmap = new Bitmap(4, 2);
								}
							}
							this.AllyBitmapClone = (this.AllyBitmap.Clone() as Bitmap);
							using (Graphics graphics4 = Graphics.FromImage(this.AllyBitmap))
							{
								graphics4.Clear(Color.Black);
								this.DrawImageWithOpacity(graphics, this.AllyBitmapClone, new RectangleF(0f, 0f, (float)this.AllyBitmap.Width, (float)this.AllyBitmap.Height), 0.6);
								if (this.analyzer._spectrumdata[0] != 0 || this.analyzer._spectrumdata[1] != 0)
								{
									if (this.analyzer._spectrumdata[0] > 220 || this.analyzer._spectrumdata[1] > 220)
									{
										using (SolidBrush solidBrush3 = new SolidBrush(Color.FromArgb(255, 255, 255, 255)))
										{
											graphics4.FillRectangle(solidBrush3, 0, 0, this.AllyBitmap.Width, this.AllyBitmap.Height);
											goto IL_4B85;
										}
									}
									Math.Pow((double)((this.analyzer._spectrumdata[0] + this.analyzer._spectrumdata[1]) / 2) / 255.0, 0.75);
									using (SolidBrush solidBrush4 = new SolidBrush(Color.FromArgb(255, 255, 255, 255)))
									{
										graphics4.FillRectangle(solidBrush4, (int)((double)this.AllyBitmap.Width * stabilizedPosition2), 0, 1, this.AllyBitmap.Height);
									}
								}
							}
							IL_4B85:
							if (this.frameWidth == 4)
							{
								this.StaticBitmap.SetPixel(0, 0, this.AllyBitmap.GetPixel(0, 1));
								this.StaticBitmap.SetPixel(1, 0, this.AllyBitmap.GetPixel(0, 0));
								this.StaticBitmap.SetPixel(2, 0, this.AllyBitmap.GetPixel(1, 0));
								this.StaticBitmap.SetPixel(3, 0, this.AllyBitmap.GetPixel(1, 1));
							}
							if (this.frameWidth == 8)
							{
								this.StaticBitmap.SetPixel(0, 0, this.AllyBitmap.GetPixel(0, 1));
								this.StaticBitmap.SetPixel(1, 0, this.AllyBitmap.GetPixel(0, 0));
								this.StaticBitmap.SetPixel(2, 0, this.AllyBitmap.GetPixel(1, 0));
								this.StaticBitmap.SetPixel(3, 0, this.AllyBitmap.GetPixel(1, 1));
								this.StaticBitmap.SetPixel(4, 0, this.AllyBitmap.GetPixel(2, 1));
								this.StaticBitmap.SetPixel(5, 0, this.AllyBitmap.GetPixel(2, 0));
								this.StaticBitmap.SetPixel(6, 0, this.AllyBitmap.GetPixel(3, 0));
								this.StaticBitmap.SetPixel(7, 0, this.AllyBitmap.GetPixel(3, 1));
								goto IL_68C5;
							}
							goto IL_68C5;
						}
						else
						{
							this.DrawImageWithOpacity(graphics, this.lastBitmap, new RectangleF(0f, 0f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height), 0.6);
							if (this.analyzer._spectrumdata[0] == 0 && this.analyzer._spectrumdata[1] == 0)
							{
								goto IL_68C5;
							}
							if (this.analyzer._spectrumdata[0] > 220 || this.analyzer._spectrumdata[1] > 220)
							{
								using (SolidBrush solidBrush5 = new SolidBrush(Color.FromArgb(255, 255, 255, 255)))
								{
									graphics.FillRectangle(solidBrush5, 0, 0, this.frameWidth, this.frameHeight);
									goto IL_68C5;
								}
							}
							double intensity = Math.Pow((double)((this.analyzer._spectrumdata[0] + this.analyzer._spectrumdata[1]) / 2) / 255.0, 0.75);
							if (this.frameHeight > 2 && this.frameWidth > 9)
							{
								if (this.SpectrumType == 202 || this.SpectrumType == 252)
								{
									this.DrawFadingCircle(graphics, new Rectangle((int)((double)this.frameWidth * stabilizedPosition2), 0, this.frameWidth / 2, this.frameHeight), Color.White, intensity);
									goto IL_68C5;
								}
								this.DrawFadingCircle(graphics, new Rectangle((int)((double)this.frameWidth * stabilizedPosition2), 0, this.frameWidth / 4, this.frameHeight), Color.White, intensity);
								goto IL_68C5;
							}
							else
							{
								using (SolidBrush solidBrush6 = new SolidBrush(Color.FromArgb(255, 255, 255, 255)))
								{
									graphics.FillRectangle(solidBrush6, (int)((double)this.frameWidth * stabilizedPosition2), 0, 1, this.frameHeight);
									goto IL_68C5;
								}
							}
						}
						IL_4ED4:
						double stabilizedPosition3 = this.GetStabilizedPosition(this.analyzer._spectrumdata);
						double num104 = Math.Pow((double)((this.analyzer._spectrumdata[0] + this.analyzer._spectrumdata[1]) / 2) / 255.0, 0.75);
						using (SolidBrush solidBrush7 = new SolidBrush(Color.FromArgb((int)(200.0 * num104) + 55, Color.White)))
						{
							if (num104 > 0.05)
							{
								if (stabilizedPosition3 < 0.5)
								{
									graphics.FillRectangle(solidBrush7, 0, 0, this.frameWidth / 2, this.frameHeight);
								}
								else
								{
									graphics.FillRectangle(solidBrush7, this.frameWidth / 2, 0, this.frameWidth / 2, this.frameHeight);
								}
							}
							goto IL_68C5;
						}
						break;
					}
					}
				}
				if (this.frameHeight > 1)
				{
					num = this.frameHeight / 2;
				}
				else
				{
					num = this.frameHeight;
				}
				for (int num105 = 0; num105 < num; num105++)
				{
					array[num105] = new RectangleF((float)(this.frameWidth - this.frameWidth * (int)this.analyzer._spectrumdata[num105] / 255), (float)(num105 * this.frameHeight / num), (float)(this.frameWidth * (int)this.analyzer._spectrumdata[num105] / 255), (float)(this.frameHeight / num));
				}
				brush = new LinearGradientBrush(new System.Drawing.Point(this.frameWidth, this.frameHeight / 2), new System.Drawing.Point(0, this.frameHeight / 2), Color.FromArgb(255, 50, 50, 50), Color.FromArgb(255, 255, 255, 255));
				graphics.FillRectangles(brush, array);
				goto IL_68C5;
				IL_67FB:
				if (this.frameWidth > 1)
				{
					num = this.frameWidth / 2;
				}
				else
				{
					num = this.frameWidth;
				}
				for (int num106 = 0; num106 < num; num106++)
				{
					array[num106] = new RectangleF((float)(num106 * this.frameWidth / num), 0f, (float)(this.frameWidth / num), (float)(this.frameHeight * (int)this.analyzer._spectrumdata[num106] / 255));
				}
				brush = new LinearGradientBrush(new System.Drawing.Point(this.frameWidth / 2, 0), new System.Drawing.Point(this.frameWidth / 2, this.frameHeight), Color.FromArgb(255, 50, 50, 50), Color.FromArgb(255, 255, 255, 255));
				graphics.FillRectangles(brush, array);
				IL_68C5:
				if (this.Afterglow)
				{
					float[][] array5 = new float[5][];
					int num107 = 0;
					float[] array6 = new float[5];
					array6[0] = 1f;
					array5[num107] = array6;
					int num108 = 1;
					float[] array7 = new float[5];
					array7[1] = 1f;
					array5[num108] = array7;
					int num109 = 2;
					float[] array8 = new float[5];
					array8[2] = 1f;
					array5[num109] = array8;
					int num110 = 3;
					float[] array9 = new float[5];
					array9[3] = this._DecayEffect;
					array5[num110] = array9;
					array5[4] = new float[]
					{
						0f,
						0f,
						0f,
						0f,
						1f
					};
					ColorMatrix newColorMatrix = new ColorMatrix(array5);
					ImageAttributes imageAttributes = new ImageAttributes();
					imageAttributes.SetColorMatrix(newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
					graphics.DrawImage(this.lastBitmap, new Rectangle(0, 0, this.StaticBitmap.Width, this.StaticBitmap.Height), 0f, 0f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height, GraphicsUnit.Pixel, imageAttributes);
				}
				graphics.Flush();
			}
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00011288 File Offset: 0x0000F488
		private bool IsAllyCase(int type, int w, int h)
		{
			bool result = false;
			if (type >= 250 && type < 300)
			{
				if (w == 4 && h == 1)
				{
					result = true;
				}
				if (w == 8 && h == 1)
				{
					result = true;
				}
			}
			return result;
		}

		// Token: 0x0600011D RID: 285 RVA: 0x000112BC File Offset: 0x0000F4BC
		private double GetStabilizedPosition(List<byte> data)
		{
			double num = (double)data[0];
			double num2 = (double)data[1];
			if (num < 3.0 && num2 < 3.0)
			{
				return 0.5;
			}
			double relativePosition = AuraLayer.GetRelativePosition(data, 0.2);
			double num3 = (Math.Abs(relativePosition - this._lastPos) > 0.3) ? 0.25 : 0.1;
			this._lastPos += (relativePosition - this._lastPos) * num3;
			return this._lastPos;
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00011358 File Offset: 0x0000F558
		private static double GetRelativePosition(List<byte> data, double gamma = 0.2)
		{
			double d = (double)data[0];
			double d2 = (double)data[1];
			double num = Math.Sqrt(d);
			double num2 = Math.Sqrt(d2);
			double num3 = num + num2;
			if (num3 <= 0.0)
			{
				return 0.5;
			}
			double value = num2 / num3 * 2.0 - 1.0;
			return ((double)Math.Sign(value) * Math.Pow(Math.Abs(value), gamma) + 1.0) / 2.0;
		}

		// Token: 0x0600011F RID: 287 RVA: 0x000113DC File Offset: 0x0000F5DC
		private void DrawFadingCircle(Graphics g, Rectangle rect, Color baseColor, double intensity = 1.0)
		{
			PixelOffsetMode pixelOffsetMode = g.PixelOffsetMode;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			using (GraphicsPath graphicsPath = new GraphicsPath())
			{
				graphicsPath.AddEllipse(rect);
				using (PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath))
				{
					pathGradientBrush.CenterColor = Color.FromArgb((int)(200.0 * intensity) + 55, baseColor);
					pathGradientBrush.SurroundColors = new Color[]
					{
						Color.FromArgb(0, baseColor)
					};
					g.FillEllipse(pathGradientBrush, rect);
				}
			}
			g.PixelOffsetMode = pixelOffsetMode;
		}

		// Token: 0x06000120 RID: 288 RVA: 0x00011484 File Offset: 0x0000F684
		public void DrawImageWithOpacity(Graphics g, Image img, RectangleF destRect, double opacity)
		{
			if (img == null || g == null)
			{
				return;
			}
			float num = (float)opacity;
			if (num > 1f)
			{
				num = 1f;
			}
			if (num < 0f)
			{
				num = 0f;
			}
			ColorMatrix colorMatrix = new ColorMatrix();
			colorMatrix.Matrix33 = num;
			colorMatrix.Matrix43 = -0.01f;
			using (ImageAttributes imageAttributes = new ImageAttributes())
			{
				imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
				g.DrawImage(img, new Rectangle((int)destRect.X, (int)destRect.Y, (int)destRect.Width, (int)destRect.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imageAttributes);
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000121 RID: 289 RVA: 0x00011538 File Offset: 0x0000F738
		// (set) Token: 0x06000122 RID: 290 RVA: 0x00011540 File Offset: 0x0000F740
		public bool isScreenCaptureLayer { get; set; }

		// Token: 0x06000123 RID: 291 RVA: 0x00011549 File Offset: 0x0000F749
		public void Dispose()
		{
			LOGGER.DEBUG("[AuraLayer] Dispose()+", Array.Empty<object>());
			if (this.DoCapture)
			{
				this.discardCap = true;
				AuraLayer.DecrementAndScheduleKill();
			}
			LOGGER.DEBUG("[AuraLayer] Dispose()-", Array.Empty<object>());
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00011580 File Offset: 0x0000F780
		public void InitScreenCaptureLayer(int x, int y, int w, int h, bool repeat = true, int offset_x = 0, int offset_y = 0, bool isFullLayer = true, int width = 21, int height = 8)
		{
			LOGGER.DEBUG("[ScreenCapture] InitScreenCaptureLayer()+ {0}, {1}, {2}, {3} --", new object[]
			{
				x,
				y,
				w,
				h
			});
			this.init(repeat, offset_x, offset_y, isFullLayer, width, height);
			this.isScreenCaptureLayer = true;
			this.isStaticFrame = true;
			this.CaptureRect = new Rectangle(x, y, w, h);
			AuraLayer._maxCaptureRight = Math.Max(AuraLayer._maxCaptureRight, x + w);
			AuraLayer._maxCaptureBottom = Math.Max(AuraLayer._maxCaptureBottom, y + h);
			this.DoCapture = true;
			this.cap_id = this.random.Next(0, 255);
			AuraLayer.SetupSupoort();
			this.discardCap = false;
			new Thread(new ThreadStart(this.CaptureThread))
			{
				IsBackground = true
			}.Start();
			LOGGER.DEBUG("[ScreenCapture] InitScreenCaptureLayer()-({0})", new object[]
			{
				this.cap_id
			});
		}

		// Token: 0x06000125 RID: 293 RVA: 0x0001167C File Offset: 0x0000F87C
		private void CPUMonitorThread()
		{
			while (!this.discardCap)
			{
				float processCpuUsage = Util.GetProcessCpuUsage(0);
				LOGGER.DEBUG("[ScreenCpature] current CPU usage: {0}%", new object[]
				{
					processCpuUsage
				});
				AuraLayer.monitor_count++;
				AuraLayer.monitor_sum += processCpuUsage;
				if (AuraLayer.monitor_count > 1000)
				{
					LOGGER.DEBUG("[ScreenCpature] Average CPU usage: {0}%", new object[]
					{
						AuraLayer.monitor_sum / (float)AuraLayer.monitor_count
					});
					AuraLayer.monitor_count = 0;
					AuraLayer.monitor_sum = 0f;
				}
				Thread.Sleep(10);
			}
		}

		// Token: 0x06000126 RID: 294 RVA: 0x00011718 File Offset: 0x0000F918
		public static void KillSupoort()
		{
			LOGGER.DEBUG("[ScreenCapture] KillSupoort() before lock", Array.Empty<object>());
			object supportLock = AuraLayer.SupportLock;
			lock (supportLock)
			{
				LOGGER.DEBUG("[ScreenCapture] KillSupoort()+", Array.Empty<object>());
				try
				{
					foreach (Process process in Process.GetProcessesByName("LM_Support"))
					{
						process.Kill();
						process.WaitForExit(1000);
						process.Dispose();
					}
				}
				catch (Exception ex)
				{
					LOGGER.DEBUG("[ScreenCapture] KillSupoort() Exception: " + ex.ToString(), Array.Empty<object>());
				}
				object shmClientLock = AuraLayer._shmClientLock;
				lock (shmClientLock)
				{
					if (AuraLayer._shmClient != null)
					{
						AuraLayer._shmClient.Dispose();
						AuraLayer._shmClient = null;
					}
				}
				SharedMemoryClient.DestroySharedMemory();
				LOGGER.DEBUG("[ScreenCapture] KillSupoort()-", Array.Empty<object>());
			}
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00011828 File Offset: 0x0000FA28
		public static bool IsFileDigitallySigned(string filePath)
		{
			bool result;
			try
			{
				result = (X509Certificate.CreateFromSignedFile(filePath) != null);
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00011858 File Offset: 0x0000FA58
		public static X509Certificate2 GetSignerInformation(string filePath)
		{
			X509Certificate2 result;
			try
			{
				result = new X509Certificate2(X509Certificate.CreateFromSignedFile(filePath));
			}
			catch
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06000129 RID: 297 RVA: 0x0001188C File Offset: 0x0000FA8C
		public static bool IsFileSignedWithASUS(string filePath)
		{
			bool result;
			try
			{
				if (!AuraLayer.IsFileDigitallySigned(filePath))
				{
					result = false;
				}
				else
				{
					X509Certificate2 signerInformation = AuraLayer.GetSignerInformation(filePath);
					string text = (signerInformation != null) ? signerInformation.SubjectName.Name : null;
					result = (!string.IsNullOrEmpty(text) && text.Contains("CN=ASUSTeK"));
				}
			}
			catch (Exception ex)
			{
				LOGGER.DEBUG("IsFileSignedWithASUS An error occurred: " + ex.Message, Array.Empty<object>());
				result = false;
			}
			return result;
		}

		// Token: 0x0600012A RID: 298 RVA: 0x00011908 File Offset: 0x0000FB08
		private static bool IsProcessRunning(string exeFileName)
		{
			return Process.GetProcessesByName(exeFileName).Any<Process>();
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00011918 File Offset: 0x0000FB18
		private static void CreateSupportProcess()
		{
			LOGGER.DEBUG("[ScreenCapture] CreateSupportProcess()+", Array.Empty<object>());
			object supportLock = AuraLayer.SupportLock;
			lock (supportLock)
			{
				if (AuraLayer.IsProcessRunning("LM_Support"))
				{
					LOGGER.DEBUG("[ScreenCapture] CreateSupportProcess()- LM_Support launched", Array.Empty<object>());
					return;
				}
				try
				{
					int sessionId = Process.GetCurrentProcess().SessionId;
					if (sessionId == 0)
					{
						LOGGER.DEBUG("[ScreenCapture] CreateSupportProcess(): Session 0, using CreateProcessAsCurrentUser", Array.Empty<object>());
						string commandLine = "\"" + AuraLayer.LM_SUPPORT_EXE + "\" --global";
						if (!AuraLayer.IsFileSignedWithASUS(AuraLayer.LM_SUPPORT_EXE))
						{
							LOGGER.DEBUG("[ScreenCapture] CreateSupportProcess()- !!! LM_Support.exe not signed!", Array.Empty<object>());
						}
						else
						{
							LOGGER.DEBUG("[ScreenCapture] CreateSupportProcess()- LM_Support.exe signed by ASUS", Array.Empty<object>());
							AuraLayer.SupportPID = ProcessAsCurrentUser.CreateProcessAsCurrentUser(AuraLayer.LM_SUPPORT_EXE, commandLine);
						}
					}
					else
					{
						LOGGER.DEBUG("[ScreenCapture] CreateSupportProcess(): Session {0}, using Process.Start", new object[]
						{
							sessionId
						});
						string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LM_Support.exe");
						if (File.Exists(text))
						{
							AuraLayer.LM_SUPPORT_EXE = text;
						}
						else if (!File.Exists(AuraLayer.LM_SUPPORT_EXE))
						{
							LOGGER.ERROR("[ScreenCapture] LM_Support.exe not found at {0} or {1}", new object[]
							{
								AuraLayer.LM_SUPPORT_EXE,
								text
							});
						}
						Process process = Process.Start(new ProcessStartInfo(AuraLayer.LM_SUPPORT_EXE)
						{
							CreateNoWindow = true,
							UseShellExecute = false
						});
						if (process != null)
						{
							AuraLayer.SupportPID = process.Id;
						}
					}
				}
				catch (Exception ex)
				{
					LOGGER.DEBUG("[ScreenCapture] CreateSupportProcess() Exception: " + ex.ToString(), Array.Empty<object>());
				}
			}
			LOGGER.DEBUG("[ScreenCapture] CreateSupportProcess()-", Array.Empty<object>());
		}

		// Token: 0x0600012C RID: 300 RVA: 0x00011AE4 File Offset: 0x0000FCE4
		private static void SetupSupoort()
		{
			object supportLock = AuraLayer.SupportLock;
			lock (supportLock)
			{
				Interlocked.Increment(ref AuraLayer._activeLayerCount);
				LOGGER.DEBUG("[ScreenCapture] SetupSupoort(): _activeLayerCount={0}", new object[]
				{
					AuraLayer._activeLayerCount
				});
				System.Threading.Timer delayedKillTimer = AuraLayer._delayedKillTimer;
				if (delayedKillTimer != null)
				{
					delayedKillTimer.Dispose();
					AuraLayer._delayedKillTimer = null;
					LOGGER.DEBUG("[ScreenCapture] SetupSupoort(): cancelled pending delayed kill", Array.Empty<object>());
				}
				AuraLayer.EnsureSharedMemoryCreated(AuraLayer._maxCaptureRight, AuraLayer._maxCaptureBottom);
				if (AuraLayer.SupportPID == 0)
				{
					LOGGER.DEBUG("[ScreenCapture] SetupSupoort(): Fisrt launch", Array.Empty<object>());
					AuraLayer.CreateSupportProcess();
				}
				else
				{
					try
					{
						Process.GetProcessById(AuraLayer.SupportPID);
						LOGGER.DEBUG("[ScreenCapture] SetupSupoort(): Found PID " + AuraLayer.SupportPID.ToString(), Array.Empty<object>());
					}
					catch (Exception ex)
					{
						LOGGER.DEBUG("[ScreenCapture] SetupSupoort(): PID " + AuraLayer.SupportPID.ToString() + " not available." + ex.ToString(), Array.Empty<object>());
						AuraLayer.CreateSupportProcess();
					}
				}
			}
			AuraLayer.WaitForLmSupportReady();
		}

		// Token: 0x0600012D RID: 301 RVA: 0x00011C04 File Offset: 0x0000FE04
		private static void WaitForLmSupportReady()
		{
			int i = 0;
			int num = 0;
			while (i < 10000)
			{
				Process[] processesByName = Process.GetProcessesByName("LM_Support");
				bool flag = processesByName.Length != 0;
				Process[] array = processesByName;
				for (int j = 0; j < array.Length; j++)
				{
					array[j].Dispose();
				}
				if (flag)
				{
					num++;
					if (num >= 2)
					{
						LOGGER.DEBUG("[ScreenCapture] WaitForLmSupportReady(): confirmed after {0}ms", new object[]
						{
							i
						});
						return;
					}
				}
				else
				{
					num = 0;
				}
				Thread.Sleep(500);
				i += 500;
			}
			LOGGER.WARN("[ScreenCapture] WaitForLmSupportReady(): timeout after {0}ms", new object[]
			{
				10000
			});
		}

		// Token: 0x0600012E RID: 302 RVA: 0x00011CA4 File Offset: 0x0000FEA4
		private static void EnsureSharedMemoryCreated(int minWidth = 0, int minHeight = 0)
		{
			int num = 0;
			int num2 = 0;
			int num3;
			int num4;
			AuraLayer.GetPhysicalDesktopBounds(out num3, out num4);
			if (num3 > 0 && num4 > 0)
			{
				num = num3;
				num2 = num4;
				LOGGER.DEBUG("[ScreenCapture] EnsureSharedMemoryCreated: physical desktop {0}x{1}", new object[]
				{
					num,
					num2
				});
			}
			if (minWidth > num || minHeight > num2)
			{
				LOGGER.DEBUG("[ScreenCapture] EnsureSharedMemoryCreated: expanding from {0}x{1} to cover capture rect {2}x{3}", new object[]
				{
					num,
					num2,
					minWidth,
					minHeight
				});
				num = Math.Max(num, minWidth);
				num2 = Math.Max(num2, minHeight);
			}
			if (num <= 0 || num2 <= 0)
			{
				num = 7680;
				num2 = 4320;
				LOGGER.DEBUG("[ScreenCapture] EnsureSharedMemoryCreated: fallback {0}x{1}", new object[]
				{
					num,
					num2
				});
			}
			num += 2000;
			num2 += 2000;
			if (!SharedMemoryClient.CreateSharedMemory(num, num2))
			{
				LOGGER.WARN("[ScreenCapture] EnsureSharedMemoryCreated: CreateSharedMemory failed for {0}x{1}", new object[]
				{
					num,
					num2
				});
				return;
			}
			LOGGER.DEBUG("[ScreenCapture] EnsureSharedMemoryCreated: MMF ready {0}x{1}", new object[]
			{
				num,
				num2
			});
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00011DCC File Offset: 0x0000FFCC
		private static void GetPhysicalDesktopBounds(out int totalWidth, out int totalHeight)
		{
			totalWidth = 0;
			totalHeight = 0;
			try
			{
				int num = int.MaxValue;
				int num2 = int.MaxValue;
				int num3 = int.MinValue;
				int num4 = int.MinValue;
				AuraLayer.DISPLAY_DEVICE display_DEVICE = default(AuraLayer.DISPLAY_DEVICE);
				display_DEVICE.cb = Marshal.SizeOf<AuraLayer.DISPLAY_DEVICE>(display_DEVICE);
				uint num5 = 0U;
				while (AuraLayer.EnumDisplayDevices(null, num5, ref display_DEVICE, 0U))
				{
					if ((display_DEVICE.StateFlags & 1U) == 0U)
					{
						display_DEVICE = default(AuraLayer.DISPLAY_DEVICE);
						display_DEVICE.cb = Marshal.SizeOf<AuraLayer.DISPLAY_DEVICE>(display_DEVICE);
					}
					else
					{
						AuraLayer.DEVMODE devmode = default(AuraLayer.DEVMODE);
						devmode.dmSize = (short)Marshal.SizeOf<AuraLayer.DEVMODE>(devmode);
						if (AuraLayer.EnumDisplaySettingsEx(display_DEVICE.DeviceName, -1, ref devmode, 0U))
						{
							int dmPositionX = devmode.dmPositionX;
							int dmPositionY = devmode.dmPositionY;
							int val = dmPositionX + devmode.dmPelsWidth;
							int val2 = dmPositionY + devmode.dmPelsHeight;
							num = Math.Min(num, dmPositionX);
							num2 = Math.Min(num2, dmPositionY);
							num3 = Math.Max(num3, val);
							num4 = Math.Max(num4, val2);
						}
						display_DEVICE = default(AuraLayer.DISPLAY_DEVICE);
						display_DEVICE.cb = Marshal.SizeOf<AuraLayer.DISPLAY_DEVICE>(display_DEVICE);
					}
					num5 += 1U;
				}
				if (num3 > num && num4 > num2)
				{
					totalWidth = num3 - num;
					totalHeight = num4 - num2;
				}
			}
			catch (Exception ex)
			{
				LOGGER.DEBUG("[ScreenCapture] GetPhysicalDesktopBounds failed: {0}", new object[]
				{
					ex.Message
				});
			}
		}

		// Token: 0x06000130 RID: 304
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref AuraLayer.DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

		// Token: 0x06000131 RID: 305
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern bool EnumDisplaySettingsEx(string lpszDeviceName, int iModeNum, ref AuraLayer.DEVMODE lpDevMode, uint dwFlags);

		// Token: 0x06000132 RID: 306 RVA: 0x00011F2C File Offset: 0x0001012C
		private static void DecrementAndScheduleKill()
		{
			int num = Interlocked.Decrement(ref AuraLayer._activeLayerCount);
			LOGGER.DEBUG("[ScreenCapture] DecrementAndScheduleKill(): _activeLayerCount={0}", new object[]
			{
				num
			});
			if (num <= 0)
			{
				object supportLock = AuraLayer.SupportLock;
				lock (supportLock)
				{
					if (AuraLayer._delayedKillTimer != null)
					{
						AuraLayer._delayedKillTimer.Dispose();
					}
					AuraLayer._delayedKillTimer = new System.Threading.Timer(delegate(object _)
					{
						if (Interlocked.CompareExchange(ref AuraLayer._activeLayerCount, 0, 0) <= 0)
						{
							LOGGER.DEBUG("[ScreenCapture] Delayed kill executing (count still 0)", Array.Empty<object>());
							AuraLayer.KillSupoort();
							return;
						}
						LOGGER.DEBUG("[ScreenCapture] Delayed kill cancelled (count > 0)", Array.Empty<object>());
					}, null, 5000, -1);
				}
			}
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00011FD0 File Offset: 0x000101D0
		private void CaptureThread()
		{
			LOGGER.DEBUG("[ScreenCapture] CaptureThread() start thread", Array.Empty<object>());
			DateTime now = DateTime.Now;
			int num = 0;
			int num2 = 0;
			DateTime d = DateTime.MinValue;
			uint num3 = 0U;
			int num4 = 0;
			DateTime d2 = DateTime.MinValue;
			for (;;)
			{
				Bitmap bitmap = null;
				try
				{
					if (num2 == 0)
					{
						LOGGER.DEBUG(string.Format("[ScreenCapture][COORD] CaptureThread: CaptureRect=({0},{1},{2},{3}) frameSize=({4},{5})", new object[]
						{
							this.CaptureRect.X,
							this.CaptureRect.Y,
							this.CaptureRect.Width,
							this.CaptureRect.Height,
							this.frameWidth,
							this.frameHeight
						}), Array.Empty<object>());
					}
					num2++;
					bitmap = AuraLayer.PipeClient.CaptureScreen2(this.CaptureRect.X, this.CaptureRect.Y, this.CaptureRect.Width, this.CaptureRect.Height, this.frameWidth, this.frameHeight, 3000);
				}
				catch (Exception ex)
				{
					LOGGER.DEBUG("[ScreenCapture] UpdateScreenCaptureFrame() CaptureScreen Exception: " + ex.ToString(), Array.Empty<object>());
				}
				if (bitmap != null)
				{
					num = 0;
					uint num5 = 0U;
					object obj;
					try
					{
						obj = AuraLayer._shmClientLock;
						lock (obj)
						{
							if (AuraLayer._shmClient != null)
							{
								num5 = AuraLayer._shmClient.LastFrameIndex;
							}
						}
					}
					catch
					{
					}
					if (num5 > 0U && num5 == num3)
					{
						num4++;
					}
					else
					{
						num4 = 0;
						num3 = num5;
					}
					if (num4 >= 5 && (DateTime.UtcNow - d2).TotalSeconds > 5.0)
					{
						d2 = DateTime.UtcNow;
						bool flag2 = false;
						try
						{
							Process[] processesByName = Process.GetProcessesByName("LM_Support");
							flag2 = (processesByName.Length != 0);
							Process[] array = processesByName;
							for (int i = 0; i < array.Length; i++)
							{
								array[i].Dispose();
							}
						}
						catch
						{
						}
						if (!flag2)
						{
							LOGGER.DEBUG("[ScreenCapture] CaptureThread: LM_Support dead (stale frame), relaunching...", Array.Empty<object>());
							obj = AuraLayer.SupportLock;
							lock (obj)
							{
								AuraLayer.SupportPID = 0;
								AuraLayer.EnsureSharedMemoryCreated(AuraLayer._maxCaptureRight, AuraLayer._maxCaptureBottom);
								AuraLayer.CreateSupportProcess();
							}
							num4 = 0;
							num3 = 0U;
						}
					}
					obj = this.lockobj;
					lock (obj)
					{
						Bitmap bitmap2 = this.capBitmap;
						this.capBitmap = bitmap;
						if (bitmap2 == null)
						{
							goto IL_335;
						}
						bitmap2.Dispose();
						goto IL_335;
					}
					goto IL_27A;
				}
				goto IL_27A;
				IL_335:
				if (this.discardCap)
				{
					break;
				}
				continue;
				IL_27A:
				num++;
				if (num < 2 || (DateTime.UtcNow - d).TotalSeconds <= 15.0)
				{
					goto IL_335;
				}
				d = DateTime.UtcNow;
				bool flag3 = false;
				try
				{
					Process[] processesByName2 = Process.GetProcessesByName("LM_Support");
					flag3 = (processesByName2.Length != 0);
					Process[] array = processesByName2;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Dispose();
					}
				}
				catch
				{
				}
				if (!flag3)
				{
					LOGGER.DEBUG("[ScreenCapture] CaptureThread: LM_Support dead, relaunching...", Array.Empty<object>());
					object obj = AuraLayer.SupportLock;
					lock (obj)
					{
						AuraLayer.SupportPID = 0;
						AuraLayer.EnsureSharedMemoryCreated(AuraLayer._maxCaptureRight, AuraLayer._maxCaptureBottom);
						AuraLayer.CreateSupportProcess();
					}
					num = 0;
					goto IL_335;
				}
				goto IL_335;
			}
		}

		// Token: 0x06000134 RID: 308 RVA: 0x00012384 File Offset: 0x00010584
		public void UpdateScreenCaptureFrame()
		{
			object obj = this.lockobj;
			lock (obj)
			{
				try
				{
					using (Graphics graphics = Graphics.FromImage(this.StaticBitmap))
					{
						if (this.capBitmap != null)
						{
							this._capBitmapNullCount = 0;
							graphics.DrawImage(this.capBitmap, new Rectangle(0, 0, this.StaticBitmap.Width, this.StaticBitmap.Height), new Rectangle(0, 0, this.capBitmap.Width, this.capBitmap.Height), GraphicsUnit.Pixel);
						}
						else
						{
							this._capBitmapNullCount++;
							if (this._capBitmapNullCount <= 3 || this._capBitmapNullCount % 100 == 0)
							{
								LOGGER.DEBUG("[ScreenCapture] capBitmap is null({0}) count={1}", new object[]
								{
									this.cap_id,
									this._capBitmapNullCount
								});
							}
						}
						graphics.Flush();
					}
				}
				catch (Exception ex)
				{
					LOGGER.DEBUG("[ScreenCapture]  UpdateScreenCaptureFrame() DrawImage Exception: " + ex.ToString(), Array.Empty<object>());
				}
			}
		}

		// Token: 0x06000135 RID: 309 RVA: 0x000124B8 File Offset: 0x000106B8
		private static Bitmap CloneImage(Bitmap sourceImage)
		{
			Rectangle rect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
			Bitmap bitmap = new Bitmap(rect.Width, rect.Height, sourceImage.PixelFormat);
			bitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
			BitmapData bitmapData = sourceImage.LockBits(rect, ImageLockMode.ReadOnly, sourceImage.PixelFormat);
			BitmapData bitmapData2 = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
			int num = (Image.GetPixelFormatSize(sourceImage.PixelFormat) * rect.Width + 7) / 8;
			int height = sourceImage.Height;
			int num2 = bitmapData.Stride;
			bool flag = num2 < 0;
			num2 = Math.Abs(num2);
			int stride = bitmapData2.Stride;
			byte[] array = new byte[num];
			IntPtr scan = bitmapData.Scan0;
			IntPtr scan2 = bitmapData2.Scan0;
			for (int i = 0; i < height; i++)
			{
				Marshal.Copy(scan, array, 0, num);
				Marshal.Copy(array, 0, scan2, num);
				scan = new IntPtr(scan.ToInt64() + (long)num2);
				scan2 = new IntPtr(scan2.ToInt64() + (long)stride);
			}
			bitmap.UnlockBits(bitmapData2);
			sourceImage.UnlockBits(bitmapData);
			if (flag)
			{
				bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
			}
			if ((sourceImage.PixelFormat & PixelFormat.Indexed) != PixelFormat.Undefined)
			{
				bitmap.Palette = sourceImage.Palette;
			}
			bitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
			return bitmap;
		}

		// Token: 0x06000136 RID: 310 RVA: 0x00012611 File Offset: 0x00010811
		private static Bitmap BitmapFromFile(string path)
		{
			return (Bitmap)Image.FromStream(new MemoryStream(File.ReadAllBytes(path)));
		}

		// Token: 0x06000137 RID: 311 RVA: 0x00012628 File Offset: 0x00010828
		public void UpdateCaptureRect(int x, int y, int w, int h)
		{
			LOGGER.DEBUG(string.Format("[ScreenCapture][COORD] UpdateCaptureRect: x={0}, y={1}, w={2}, h={3}", new object[]
			{
				x,
				y,
				w,
				h
			}), Array.Empty<object>());
			this.CaptureRect = new Rectangle(x, y, w, h);
			AuraLayer._maxCaptureRight = Math.Max(AuraLayer._maxCaptureRight, x + w);
			AuraLayer._maxCaptureBottom = Math.Max(AuraLayer._maxCaptureBottom, y + h);
		}

		// Token: 0x06000138 RID: 312 RVA: 0x000126AC File Offset: 0x000108AC
		private bool IsInsideScreen(Rectangle rect)
		{
			int left = SystemInformation.VirtualScreen.Left;
			int top = SystemInformation.VirtualScreen.Top;
			int width = SystemInformation.VirtualScreen.Width;
			int height = SystemInformation.VirtualScreen.Height;
			LOGGER.DEBUG(string.Format("[ScreenCapture][COORD] IsInsideScreen: rect=({0},{1},{2},{3}) virtualScreen=({4},{5},{6},{7})", new object[]
			{
				rect.X,
				rect.Y,
				rect.Width,
				rect.Height,
				left,
				top,
				width,
				height
			}), Array.Empty<object>());
			return rect.X >= left && rect.X <= left + width && rect.Y >= top && rect.Y <= top + height && rect.Right >= left && rect.Right <= left + width && rect.Bottom >= top && rect.Bottom <= top + height;
		}

		// Token: 0x06000139 RID: 313 RVA: 0x000127D0 File Offset: 0x000109D0
		private Bitmap CaptureScreen(Rectangle rect)
		{
			int left = SystemInformation.VirtualScreen.Left;
			int top = SystemInformation.VirtualScreen.Top;
			int width = SystemInformation.VirtualScreen.Width;
			int height = SystemInformation.VirtualScreen.Height;
			Bitmap result;
			try
			{
				Bitmap bitmap = new Bitmap(rect.Width, rect.Height);
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					if (this.IsInsideScreen(rect))
					{
						graphics.CopyFromScreen(left + rect.X, top + rect.Y, 0, 0, bitmap.Size);
					}
					else
					{
						graphics.Clear(Color.FromArgb(255, 0, 0, 0));
					}
					graphics.Flush();
				}
				result = bitmap;
			}
			catch (Exception)
			{
				Bitmap bitmap2 = new Bitmap(100, 100);
				using (Graphics graphics2 = Graphics.FromImage(bitmap2))
				{
					graphics2.Clear(Color.FromArgb(255, 0, 0, 0));
					graphics2.Flush();
				}
				result = bitmap2;
			}
			return result;
		}

		// Token: 0x0600013A RID: 314 RVA: 0x000128FC File Offset: 0x00010AFC
		private Image CaptureWindow(IntPtr handle)
		{
			IntPtr windowDC = AuraLayer.User32.GetWindowDC(handle);
			AuraLayer.User32.RECT rect = default(AuraLayer.User32.RECT);
			AuraLayer.User32.GetWindowRect(handle, ref rect);
			int nWidth = rect.right - rect.left;
			int nHeight = rect.bottom - rect.top;
			IntPtr intPtr = AuraLayer.GDI32.CreateCompatibleDC(windowDC);
			IntPtr intPtr2 = AuraLayer.GDI32.CreateCompatibleBitmap(windowDC, nWidth, nHeight);
			IntPtr hObject = AuraLayer.GDI32.SelectObject(intPtr, intPtr2);
			AuraLayer.GDI32.BitBlt(intPtr, 0, 0, nWidth, nHeight, windowDC, 0, 0, 13369376);
			AuraLayer.GDI32.SelectObject(intPtr, hObject);
			AuraLayer.GDI32.DeleteDC(intPtr);
			AuraLayer.User32.ReleaseDC(handle, windowDC);
			Image result = Image.FromHbitmap(intPtr2);
			AuraLayer.GDI32.DeleteObject(intPtr2);
			return result;
		}

		// Token: 0x0600013B RID: 315 RVA: 0x00012990 File Offset: 0x00010B90
		private Image CaptureScreen()
		{
			return this.CaptureWindow(AuraLayer.User32.GetDesktopWindow());
		}

		// Token: 0x0600013C RID: 316 RVA: 0x000129A0 File Offset: 0x00010BA0
		private static AuraLayer.ColorRGB HSL2RGB(double h, double sl, double l)
		{
			double num = l;
			double num2 = l;
			double num3 = l;
			double num4 = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);
			if (num4 > 0.0)
			{
				double num5 = l + l - num4;
				double num6 = (num4 - num5) / num4;
				h *= 6.0;
				int num7 = (int)h;
				double num8 = h - (double)num7;
				double num9 = num4 * num6 * num8;
				double num10 = num5 + num9;
				double num11 = num4 - num9;
				switch (num7)
				{
				case 0:
					num = num4;
					num2 = num10;
					num3 = num5;
					break;
				case 1:
					num = num11;
					num2 = num4;
					num3 = num5;
					break;
				case 2:
					num = num5;
					num2 = num4;
					num3 = num10;
					break;
				case 3:
					num = num5;
					num2 = num11;
					num3 = num4;
					break;
				case 4:
					num = num10;
					num2 = num5;
					num3 = num4;
					break;
				case 5:
					num = num4;
					num2 = num5;
					num3 = num11;
					break;
				}
			}
			AuraLayer.ColorRGB result;
			result.R = Convert.ToByte(num * 255.0);
			result.G = Convert.ToByte(num2 * 255.0);
			result.B = Convert.ToByte(num3 * 255.0);
			return result;
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00012AC4 File Offset: 0x00010CC4
		private static void RGB2HSL(AuraLayer.ColorRGB rgb, out double h, out double s, out double l)
		{
			double num = (double)rgb.R / 255.0;
			double num2 = (double)rgb.G / 255.0;
			double num3 = (double)rgb.B / 255.0;
			h = 0.0;
			s = 0.0;
			l = 0.0;
			double num4 = Math.Max(num, num2);
			num4 = Math.Max(num4, num3);
			double num5 = Math.Min(num, num2);
			num5 = Math.Min(num5, num3);
			l = (num5 + num4) / 2.0;
			if (l <= 0.0)
			{
				return;
			}
			double num6 = num4 - num5;
			s = num6;
			if (s > 0.0)
			{
				s /= ((l <= 0.5) ? (num4 + num5) : (2.0 - num4 - num5));
				double num7 = (num4 - num) / num6;
				double num8 = (num4 - num2) / num6;
				double num9 = (num4 - num3) / num6;
				if (num == num4)
				{
					h = ((num2 == num5) ? (5.0 + num9) : (1.0 - num8));
				}
				else if (num2 == num4)
				{
					h = ((num3 == num5) ? (1.0 + num7) : (3.0 - num9));
				}
				else
				{
					h = ((num == num5) ? (3.0 + num8) : (5.0 - num7));
				}
				h /= 6.0;
				return;
			}
		}

		// Token: 0x0600013E RID: 318 RVA: 0x00012C40 File Offset: 0x00010E40
		public void InitSlashLightingLayer(bool repeat = true, int offset_x = 0, int offset_y = 0, bool isFullLayer = true, int width = 7, int height = 1)
		{
			LOGGER.DEBUG("[SlashLighting] InitSlashLightingLayer {0}, {1}, {2}, {3}, {4}, {5}", new object[]
			{
				repeat,
				offset_x,
				offset_y,
				isFullLayer,
				width,
				height
			});
			AuraLayer.SLASHLIGHTING_COUNT = ((width != 0) ? width : 7);
			this.init(repeat, offset_x, offset_y, isFullLayer, width, height);
			this.isSlashLightingLayer = true;
			this.isStaticFrame = true;
			for (int i = 0; i < AuraLayer.SLASHLIGHTING_PRIORITY_COUNT; i++)
			{
				this.SlashLightingsEffectPool[i] = new List<AuraLayer.SlashLightingEffect>();
			}
		}

		// Token: 0x0600013F RID: 319 RVA: 0x00012CDE File Offset: 0x00010EDE
		public void ChangeSlashLighingCount(int count)
		{
			AuraLayer.SLASHLIGHTING_COUNT = count;
		}

		// Token: 0x06000140 RID: 320 RVA: 0x00012CE8 File Offset: 0x00010EE8
		public void AddSlashLightingGlyph(string filename, int priority, bool isloop, int interval, int repeat_times, int brightness, string wavefile = null)
		{
			LOGGER.DEBUG("[SlashLighting] AddSlashLightingGlyph+ {0}, {1}, {2}, {3}, {4}, {5}, {6}", new object[]
			{
				filename,
				priority,
				isloop,
				interval,
				repeat_times,
				brightness,
				wavefile
			});
			object sllock = this.SLLock;
			lock (sllock)
			{
				AuraLayer.Glyph effect = new AuraLayer.Glyph(filename, priority, isloop, interval, repeat_times, brightness, wavefile);
				this.AddSlashLightingEffectToPool(effect, priority);
			}
			LOGGER.DEBUG("[SlashLighting] AddSlashLightingGlyph- {0}, {1}, {2}, {3}, {4}, {5}, {6}", new object[]
			{
				filename,
				priority,
				isloop,
				interval,
				repeat_times,
				brightness,
				wavefile
			});
		}

		// Token: 0x06000141 RID: 321 RVA: 0x00012DD0 File Offset: 0x00010FD0
		public void AddSlashLightingPlayGlyph(string dev, int priority, int type, int decay, int strength, bool isFullRange, int rangeIndex, int brightness)
		{
			LOGGER.DEBUG("[SlashLighting] AddSlashLightingPlayGlyph+ {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", new object[]
			{
				dev,
				priority,
				type,
				decay,
				strength,
				isFullRange,
				rangeIndex,
				brightness
			});
			object sllock = this.SLLock;
			lock (sllock)
			{
				AuraLayer.PlayGlyph effect = new AuraLayer.PlayGlyph(dev, priority, type, decay, strength, isFullRange, rangeIndex, brightness);
				this.AddSlashLightingEffectToPool(effect, priority);
			}
			LOGGER.DEBUG("[SlashLighting] AddSlashLightingPlayGlyph- {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", new object[]
			{
				dev,
				priority,
				type,
				decay,
				strength,
				isFullRange,
				rangeIndex,
				brightness
			});
		}

		// Token: 0x06000142 RID: 322 RVA: 0x00012ED8 File Offset: 0x000110D8
		private static int PriorityConv(int p)
		{
			int result;
			if (p < 1)
			{
				result = 0;
			}
			else if (p > 4)
			{
				result = 3;
			}
			else
			{
				result = p - 1;
			}
			return result;
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00012EFC File Offset: 0x000110FC
		public void ClearAllGlyph()
		{
			LOGGER.DEBUG("[SlashLighting] ClearAllGlyph+", Array.Empty<object>());
			object sllock = this.SLLock;
			lock (sllock)
			{
				for (int i = 0; i < AuraLayer.SLASHLIGHTING_PRIORITY_COUNT; i++)
				{
					while (this.SlashLightingsEffectPool[i].Count<AuraLayer.SlashLightingEffect>() > 0)
					{
						this.SlashLightingsEffectPool[i][0].Finalize();
						this.SlashLightingsEffectPool[i].RemoveAt(0);
					}
				}
				AuraLayer.WavePlayer.Stop();
			}
			LOGGER.DEBUG("[SlashLighting] ClearAllGlyph-", Array.Empty<object>());
		}

		// Token: 0x06000144 RID: 324 RVA: 0x00012F9C File Offset: 0x0001119C
		private void UpdateSlashLightingFrame()
		{
			object sllock = this.SLLock;
			lock (sllock)
			{
				using (Graphics graphics = Graphics.FromImage(this.StaticBitmap))
				{
					graphics.Clear(Color.Black);
					graphics.Flush();
				}
				for (int i = 0; i < AuraLayer.SLASHLIGHTING_PRIORITY_COUNT; i++)
				{
					if (this.SlashLightingsEffectPool[i].Count<AuraLayer.SlashLightingEffect>() > 0)
					{
						Bitmap frame = this.SlashLightingsEffectPool[i][0].NextFrame();
						if (this.SlashLightingsEffectPool[i][0].IsEnded)
						{
							this.SlashLightingsEffectPool[i][0].Finalize();
							this.SlashLightingsEffectPool[i].RemoveAt(0);
						}
						this.DrawStaticBitmap(frame);
						break;
					}
				}
			}
		}

		// Token: 0x06000145 RID: 325 RVA: 0x0001308C File Offset: 0x0001128C
		private void AddSlashLightingEffectToPool(AuraLayer.SlashLightingEffect effect, int priority)
		{
			int num = AuraLayer.PriorityConv(priority);
			if (this.SlashLightingsEffectPool[num].Count<AuraLayer.SlashLightingEffect>() > 0 && this.SlashLightingsEffectPool[num][0].IsEndless && effect.IsEndless)
			{
				this.SlashLightingsEffectPool[num][0].Finalize();
				this.SlashLightingsEffectPool[num].RemoveAt(0);
			}
			this.SlashLightingsEffectPool[num].Insert(0, effect);
		}

		// Token: 0x06000146 RID: 326 RVA: 0x00013100 File Offset: 0x00011300
		public static Bitmap SetBitmapBrightness(Bitmap bmp, int brightness)
		{
			float num = (float)brightness / 100f;
			if (brightness > 100)
			{
				num = 1f;
			}
			if (brightness < 0)
			{
				num = 0f;
			}
			Bitmap bitmap = new Bitmap(bmp.Width, bmp.Height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				float[][] array = new float[5][];
				int num2 = 0;
				float[] array2 = new float[5];
				array2[0] = 1f;
				array[num2] = array2;
				int num3 = 1;
				float[] array3 = new float[5];
				array3[1] = 1f;
				array[num3] = array3;
				int num4 = 2;
				float[] array4 = new float[5];
				array4[2] = 1f;
				array[num4] = array4;
				int num5 = 3;
				float[] array5 = new float[5];
				array5[3] = num;
				array[num5] = array5;
				array[4] = new float[]
				{
					0f,
					0f,
					0f,
					0f,
					1f
				};
				ColorMatrix newColorMatrix = new ColorMatrix(array);
				ImageAttributes imageAttributes = new ImageAttributes();
				imageAttributes.SetColorMatrix(newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
				graphics.DrawImage(bmp, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0f, 0f, (float)bmp.Width, (float)bmp.Height, GraphicsUnit.Pixel, imageAttributes);
				graphics.Flush();
			}
			return bitmap;
		}

		// Token: 0x04000069 RID: 105
		private const string libpath = "ACStoreFileManager_x86.dll";

		// Token: 0x0400006A RID: 106
		public static AuraLayer.ACFM_InitializeDelegate initialize;

		// Token: 0x0400006B RID: 107
		public static AuraLayer.ACFM_InitializeWithPathDelegate InitializeWithPath;

		// Token: 0x0400006C RID: 108
		public static AuraLayer.GetFileStreamCSharpDelegate GetFileStreamCSharp;

		// Token: 0x0400006D RID: 109
		public static AuraLayer.GetFileSizeCSharpDelegate GetFileSizeCSharp;

		// Token: 0x0400006E RID: 110
		public static AuraLayer.GetFileReadedSizeCSharpDelegate GetFileReadedSizeCSharp;

		// Token: 0x0400006F RID: 111
		public static AuraLayer.ReadStreamCSharpDelegate ReadStreamCSharp;

		// Token: 0x04000070 RID: 112
		public static AuraLayer.ReleaseStreamDataCSharpDelegate ReleaseStreamDataCSharp;

		// Token: 0x04000071 RID: 113
		public static AuraLayer.CloseStreamCSharpDelegate CloseStreamCSharp;

		// Token: 0x04000072 RID: 114
		public static AuraLayer.DestroyStreamCSharpDelegate DestroyStreamCSharp;

		// Token: 0x04000073 RID: 115
		public static AuraLayer.GetPluginVersionCSharpDelegate GetPluginVersionCSharp;

		// Token: 0x04000074 RID: 116
		private string existingPath;

		// Token: 0x04000075 RID: 117
		private List<AuraLayer.ColorPoint> colorpoints = new List<AuraLayer.ColorPoint>();

		// Token: 0x04000076 RID: 118
		private int gradientW = 100;

		// Token: 0x04000077 RID: 119
		private int gradientH = 100;

		// Token: 0x04000078 RID: 120
		private int _gradientFactor = 7;

		// Token: 0x04000079 RID: 121
		private bool isGradientLayer;

		// Token: 0x0400007A RID: 122
		private const int MAX_FRAME_COUNT = 1000;

		// Token: 0x0400007B RID: 123
		private const int DEFAULT_FRAME_WIDTH = 21;

		// Token: 0x0400007C RID: 124
		private const int DEFAULT_FRAME_HEIGHT = 8;

		// Token: 0x0400007D RID: 125
		private const int MAX_TEXTCONTENT_COUNT = 100;

		// Token: 0x0400007E RID: 126
		private List<Bitmap> frameList;

		// Token: 0x0400007F RID: 127
		private AuraAnimation parentAnimation;

		// Token: 0x04000080 RID: 128
		private AuraLayer.ImageInfo imageInfo;

		// Token: 0x04000081 RID: 129
		private DateTime localDate;

		// Token: 0x04000082 RID: 130
		private AuraLayer.SYSTEM_POWER_STATUS SysPower;

		// Token: 0x04000083 RID: 131
		private bool hasTextPattern;

		// Token: 0x04000084 RID: 132
		private int[] LeftLevel = new int[200];

		// Token: 0x04000085 RID: 133
		private int[] RightLevel = new int[200];

		// Token: 0x04000086 RID: 134
		private int _rotationDegree;

		// Token: 0x04000087 RID: 135
		private int _audioEffectStrength;

		// Token: 0x04000088 RID: 136
		private float _DecayEffect;

		// Token: 0x04000089 RID: 137
		private Bitmap lastBitmap;

		// Token: 0x0400008A RID: 138
		private Rectangle CropRect;

		// Token: 0x0400008B RID: 139
		private List<Bitmap> CropframeList;

		// Token: 0x0400008C RID: 140
		private bool IsDRMInited;

		// Token: 0x0400008D RID: 141
		private List<AuraLayer.TextContent> TextList;

		// Token: 0x0400008E RID: 142
		private int TextUpdateCount;

		// Token: 0x0400009F RID: 159
		private float _AnimationSpeedRatio = 1f;

		// Token: 0x040000A3 RID: 163
		private AudioAnalyzer analyzer;

		// Token: 0x040000A4 RID: 164
		private bool IsAnalyzerEnabled;

		// Token: 0x040000A5 RID: 165
		private int _SpectrumType;

		// Token: 0x040000A6 RID: 166
		private int _StrobingBeatThreadhold;

		// Token: 0x040000A7 RID: 167
		private int _StrobingBeatStartFreq;

		// Token: 0x040000A8 RID: 168
		private int _StrobingBeatEndFreq;

		// Token: 0x040000AA RID: 170
		private bool _ShowBeading;

		// Token: 0x040000AB RID: 171
		private const int MaxSpectrumCount = 1024;

		// Token: 0x040000AC RID: 172
		private int MS_DecreaseCount = 5;

		// Token: 0x040000AD RID: 173
		private int[] MaxSpectrum = new int[1024];

		// Token: 0x040000AE RID: 174
		private const int MatrixSpectrumBarWidth = 4;

		// Token: 0x040000AF RID: 175
		private Bitmap AllyBitmap;

		// Token: 0x040000B0 RID: 176
		private Bitmap AllyBitmapClone;

		// Token: 0x040000B1 RID: 177
		private double _lastPos = 0.5;

		// Token: 0x040000B3 RID: 179
		public int ScreenCaptureType;

		// Token: 0x040000B4 RID: 180
		private Rectangle CaptureRect = new Rectangle(0, 0, 1, 1);

		// Token: 0x040000B5 RID: 181
		private bool DoCapture;

		// Token: 0x040000B6 RID: 182
		private object lockobj = new object();

		// Token: 0x040000B7 RID: 183
		private Bitmap capBitmap;

		// Token: 0x040000B8 RID: 184
		private int _capBitmapNullCount;

		// Token: 0x040000B9 RID: 185
		private static string LM_SUPPORT_EXE = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ASUS\\AURA lighting effect add-on\\LM_Support.exe");

		// Token: 0x040000BA RID: 186
		private static readonly object SupportLock = new object();

		// Token: 0x040000BB RID: 187
		private static int SupportPID = 0;

		// Token: 0x040000BC RID: 188
		private bool discardCap;

		// Token: 0x040000BD RID: 189
		private Random random = new Random();

		// Token: 0x040000BE RID: 190
		private int cap_id;

		// Token: 0x040000BF RID: 191
		private static int _activeLayerCount = 0;

		// Token: 0x040000C0 RID: 192
		private static int _maxCaptureRight = 0;

		// Token: 0x040000C1 RID: 193
		private static int _maxCaptureBottom = 0;

		// Token: 0x040000C2 RID: 194
		private static System.Threading.Timer _delayedKillTimer;

		// Token: 0x040000C3 RID: 195
		private const int DELAYED_KILL_MS = 5000;

		// Token: 0x040000C4 RID: 196
		private static SharedMemoryClient _shmClient;

		// Token: 0x040000C5 RID: 197
		private static readonly object _shmClientLock = new object();

		// Token: 0x040000C6 RID: 198
		private static Thread cpu_t = null;

		// Token: 0x040000C7 RID: 199
		private static int monitor_count = 0;

		// Token: 0x040000C8 RID: 200
		private static float monitor_sum = 0f;

		// Token: 0x040000C9 RID: 201
		private int GCcount = 10;

		// Token: 0x040000CA RID: 202
		private static int SLASHLIGHTING_COUNT = 7;

		// Token: 0x040000CB RID: 203
		private static int SLASHLIGHTING_PRIORITY_COUNT = 4;

		// Token: 0x040000CC RID: 204
		private bool isSlashLightingLayer;

		// Token: 0x040000CD RID: 205
		private object SLLock = new object();

		// Token: 0x040000CE RID: 206
		private List<AuraLayer.SlashLightingEffect>[] SlashLightingsEffectPool = new List<AuraLayer.SlashLightingEffect>[AuraLayer.SLASHLIGHTING_PRIORITY_COUNT];

		// Token: 0x02000034 RID: 52
		// (Invoke) Token: 0x060002D9 RID: 729
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void ListAdd([MarshalAs(UnmanagedType.LPWStr)] string msg);

		// Token: 0x02000035 RID: 53
		// (Invoke) Token: 0x060002DD RID: 733
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public delegate int ACFM_InitializeDelegate();

		// Token: 0x02000036 RID: 54
		// (Invoke) Token: 0x060002E1 RID: 737
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public delegate int ACFM_InitializeWithPathDelegate([MarshalAs(UnmanagedType.LPWStr)] string path);

		// Token: 0x02000037 RID: 55
		// (Invoke) Token: 0x060002E5 RID: 741
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public delegate int GetFileStreamCSharpDelegate([MarshalAs(UnmanagedType.LPWStr)] string user_id, [MarshalAs(UnmanagedType.LPWStr)] string content_id, [MarshalAs(UnmanagedType.LPWStr)] string path, ref IntPtr ACSDRMFileStream);

		// Token: 0x02000038 RID: 56
		// (Invoke) Token: 0x060002E9 RID: 745
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public delegate int GetFileSizeCSharpDelegate(IntPtr ACSDRMFileStream);

		// Token: 0x02000039 RID: 57
		// (Invoke) Token: 0x060002ED RID: 749
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public delegate int GetFileReadedSizeCSharpDelegate(IntPtr ACSDRMFileStream);

		// Token: 0x0200003A RID: 58
		// (Invoke) Token: 0x060002F1 RID: 753
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public delegate int ReadStreamCSharpDelegate(IntPtr ACSDRMFileStream, ref IntPtr out_vect, ref int out_len, bool MD5Check);

		// Token: 0x0200003B RID: 59
		// (Invoke) Token: 0x060002F5 RID: 757
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public delegate int ReleaseStreamDataCSharpDelegate(ref IntPtr out_vect);

		// Token: 0x0200003C RID: 60
		// (Invoke) Token: 0x060002F9 RID: 761
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public delegate int CloseStreamCSharpDelegate(IntPtr ACSDRMFileStream);

		// Token: 0x0200003D RID: 61
		// (Invoke) Token: 0x060002FD RID: 765
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public delegate int DestroyStreamCSharpDelegate(IntPtr ACSDRMFileStream);

		// Token: 0x0200003E RID: 62
		// (Invoke) Token: 0x06000301 RID: 769
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public delegate IntPtr GetPluginVersionCSharpDelegate();

		// Token: 0x0200003F RID: 63
		public static class DllLoader
		{
			// Token: 0x06000304 RID: 772
			[DllImport("kernel32.dll")]
			public static extern IntPtr LoadLibrary(string dllToLoad);

			// Token: 0x06000305 RID: 773
			[DllImport("kernel32.dll")]
			public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

			// Token: 0x06000306 RID: 774
			[DllImport("kernel32.dll")]
			public static extern bool FreeLibrary(IntPtr hModule);

			// Token: 0x06000307 RID: 775 RVA: 0x0001776C File Offset: 0x0001596C
			public static T LoadFunction<T>(string dllPath, string functionName) where T : Delegate
			{
				IntPtr intPtr = AuraLayer.DllLoader.LoadLibrary(dllPath);
				if (intPtr == IntPtr.Zero)
				{
					throw new Exception(string.Format("Failed to load library {0}. Error code: {1}", dllPath, Marshal.GetLastWin32Error()));
				}
				IntPtr procAddress = AuraLayer.DllLoader.GetProcAddress(intPtr, functionName);
				if (procAddress == IntPtr.Zero)
				{
					throw new Exception(string.Format("Failed to find function {0}. Error code: {1}", functionName, Marshal.GetLastWin32Error()));
				}
				return (T)((object)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(T)));
			}
		}

		// Token: 0x02000040 RID: 64
		private class ColorPoint
		{
			// Token: 0x06000308 RID: 776 RVA: 0x000177EC File Offset: 0x000159EC
			public ColorPoint(Vector p, Color c)
			{
				this.point = p;
				this.color = c;
				HSLColor.RGBToHSL((int)c.R, (int)c.G, (int)c.B, out this.H, out this.S, out this.L);
			}

			// Token: 0x0400016C RID: 364
			public Vector point;

			// Token: 0x0400016D RID: 365
			public Color color;

			// Token: 0x0400016E RID: 366
			public double H;

			// Token: 0x0400016F RID: 367
			public double S;

			// Token: 0x04000170 RID: 368
			public double L;
		}

		// Token: 0x02000041 RID: 65
		private class TextContent
		{
			// Token: 0x04000171 RID: 369
			public string content;

			// Token: 0x04000172 RID: 370
			public bool bSlide;

			// Token: 0x04000173 RID: 371
			public bool bRevert;

			// Token: 0x04000174 RID: 372
			public int offset_x;

			// Token: 0x04000175 RID: 373
			public int offset_y;

			// Token: 0x04000176 RID: 374
			public int width;

			// Token: 0x04000177 RID: 375
			public int height;

			// Token: 0x04000178 RID: 376
			public string fontname;

			// Token: 0x04000179 RID: 377
			public int fontsize;

			// Token: 0x0400017A RID: 378
			public int speed;

			// Token: 0x0400017B RID: 379
			public bool isAntiAlias;

			// Token: 0x0400017C RID: 380
			public bool bBorder;

			// Token: 0x0400017D RID: 381
			public Color color;

			// Token: 0x0400017E RID: 382
			public int h_align;

			// Token: 0x0400017F RID: 383
			public int v_align;
		}

		// Token: 0x02000042 RID: 66
		public struct ImageInfo
		{
			// Token: 0x04000180 RID: 384
			public int Width;

			// Token: 0x04000181 RID: 385
			public int Height;

			// Token: 0x04000182 RID: 386
			public int FrameCount;

			// Token: 0x04000183 RID: 387
			public int[] FrameDelay;

			// Token: 0x04000184 RID: 388
			public bool IsAnimated;

			// Token: 0x04000185 RID: 389
			public bool IsLooped;

			// Token: 0x04000186 RID: 390
			public int AnimationLength;

			// Token: 0x04000187 RID: 391
			public int newWidth;

			// Token: 0x04000188 RID: 392
			public int newHeight;

			// Token: 0x04000189 RID: 393
			public bool IsMatrixDefault;

			// Token: 0x0400018A RID: 394
			public int default_Width;

			// Token: 0x0400018B RID: 395
			public int default_Height;
		}

		// Token: 0x02000043 RID: 67
		public struct SYSTEM_POWER_STATUS
		{
			// Token: 0x0400018C RID: 396
			public byte ACLineStatus;

			// Token: 0x0400018D RID: 397
			public byte BatteryFlag;

			// Token: 0x0400018E RID: 398
			public byte BatteryLifePercent;

			// Token: 0x0400018F RID: 399
			public byte Reserved1;

			// Token: 0x04000190 RID: 400
			public int BatteryLifeTime;

			// Token: 0x04000191 RID: 401
			public int BatteryFullLifeTime;
		}

		// Token: 0x02000044 RID: 68
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct DISPLAY_DEVICE
		{
			// Token: 0x04000192 RID: 402
			public int cb;

			// Token: 0x04000193 RID: 403
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string DeviceName;

			// Token: 0x04000194 RID: 404
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string DeviceString;

			// Token: 0x04000195 RID: 405
			public uint StateFlags;

			// Token: 0x04000196 RID: 406
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string DeviceID;

			// Token: 0x04000197 RID: 407
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string DeviceKey;
		}

		// Token: 0x02000045 RID: 69
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct DEVMODE
		{
			// Token: 0x04000198 RID: 408
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string dmDeviceName;

			// Token: 0x04000199 RID: 409
			public short dmSpecVersion;

			// Token: 0x0400019A RID: 410
			public short dmDriverVersion;

			// Token: 0x0400019B RID: 411
			public short dmSize;

			// Token: 0x0400019C RID: 412
			public short dmDriverExtra;

			// Token: 0x0400019D RID: 413
			public int dmFields;

			// Token: 0x0400019E RID: 414
			public int dmPositionX;

			// Token: 0x0400019F RID: 415
			public int dmPositionY;

			// Token: 0x040001A0 RID: 416
			public int dmDisplayOrientation;

			// Token: 0x040001A1 RID: 417
			public int dmDisplayFixedOutput;

			// Token: 0x040001A2 RID: 418
			public short dmColor;

			// Token: 0x040001A3 RID: 419
			public short dmDuplex;

			// Token: 0x040001A4 RID: 420
			public short dmYResolution;

			// Token: 0x040001A5 RID: 421
			public short dmTTOption;

			// Token: 0x040001A6 RID: 422
			public short dmCollate;

			// Token: 0x040001A7 RID: 423
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string dmFormName;

			// Token: 0x040001A8 RID: 424
			public short dmLogPixels;

			// Token: 0x040001A9 RID: 425
			public int dmBitsPerPel;

			// Token: 0x040001AA RID: 426
			public int dmPelsWidth;

			// Token: 0x040001AB RID: 427
			public int dmPelsHeight;

			// Token: 0x040001AC RID: 428
			public int dmDisplayFlags;

			// Token: 0x040001AD RID: 429
			public int dmDisplayFrequency;

			// Token: 0x040001AE RID: 430
			public int dmICMMethod;

			// Token: 0x040001AF RID: 431
			public int dmICMIntent;

			// Token: 0x040001B0 RID: 432
			public int dmMediaType;

			// Token: 0x040001B1 RID: 433
			public int dmDitherType;

			// Token: 0x040001B2 RID: 434
			public int dmReserved1;

			// Token: 0x040001B3 RID: 435
			public int dmReserved2;

			// Token: 0x040001B4 RID: 436
			public int dmPanningWidth;

			// Token: 0x040001B5 RID: 437
			public int dmPanningHeight;
		}

		// Token: 0x02000046 RID: 70
		public class StreamString
		{
			// Token: 0x0600030A RID: 778 RVA: 0x00017841 File Offset: 0x00015A41
			public StreamString(Stream ioStream)
			{
				this.ioStream = ioStream;
				this.streamEncoding = new UnicodeEncoding();
			}

			// Token: 0x0600030B RID: 779 RVA: 0x0001785C File Offset: 0x00015A5C
			public string ReadString()
			{
				int num = this.ioStream.ReadByte() * 256;
				num += this.ioStream.ReadByte();
				byte[] array = new byte[num];
				this.ioStream.Read(array, 0, num);
				return this.streamEncoding.GetString(array);
			}

			// Token: 0x0600030C RID: 780 RVA: 0x000178B0 File Offset: 0x00015AB0
			public int WriteString(string outString)
			{
				byte[] bytes = this.streamEncoding.GetBytes(outString);
				int num = bytes.Length;
				if (num > 65535)
				{
					num = 65535;
				}
				this.ioStream.WriteByte((byte)(num / 256));
				this.ioStream.WriteByte((byte)(num & 255));
				this.ioStream.Write(bytes, 0, num);
				this.ioStream.Flush();
				return bytes.Length + 2;
			}

			// Token: 0x040001B6 RID: 438
			private Stream ioStream;

			// Token: 0x040001B7 RID: 439
			private UnicodeEncoding streamEncoding;
		}

		// Token: 0x02000047 RID: 71
		private class PipeClient
		{
			// Token: 0x0600030D RID: 781 RVA: 0x00017920 File Offset: 0x00015B20
			public static void EndServer()
			{
				LOGGER.DEBUG("[ScreenCapture] EndServer()+", Array.Empty<object>());
				try
				{
					NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", AuraLayer.PipeClient.capPipeID, PipeDirection.InOut, PipeOptions.Asynchronous);
					namedPipeClientStream.Connect(1000);
					using (StreamWriter streamWriter = new StreamWriter(namedPipeClientStream))
					{
						streamWriter.WriteLine("LM_SUPPORT_ENDSERVER");
					}
				}
				catch (Exception ex)
				{
					LOGGER.DEBUG("[ScreenCapture] EndServer() Exception: " + ex.Message, Array.Empty<object>());
				}
				LOGGER.DEBUG("[ScreenCapture] EndServer()-", Array.Empty<object>());
			}

			// Token: 0x0600030E RID: 782 RVA: 0x000179C4 File Offset: 0x00015BC4
			public static Bitmap CaptureScreen(int x, int y, int w, int h, int TimeOut)
			{
				Bitmap result = null;
				try
				{
					using (new Profiling(string.Concat(new string[]
					{
						MethodBase.GetCurrentMethod().Name,
						",",
						x.ToString(),
						",",
						y.ToString(),
						",",
						w.ToString(),
						",",
						h.ToString()
					})))
					{
						NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", AuraLayer.PipeClient.capPipeID, PipeDirection.InOut, PipeOptions.Asynchronous);
						namedPipeClientStream.Connect(TimeOut);
						AuraLayer.StreamString streamString = new AuraLayer.StreamString(namedPipeClientStream);
						streamString.WriteString("LM_SUPPORT");
						streamString.WriteString(string.Format(CultureInfo.InvariantCulture, "CAPTURESCREEN/{0}/{1}/{2}/{3}", new object[]
						{
							x,
							y,
							w,
							h
						}));
						result = new Bitmap(new BinaryFormatter().Deserialize(namedPipeClientStream) as MemoryStream);
						streamString.WriteString("LM_SUPPORT_END");
						namedPipeClientStream.Close();
						namedPipeClientStream.Dispose();
					}
				}
				catch (Exception ex)
				{
					LOGGER.DEBUG("[ScreenCapture] CaptureScreen() Exception: " + ex.Message, Array.Empty<object>());
				}
				return result;
			}

			// Token: 0x0600030F RID: 783 RVA: 0x00017B3C File Offset: 0x00015D3C
			public static Bitmap CaptureScreen2(int x, int y, int w, int h, int dw, int dh, int TimeOut)
			{
				Bitmap bitmap = null;
				try
				{
					object shmClientLock = AuraLayer._shmClientLock;
					SharedMemoryClient shmClient;
					lock (shmClientLock)
					{
						if (AuraLayer._shmClient == null)
						{
							AuraLayer._shmClient = new SharedMemoryClient();
						}
						if (!AuraLayer._shmClient.IsConnected)
						{
							if (!AuraLayer._shmClient.TryConnect())
							{
								return null;
							}
							LOGGER.DEBUG(string.Format("[ScreenCapture][COORD] SharedMemory connected: origin=({0},{1}) size=({2},{3})", new object[]
							{
								AuraLayer._shmClient.OriginX,
								AuraLayer._shmClient.OriginY,
								AuraLayer._shmClient.Width,
								AuraLayer._shmClient.Height
							}), Array.Empty<object>());
						}
						shmClient = AuraLayer._shmClient;
					}
					if (!shmClient.WaitForFrame(TimeOut))
					{
						return null;
					}
					int width = shmClient.Width;
					int height = shmClient.Height;
					if (width <= 0 || height <= 0)
					{
						return null;
					}
					int num = Math.Max(0, Math.Min(x, width - 1));
					int num2 = Math.Max(0, Math.Min(y, height - 1));
					int num3 = Math.Min(w, width - num);
					int num4 = Math.Min(h, height - num2);
					if (AuraLayer.PipeClient._coordLogCount == 0L)
					{
						LOGGER.DEBUG(string.Format("[ScreenCapture][COORD] CaptureScreen2: input=({0},{1},{2},{3}) bufferSize=({4},{5}) -> bx,by=({6},{7}) -> clamped sx,sy=({8},{9}) sw,sh=({10},{11}) dest=({12},{13})", new object[]
						{
							x,
							y,
							w,
							h,
							width,
							height,
							x,
							y,
							num,
							num2,
							num3,
							num4,
							dw,
							dh
						}), Array.Empty<object>());
						if (x != num || y != num2)
						{
							LOGGER.WARN(string.Format("[ScreenCapture][COORD] CLAMP OCCURRED: bx={0}->sx={1}, by={2}->sy={3} (possible misalignment!)", new object[]
							{
								x,
								num,
								y,
								num2
							}), Array.Empty<object>());
						}
					}
					AuraLayer.PipeClient._coordLogCount += 1L;
					if (num3 <= 0 || num4 <= 0)
					{
						return null;
					}
					bitmap = new Bitmap(dw, dh, PixelFormat.Format32bppArgb);
					BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, dw, dh), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
					try
					{
						shmClient.SampleRect(num, num2, num3, num4, bitmapData.Scan0, bitmapData.Stride, dw, dh);
					}
					finally
					{
						bitmap.UnlockBits(bitmapData);
					}
				}
				catch (Exception ex)
				{
					LOGGER.DEBUG("[ScreenCapture] CaptureScreen() Exception: " + ex.Message, Array.Empty<object>());
					AuraLayer.CreateSupportProcess();
				}
				return bitmap;
			}

			// Token: 0x06000310 RID: 784 RVA: 0x00017E60 File Offset: 0x00016060
			public static Bitmap CaptureFullScreen(int TimeOut)
			{
				Bitmap bitmap = null;
				try
				{
					object shmClientLock = AuraLayer._shmClientLock;
					SharedMemoryClient shmClient;
					lock (shmClientLock)
					{
						if (AuraLayer._shmClient == null)
						{
							AuraLayer._shmClient = new SharedMemoryClient();
						}
						if (!AuraLayer._shmClient.IsConnected && !AuraLayer._shmClient.TryConnect())
						{
							return null;
						}
						shmClient = AuraLayer._shmClient;
					}
					if (!shmClient.WaitForFrame(TimeOut))
					{
						return null;
					}
					int width = shmClient.Width;
					int height = shmClient.Height;
					if (width <= 0 || height <= 0)
					{
						return null;
					}
					bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
					BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
					try
					{
						shmClient.CopyRect(0, 0, width, height, bitmapData.Scan0, bitmapData.Stride);
					}
					finally
					{
						bitmap.UnlockBits(bitmapData);
					}
				}
				catch (Exception ex)
				{
					LOGGER.DEBUG("[ScreenCapture] CaptureFullScreen() Exception: " + ex.Message, Array.Empty<object>());
				}
				return bitmap;
			}

			// Token: 0x06000311 RID: 785
			[DllImport("user32.dll", CharSet = CharSet.Auto)]
			private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref AuraLayer.PipeClient.DEVMODE devMode);

			// Token: 0x06000312 RID: 786 RVA: 0x00017F88 File Offset: 0x00016188
			public static float GetPrimaryDpiScale()
			{
				try
				{
					Screen primaryScreen = Screen.PrimaryScreen;
					if (primaryScreen == null)
					{
						return 1f;
					}
					AuraLayer.PipeClient.DEVMODE devmode = default(AuraLayer.PipeClient.DEVMODE);
					devmode.dmSize = (short)Marshal.SizeOf(typeof(AuraLayer.PipeClient.DEVMODE));
					if (AuraLayer.PipeClient.EnumDisplaySettings(primaryScreen.DeviceName, -1, ref devmode) && devmode.dmPelsWidth > 0 && primaryScreen.Bounds.Width > 0)
					{
						float num = (float)devmode.dmPelsWidth / (float)primaryScreen.Bounds.Width;
						if (num >= 1f)
						{
							return num;
						}
					}
				}
				catch
				{
				}
				return 1f;
			}

			// Token: 0x040001B8 RID: 440
			private static string capPipeID = "LMCAP";

			// Token: 0x040001B9 RID: 441
			private static long _coordLogCount = 0L;

			// Token: 0x040001BA RID: 442
			private const int ENUM_CURRENT_SETTINGS = -1;

			// Token: 0x02000061 RID: 97
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			private struct DEVMODE
			{
				// Token: 0x04000250 RID: 592
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
				public string dmDeviceName;

				// Token: 0x04000251 RID: 593
				public short dmSpecVersion;

				// Token: 0x04000252 RID: 594
				public short dmDriverVersion;

				// Token: 0x04000253 RID: 595
				public short dmSize;

				// Token: 0x04000254 RID: 596
				public short dmDriverExtra;

				// Token: 0x04000255 RID: 597
				public int dmFields;

				// Token: 0x04000256 RID: 598
				public int dmPositionX;

				// Token: 0x04000257 RID: 599
				public int dmPositionY;

				// Token: 0x04000258 RID: 600
				public int dmDisplayOrientation;

				// Token: 0x04000259 RID: 601
				public int dmDisplayFixedOutput;

				// Token: 0x0400025A RID: 602
				public short dmColor;

				// Token: 0x0400025B RID: 603
				public short dmDuplex;

				// Token: 0x0400025C RID: 604
				public short dmYResolution;

				// Token: 0x0400025D RID: 605
				public short dmTTOption;

				// Token: 0x0400025E RID: 606
				public short dmCollate;

				// Token: 0x0400025F RID: 607
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
				public string dmFormName;

				// Token: 0x04000260 RID: 608
				public short dmLogPixels;

				// Token: 0x04000261 RID: 609
				public int dmBitsPerPel;

				// Token: 0x04000262 RID: 610
				public int dmPelsWidth;

				// Token: 0x04000263 RID: 611
				public int dmPelsHeight;

				// Token: 0x04000264 RID: 612
				public int dmDisplayFlags;

				// Token: 0x04000265 RID: 613
				public int dmDisplayFrequency;

				// Token: 0x04000266 RID: 614
				public int dmICMMethod;

				// Token: 0x04000267 RID: 615
				public int dmICMIntent;

				// Token: 0x04000268 RID: 616
				public int dmMediaType;

				// Token: 0x04000269 RID: 617
				public int dmDitherType;

				// Token: 0x0400026A RID: 618
				public int dmReserved1;

				// Token: 0x0400026B RID: 619
				public int dmReserved2;

				// Token: 0x0400026C RID: 620
				public int dmPanningWidth;

				// Token: 0x0400026D RID: 621
				public int dmPanningHeight;
			}
		}

		// Token: 0x02000048 RID: 72
		private class GDI32
		{
			// Token: 0x06000315 RID: 789
			[DllImport("gdi32.dll")]
			public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);

			// Token: 0x06000316 RID: 790
			[DllImport("gdi32.dll")]
			public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

			// Token: 0x06000317 RID: 791
			[DllImport("gdi32.dll")]
			public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

			// Token: 0x06000318 RID: 792
			[DllImport("gdi32.dll")]
			public static extern bool DeleteDC(IntPtr hDC);

			// Token: 0x06000319 RID: 793
			[DllImport("gdi32.dll")]
			public static extern bool DeleteObject(IntPtr hObject);

			// Token: 0x0600031A RID: 794
			[DllImport("gdi32.dll")]
			public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

			// Token: 0x040001BB RID: 443
			public const int SRCCOPY = 13369376;
		}

		// Token: 0x02000049 RID: 73
		private class User32
		{
			// Token: 0x0600031C RID: 796
			[DllImport("user32.dll")]
			public static extern IntPtr GetDesktopWindow();

			// Token: 0x0600031D RID: 797
			[DllImport("user32.dll")]
			public static extern IntPtr GetWindowDC(IntPtr hWnd);

			// Token: 0x0600031E RID: 798
			[DllImport("user32.dll")]
			public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

			// Token: 0x0600031F RID: 799
			[DllImport("user32.dll")]
			public static extern IntPtr GetWindowRect(IntPtr hWnd, ref AuraLayer.User32.RECT rect);

			// Token: 0x02000062 RID: 98
			public struct RECT
			{
				// Token: 0x0400026E RID: 622
				public int left;

				// Token: 0x0400026F RID: 623
				public int top;

				// Token: 0x04000270 RID: 624
				public int right;

				// Token: 0x04000271 RID: 625
				public int bottom;
			}
		}

		// Token: 0x0200004A RID: 74
		public struct ColorRGB
		{
			// Token: 0x06000321 RID: 801 RVA: 0x0001805F File Offset: 0x0001625F
			public ColorRGB(Color value)
			{
				this.R = value.R;
				this.G = value.G;
				this.B = value.B;
			}

			// Token: 0x06000322 RID: 802 RVA: 0x00018088 File Offset: 0x00016288
			public static implicit operator Color(AuraLayer.ColorRGB rgb)
			{
				return Color.FromArgb((int)rgb.R, (int)rgb.G, (int)rgb.B);
			}

			// Token: 0x06000323 RID: 803 RVA: 0x000180A1 File Offset: 0x000162A1
			public static explicit operator AuraLayer.ColorRGB(Color c)
			{
				return new AuraLayer.ColorRGB(c);
			}

			// Token: 0x040001BC RID: 444
			public byte R;

			// Token: 0x040001BD RID: 445
			public byte G;

			// Token: 0x040001BE RID: 446
			public byte B;
		}

		// Token: 0x0200004B RID: 75
		public class SlashLighting
		{
			// Token: 0x17000070 RID: 112
			public int this[int index]
			{
				get
				{
					return this.data[index];
				}
				set
				{
					this.data[index] = value;
				}
			}

			// Token: 0x06000326 RID: 806 RVA: 0x000180C0 File Offset: 0x000162C0
			public Bitmap ToBitmap()
			{
				Bitmap bitmap = new Bitmap(AuraLayer.SLASHLIGHTING_COUNT, 1);
				for (int i = 0; i < AuraLayer.SLASHLIGHTING_COUNT; i++)
				{
					bitmap.SetPixel(i, 0, Color.FromArgb(this.data[i] % 256, this.data[i] % 256, this.data[i] % 256));
				}
				return bitmap;
			}

			// Token: 0x06000327 RID: 807 RVA: 0x00018121 File Offset: 0x00016321
			public override string ToString()
			{
				return string.Join<int>(", ", this.data) + ",";
			}

			// Token: 0x06000328 RID: 808 RVA: 0x00018140 File Offset: 0x00016340
			public static AuraLayer.SlashLighting FromString(string input)
			{
				string[] array = (from s in input.Split(new char[]
				{
					','
				})
				where !string.IsNullOrEmpty(s)
				select s).ToArray<string>();
				AuraLayer.SlashLighting slashLighting = new AuraLayer.SlashLighting();
				if (array.Length < AuraLayer.SLASHLIGHTING_COUNT)
				{
					int num = AuraLayer.SLASHLIGHTING_COUNT / array.Length;
					if (num == 0)
					{
						num = 1;
					}
					for (int i = 0; i < AuraLayer.SLASHLIGHTING_COUNT; i++)
					{
						slashLighting[i] = int.Parse(array[i / num].Trim(), CultureInfo.InvariantCulture);
					}
				}
				else if (array.Length > AuraLayer.SLASHLIGHTING_COUNT)
				{
					int num2 = array.Length / AuraLayer.SLASHLIGHTING_COUNT;
					if (num2 == 0)
					{
						num2 = 1;
					}
					for (int j = 0; j < AuraLayer.SLASHLIGHTING_COUNT; j++)
					{
						slashLighting[j] = int.Parse(array[j * num2].Trim(), CultureInfo.InvariantCulture);
					}
				}
				else
				{
					for (int k = 0; k < AuraLayer.SLASHLIGHTING_COUNT; k++)
					{
						slashLighting[k] = int.Parse(array[k].Trim(), CultureInfo.InvariantCulture);
					}
				}
				return slashLighting;
			}

			// Token: 0x040001BF RID: 447
			private int[] data = new int[AuraLayer.SLASHLIGHTING_COUNT];
		}

		// Token: 0x0200004C RID: 76
		public abstract class SlashLightingEffect
		{
			// Token: 0x17000071 RID: 113
			// (get) Token: 0x0600032A RID: 810
			// (set) Token: 0x0600032B RID: 811
			public abstract bool IsEnded { get; set; }

			// Token: 0x17000072 RID: 114
			// (get) Token: 0x0600032C RID: 812
			// (set) Token: 0x0600032D RID: 813
			public abstract bool IsEndless { get; set; }

			// Token: 0x17000073 RID: 115
			// (get) Token: 0x0600032E RID: 814
			// (set) Token: 0x0600032F RID: 815
			public abstract int Priority { get; set; }

			// Token: 0x06000330 RID: 816
			public abstract Bitmap NextFrame();

			// Token: 0x06000331 RID: 817
			public new abstract void Finalize();
		}

		// Token: 0x0200004D RID: 77
		public class PlayGlyph : AuraLayer.SlashLightingEffect
		{
			// Token: 0x17000074 RID: 116
			// (get) Token: 0x06000333 RID: 819 RVA: 0x00018274 File Offset: 0x00016474
			// (set) Token: 0x06000334 RID: 820 RVA: 0x0001827C File Offset: 0x0001647C
			public override bool IsEnded
			{
				get
				{
					return this._isEnded;
				}
				set
				{
					this._isEnded = value;
				}
			}

			// Token: 0x17000075 RID: 117
			// (get) Token: 0x06000335 RID: 821 RVA: 0x00018285 File Offset: 0x00016485
			// (set) Token: 0x06000336 RID: 822 RVA: 0x0001828D File Offset: 0x0001648D
			public override bool IsEndless
			{
				get
				{
					return this._isendless;
				}
				set
				{
					this._isendless = value;
				}
			}

			// Token: 0x17000076 RID: 118
			// (get) Token: 0x06000337 RID: 823 RVA: 0x00018296 File Offset: 0x00016496
			// (set) Token: 0x06000338 RID: 824 RVA: 0x0001829E File Offset: 0x0001649E
			public override int Priority
			{
				get
				{
					return this._priority;
				}
				set
				{
					this._priority = value;
				}
			}

			// Token: 0x06000339 RID: 825 RVA: 0x000182A8 File Offset: 0x000164A8
			public static byte CalculateAverage(List<byte> numbers)
			{
				if (numbers.Count == 0)
				{
					throw new ArgumentException("Input array cannot be empty.");
				}
				int num = 0;
				using (List<byte>.Enumerator enumerator = numbers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						int num2 = (int)enumerator.Current;
						num += num2;
					}
				}
				return (byte)(num / numbers.Count);
			}

			// Token: 0x0600033A RID: 826 RVA: 0x00018314 File Offset: 0x00016514
			public override Bitmap NextFrame()
			{
				int num = AuraLayer.SLASHLIGHTING_COUNT;
				new RectangleF[2048];
				int slashlighting_COUNT = AuraLayer.SLASHLIGHTING_COUNT;
				int num2 = 1;
				if (this.analyzer == null)
				{
					return null;
				}
				this.lastBitmap = (Bitmap)this.StaticBitmap.Clone();
				using (Graphics graphics = Graphics.FromImage(this.StaticBitmap))
				{
					graphics.Clear(Color.FromArgb(0, 0, 0, 0));
					int spectrumType = this.SpectrumType;
					if (spectrumType <= 23)
					{
						if (spectrumType == 22)
						{
							num = 3;
							this.analyzer.SetLines(num);
							int num3 = (int)this.analyzer.GetAverageSpectrumData();
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), new RectangleF(0f, 0f, (float)(slashlighting_COUNT * num3 / 255), 1f));
							goto IL_8BF;
						}
						if (spectrumType == 23)
						{
							num = 3;
							this.analyzer.SetLines(num);
							int num4 = (int)this.analyzer.GetAverageSpectrumData();
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), new RectangleF((float)(slashlighting_COUNT - slashlighting_COUNT * num4 / 255), 0f, (float)(slashlighting_COUNT * num4 / 255), 1f));
							goto IL_8BF;
						}
					}
					else
					{
						if (spectrumType == 31)
						{
							num = 3;
							this.analyzer.SetLines(num);
							int num5 = (int)this.analyzer.GetAverageSpectrumData();
							graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), new RectangleF((float)((slashlighting_COUNT - slashlighting_COUNT * num5 / 255) / 2), 0f, (float)(slashlighting_COUNT * num5 / 255), 1f));
							goto IL_8BF;
						}
						switch (spectrumType)
						{
						case 2001:
						{
							byte[] item = this.analyzer.UpdateSpectrumData2().Item2;
							for (int i = 0; i < this.slashlighting_audioeffect_mic.Length; i++)
							{
								this.slashlighting_audioeffect_mic[i] = 0;
							}
							for (int j = this.effect2001_List.Count - 1; j >= 0; j--)
							{
								int num6 = this.effect2001_List[j] + 1;
								if (this.effect2001_Location[num6] == -1)
								{
									this.effect2001_List.Remove(this.effect2001_List[j]);
								}
								else
								{
									this.slashlighting_audioeffect_mic[this.effect2001_Location[num6]] = Math.Max((byte)this.effect2001_Gray[num6], this.slashlighting_audioeffect_mic[this.effect2001_Location[num6]]);
									this.effect2001_List[j] = (int)((byte)num6);
								}
							}
							this.effect2001_activecount = (this.effect2001_activecount + 1) % 4;
							if (this.effect2001_activecount == 0)
							{
								if (this.Gamma06LUT[(int)((byte)item.Average((byte x) => (int)x))] >= 100)
								{
									this.slashlighting_audioeffect_mic[0] = byte.MaxValue;
									this.effect2001_List.Add(0);
								}
							}
							for (int k = 0; k < this.slashlighting_audioeffect_mic.Length / 2; k++)
							{
								this.slashlighting_audioeffect_mic[this.slashlighting_audioeffect_mic.Length / 2 + k] = this.slashlighting_audioeffect_mic[this.slashlighting_audioeffect_mic.Length / 2 - k];
							}
							for (int l = 0; l < this.slashlighting_audioeffect_mic.Length; l++)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.slashlighting_audioeffect_mic[l], (int)this.slashlighting_audioeffect_mic[l], (int)this.slashlighting_audioeffect_mic[l])), new RectangleF((float)l, 0f, 1f, 1f));
							}
							goto IL_8BF;
						}
						case 2002:
						{
							byte[] item2 = this.analyzer.UpdateSpectrumData2().Item1;
							int num7 = (int)this.Gamma02LUT[(int)((byte)item2.Average((byte x) => (int)x))] * this.slashlighting_audioeffect_sys.Length / 2 / 255;
							byte[] currentSinFrame = this.animator.GetCurrentSinFrame();
							for (int m = 0; m < this.slashlighting_audioeffect_sys.Length / 2; m++)
							{
								this.slashlighting_audioeffect_sys[this.slashlighting_audioeffect_sys.Length / 2 - m] = Math.Min((m <= num7) ? byte.MaxValue : 0, currentSinFrame[m]);
								this.slashlighting_audioeffect_sys[this.slashlighting_audioeffect_sys.Length / 2 + m] = this.slashlighting_audioeffect_sys[this.slashlighting_audioeffect_sys.Length / 2 - m];
							}
							for (int n = 0; n < this.slashlighting_audioeffect_sys.Length; n++)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)this.slashlighting_audioeffect_sys[n], (int)this.slashlighting_audioeffect_sys[n], (int)this.slashlighting_audioeffect_sys[n])), new RectangleF((float)n, 0f, 1f, 1f));
							}
							goto IL_8BF;
						}
						case 2003:
						{
							ValueTuple<byte[], byte[]> valueTuple = this.analyzer.UpdateSpectrumData2();
							byte[] item3 = valueTuple.Item1;
							byte[] item4 = valueTuple.Item2;
							int num8 = (int)this.Gamma02LUT[(int)((byte)item3.Average((byte x) => (int)x))] * this.slashlighting_audioeffect_sys.Length / 2 / 255;
							byte[] currentSinFrame2 = this.animator.GetCurrentSinFrame();
							for (int num9 = 0; num9 < this.slashlighting_audioeffect_sys.Length / 2; num9++)
							{
								this.slashlighting_audioeffect_sys[this.slashlighting_audioeffect_sys.Length / 2 - num9] = Math.Min((num9 <= num8) ? byte.MaxValue : 0, currentSinFrame2[num9]);
								this.slashlighting_audioeffect_sys[this.slashlighting_audioeffect_sys.Length / 2 + num9] = this.slashlighting_audioeffect_sys[this.slashlighting_audioeffect_sys.Length / 2 - num9];
							}
							for (int num10 = 0; num10 < this.slashlighting_audioeffect_mic.Length; num10++)
							{
								this.slashlighting_audioeffect_mic[num10] = 0;
							}
							for (int num11 = this.effect2001_List.Count - 1; num11 >= 0; num11--)
							{
								int num12 = this.effect2001_List[num11] + 1;
								if (this.effect2001_Location[num12] == -1)
								{
									this.effect2001_List.Remove(this.effect2001_List[num11]);
								}
								else
								{
									this.slashlighting_audioeffect_mic[this.effect2001_Location[num12]] = Math.Max((byte)this.effect2001_Gray[num12], this.slashlighting_audioeffect_mic[this.effect2001_Location[num12]]);
									this.effect2001_List[num11] = (int)((byte)num12);
								}
							}
							this.effect2001_activecount = (this.effect2001_activecount + 1) % 4;
							if (this.effect2001_activecount == 0)
							{
								if (this.Gamma06LUT[(int)((byte)item4.Average((byte x) => (int)x))] >= 100)
								{
									this.slashlighting_audioeffect_mic[0] = byte.MaxValue;
									this.effect2001_List.Add(0);
								}
							}
							for (int num13 = 0; num13 < this.slashlighting_audioeffect_mic.Length / 2; num13++)
							{
								this.slashlighting_audioeffect_mic[this.slashlighting_audioeffect_mic.Length / 2 + num13] = this.slashlighting_audioeffect_mic[this.slashlighting_audioeffect_mic.Length / 2 - num13];
							}
							for (int num14 = 0; num14 < this.slashlighting_audioeffect_sys.Length; num14++)
							{
								graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, (int)Math.Max(this.slashlighting_audioeffect_sys[num14], this.slashlighting_audioeffect_mic[num14]), (int)Math.Max(this.slashlighting_audioeffect_sys[num14], this.slashlighting_audioeffect_mic[num14]), (int)Math.Max(this.slashlighting_audioeffect_sys[num14], this.slashlighting_audioeffect_mic[num14]))), new RectangleF((float)num14, 0f, 1f, 1f));
							}
							goto IL_8BF;
						}
						}
					}
					num = AuraLayer.SLASHLIGHTING_COUNT;
					this.analyzer.SetLines(this.IsFullRange ? num : (num * 2));
					byte[] array = this.analyzer.UpdateSpectrumData();
					if (array.Length == 0)
					{
						return AuraLayer.SetBitmapBrightness(this.StaticBitmap, this.Brightness);
					}
					for (int num15 = 0; num15 < array.Count<byte>(); num15++)
					{
						array[num15] = this.Gamma18LUT[(int)array[num15]];
					}
					for (int num16 = 0; num16 < num; num16++)
					{
						graphics.FillRectangle(new SolidBrush(Color.FromArgb((int)array[num16 + this.RangeIndex], 255, 255, 255)), new RectangleF((float)(num16 * slashlighting_COUNT / num), 0f, (float)(slashlighting_COUNT / num), (float)(num2 * (int)array[num16] / 255)));
					}
					for (int num17 = 0; num17 < num; num17++)
					{
						graphics.FillRectangle(new SolidBrush(Color.FromArgb((int)array[num17 + this.RangeIndex], 255, 255, 255)), new RectangleF((float)num17, 0f, 1f, 1f));
					}
					IL_8BF:
					if (this.Decay != 0f)
					{
						float[][] array2 = new float[5][];
						int num18 = 0;
						float[] array3 = new float[5];
						array3[0] = 1f;
						array2[num18] = array3;
						int num19 = 1;
						float[] array4 = new float[5];
						array4[1] = 1f;
						array2[num19] = array4;
						int num20 = 2;
						float[] array5 = new float[5];
						array5[2] = 1f;
						array2[num20] = array5;
						int num21 = 3;
						float[] array6 = new float[5];
						array6[3] = this.Decay;
						array2[num21] = array6;
						array2[4] = new float[]
						{
							0f,
							0f,
							0f,
							0f,
							1f
						};
						ColorMatrix newColorMatrix = new ColorMatrix(array2);
						ImageAttributes imageAttributes = new ImageAttributes();
						imageAttributes.SetColorMatrix(newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
						graphics.DrawImage(this.lastBitmap, new Rectangle(0, 0, this.StaticBitmap.Width, this.StaticBitmap.Height), 0f, 0f, (float)this.StaticBitmap.Width, (float)this.StaticBitmap.Height, GraphicsUnit.Pixel, imageAttributes);
					}
					graphics.Flush();
				}
				return AuraLayer.SetBitmapBrightness(this.StaticBitmap, this.Brightness);
			}

			// Token: 0x0600033B RID: 827 RVA: 0x00018CF8 File Offset: 0x00016EF8
			public override void Finalize()
			{
				this.analyzer.Enable2(false, this.DevName);
			}

			// Token: 0x0600033C RID: 828 RVA: 0x00018D0C File Offset: 0x00016F0C
			private int RangeConv(int r)
			{
				if (this.IsFullRange)
				{
					return 0;
				}
				int result;
				if (r == 0)
				{
					result = 0;
				}
				else if (r == 1)
				{
					result = 3;
				}
				else
				{
					result = 7;
				}
				return result;
			}

			// Token: 0x0600033D RID: 829 RVA: 0x00018D38 File Offset: 0x00016F38
			public PlayGlyph(string dev, int priority, int type, int decay, int strength, bool isFullRange, int rangeIndex, int brightness)
			{
				this.Strength = strength;
				if (strength < -100)
				{
					this.Strength = -100;
				}
				this.Decay = (float)decay / 100f;
				if (decay > 99)
				{
					this.Decay = 0.9f;
				}
				if (decay < 0)
				{
					this.Decay = 0f;
				}
				this.DevName = dev;
				this.Brightness = brightness;
				this.Priority = priority;
				this.IsFullRange = isFullRange;
				this.RangeIndex = this.RangeConv(rangeIndex);
				this.lastBitmap = new Bitmap(AuraLayer.SLASHLIGHTING_COUNT, 1);
				this.StaticBitmap = new Bitmap(AuraLayer.SLASHLIGHTING_COUNT, 1);
				this.SpectrumType = type;
				this.animator = new AuraLayer.SineWaveAnimator(AuraLayer.SLASHLIGHTING_COUNT / 2, (double)(AuraLayer.SLASHLIGHTING_COUNT / 2), 0.5, 1.0, 0.0);
				this.animator.Direction = -1;
				this.animator.Start();
				this.analyzer = new AudioAnalyzer_NAudio();
				this.analyzer.Init();
				if (this.SpectrumType > 2000)
				{
					this.analyzer.SetAudioSource(true, true);
				}
				this.analyzer.Enable2(true, this.DevName);
				this.slashlighting_audioeffect_mic = new byte[AuraLayer.SLASHLIGHTING_COUNT];
				this.slashlighting_audioeffect_sys = new byte[AuraLayer.SLASHLIGHTING_COUNT];
				for (int i = 0; i < this.slashlighting_audioeffect_mic.Length; i++)
				{
					this.slashlighting_audioeffect_mic[i] = 0;
					this.slashlighting_audioeffect_sys[i] = 0;
				}
				this.Gamma02LUT = new byte[256];
				this.Gamma04LUT = new byte[256];
				this.Gamma06LUT = new byte[256];
				this.Gamma08LUT = new byte[256];
				this.Gamma10LUT = new byte[256];
				this.Gamma14LUT = new byte[256];
				this.Gamma18LUT = new byte[256];
				this.Gamma20LUT = new byte[256];
				this.Gamma22LUT = new byte[256];
				this.Gamma25LUT = new byte[256];
				for (int j = 0; j < 256; j++)
				{
					this.Gamma02LUT[j] = (byte)(255.0 * Math.Pow((double)j / 255.0, 0.2));
					this.Gamma04LUT[j] = (byte)(255.0 * Math.Pow((double)j / 255.0, 0.4));
					this.Gamma06LUT[j] = (byte)(255.0 * Math.Pow((double)j / 255.0, 0.6));
					this.Gamma08LUT[j] = (byte)(255.0 * Math.Pow((double)j / 255.0, 0.8));
					this.Gamma10LUT[j] = (byte)(255.0 * Math.Pow((double)j / 255.0, 1.0));
					this.Gamma14LUT[j] = (byte)(255.0 * Math.Pow((double)j / 255.0, 1.4));
					this.Gamma18LUT[j] = (byte)(255.0 * Math.Pow((double)j / 255.0, 1.8));
					this.Gamma20LUT[j] = (byte)(255.0 * Math.Pow((double)j / 255.0, 2.0));
					this.Gamma22LUT[j] = (byte)(255.0 * Math.Pow((double)j / 255.0, 2.2));
					this.Gamma25LUT[j] = (byte)(255.0 * Math.Pow((double)j / 255.0, 2.5));
				}
				if (type == 10)
				{
					if (this.IsFullRange)
					{
						this.analyzer.SetLines(AuraLayer.SLASHLIGHTING_COUNT);
						return;
					}
					this.analyzer.SetLines(AuraLayer.SLASHLIGHTING_COUNT * 2);
					return;
				}
				else
				{
					if (type == 22 || type == 23)
					{
						this.analyzer.SetLines(AuraLayer.SLASHLIGHTING_COUNT);
						return;
					}
					if (type == 31)
					{
						this.analyzer.SetLines(AuraLayer.SLASHLIGHTING_COUNT);
						return;
					}
					if (this.IsFullRange)
					{
						this.analyzer.SetLines(AuraLayer.SLASHLIGHTING_COUNT);
						return;
					}
					this.analyzer.SetLines(AuraLayer.SLASHLIGHTING_COUNT * 2);
					return;
				}
			}

			// Token: 0x040001C0 RID: 448
			private bool _isEnded;

			// Token: 0x040001C1 RID: 449
			private bool _isendless = true;

			// Token: 0x040001C2 RID: 450
			private int _priority = 4;

			// Token: 0x040001C3 RID: 451
			private float Decay;

			// Token: 0x040001C4 RID: 452
			private int Strength;

			// Token: 0x040001C5 RID: 453
			private int Brightness = 100;

			// Token: 0x040001C6 RID: 454
			private Bitmap lastBitmap;

			// Token: 0x040001C7 RID: 455
			private Bitmap StaticBitmap;

			// Token: 0x040001C8 RID: 456
			private string DevName = "";

			// Token: 0x040001C9 RID: 457
			private bool IsFullRange = true;

			// Token: 0x040001CA RID: 458
			private int RangeIndex;

			// Token: 0x040001CB RID: 459
			private byte[] Gamma02LUT;

			// Token: 0x040001CC RID: 460
			private byte[] Gamma04LUT;

			// Token: 0x040001CD RID: 461
			private byte[] Gamma06LUT;

			// Token: 0x040001CE RID: 462
			private byte[] Gamma08LUT;

			// Token: 0x040001CF RID: 463
			private byte[] Gamma10LUT;

			// Token: 0x040001D0 RID: 464
			private byte[] Gamma14LUT;

			// Token: 0x040001D1 RID: 465
			private byte[] Gamma18LUT;

			// Token: 0x040001D2 RID: 466
			private byte[] Gamma20LUT;

			// Token: 0x040001D3 RID: 467
			private byte[] Gamma22LUT;

			// Token: 0x040001D4 RID: 468
			private byte[] Gamma25LUT;

			// Token: 0x040001D5 RID: 469
			private byte[] slashlighting_audioeffect_mic;

			// Token: 0x040001D6 RID: 470
			private byte[] slashlighting_audioeffect_sys;

			// Token: 0x040001D7 RID: 471
			private int[] effect2001_Location = new int[]
			{
				0,
				2,
				8,
				10,
				12,
				13,
				14,
				15,
				16,
				16,
				17,
				17,
				17,
				17,
				-1
			};

			// Token: 0x040001D8 RID: 472
			private int[] effect2001_Gray = new int[]
			{
				255,
				255,
				255,
				255,
				255,
				255,
				255,
				255,
				255,
				255,
				204,
				153,
				102,
				51,
				0
			};

			// Token: 0x040001D9 RID: 473
			private int effect2001_activecount;

			// Token: 0x040001DA RID: 474
			private List<int> effect2001_List = new List<int>();

			// Token: 0x040001DB RID: 475
			private AudioAnalyzer_NAudio analyzer;

			// Token: 0x040001DC RID: 476
			private int SpectrumType;

			// Token: 0x040001DD RID: 477
			private AuraLayer.SineWaveAnimator animator;
		}

		// Token: 0x0200004E RID: 78
		public class Glyph : AuraLayer.SlashLightingEffect
		{
			// Token: 0x17000077 RID: 119
			// (get) Token: 0x0600033E RID: 830 RVA: 0x00019221 File Offset: 0x00017421
			// (set) Token: 0x0600033F RID: 831 RVA: 0x00019229 File Offset: 0x00017429
			public override bool IsEnded
			{
				get
				{
					return this._isEnded;
				}
				set
				{
					this._isEnded = value;
				}
			}

			// Token: 0x17000078 RID: 120
			// (get) Token: 0x06000340 RID: 832 RVA: 0x00019232 File Offset: 0x00017432
			// (set) Token: 0x06000341 RID: 833 RVA: 0x0001923A File Offset: 0x0001743A
			public override bool IsEndless
			{
				get
				{
					return this._isendless;
				}
				set
				{
					this._isendless = value;
				}
			}

			// Token: 0x17000079 RID: 121
			// (get) Token: 0x06000342 RID: 834 RVA: 0x00019243 File Offset: 0x00017443
			// (set) Token: 0x06000343 RID: 835 RVA: 0x0001924B File Offset: 0x0001744B
			public override int Priority
			{
				get
				{
					return this._priority;
				}
				set
				{
					this._priority = value;
				}
			}

			// Token: 0x06000344 RID: 836 RVA: 0x00019254 File Offset: 0x00017454
			private static void CopyEffect(AuraLayer.SlashLighting[] src, AuraLayer.SlashLighting[] dst, int srcIndex, int dstIndex, int length)
			{
				try
				{
					for (int i = 0; i < length; i++)
					{
						dst[dstIndex + i] = AuraLayer.SlashLighting.FromString(src[srcIndex + i].ToString());
					}
				}
				catch (Exception ex)
				{
					LOGGER.DEBUG("[SlashLighting] CopyEffect() Exception: " + ex.ToString(), Array.Empty<object>());
				}
			}

			// Token: 0x06000345 RID: 837 RVA: 0x000192B4 File Offset: 0x000174B4
			private static string MakeEmptyFrameString()
			{
				string str = "";
				for (int i = 0; i < AuraLayer.SLASHLIGHTING_COUNT - 1; i++)
				{
					str += "0,";
				}
				return str + "0";
			}

			// Token: 0x06000346 RID: 838 RVA: 0x000192F4 File Offset: 0x000174F4
			private static void MakeEmptyEffect(AuraLayer.SlashLighting[] dst, int dstIndex, int length)
			{
				for (int i = 0; i < length; i++)
				{
					dst[dstIndex + i] = AuraLayer.SlashLighting.FromString(AuraLayer.Glyph.MakeEmptyFrameString());
				}
			}

			// Token: 0x06000347 RID: 839 RVA: 0x0001931C File Offset: 0x0001751C
			private int GetIndexInsideEffect(int i)
			{
				if (i < 0)
				{
					return -1;
				}
				if (this.IsEndless)
				{
					if (i >= this.EffectFrameCount)
					{
						return -1;
					}
					return i;
				}
				else
				{
					if (i >= this.TotalFrameCount)
					{
						return -1;
					}
					int num = i % (this.EffectFrameCount + this.interFrameCount);
					if (num >= this.EffectFrameCount)
					{
						return -1;
					}
					return num;
				}
			}

			// Token: 0x06000348 RID: 840 RVA: 0x0001936C File Offset: 0x0001756C
			public override Bitmap NextFrame()
			{
				Bitmap bitmap;
				if (this.IsEndless)
				{
					this.CurrentIndex %= this.TotalFrameCount;
					bitmap = this.TotalEffect[this.CurrentIndex].ToBitmap();
				}
				else
				{
					if (this.CurrentIndex >= this.TotalFrameCount)
					{
						bitmap = null;
					}
					if (this.CurrentIndex == this.TotalFrameCount - 1)
					{
						this.Finalize();
						this.IsEnded = true;
					}
					bitmap = this.TotalEffect[this.CurrentIndex].ToBitmap();
				}
				if (this.WaveFileName != null)
				{
					try
					{
						if (bitmap != null && this.CurrentIndex == 0)
						{
							AuraLayer.WavePlayer.Play(this.WaveFileName, this);
						}
						else
						{
							int indexInsideEffect = this.GetIndexInsideEffect(this.CurrentIndex);
							if (indexInsideEffect < 0)
							{
								AuraLayer.WavePlayer.Stop();
							}
							else
							{
								AuraLayer.WavePlayer.PlayFromTimestamp(this.WaveFileName, TimeSpan.FromMilliseconds((double)(60 * indexInsideEffect)), this);
							}
						}
					}
					catch (Exception ex)
					{
						LOGGER.DEBUG("Play wave exception: " + ex.ToString(), Array.Empty<object>());
					}
				}
				this.CurrentIndex++;
				return AuraLayer.SetBitmapBrightness(bitmap, this.Brightness);
			}

			// Token: 0x06000349 RID: 841 RVA: 0x00019484 File Offset: 0x00017684
			public override void Finalize()
			{
			}

			// Token: 0x0600034A RID: 842 RVA: 0x00019488 File Offset: 0x00017688
			private void ContructEffect(string filename)
			{
				IniFile iniFile = new IniFile();
				iniFile.Load(filename, false);
				string value = iniFile["CONFIG"]["FrameCount"].ToString();
				this.EffectFrameCount = Convert.ToInt32(value);
				this.SingleEffect = new AuraLayer.SlashLighting[this.EffectFrameCount];
				foreach (KeyValuePair<string, IniSection> keyValuePair in iniFile)
				{
					if (keyValuePair.Key == "FRAMES")
					{
						foreach (KeyValuePair<string, IniValue> keyValuePair2 in keyValuePair.Value)
						{
							this.SingleEffect[Convert.ToInt32(keyValuePair2.Key.ToString())] = AuraLayer.SlashLighting.FromString(keyValuePair2.Value.ToString());
						}
					}
				}
				string input = AuraLayer.Glyph.MakeEmptyFrameString();
				for (int i = 0; i < this.EffectFrameCount; i++)
				{
					if (this.SingleEffect[i] != null)
					{
						input = this.SingleEffect[i].ToString();
					}
					else
					{
						this.SingleEffect[i] = AuraLayer.SlashLighting.FromString(input);
					}
				}
			}

			// Token: 0x0600034B RID: 843 RVA: 0x000195E4 File Offset: 0x000177E4
			public Glyph(string filename, int priority, bool isloop, int interval, int repeat_times, int brightness, string wavefilename = null)
			{
				this.Brightness = brightness;
				this._priority = priority;
				this._isendless = isloop;
				this.WaveFileName = wavefilename;
				try
				{
					this.ContructEffect(filename);
				}
				catch (Exception ex)
				{
					LOGGER.DEBUG("Read slashlighting file exception: " + ex.ToString(), Array.Empty<object>());
				}
				this.interFrameCount = (int)((double)interval * 16.6);
				if (isloop)
				{
					this.TotalFrameCount = this.EffectFrameCount + this.interFrameCount;
					this.TotalEffect = new AuraLayer.SlashLighting[this.TotalFrameCount];
					AuraLayer.Glyph.CopyEffect(this.SingleEffect, this.TotalEffect, 0, 0, this.SingleEffect.Length);
					AuraLayer.Glyph.MakeEmptyEffect(this.TotalEffect, this.SingleEffect.Length, this.interFrameCount);
					return;
				}
				this.TotalFrameCount = this.EffectFrameCount * repeat_times + this.interFrameCount * (repeat_times - 1);
				this.TotalEffect = new AuraLayer.SlashLighting[this.TotalFrameCount];
				for (int i = 0; i < repeat_times - 1; i++)
				{
					AuraLayer.Glyph.CopyEffect(this.SingleEffect, this.TotalEffect, 0, i * (this.SingleEffect.Length + this.interFrameCount), this.SingleEffect.Length);
					AuraLayer.Glyph.MakeEmptyEffect(this.TotalEffect, i * (this.SingleEffect.Length + this.interFrameCount) + this.SingleEffect.Length, this.interFrameCount);
				}
				AuraLayer.Glyph.CopyEffect(this.SingleEffect, this.TotalEffect, 0, (repeat_times - 1) * (this.SingleEffect.Length + this.interFrameCount), this.SingleEffect.Length);
			}

			// Token: 0x040001DE RID: 478
			private const int FrameInterval = 60;

			// Token: 0x040001DF RID: 479
			private bool _isEnded;

			// Token: 0x040001E0 RID: 480
			private bool _isendless;

			// Token: 0x040001E1 RID: 481
			private int _priority = 4;

			// Token: 0x040001E2 RID: 482
			public string WaveFileName;

			// Token: 0x040001E3 RID: 483
			private int CurrentIndex;

			// Token: 0x040001E4 RID: 484
			private int TotalFrameCount;

			// Token: 0x040001E5 RID: 485
			private int EffectFrameCount;

			// Token: 0x040001E6 RID: 486
			private int interFrameCount;

			// Token: 0x040001E7 RID: 487
			private AuraLayer.SlashLighting[] SingleEffect;

			// Token: 0x040001E8 RID: 488
			private AuraLayer.SlashLighting[] TotalEffect;

			// Token: 0x040001E9 RID: 489
			private int Brightness = 100;
		}

		// Token: 0x0200004F RID: 79
		public class WavePlayer
		{
			// Token: 0x0600034C RID: 844 RVA: 0x0001978C File Offset: 0x0001798C
			public static void Play(string filePath, AuraLayer.Glyph g)
			{
				AuraLayer.WavePlayer.Stop();
				AuraLayer.WavePlayer.currentGlyph = g;
				AuraLayer.WavePlayer.audioFile = new AudioFileReader(filePath);
				AuraLayer.WavePlayer.waveOut = new WaveOutEvent();
				AuraLayer.WavePlayer.waveOut.Init(AuraLayer.WavePlayer.audioFile);
				AuraLayer.WavePlayer.waveOut.Play();
			}

			// Token: 0x0600034D RID: 845 RVA: 0x000197C8 File Offset: 0x000179C8
			public static void Stop()
			{
				if (AuraLayer.WavePlayer.waveOut != null)
				{
					if (AuraLayer.WavePlayer.waveOut.PlaybackState == 1)
					{
						AuraLayer.WavePlayer.waveOut.Stop();
					}
					AuraLayer.WavePlayer.waveOut.Dispose();
					AuraLayer.WavePlayer.waveOut = null;
				}
				if (AuraLayer.WavePlayer.audioFile != null)
				{
					AuraLayer.WavePlayer.audioFile.Dispose();
					AuraLayer.WavePlayer.audioFile = null;
				}
			}

			// Token: 0x0600034E RID: 846 RVA: 0x0001981C File Offset: 0x00017A1C
			public static TimeSpan GetDuration(string filePath)
			{
				TimeSpan totalTime;
				using (AudioFileReader audioFileReader = new AudioFileReader(filePath))
				{
					totalTime = audioFileReader.TotalTime;
				}
				return totalTime;
			}

			// Token: 0x0600034F RID: 847 RVA: 0x00019854 File Offset: 0x00017A54
			public static void PlayFromTimestamp(string filePath, TimeSpan timestamp, AuraLayer.Glyph g)
			{
				if (AuraLayer.WavePlayer.currentGlyph == g && timestamp > AuraLayer.WavePlayer.lastTimestamp)
				{
					AuraLayer.WavePlayer.lastTimestamp = timestamp;
					return;
				}
				AuraLayer.WavePlayer.lastTimestamp = timestamp;
				AuraLayer.WavePlayer.Stop();
				AuraLayer.WavePlayer.audioFile = new AudioFileReader(filePath);
				AuraLayer.WavePlayer.waveOut = new WaveOutEvent();
				AuraLayer.WavePlayer.currentGlyph = g;
				AuraLayer.WavePlayer.audioFile.CurrentTime = timestamp;
				AuraLayer.WavePlayer.waveOut.Init(AuraLayer.WavePlayer.audioFile);
				AuraLayer.WavePlayer.waveOut.Play();
			}

			// Token: 0x040001EA RID: 490
			private static WaveOutEvent waveOut;

			// Token: 0x040001EB RID: 491
			private static AudioFileReader audioFile;

			// Token: 0x040001EC RID: 492
			private static AuraLayer.Glyph currentGlyph = null;

			// Token: 0x040001ED RID: 493
			private static TimeSpan lastTimestamp = TimeSpan.Zero;
		}

		// Token: 0x02000050 RID: 80
		public sealed class SineWaveAnimator
		{
			// Token: 0x1700007A RID: 122
			// (get) Token: 0x06000352 RID: 850 RVA: 0x000198E1 File Offset: 0x00017AE1
			// (set) Token: 0x06000353 RID: 851 RVA: 0x000198E9 File Offset: 0x00017AE9
			public int Direction { get; set; } = 1;

			// Token: 0x06000354 RID: 852 RVA: 0x000198F4 File Offset: 0x00017AF4
			public SineWaveAnimator(int length, double wavelength, double temporalFreqHz, double amplitude = 1.0, double basePhaseRad = 0.0)
			{
				if (length <= 0)
				{
					throw new ArgumentOutOfRangeException("length");
				}
				if (wavelength <= 0.0)
				{
					throw new ArgumentOutOfRangeException("wavelength");
				}
				if (amplitude < 0.0 || amplitude > 1.0)
				{
					throw new ArgumentOutOfRangeException("amplitude");
				}
				this.length = length;
				this.wavelength = wavelength;
				this.temporalFreqHz = temporalFreqHz;
				this.amplitude = amplitude;
				this.basePhaseRad = basePhaseRad;
				this.stopwatch = new Stopwatch();
			}

			// Token: 0x06000355 RID: 853 RVA: 0x00019989 File Offset: 0x00017B89
			public void Start()
			{
				this.stopwatch.Restart();
			}

			// Token: 0x06000356 RID: 854 RVA: 0x00019996 File Offset: 0x00017B96
			public void Stop()
			{
				this.stopwatch.Stop();
			}

			// Token: 0x06000357 RID: 855 RVA: 0x000199A4 File Offset: 0x00017BA4
			public byte[] GetCurrentSinFrame()
			{
				double totalSeconds = this.stopwatch.Elapsed.TotalSeconds;
				double phaseRad = this.basePhaseRad + (double)this.Direction * 2.0 * 3.1415926535897931 * this.temporalFreqHz * totalSeconds;
				return AuraLayer.SineWaveAnimator.BuildSineFrame(this.length, this.wavelength, phaseRad, this.amplitude);
			}

			// Token: 0x06000358 RID: 856 RVA: 0x00019A0C File Offset: 0x00017C0C
			private static byte[] BuildSineFrame(int length, double wavelength, double phaseRad, double amplitude)
			{
				byte[] array = new byte[length];
				double num = 6.2831853071795862 / wavelength;
				double num2 = amplitude * 127.0;
				for (int i = 0; i < length; i++)
				{
					int num3 = (int)Math.Round(Math.Sin(num * (double)i + phaseRad) * num2 + 128.0);
					num3 = Math.Max(0, Math.Min(255, num3));
					array[i] = (byte)num3;
				}
				return array;
			}

			// Token: 0x040001EE RID: 494
			private readonly int length;

			// Token: 0x040001EF RID: 495
			private readonly double wavelength;

			// Token: 0x040001F0 RID: 496
			private readonly double temporalFreqHz;

			// Token: 0x040001F1 RID: 497
			private readonly double amplitude;

			// Token: 0x040001F2 RID: 498
			private readonly double basePhaseRad;

			// Token: 0x040001F3 RID: 499
			private readonly Stopwatch stopwatch;
		}
	}
}
