using System;
using System.Runtime.InteropServices;

namespace AsusAuraAnimation
{
	// Token: 0x02000019 RID: 25
	[Guid("98F48E4B-42E3-40D7-A7FD-6827B46948A2")]
	public interface IAudioAnalyzer
	{
		// Token: 0x0600024B RID: 587
		[DispId(1)]
		void Enable(bool isEnable, int index);

		// Token: 0x0600024C RID: 588
		[DispId(2)]
		string GetAudioDeviceList();

		// Token: 0x0600024D RID: 589
		[DispId(3)]
		int GetAudioDeviceCount();

		// Token: 0x0600024E RID: 590
		[DispId(4)]
		void Init();

		// Token: 0x0600024F RID: 591
		[DispId(5)]
		void SetLines(int lines);

		// Token: 0x06000250 RID: 592
		[DispId(6)]
		string UpdateAudioDataString(int line, int Strength);

		// Token: 0x06000251 RID: 593
		[DispId(7)]
		void Free();

		// Token: 0x06000252 RID: 594
		[DispId(8)]
		void Enable2(bool isEnable, string devName);

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x06000253 RID: 595
		// (set) Token: 0x06000254 RID: 596
		[DispId(9)]
		int BassCompressRatio { get; set; }

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000255 RID: 597
		// (set) Token: 0x06000256 RID: 598
		[DispId(10)]
		int TrebleCompressRatio { get; set; }
	}
}
