using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AsusAuraAnimation
{
	// Token: 0x02000017 RID: 23
	[Guid("3c80d7a5-2809-3d34-b97b-e75e904edb9c")]
	public interface IAuraAnimation
	{
		// Token: 0x060001CA RID: 458
		[DispId(1)]
		void init(int width, int height);

		// Token: 0x060001CB RID: 459
		[DispId(2)]
		IAuraLayer CreateLayer();

		// Token: 0x060001CC RID: 460
		[DispId(3)]
		void AddLayer(IAuraLayer layer);

		// Token: 0x060001CD RID: 461
		[DispId(4)]
		void InsertLayerAt(int index, IAuraLayer layer);

		// Token: 0x060001CE RID: 462
		[DispId(5)]
		void RemoveLayer(IAuraLayer layer);

		// Token: 0x060001CF RID: 463
		[DispId(6)]
		void RemoveLayerAt(int index);

		// Token: 0x060001D0 RID: 464
		[DispId(7)]
		int GetWidth();

		// Token: 0x060001D1 RID: 465
		[DispId(8)]
		int GetHeight();

		// Token: 0x060001D2 RID: 466
		[DispId(9)]
		void ClearLayers();

		// Token: 0x060001D3 RID: 467
		[DispId(10)]
		IAuraLayer ReplaceBaseLayer(IAuraLayer layer);

		// Token: 0x060001D4 RID: 468
		[DispId(11)]
		IntPtr Update();

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060001D5 RID: 469
		// (set) Token: 0x060001D6 RID: 470
		[DispId(12)]
		int ContrastThreshold { get; set; }

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060001D7 RID: 471
		// (set) Token: 0x060001D8 RID: 472
		[DispId(13)]
		int BrightnessThreshold { get; set; }

		// Token: 0x060001D9 RID: 473
		[DispId(14)]
		IntPtr UpdateFromTime(int timeFromAnimationStart);

		// Token: 0x060001DA RID: 474
		[DispId(15)]
		Bitmap UpdateBitmap();

		// Token: 0x060001DB RID: 475
		[DispId(16)]
		Bitmap UpdateFromTimeBitmap(int timeFromAnimationStart);

		// Token: 0x060001DC RID: 476
		[DispId(17)]
		void SetMatrixParameter(int Width, int Height, int SlashHeight);

		// Token: 0x060001DD RID: 477
		[DispId(18)]
		int GetMatrixParameterWidth();

		// Token: 0x060001DE RID: 478
		[DispId(19)]
		int GetMatrixParameterHeight();

		// Token: 0x060001DF RID: 479
		[DispId(20)]
		int GetMatrixParameterSlashHeight();

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060001E0 RID: 480
		// (set) Token: 0x060001E1 RID: 481
		[DispId(21)]
		int MatrixDirection { get; set; }
	}
}
