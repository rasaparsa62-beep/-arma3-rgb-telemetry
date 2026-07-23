using System;

namespace AsusAuraAnimation
{
	// Token: 0x0200000F RID: 15
	public interface IAudioSource
	{
		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000148 RID: 328
		// (remove) Token: 0x06000149 RID: 329
		event EventHandler<float[]> SpectrumDataAvailable;

		// Token: 0x0600014A RID: 330
		void Start();

		// Token: 0x0600014B RID: 331
		void Stop();

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x0600014C RID: 332
		bool IsRunning { get; }
	}
}
