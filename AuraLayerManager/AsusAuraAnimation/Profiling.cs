using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AsusAuraAnimation
{
	// Token: 0x0200001B RID: 27
	internal class Profiling : IDisposable
	{
		// Token: 0x06000265 RID: 613 RVA: 0x00014CF0 File Offset: 0x00012EF0
		public Profiling(string name)
		{
			this.actionanme = name;
			this.sw = Stopwatch.StartNew();
			Profiling.LastValue[name] = 0L;
			Profiling.Sum[name] = 0L;
			Profiling.Count[name] = 0L;
			Profiling.Average[name] = 0L;
		}

		// Token: 0x06000266 RID: 614 RVA: 0x00014D4C File Offset: 0x00012F4C
		private static void AddValue(string name, long value)
		{
			Profiling.LastValue[name] = value;
			Dictionary<string, long> dictionary = Profiling.Sum;
			dictionary[name] += value;
			dictionary = Profiling.Count;
			dictionary[name] += 1L;
			if (Profiling.Count[name] >= (long)Profiling.AvgRange)
			{
				Profiling.Average[name] = Profiling.Sum[name] / Profiling.Count[name];
				Profiling.Sum[name] = 0L;
				Profiling.Count[name] = 0L;
			}
		}

		// Token: 0x06000267 RID: 615 RVA: 0x00014DE8 File Offset: 0x00012FE8
		public void Dispose()
		{
			this.sw.Stop();
			Profiling.AddValue(this.actionanme, this.sw.ElapsedMilliseconds);
			Trace.WriteLine(string.Concat(new string[]
			{
				"[LayerManager] ** PROFILING ** Action(",
				this.actionanme,
				") use ",
				this.sw.ElapsedMilliseconds.ToString(),
				"ms to run"
			}));
		}

		// Token: 0x040000F0 RID: 240
		private string actionanme;

		// Token: 0x040000F1 RID: 241
		private Stopwatch sw;

		// Token: 0x040000F2 RID: 242
		public static Dictionary<string, long> LastValue = new Dictionary<string, long>();

		// Token: 0x040000F3 RID: 243
		public static Dictionary<string, long> Sum = new Dictionary<string, long>();

		// Token: 0x040000F4 RID: 244
		public static Dictionary<string, long> Count = new Dictionary<string, long>();

		// Token: 0x040000F5 RID: 245
		public static Dictionary<string, long> Average = new Dictionary<string, long>();

		// Token: 0x040000F6 RID: 246
		public static int AvgRange = 1000;
	}
}
