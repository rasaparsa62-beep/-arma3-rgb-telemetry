using System;

namespace AsusAuraColorGenerator
{
	// Token: 0x02000005 RID: 5
	internal class FtRange
	{
		// Token: 0x06000024 RID: 36 RVA: 0x0000326E File Offset: 0x0000146E
		public FtRange(double min, double max)
		{
			this.m_min = min;
			this.m_max = max;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00003284 File Offset: 0x00001484
		public bool InRange(double num)
		{
			return num >= this.m_min & num <= this.m_max;
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000026 RID: 38 RVA: 0x000032A4 File Offset: 0x000014A4
		// (set) Token: 0x06000027 RID: 39 RVA: 0x000032AC File Offset: 0x000014AC
		public double min
		{
			get
			{
				return this.m_min;
			}
			set
			{
				this.m_min = value;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000028 RID: 40 RVA: 0x000032B5 File Offset: 0x000014B5
		// (set) Token: 0x06000029 RID: 41 RVA: 0x000032BD File Offset: 0x000014BD
		public double max
		{
			get
			{
				return this.m_max;
			}
			set
			{
				this.m_max = value;
			}
		}

		// Token: 0x0400001E RID: 30
		private double m_min;

		// Token: 0x0400001F RID: 31
		private double m_max;
	}
}
