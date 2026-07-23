using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace AsusAuraAnimation
{
	// Token: 0x02000007 RID: 7
	[Guid("756e6c18-79cc-3842-9e47-7c80011d303a")]
	[ClassInterface(ClassInterfaceType.None)]
	public class AuraAnimation : IAuraAnimation
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000039 RID: 57 RVA: 0x000033DF File Offset: 0x000015DF
		// (set) Token: 0x0600003A RID: 58 RVA: 0x000033E7 File Offset: 0x000015E7
		public int MatrixDirection { get; set; }

		// Token: 0x0600003B RID: 59 RVA: 0x000033F0 File Offset: 0x000015F0
		public AuraAnimation()
		{
			AuraAnimation.refcount++;
			this.id = Thread.CurrentThread.ManagedThreadId;
			string empty = string.Empty;
			if (Util.IsX86Build())
			{
				empty = AuraAnimation.x86Name;
			}
			else
			{
				empty = AuraAnimation.x64Name;
			}
			LOGGER.DEBUG("[AuraAnimation] AuraLayerManager {0} Build@ {1}, refcount = {2}", new object[]
			{
				Util.GetAppVersion(empty),
				Util.GetCurrentBuildTime().ToString("yyyy-MM-dd HH:mm:ss"),
				AuraAnimation.refcount
			});
			LOGGER.EnforceLogLimits();
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000034A0 File Offset: 0x000016A0
		public void init(int width, int height)
		{
			if (width <= 0 || height <= 0)
			{
				LOGGER.WARN("[AuraAnimation] init() invalid size: {0}x{1}, clamping to 1x1", new object[]
				{
					width,
					height
				});
				width = Math.Max(1, width);
				height = Math.Max(1, height);
			}
			this.layerList = new List<IAuraLayer>(10);
			this.devwidth = width;
			this.devheight = height;
			this.currentOutputFrame = new Bitmap(this.devwidth, this.devheight);
			this.blackBitmap = new Bitmap(this.devwidth, this.devheight);
			this.globalFrameID = 0;
			this.MatrixDirection = 0;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00003540 File Offset: 0x00001740
		protected override void Finalize()
		{
			try
			{
				LOGGER.DEBUG("[AuraAnimation] ~AuraAnimation()+", Array.Empty<object>());
				AuraAnimation.refcount--;
				if (this.layerList != null)
				{
					foreach (IAuraLayer auraLayer in this.layerList)
					{
						auraLayer.deInit();
					}
					this.layerList.Clear();
				}
				LOGGER.DEBUG("[AuraAnimation] ~AuraAnimation()- [{0}] remain refcount={1}", new object[]
				{
					this.id,
					AuraAnimation.refcount
				});
			}
			finally
			{
				base.Finalize();
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00003600 File Offset: 0x00001800
		public IAuraLayer CreateLayer()
		{
			return new AuraLayer(this);
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00003608 File Offset: 0x00001808
		public void AddLayer(IAuraLayer layer)
		{
			LOGGER.DEBUG("[AuraAnimation] AddLayer()+", Array.Empty<object>());
			this.layerList.Add(layer);
			layer.startGlobalFrameID = this.globalFrameID;
			LOGGER.DEBUG("[AuraAnimation] AddLayer()-", Array.Empty<object>());
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00003640 File Offset: 0x00001840
		public void InsertLayerAt(int index, IAuraLayer layer)
		{
			LOGGER.DEBUG("[AuraAnimation] InsertLayerAt()+", Array.Empty<object>());
			if (index >= 0 && index <= this.layerList.Count)
			{
				this.layerList.Insert(index, layer);
				layer.startGlobalFrameID = this.globalFrameID;
			}
			LOGGER.DEBUG("[AuraAnimation] InsertLayerAt()-", Array.Empty<object>());
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00003698 File Offset: 0x00001898
		public void RemoveLayer(IAuraLayer layer)
		{
			LOGGER.DEBUG("[AuraAnimation] RemoveLayer()+", Array.Empty<object>());
			if (this.layerList.IndexOf(layer) != -1)
			{
				this.layerList.Remove(layer);
			}
			GC.Collect();
			LOGGER.DEBUG("[AuraAnimation] RemoveLayer()-", Array.Empty<object>());
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000036E4 File Offset: 0x000018E4
		public void RemoveLayerAt(int index)
		{
			LOGGER.DEBUG("[AuraAnimation] RemoveLayerAt()+", Array.Empty<object>());
			if (index < this.layerList.Count)
			{
				this.layerList.RemoveAt(index);
			}
			GC.Collect();
			LOGGER.DEBUG("[AuraAnimation] RemoveLayerAt()-", Array.Empty<object>());
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00003723 File Offset: 0x00001923
		public int GetWidth()
		{
			return this.devwidth;
		}

		// Token: 0x06000044 RID: 68 RVA: 0x0000372B File Offset: 0x0000192B
		public int GetHeight()
		{
			return this.devheight;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00003734 File Offset: 0x00001934
		public void ClearLayers()
		{
			object renderLock = this._renderLock;
			lock (renderLock)
			{
				LOGGER.DEBUG("[AuraAnimation] ClearLayers()+ deinit layers", Array.Empty<object>());
				foreach (IAuraLayer auraLayer in this.layerList)
				{
					auraLayer.deInit();
				}
				this.layerList.Clear();
				GC.Collect();
				LOGGER.DEBUG("[AuraAnimation] ClearLayers()-", Array.Empty<object>());
			}
		}

		// Token: 0x06000046 RID: 70 RVA: 0x000037DC File Offset: 0x000019DC
		private Bitmap BlendingBitmap(Bitmap oriBitmap, Bitmap overlayBitmap, Bitmap overlayMask, Color maskcolor, int offset_x, int offset_y)
		{
			Bitmap bitmap = (Bitmap)oriBitmap.Clone();
			int num = (offset_x >= 0) ? offset_x : 0;
			int num2 = (offset_y >= 0) ? offset_y : 0;
			int num3 = (offset_x >= 0) ? oriBitmap.Width : (oriBitmap.Width + offset_x);
			int num4 = (offset_y >= 0) ? oriBitmap.Height : (oriBitmap.Height + offset_y);
			for (int i = num; i < num3; i++)
			{
				for (int j = num2; j < num4; j++)
				{
					if (overlayMask.GetPixel(i - offset_x, j - offset_y).R == maskcolor.R && overlayMask.GetPixel(i - offset_x, j - offset_y).G == maskcolor.G && overlayMask.GetPixel(i - offset_x, j - offset_y).B == maskcolor.B)
					{
						bitmap.SetPixel(i, j, oriBitmap.GetPixel(i, j));
					}
					else if (overlayBitmap.GetPixel(i - offset_x, j - offset_y).A == 255)
					{
						bitmap.SetPixel(i, j, overlayBitmap.GetPixel(i - offset_x, j - offset_y));
					}
					else
					{
						int a = (int)overlayBitmap.GetPixel(i - offset_x, j - offset_y).A;
						bitmap.SetPixel(i, j, Color.FromArgb((int)oriBitmap.GetPixel(i, j).A, a * (int)overlayBitmap.GetPixel(i - offset_x, j - offset_y).R + (255 - a) * (int)oriBitmap.GetPixel(i, j).R >> 8, a * (int)overlayBitmap.GetPixel(i - offset_x, j - offset_y).G + (255 - a) * (int)oriBitmap.GetPixel(i, j).G >> 8, a * (int)overlayBitmap.GetPixel(i - offset_x, j - offset_y).B + (255 - a) * (int)oriBitmap.GetPixel(i, j).B >> 8));
					}
				}
			}
			return bitmap;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00003A10 File Offset: 0x00001C10
		private void ResetBaseBitmap()
		{
			for (int i = 0; i < this.devwidth; i++)
			{
				for (int j = 0; j < this.devheight; j++)
				{
					this.currentOutputFrame.SetPixel(i, j, Color.FromKnownColor(KnownColor.Black));
				}
			}
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00003A53 File Offset: 0x00001C53
		public IAuraLayer ReplaceBaseLayer(IAuraLayer layer)
		{
			if (this.layerList.Count<IAuraLayer>() == 0)
			{
				return null;
			}
			if (layer == null)
			{
				return null;
			}
			this.layerList.Insert(0, layer);
			IAuraLayer result = this.layerList[1];
			this.layerList.RemoveAt(1);
			return result;
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00003A8E File Offset: 0x00001C8E
		public IntPtr Update()
		{
			return this.UpdateBitmap().GetHbitmap();
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00003A9C File Offset: 0x00001C9C
		public Bitmap UpdateBitmap()
		{
			object renderLock = this._renderLock;
			Bitmap result;
			lock (renderLock)
			{
				using (Graphics graphics = Graphics.FromImage(this.currentOutputFrame))
				{
					graphics.Clear(Color.Black);
					for (int i = 0; i < this.layerList.Count; i++)
					{
						IAuraLayer auraLayer = this.layerList[i];
						if (auraLayer.isStaticFrame)
						{
							auraLayer.UpdateStaticeFrame();
							graphics.DrawImage(auraLayer.StaticBitmap, new Rectangle(auraLayer.layeroffset_x, auraLayer.layeroffset_y, auraLayer.StaticBitmap.Width, auraLayer.StaticBitmap.Height));
						}
						else if (this.globalFrameID >= auraLayer.startGlobalFrameID)
						{
							Bitmap bitmap = null;
							if (this.globalFrameID - auraLayer.startGlobalFrameID + 1 <= auraLayer.GetFrameCount())
							{
								bitmap = auraLayer.GetFrameAt(this.globalFrameID - auraLayer.startGlobalFrameID);
							}
							else if (auraLayer.repeatable)
							{
								auraLayer.startGlobalFrameID = this.globalFrameID;
								bitmap = auraLayer.GetFrameAt(this.globalFrameID - auraLayer.startGlobalFrameID);
							}
							if (bitmap == null)
							{
								LOGGER.DEBUG("Remove Layer #" + this.layerList.IndexOf(auraLayer).ToString(), Array.Empty<object>());
								this.layerList.Remove(auraLayer);
								i--;
							}
							else if (auraLayer.RotationDegree != 0)
							{
								GraphicsState gstate = graphics.Save();
								graphics.ResetTransform();
								graphics.TranslateTransform((float)(auraLayer.layeroffset_x + auraLayer.frameWidth / 2), (float)(auraLayer.layeroffset_y + auraLayer.frameHeight / 2), MatrixOrder.Append);
								graphics.RotateTransform((float)auraLayer.RotationDegree);
								if (auraLayer.ApplyMatrix)
								{
									float num = 1f;
									if (auraLayer.RotationDegree > 0 && auraLayer.RotationDegree <= 90)
									{
										num = (float)(270 - auraLayer.RotationDegree) / 270f;
									}
									if (auraLayer.RotationDegree > 90 && auraLayer.RotationDegree <= 180)
									{
										num = (float)(180 + auraLayer.RotationDegree - 90) / 270f;
									}
									if (auraLayer.RotationDegree > 180 && auraLayer.RotationDegree <= 270)
									{
										num = (float)(270 - (auraLayer.RotationDegree - 180)) / 270f;
									}
									if (auraLayer.RotationDegree > 270 && auraLayer.RotationDegree <= 359)
									{
										num = (float)(180 + auraLayer.RotationDegree - 270) / 270f;
									}
									graphics.DrawImage(bitmap, new Rectangle((int)((float)(-(float)bitmap.Width) * num / 2f), (int)(-((float)bitmap.Height / num) / 2f), (int)((float)bitmap.Width * num), (int)((float)bitmap.Height / num)));
								}
								else
								{
									graphics.DrawImage(bitmap, new Rectangle(-bitmap.Width / 2, -bitmap.Height / 2, bitmap.Width, bitmap.Height));
								}
								graphics.Restore(gstate);
							}
							else if (auraLayer.IsShowAsMatrixDefault)
							{
								graphics.DrawImage(bitmap, new Rectangle(auraLayer.layeroffset_x - auraLayer.frameWidth, auraLayer.layeroffset_y - auraLayer.frameHeight, bitmap.Width, bitmap.Height));
							}
							else
							{
								graphics.DrawImage(bitmap, new Rectangle(auraLayer.layeroffset_x, auraLayer.layeroffset_y, bitmap.Width, bitmap.Height));
							}
						}
					}
				}
				this.globalFrameID++;
				if (this.globalFrameID % 1000 == 0)
				{
					LOGGER.EnforceLogLimits();
				}
				if (this._contrastThreshold != 0)
				{
					this.currentOutputFrame = AuraAnimation.Contrast(this.currentOutputFrame, this._contrastThreshold);
				}
				if (this._brightnessThreshold != 0)
				{
					this.currentOutputFrame = AuraAnimation.Brightness(this.currentOutputFrame, this._brightnessThreshold);
				}
				result = this.currentOutputFrame;
			}
			return result;
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00003EDC File Offset: 0x000020DC
		public IntPtr UpdateFromTime(int timeFromAnimationStart)
		{
			Bitmap bitmap = this.UpdateFromTimeBitmap(timeFromAnimationStart);
			if (bitmap == null)
			{
				return IntPtr.Zero;
			}
			return bitmap.GetHbitmap();
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00003F00 File Offset: 0x00002100
		public Bitmap UpdateFromTimeBitmap(int timeFromAnimationStart)
		{
			object renderLock = this._renderLock;
			Bitmap result;
			lock (renderLock)
			{
				bool flag2 = false;
				bool flag3 = false;
				using (Graphics graphics = Graphics.FromImage(this.currentOutputFrame))
				{
					graphics.Clear(Color.Black);
					for (int i = 0; i < this.layerList.Count; i++)
					{
						IAuraLayer auraLayer = this.layerList[i];
						if (auraLayer.isStaticFrame)
						{
							auraLayer.UpdateStaticeFrame();
							graphics.DrawImage(auraLayer.StaticBitmap, new Rectangle(auraLayer.layeroffset_x, auraLayer.layeroffset_y, auraLayer.StaticBitmap.Width, auraLayer.StaticBitmap.Height));
							flag3 = true;
						}
						else if (auraLayer.timeStart > timeFromAnimationStart)
						{
							flag2 = true;
						}
						else if (timeFromAnimationStart < auraLayer.timeStart + auraLayer.timeLength)
						{
							Bitmap pictureFrame = auraLayer.GetPictureFrame(timeFromAnimationStart - auraLayer.timeStart);
							if (pictureFrame == null)
							{
								flag3 = true;
							}
							else
							{
								if (auraLayer.RotationDegree != 0)
								{
									GraphicsState gstate = graphics.Save();
									graphics.ResetTransform();
									graphics.TranslateTransform((float)(auraLayer.layeroffset_x + auraLayer.frameWidth / 2), (float)(auraLayer.layeroffset_y + auraLayer.frameHeight / 2), MatrixOrder.Append);
									graphics.RotateTransform((float)auraLayer.RotationDegree);
									if (auraLayer.ApplyMatrix)
									{
										float num = 1f;
										if (auraLayer.RotationDegree > 0 && auraLayer.RotationDegree <= 90)
										{
											num = (float)(270 - auraLayer.RotationDegree) / 270f;
										}
										if (auraLayer.RotationDegree > 90 && auraLayer.RotationDegree <= 180)
										{
											num = (float)(180 + auraLayer.RotationDegree - 90) / 270f;
										}
										if (auraLayer.RotationDegree > 180 && auraLayer.RotationDegree <= 270)
										{
											num = (float)(270 - (auraLayer.RotationDegree - 180)) / 270f;
										}
										if (auraLayer.RotationDegree > 270 && auraLayer.RotationDegree <= 359)
										{
											num = (float)(180 + auraLayer.RotationDegree - 270) / 270f;
										}
										graphics.DrawImage(pictureFrame, new Rectangle((int)((float)(-(float)pictureFrame.Width) * num / 2f), (int)(-((float)pictureFrame.Height / num) / 2f), (int)((float)pictureFrame.Width * num), (int)((float)pictureFrame.Height / num)));
									}
									else
									{
										graphics.DrawImage(pictureFrame, new Rectangle(-pictureFrame.Width / 2, -pictureFrame.Height / 2, pictureFrame.Width, pictureFrame.Height));
									}
									graphics.Restore(gstate);
								}
								else if (auraLayer.IsShowAsMatrixDefault)
								{
									graphics.DrawImage(pictureFrame, new Rectangle(auraLayer.layeroffset_x - auraLayer.frameWidth, auraLayer.layeroffset_y - auraLayer.frameHeight, pictureFrame.Width, pictureFrame.Height));
								}
								else
								{
									graphics.Clear(Color.FromArgb(0, 0, 0, 0));
									graphics.DrawImage(pictureFrame, new Rectangle(auraLayer.layeroffset_x, auraLayer.layeroffset_y, pictureFrame.Width, pictureFrame.Height));
								}
								flag3 = true;
							}
						}
					}
				}
				if (this._contrastThreshold != 0)
				{
					this.currentOutputFrame = AuraAnimation.Contrast(this.currentOutputFrame, this._contrastThreshold);
				}
				if (this._brightnessThreshold != 0)
				{
					this.currentOutputFrame = AuraAnimation.Brightness(this.currentOutputFrame, this._brightnessThreshold);
				}
				if (!flag3 && !flag2)
				{
					result = null;
				}
				else
				{
					result = this.currentOutputFrame;
				}
			}
			return result;
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600004D RID: 77 RVA: 0x000042E0 File Offset: 0x000024E0
		// (set) Token: 0x0600004E RID: 78 RVA: 0x000042E8 File Offset: 0x000024E8
		public int ContrastThreshold
		{
			get
			{
				return this._contrastThreshold;
			}
			set
			{
				this._contrastThreshold = value;
				if (value > 100)
				{
					this._contrastThreshold = 100;
				}
				if (value < -100)
				{
					this._contrastThreshold = -100;
				}
			}
		}

		// Token: 0x0600004F RID: 79 RVA: 0x0000430C File Offset: 0x0000250C
		private static Bitmap Contrast(Bitmap sourceBitmap, int threshold)
		{
			BitmapData bitmapData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			byte[] array = new byte[bitmapData.Stride * bitmapData.Height];
			Marshal.Copy(bitmapData.Scan0, array, 0, array.Length);
			sourceBitmap.UnlockBits(bitmapData);
			double num = Math.Pow((100.0 + (double)threshold) / 100.0, 2.0);
			int num2 = 0;
			while (num2 + 4 < array.Length)
			{
				if (array[num2] != 0 || array[num2 + 1] != 0 || array[num2 + 2] != 0)
				{
					double num3 = (((double)array[num2] / 255.0 - 0.5) * num + 0.5) * 255.0;
					double num4 = (((double)array[num2 + 1] / 255.0 - 0.5) * num + 0.5) * 255.0;
					double num5 = (((double)array[num2 + 2] / 255.0 - 0.5) * num + 0.5) * 255.0;
					if (num3 > 255.0)
					{
						num3 = 255.0;
					}
					else if (num3 < 0.0)
					{
						num3 = 0.0;
					}
					if (num4 > 255.0)
					{
						num4 = 255.0;
					}
					else if (num4 < 0.0)
					{
						num4 = 0.0;
					}
					if (num5 > 255.0)
					{
						num5 = 255.0;
					}
					else if (num5 < 0.0)
					{
						num5 = 0.0;
					}
					array[num2] = (byte)num3;
					array[num2 + 1] = (byte)num4;
					array[num2 + 2] = (byte)num5;
				}
				num2 += 4;
			}
			Bitmap bitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			Marshal.Copy(array, 0, bitmapData2.Scan0, array.Length);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000050 RID: 80 RVA: 0x0000456E File Offset: 0x0000276E
		// (set) Token: 0x06000051 RID: 81 RVA: 0x00004576 File Offset: 0x00002776
		public int BrightnessThreshold
		{
			get
			{
				return this._brightnessThreshold;
			}
			set
			{
				this._brightnessThreshold = value;
				if (value > 100)
				{
					this._brightnessThreshold = 100;
				}
				if (value < -100)
				{
					this._brightnessThreshold = -100;
				}
			}
		}

		// Token: 0x06000052 RID: 82 RVA: 0x0000459C File Offset: 0x0000279C
		public static Bitmap Brightness(Bitmap sourceBitmap, int threshold)
		{
			BitmapData bitmapData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			byte[] array = new byte[bitmapData.Stride * bitmapData.Height];
			Marshal.Copy(bitmapData.Scan0, array, 0, array.Length);
			sourceBitmap.UnlockBits(bitmapData);
			int num = 0;
			while (num + 4 < array.Length)
			{
				double num2 = (double)array[num + 2];
				double num3 = (double)array[num + 1];
				double num4 = (double)array[num];
				if (num2 != 0.0 || num3 != 0.0 || num4 != 0.0)
				{
					if (threshold > 0)
					{
						num2 = (255.0 - num2) * (double)threshold / 100.0 + num2;
						num3 = (255.0 - num3) * (double)threshold / 100.0 + num3;
						num4 = (255.0 - num4) * (double)threshold / 100.0 + num4;
					}
					else
					{
						num2 *= (double)threshold / 100.0 + 1.0;
						num3 *= (double)threshold / 100.0 + 1.0;
						num4 *= (double)threshold / 100.0 + 1.0;
					}
					if (num4 > 255.0)
					{
						num4 = 255.0;
					}
					else if (num4 < 0.0)
					{
						num4 = 0.0;
					}
					if (num3 > 255.0)
					{
						num3 = 255.0;
					}
					else if (num3 < 0.0)
					{
						num3 = 0.0;
					}
					if (num2 > 255.0)
					{
						num2 = 255.0;
					}
					else if (num2 < 0.0)
					{
						num2 = 0.0;
					}
					array[num] = (byte)num4;
					array[num + 1] = (byte)num3;
					array[num + 2] = (byte)num2;
				}
				num += 4;
			}
			Bitmap bitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
			BitmapData bitmapData2 = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			Marshal.Copy(array, 0, bitmapData2.Scan0, array.Length);
			bitmap.UnlockBits(bitmapData2);
			return bitmap;
		}

		// Token: 0x06000053 RID: 83 RVA: 0x0000480A File Offset: 0x00002A0A
		public void SetMatrixParameter(int Width, int Height, int SlashHeight)
		{
			this.MatrixParameterWidth = Width;
			this.MatrixParameterHeight = Height;
			this.MatrixParameterSlashHeight = SlashHeight;
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00004821 File Offset: 0x00002A21
		public int GetMatrixParameterWidth()
		{
			return this.MatrixParameterWidth;
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00004829 File Offset: 0x00002A29
		public int GetMatrixParameterHeight()
		{
			return this.MatrixParameterHeight;
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00004831 File Offset: 0x00002A31
		public int GetMatrixParameterSlashHeight()
		{
			return this.MatrixParameterSlashHeight;
		}

		// Token: 0x04000028 RID: 40
		private const int MAX_LAYER_COUNT = 10;

		// Token: 0x04000029 RID: 41
		private const int DEFAULT_FRAME_INTERVAL = 100;

		// Token: 0x0400002A RID: 42
		private List<IAuraLayer> layerList;

		// Token: 0x0400002B RID: 43
		private int globalFrameID;

		// Token: 0x0400002C RID: 44
		private Bitmap currentOutputFrame;

		// Token: 0x0400002D RID: 45
		private Bitmap blackBitmap;

		// Token: 0x0400002E RID: 46
		private int devwidth;

		// Token: 0x0400002F RID: 47
		private int devheight;

		// Token: 0x04000030 RID: 48
		private readonly object _renderLock = new object();

		// Token: 0x04000031 RID: 49
		private int _contrastThreshold;

		// Token: 0x04000032 RID: 50
		private int _brightnessThreshold;

		// Token: 0x04000033 RID: 51
		private static int refcount = 0;

		// Token: 0x04000034 RID: 52
		private int id;

		// Token: 0x04000035 RID: 53
		private int MatrixParameterWidth = 68;

		// Token: 0x04000036 RID: 54
		private int MatrixParameterHeight = 28;

		// Token: 0x04000037 RID: 55
		private int MatrixParameterSlashHeight = 36;

		// Token: 0x04000039 RID: 57
		public static string x86Name = "AURA lighting effect add-on";

		// Token: 0x0400003A RID: 58
		public static string x64Name = "AURA lighting effect add-on x64";
	}
}
