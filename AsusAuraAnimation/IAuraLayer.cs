using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AsusAuraAnimation
{
	// Token: 0x02000018 RID: 24
	[Guid("3a4138af-38d0-3efd-a20e-93a4c4575033")]
	public interface IAuraLayer
	{
		// Token: 0x060001E2 RID: 482
		[DispId(1)]
		void init(bool repeat, int offset_x, int offset_y, bool isFullLayer, int width, int height);

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060001E3 RID: 483
		// (set) Token: 0x060001E4 RID: 484
		[DispId(2)]
		Color layermaskColor { get; set; }

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060001E5 RID: 485
		// (set) Token: 0x060001E6 RID: 486
		[DispId(3)]
		int layeroffset_x { get; set; }

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060001E7 RID: 487
		// (set) Token: 0x060001E8 RID: 488
		[DispId(4)]
		int layeroffset_y { get; set; }

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060001E9 RID: 489
		// (set) Token: 0x060001EA RID: 490
		[DispId(5)]
		bool repeatable { get; set; }

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x060001EB RID: 491
		// (set) Token: 0x060001EC RID: 492
		[DispId(6)]
		int startGlobalFrameID { get; set; }

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x060001ED RID: 493
		// (set) Token: 0x060001EE RID: 494
		[DispId(7)]
		int frameWidth { get; set; }

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x060001EF RID: 495
		// (set) Token: 0x060001F0 RID: 496
		[DispId(8)]
		int frameHeight { get; set; }

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060001F1 RID: 497
		// (set) Token: 0x060001F2 RID: 498
		[DispId(9)]
		bool isStaticFrame { get; set; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060001F3 RID: 499
		// (set) Token: 0x060001F4 RID: 500
		[DispId(10)]
		Bitmap StaticBitmap { get; set; }

		// Token: 0x060001F5 RID: 501
		[DispId(11)]
		int GetFrameCount();

		// Token: 0x060001F6 RID: 502
		[DispId(12)]
		Bitmap GetFrameAt(int index);

		// Token: 0x060001F7 RID: 503
		[DispId(13)]
		void AddGif(string path);

		// Token: 0x060001F8 RID: 504
		[DispId(14)]
		void AddStaticPicture(string path, int framecount);

		// Token: 0x060001F9 RID: 505
		[DispId(15)]
		void MaskFromStaticPicture(string path);

		// Token: 0x060001FA RID: 506
		[DispId(16)]
		void AddSlideText(string text, string fontName, int fontSize, bool bReverted);

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060001FB RID: 507
		// (set) Token: 0x060001FC RID: 508
		[DispId(39)]
		bool isMusicLayer { get; set; }

		// Token: 0x060001FD RID: 509
		[DispId(17)]
		void InitMusicLayer(bool repeat, int offset_x, int offset_y, bool isFullLayer, int width, int height);

		// Token: 0x060001FE RID: 510
		[DispId(18)]
		void EnableMusicLayer(bool isEnable, int audioDevID);

		// Token: 0x060001FF RID: 511
		[DispId(19)]
		string GetAudioDeviceList();

		// Token: 0x06000200 RID: 512
		[DispId(20)]
		void UpdateMusicFrame();

		// Token: 0x06000201 RID: 513
		[DispId(21)]
		int GetAudioDeviceCount();

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000202 RID: 514
		// (set) Token: 0x06000203 RID: 515
		[DispId(22)]
		int SpectrumType { get; set; }

		// Token: 0x06000204 RID: 516
		[DispId(23)]
		int GetMusicSPectrumTypeCount();

		// Token: 0x06000205 RID: 517
		[DispId(24)]
		void InitTextLayer(bool repeat, int offset_x, int offset_y, bool isFullLayer, int width, int height);

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000206 RID: 518
		// (set) Token: 0x06000207 RID: 519
		[DispId(25)]
		bool isTextLayer { get; set; }

		// Token: 0x06000208 RID: 520
		[DispId(26)]
		void AddTextContent(string content, bool bSlide, bool bRevert, int offset_x, int offset_y, int width, int height, string fontname, int fontsize, int speed, bool IsAntiAlias, bool Border, Color TextColor);

		// Token: 0x06000209 RID: 521
		[DispId(27)]
		void ClearTextContent();

		// Token: 0x0600020A RID: 522
		[DispId(28)]
		void UpdateTextFrame();

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x0600020B RID: 523
		// (set) Token: 0x0600020C RID: 524
		[DispId(29)]
		int timeLength { get; set; }

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x0600020D RID: 525
		// (set) Token: 0x0600020E RID: 526
		[DispId(30)]
		int timeStart { get; set; }

		// Token: 0x0600020F RID: 527
		[DispId(31)]
		Bitmap GetPictureFrame(int timeFromStart);

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000210 RID: 528
		// (set) Token: 0x06000211 RID: 529
		[DispId(32)]
		int RotationDegree { get; set; }

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000212 RID: 530
		// (set) Token: 0x06000213 RID: 531
		[DispId(33)]
		bool ApplyMatrix { get; set; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000214 RID: 532
		// (set) Token: 0x06000215 RID: 533
		[DispId(34)]
		int AudioEffectStrength { get; set; }

		// Token: 0x06000216 RID: 534
		[DispId(35)]
		void OpenPicture(string path);

		// Token: 0x06000217 RID: 535
		[DispId(36)]
		void OpenPicture(string path, bool matrixLayout, bool isloaddefault);

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x06000218 RID: 536
		// (set) Token: 0x06000219 RID: 537
		[DispId(37)]
		bool Afterglow { get; set; }

		// Token: 0x0600021A RID: 538
		[DispId(38)]
		void OpenPicture(string path, bool matrixLayout);

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x0600021B RID: 539
		// (set) Token: 0x0600021C RID: 540
		[DispId(40)]
		float AnimationSpeedRatio { get; set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x0600021D RID: 541
		// (set) Token: 0x0600021E RID: 542
		[DispId(41)]
		bool IsShowAsMatrixDefault { get; set; }

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x0600021F RID: 543
		// (set) Token: 0x06000220 RID: 544
		[DispId(42)]
		int DecayEffect { get; set; }

		// Token: 0x06000221 RID: 545
		[DispId(43)]
		int GetAnimationLength();

		// Token: 0x06000222 RID: 546
		[DispId(44)]
		void EnableMusicLayer2(bool isEnable, string audioDevName);

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000223 RID: 547
		// (set) Token: 0x06000224 RID: 548
		[DispId(45)]
		int StrobingBeatThreadhold { get; set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000225 RID: 549
		// (set) Token: 0x06000226 RID: 550
		[DispId(46)]
		int StrobingBeatStartFreq { get; set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000227 RID: 551
		// (set) Token: 0x06000228 RID: 552
		[DispId(47)]
		int StrobingBeatEndFreq { get; set; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x06000229 RID: 553
		// (set) Token: 0x0600022A RID: 554
		[DispId(48)]
		bool StrobingBeatUseMax { get; set; }

		// Token: 0x0600022B RID: 555
		[DispId(49)]
		void deInit();

		// Token: 0x0600022C RID: 556
		[DispId(50)]
		void SetAudioBassCompressRatio(int ratio);

		// Token: 0x0600022D RID: 557
		[DispId(51)]
		void SetAudioTrebleCompressRatio(int ratio);

		// Token: 0x0600022E RID: 558
		[DispId(52)]
		void SetCrop(int x, int y, int w, int h);

		// Token: 0x0600022F RID: 559
		[DispId(53)]
		Rectangle GetCropRect();

		// Token: 0x06000230 RID: 560
		[DispId(54)]
		bool IsCropped();

		// Token: 0x06000231 RID: 561
		[DispId(55)]
		void ClearCrop();

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000232 RID: 562
		// (set) Token: 0x06000233 RID: 563
		[DispId(56)]
		bool IsShowCropPreview { get; set; }

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x06000234 RID: 564
		// (set) Token: 0x06000235 RID: 565
		[DispId(57)]
		int MatrixDirection { get; set; }

		// Token: 0x06000236 RID: 566
		[DispId(58)]
		void AddTextContent_withAlignment(string Content, bool Slide, bool Revert, int Offset_x, int Offset_y, int Width, int Height, string Fontname, int Fontsize, int Speed, bool IsAntiAlias, bool Border, Color TextColor, int Halign, int Valign);

		// Token: 0x06000237 RID: 567
		[DispId(59)]
		int GetTextUpdateCount();

		// Token: 0x06000238 RID: 568
		[DispId(60)]
		void InitGradientLayer(int gw, int gh, bool repeat, int offset_x, int offset_y, bool isFullLayer, int width, int height);

		// Token: 0x06000239 RID: 569
		[DispId(61)]
		bool AddGradientColorPoint(int x, int y, int r, int g, int b);

		// Token: 0x0600023A RID: 570
		[DispId(62)]
		void ClearGradientColorPoint();

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x0600023B RID: 571
		// (set) Token: 0x0600023C RID: 572
		[DispId(63)]
		int GradientFactor { get; set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x0600023D RID: 573
		// (set) Token: 0x0600023E RID: 574
		[DispId(64)]
		bool isScreenCaptureLayer { get; set; }

		// Token: 0x0600023F RID: 575
		[DispId(65)]
		void InitScreenCaptureLayer(int x, int y, int w, int h, bool repeat, int offset_x, int offset_y, bool isFullLayer, int width, int height);

		// Token: 0x06000240 RID: 576
		[DispId(66)]
		void UpdateCaptureRect(int x, int y, int w, int h);

		// Token: 0x06000241 RID: 577
		[DispId(67)]
		void UpdateScreenCaptureFrame();

		// Token: 0x06000242 RID: 578
		[DispId(68)]
		void UpdateStaticeFrame();

		// Token: 0x06000243 RID: 579
		[DispId(69)]
		void InitSlashLightingLayer(bool repeat, int offset_x, int offset_y, bool isFullLayer, int width, int height);

		// Token: 0x06000244 RID: 580
		[DispId(70)]
		void AddSlashLightingGlyph(string filename, int priority, bool isloop, int interval, int repeat_times, int brightness, string wavefile);

		// Token: 0x06000245 RID: 581
		[DispId(71)]
		void AddSlashLightingPlayGlyph(string dev, int priority, int type, int decay, int strength, bool isFullRange, int rangeIndex, int brightness);

		// Token: 0x06000246 RID: 582
		[DispId(72)]
		void ClearAllGlyph();

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x06000247 RID: 583
		// (set) Token: 0x06000248 RID: 584
		[DispId(73)]
		bool ShowBleading { get; set; }

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x06000249 RID: 585
		// (set) Token: 0x0600024A RID: 586
		[DispId(74)]
		int BleadingSpeed { get; set; }
	}
}
