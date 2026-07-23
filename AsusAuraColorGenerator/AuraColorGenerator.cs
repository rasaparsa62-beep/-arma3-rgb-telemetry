using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AsusAuraColorGenerator
{
	// Token: 0x02000003 RID: 3
	public class AuraColorGenerator
	{
		// Token: 0x06000005 RID: 5 RVA: 0x00002432 File Offset: 0x00000632
		public void SetHWave(object wave)
		{
			this.H_Wave = (wave as FtWave);
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002440 File Offset: 0x00000640
		public void SetSWave(object wave)
		{
			this.S_Wave = (wave as FtWave);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x0000244E File Offset: 0x0000064E
		public void SetLWave(object wave)
		{
			this.L_Wave = (wave as FtWave);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000245C File Offset: 0x0000065C
		public AuraColorGenerator(int w, int h)
		{
			this.width = w;
			this.height = h;
			this.outBitmap = new Bitmap(w, h);
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002499 File Offset: 0x00000699
		public static AuraColorGenerator STATIC_EFFECT(int width, int height, Color staticColor)
		{
			return new AuraColorGenerator(width, height)
			{
				StaticColor = staticColor,
				ShowStaticColor = true
			};
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000024B0 File Offset: 0x000006B0
		public static AuraColorGenerator COLORCYCLE_EFFECT(int width, int height)
		{
			AuraColorGenerator auraColorGenerator = new AuraColorGenerator(width, height);
			auraColorGenerator.Init_H = 0.0;
			auraColorGenerator.Init_S = 1.0;
			auraColorGenerator.Init_L = 0.5;
			FtWave.FtQuarterSineWave hwave = new FtWave.FtQuarterSineWave(0.0, 1.0, (double)width, -0.02, 0.0);
			auraColorGenerator.SetHWave(hwave);
			auraColorGenerator.FullFrameColor = true;
			return auraColorGenerator;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000252C File Offset: 0x0000072C
		public static AuraColorGenerator RAINBOW_EFFECT_1(int width, int height)
		{
			AuraColorGenerator auraColorGenerator = new AuraColorGenerator(width, height);
			List<FtWave.CustomNode> list = new List<FtWave.CustomNode>();
			list.Add(new FtWave.CustomNode(0.0, 0.826797386010488));
			list.Add(new FtWave.CustomNode(0.137142857142857, 0.662745097962519));
			list.Add(new FtWave.CustomNode(0.274285714285714, 0.503267973661423));
			list.Add(new FtWave.CustomNode(0.411428571428571, 0.332679738523439));
			list.Add(new FtWave.CustomNode(0.548571428571429, 0.160784314076106));
			list.Add(new FtWave.CustomNode(0.685714285714286, 0.0759408604943567));
			list.Add(new FtWave.CustomNode(0.857142857142857, 0.991503267859419));
			list.Add(new FtWave.CustomNode(1.0, 0.826797386010488));
			FtWave.FtShortPathLinearWave hwave = new FtWave.FtShortPathLinearWave(0.0, 1.0, (double)width, 0.175, 0.0, list);
			auraColorGenerator.SetHWave(hwave);
			List<FtWave.CustomNode> list2 = new List<FtWave.CustomNode>();
			list2.Add(new FtWave.CustomNode(0.0, 1.0));
			list2.Add(new FtWave.CustomNode(0.137142857142857, 1.0));
			list2.Add(new FtWave.CustomNode(0.274285714285714, 1.0));
			list2.Add(new FtWave.CustomNode(0.411428571428571, 1.0));
			list2.Add(new FtWave.CustomNode(0.548571428571429, 1.0));
			list2.Add(new FtWave.CustomNode(0.685714285714286, 1.0));
			list2.Add(new FtWave.CustomNode(0.857142857142857, 1.0));
			list2.Add(new FtWave.CustomNode(1.0, 1.0));
			FtWave.FtConstantWave swave = new FtWave.FtConstantWave(1.0, (double)width);
			auraColorGenerator.SetSWave(swave);
			List<FtWave.CustomNode> list3 = new List<FtWave.CustomNode>();
			list3.Add(new FtWave.CustomNode(0.0, 0.5));
			list3.Add(new FtWave.CustomNode(0.137142857142857, 0.5));
			list3.Add(new FtWave.CustomNode(0.274285714285714, 0.5));
			list3.Add(new FtWave.CustomNode(0.411428571428571, 0.5));
			list3.Add(new FtWave.CustomNode(0.548571428571429, 0.5));
			list3.Add(new FtWave.CustomNode(0.685714285714286, 0.5));
			list3.Add(new FtWave.CustomNode(0.857142857142857, 0.5));
			list3.Add(new FtWave.CustomNode(1.0, 0.5));
			FtWave.FtLinearWave lwave = new FtWave.FtLinearWave(0.0, 1.0, (double)width, 0.175, 0.0, list3);
			auraColorGenerator.SetLWave(lwave);
			return auraColorGenerator;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x0000288A File Offset: 0x00000A8A
		public Bitmap GetColorFrame()
		{
			this.CurrentTime += this.UpdateInterval;
			return this.GetColorFrame(this.CurrentTime);
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000028AC File Offset: 0x00000AAC
		public Bitmap GetColorFrame(double t)
		{
			BitmapData bitmapData = this.outBitmap.LockBits(new Rectangle(0, 0, this.outBitmap.Width, this.outBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			byte[] array = new byte[bitmapData.Stride * bitmapData.Height];
			Marshal.Copy(bitmapData.Scan0, array, 0, array.Length);
			this.outBitmap.UnlockBits(bitmapData);
			int num = 0;
			while (num + 3 < array.Length)
			{
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = this.FullFrameColor ? 0 : (num % bitmapData.Stride / 4);
				if (this.ShowStaticColor)
				{
					num4 = (int)this.StaticColor.R;
					num3 = (int)this.StaticColor.G;
					num2 = (int)this.StaticColor.B;
				}
				else
				{
					HSLColor.HSLToRGB((this.H_Wave != null) ? this.H_Wave.output(t, (double)num5) : this.Init_H, (this.S_Wave != null) ? this.S_Wave.output(t, (double)num5) : this.Init_S, (this.L_Wave != null) ? this.L_Wave.output(t, (double)num5) : this.Init_L, out num4, out num3, out num2);
				}
				if (num2 > 255)
				{
					num2 = 255;
				}
				else if (num2 < 0)
				{
					num2 = 0;
				}
				if (num3 > 255)
				{
					num3 = 255;
				}
				else if (num3 < 0)
				{
					num3 = 0;
				}
				if (num4 > 255)
				{
					num4 = 255;
				}
				else if (num4 < 0)
				{
					num4 = 0;
				}
				array[num] = (byte)num2;
				array[num + 1] = (byte)num3;
				array[num + 2] = (byte)num4;
				num += 4;
			}
			Bitmap bitmap = new Bitmap(this.outBitmap.Width, this.outBitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			Marshal.Copy(array, 0, bitmapData2.Scan0, array.Length);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002AA6 File Offset: 0x00000CA6
		public Bitmap BlendColorFrame(Bitmap oriBitmap)
		{
			this.CurrentTime += this.UpdateInterval;
			return this.BlendColorFrame(oriBitmap, this.CurrentTime);
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002AC8 File Offset: 0x00000CC8
		public Bitmap BlendColorFrame(Bitmap oriBitmap, double t)
		{
			BitmapData bitmapData = oriBitmap.LockBits(new Rectangle(0, 0, oriBitmap.Width, oriBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			byte[] array = new byte[bitmapData.Stride * bitmapData.Height];
			Marshal.Copy(bitmapData.Scan0, array, 0, array.Length);
			oriBitmap.UnlockBits(bitmapData);
			int num = 0;
			while (num + 4 < array.Length)
			{
				if (array[num] != 0 || array[num + 1] != 0 || array[num + 2] != 0)
				{
					int num2 = 0;
					int num3 = 0;
					int num4 = 0;
					int num5 = this.FullFrameColor ? 0 : (num % bitmapData.Stride / 4);
					if (this.ShowStaticColor)
					{
						num4 = (int)this.StaticColor.R;
						num3 = (int)this.StaticColor.G;
						num2 = (int)this.StaticColor.B;
					}
					else
					{
						HSLColor.HSLToRGB((this.H_Wave != null) ? this.H_Wave.output(t, (double)num5) : this.Init_H, (this.S_Wave != null) ? this.S_Wave.output(t, (double)num5) : this.Init_S, (this.L_Wave != null) ? this.L_Wave.output(t, (double)num5) : this.Init_L, out num4, out num3, out num2);
					}
					if (num2 > 255)
					{
						num2 = 255;
					}
					else if (num2 < 0)
					{
						num2 = 0;
					}
					if (num3 > 255)
					{
						num3 = 255;
					}
					else if (num3 < 0)
					{
						num3 = 0;
					}
					if (num4 > 255)
					{
						num4 = 255;
					}
					else if (num4 < 0)
					{
						num4 = 0;
					}
					array[num] = (byte)num2;
					array[num + 1] = (byte)num3;
					array[num + 2] = (byte)num4;
				}
				num += 4;
			}
			Bitmap bitmap = new Bitmap(oriBitmap.Width, oriBitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			Marshal.Copy(array, 0, bitmapData2.Scan0, array.Length);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		// Token: 0x0400000A RID: 10
		private int width;

		// Token: 0x0400000B RID: 11
		private int height;

		// Token: 0x0400000C RID: 12
		private double CurrentTime;

		// Token: 0x0400000D RID: 13
		public double UpdateInterval = 0.05;

		// Token: 0x0400000E RID: 14
		private FtWave H_Wave;

		// Token: 0x0400000F RID: 15
		private FtWave S_Wave;

		// Token: 0x04000010 RID: 16
		private FtWave L_Wave;

		// Token: 0x04000011 RID: 17
		public double Init_H;

		// Token: 0x04000012 RID: 18
		public double Init_S;

		// Token: 0x04000013 RID: 19
		public double Init_L;

		// Token: 0x04000014 RID: 20
		private Bitmap outBitmap;

		// Token: 0x04000015 RID: 21
		public Color StaticColor = Color.Black;

		// Token: 0x04000016 RID: 22
		public bool ShowStaticColor;

		// Token: 0x04000017 RID: 23
		public bool FullFrameColor;
	}
}
