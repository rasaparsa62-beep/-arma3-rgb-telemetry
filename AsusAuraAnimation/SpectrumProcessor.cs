using System;
using NAudio.Dsp;
using NAudio.Wave;

namespace AsusAuraAnimation
{
	// Token: 0x0200001F RID: 31
	public class SpectrumProcessor
	{
		// Token: 0x06000290 RID: 656 RVA: 0x00015B69 File Offset: 0x00013D69
		public SpectrumProcessor(int fftLength = 1024, int numBars = 256)
		{
			this.fftLength = fftLength;
			this.numBars = numBars;
			this.fftBuffer = new Complex[fftLength];
			this.resultBuffer = new float[numBars];
			this.smoothedBuffer = new float[numBars];
		}

		// Token: 0x06000291 RID: 657 RVA: 0x00015BA4 File Offset: 0x00013DA4
		public void Process(WaveInEventArgs e, IWaveIn source, Action<float[]> onProcessed)
		{
			WaveFormat waveFormat = source.WaveFormat;
			int channels = waveFormat.Channels;
			bool flag = waveFormat.Encoding == 3;
			int num = flag ? 4 : (waveFormat.BitsPerSample / 8);
			int num2 = e.BytesRecorded / num;
			for (int i = 0; i < num2; i++)
			{
				float x;
				if (flag)
				{
					x = BitConverter.ToSingle(e.Buffer, i * 4);
				}
				else
				{
					if (waveFormat.BitsPerSample != 16)
					{
						return;
					}
					x = (float)BitConverter.ToInt16(e.Buffer, i * 2) / 32768f;
				}
				if (channels <= 1 || i % channels == 0)
				{
					if (this.fftPos < this.fftBuffer.Length)
					{
						this.fftBuffer[this.fftPos].X = x;
						this.fftBuffer[this.fftPos].Y = 0f;
						this.fftPos++;
					}
					else
					{
						this.ApplyHannWindow(this.fftBuffer);
						FastFourierTransform.FFT(true, (int)Math.Log((double)this.fftBuffer.Length, 2.0), this.fftBuffer);
						for (int j = 0; j < this.numBars; j++)
						{
							float num3 = (float)j / (float)(this.numBars - 1);
							float num4 = 0.6f;
							float num6;
							if (num3 < num4)
							{
								float num5 = num3 / num4;
								num6 = 40f * (float)Math.Pow(12.0, (double)num5);
							}
							else
							{
								float num7 = (num3 - num4) / (1f - num4);
								num6 = 500f * (float)Math.Pow(24.0, (double)num7);
							}
							int num8 = (int)(num6 * (float)this.fftBuffer.Length / 44100f);
							if (num8 >= this.fftBuffer.Length / 2)
							{
								num8 = this.fftBuffer.Length / 2 - 1;
							}
							float num9 = (float)Math.Sqrt((double)(this.fftBuffer[num8].X * this.fftBuffer[num8].X + this.fftBuffer[num8].Y * this.fftBuffer[num8].Y));
							this.resultBuffer[j] = ((num9 > 0.0015f) ? num9 : 0f);
							this.smoothedBuffer[j] = this.smoothedBuffer[j] * 0.7f + this.resultBuffer[j] * 0.3f;
						}
						this.fftPos = 0;
						if (onProcessed != null)
						{
							onProcessed(this.smoothedBuffer);
						}
					}
				}
			}
		}

		// Token: 0x06000292 RID: 658 RVA: 0x00015E30 File Offset: 0x00014030
		private void ApplyHannWindow(Complex[] buffer)
		{
			int num = buffer.Length;
			for (int i = 0; i < num; i++)
			{
				float num2 = (float)(0.5 * (1.0 - Math.Cos(6.2831853071795862 * (double)i / (double)(num - 1))));
				int num3 = i;
				buffer[num3].X = buffer[num3].X * num2;
				int num4 = i;
				buffer[num4].Y = buffer[num4].Y * num2;
			}
		}

		// Token: 0x04000122 RID: 290
		private readonly Complex[] fftBuffer;

		// Token: 0x04000123 RID: 291
		private readonly float[] resultBuffer;

		// Token: 0x04000124 RID: 292
		private readonly float[] smoothedBuffer;

		// Token: 0x04000125 RID: 293
		private readonly int fftLength;

		// Token: 0x04000126 RID: 294
		private readonly int numBars;

		// Token: 0x04000127 RID: 295
		private const float smoothFactor = 0.7f;

		// Token: 0x04000128 RID: 296
		private const float threshold = 0.0015f;

		// Token: 0x04000129 RID: 297
		private int fftPos;
	}
}
