using System;
using System.Drawing;

namespace AsusAuraColorGenerator
{
	// Token: 0x02000004 RID: 4
	public class HSLColor
	{
		// Token: 0x06000010 RID: 16 RVA: 0x00002CC0 File Offset: 0x00000EC0
		public HSLColor()
		{
			Color black = Color.Black;
			this.m_R = (int)black.R;
			this.m_G = (int)black.G;
			this.m_B = (int)black.B;
			HSLColor.RGBToHSL(this.m_R, this.m_G, this.m_B, out this.m_Hue, out this.m_Saturation, out this.m_Lightness);
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002D2C File Offset: 0x00000F2C
		public HSLColor(Color color)
		{
			this.m_R = (int)color.R;
			this.m_G = (int)color.G;
			this.m_B = (int)color.B;
			HSLColor.RGBToHSL(this.m_R, this.m_G, this.m_B, out this.m_Hue, out this.m_Saturation, out this.m_Lightness);
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002D90 File Offset: 0x00000F90
		public HSLColor(double h, double s, double l)
		{
			this.m_Hue = h;
			this.m_Saturation = s;
			this.m_Lightness = l;
			HSLColor.HSLToRGB(this.m_Hue, this.m_Saturation, this.m_Lightness, out this.m_R, out this.m_G, out this.m_B);
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002DE1 File Offset: 0x00000FE1
		public Color GetColor()
		{
			return Color.FromArgb(255, this.m_R, this.m_G, this.m_B);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002E00 File Offset: 0x00001000
		public void SetColor(Color color)
		{
			this.m_R = (int)color.R;
			this.m_G = (int)color.G;
			this.m_B = (int)color.B;
			HSLColor.RGBToHSL(this.m_R, this.m_G, this.m_B, out this.m_Hue, out this.m_Saturation, out this.m_Lightness);
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000015 RID: 21 RVA: 0x00002E5D File Offset: 0x0000105D
		// (set) Token: 0x06000016 RID: 22 RVA: 0x00002E65 File Offset: 0x00001065
		public int R
		{
			get
			{
				return this.m_R;
			}
			set
			{
				this.m_R = value;
				HSLColor.RGBToHSL(this.m_R, this.m_G, this.m_B, out this.m_Hue, out this.m_Saturation, out this.m_Lightness);
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000017 RID: 23 RVA: 0x00002E97 File Offset: 0x00001097
		// (set) Token: 0x06000018 RID: 24 RVA: 0x00002E9F File Offset: 0x0000109F
		public int G
		{
			get
			{
				return this.m_G;
			}
			set
			{
				this.m_G = value;
				HSLColor.RGBToHSL(this.m_R, this.m_G, this.m_B, out this.m_Hue, out this.m_Saturation, out this.m_Lightness);
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000019 RID: 25 RVA: 0x00002ED1 File Offset: 0x000010D1
		// (set) Token: 0x0600001A RID: 26 RVA: 0x00002ED9 File Offset: 0x000010D9
		public int B
		{
			get
			{
				return this.m_B;
			}
			set
			{
				this.m_B = value;
				HSLColor.RGBToHSL(this.m_R, this.m_G, this.m_B, out this.m_Hue, out this.m_Saturation, out this.m_Lightness);
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600001B RID: 27 RVA: 0x00002F0B File Offset: 0x0000110B
		// (set) Token: 0x0600001C RID: 28 RVA: 0x00002F13 File Offset: 0x00001113
		public double H
		{
			get
			{
				return this.m_Hue;
			}
			set
			{
				this.m_Hue = value;
				HSLColor.HSLToRGB(this.m_Hue, this.m_Saturation, this.m_Lightness, out this.m_R, out this.m_G, out this.m_B);
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600001D RID: 29 RVA: 0x00002F45 File Offset: 0x00001145
		// (set) Token: 0x0600001E RID: 30 RVA: 0x00002F4D File Offset: 0x0000114D
		public double S
		{
			get
			{
				return this.m_Saturation;
			}
			set
			{
				this.m_Saturation = value;
				HSLColor.HSLToRGB(this.m_Hue, this.m_Saturation, this.m_Lightness, out this.m_R, out this.m_G, out this.m_B);
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600001F RID: 31 RVA: 0x00002F7F File Offset: 0x0000117F
		// (set) Token: 0x06000020 RID: 32 RVA: 0x00002F87 File Offset: 0x00001187
		public double L
		{
			get
			{
				return this.m_Lightness;
			}
			set
			{
				this.m_Lightness = value;
				HSLColor.HSLToRGB(this.m_Hue, this.m_Saturation, this.m_Lightness, out this.m_R, out this.m_G, out this.m_B);
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002FBC File Offset: 0x000011BC
		public static void RGBToHSL(int r, int g, int b, out double d_h, out double s, out double l)
		{
			double num = (double)r / 255.0;
			double num2 = (double)g / 255.0;
			double num3 = (double)b / 255.0;
			double num4 = num;
			if (num4 < num2)
			{
				num4 = num2;
			}
			if (num4 < num3)
			{
				num4 = num3;
			}
			double num5 = num;
			if (num5 > num2)
			{
				num5 = num2;
			}
			if (num5 > num3)
			{
				num5 = num3;
			}
			double num6 = num4 - num5;
			l = (num4 + num5) / 2.0;
			double num7;
			if (Math.Abs(num6) < 1E-05)
			{
				s = 0.0;
				num7 = 0.0;
			}
			else
			{
				if (l <= 0.5)
				{
					s = num6 / (num4 + num5);
				}
				else
				{
					s = num6 / (2.0 - num4 - num5);
				}
				double num8 = (num4 - num) / num6;
				double num9 = (num4 - num2) / num6;
				double num10 = (num4 - num3) / num6;
				if (num == num4)
				{
					num7 = num10 - num9;
				}
				else if (num2 == num4)
				{
					num7 = 2.0 + num8 - num10;
				}
				else
				{
					num7 = 4.0 + num9 - num8;
				}
				num7 *= 60.0;
				if (num7 < 0.0)
				{
					num7 += 360.0;
				}
			}
			d_h = num7 / 360.0;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00003118 File Offset: 0x00001318
		public static void HSLToRGB(double d_h, double s, double l, out int r, out int g, out int b)
		{
			double num = 360.0 * d_h;
			double num2;
			if (l <= 0.5)
			{
				num2 = l * (1.0 + s);
			}
			else
			{
				num2 = l + s - l * s;
			}
			double q = 2.0 * l - num2;
			double num3;
			double num4;
			double num5;
			if (s == 0.0)
			{
				num3 = l;
				num4 = l;
				num5 = l;
			}
			else
			{
				num3 = HSLColor.QqhToRgb(q, num2, num + 120.0);
				num4 = HSLColor.QqhToRgb(q, num2, num);
				num5 = HSLColor.QqhToRgb(q, num2, num - 120.0);
			}
			r = (int)(num3 * 255.0);
			g = (int)(num4 * 255.0);
			b = (int)(num5 * 255.0);
		}

		// Token: 0x06000023 RID: 35 RVA: 0x000031D8 File Offset: 0x000013D8
		private static double QqhToRgb(double q1, double q2, double hue)
		{
			if (hue > 360.0)
			{
				hue -= 360.0;
			}
			else if (hue < 0.0)
			{
				hue += 360.0;
			}
			if (hue < 60.0)
			{
				return q1 + (q2 - q1) * hue / 60.0;
			}
			if (hue < 180.0)
			{
				return q2;
			}
			if (hue < 240.0)
			{
				return q1 + (q2 - q1) * (240.0 - hue) / 60.0;
			}
			return q1;
		}

		// Token: 0x04000018 RID: 24
		private int m_R;

		// Token: 0x04000019 RID: 25
		private int m_G;

		// Token: 0x0400001A RID: 26
		private int m_B;

		// Token: 0x0400001B RID: 27
		private double m_Hue;

		// Token: 0x0400001C RID: 28
		private double m_Saturation;

		// Token: 0x0400001D RID: 29
		private double m_Lightness;
	}
}
