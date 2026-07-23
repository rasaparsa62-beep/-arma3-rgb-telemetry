using System;
using NAudio.Dsp;

// Token: 0x02000002 RID: 2
public class FftSpectrumAnalyzer
{
	// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
	public FftSpectrumAnalyzer(int bands)
	{
		this._bands = bands;
		this.spectrum = new int[this._bands];
		this._fftLength = 8192;
		this.fftBuffer = new Complex[this._fftLength];
		for (int i = 0; i < this._fftLength; i++)
		{
			this.fftBuffer[i] = default(Complex);
		}
		this._fftPos = 0;
	}

	// Token: 0x06000002 RID: 2 RVA: 0x000020E8 File Offset: 0x000002E8
	public int[] ProcessAudioData(byte[] buffer, int bytesRecorded)
	{
		int num = 2;
		int num2 = bytesRecorded / (num * this._channels);
		int num3 = 0;
		while (num3 < num2 && this._fftPos < this._fftLength - 1 && num3 * this._channels * num + num * this._channels - 1 < buffer.Length)
		{
			int num4 = num3 * this._channels * num;
			if (num4 + 1 >= buffer.Length || num4 + 3 >= buffer.Length)
			{
				break;
			}
			float num5 = (float)((short)((int)buffer[num4 + 1] << 8 | (int)buffer[num4]));
			short num6 = (short)((int)buffer[num4 + 3] << 8 | (int)buffer[num4 + 2]);
			float num7 = (num5 + (float)num6) / 2f / 32768f;
			float num8 = (float)(0.5 * (1.0 - Math.Cos(6.2831853071795862 * (double)this._fftPos / (double)(this._fftLength - 1))));
			if (this._fftPos < this.fftBuffer.Length)
			{
				this.fftBuffer[this._fftPos].X = num7 * num8;
				this.fftBuffer[this._fftPos].Y = 0f;
				this._fftPos++;
			}
			num3++;
		}
		if ((double)this._fftPos >= (double)this._fftLength * 0.75)
		{
			try
			{
				FastFourierTransform.FFT(true, (int)Math.Log((double)this._fftLength, 2.0), this.fftBuffer);
				this.CalculateSpectrum(this.fftBuffer);
			}
			catch (Exception ex)
			{
				Console.WriteLine("FFT 處理錯誤: " + ex.Message);
			}
			this._fftPos = 0;
		}
		return this.spectrum;
	}

	// Token: 0x06000003 RID: 3 RVA: 0x0000229C File Offset: 0x0000049C
	private void CalculateSpectrum(Complex[] fftBuffer)
	{
		Array.Clear(this.spectrum, 0, this.spectrum.Length);
		try
		{
			for (int i = 1; i < this._fftLength / 2; i++)
			{
				int num = (int)(Math.Log((double)(i * this._sampleRate / this._fftLength) / 20.0, 2.0) * (double)this.spectrum.Length / Math.Log((double)this._sampleRate / 40.0, 2.0));
				if (num >= 0 && num < this.spectrum.Length)
				{
					int num2 = (int)(Math.Sqrt((double)(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y)) * (double)this._maxSpectrumValue * 10.0);
					if (num2 > this._maxSpectrumValue)
					{
						num2 = this._maxSpectrumValue;
					}
					if (num2 > this.spectrum[num])
					{
						this.spectrum[num] = num2;
					}
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("頻譜計算錯誤: " + ex.Message);
		}
	}

	// Token: 0x06000004 RID: 4 RVA: 0x000023D4 File Offset: 0x000005D4
	public int[] GetSmoothedSpectrum()
	{
		if (this._lastSpectrum == null)
		{
			this._lastSpectrum = new int[this.spectrum.Length];
		}
		for (int i = 0; i < this.spectrum.Length; i++)
		{
			this._lastSpectrum[i] = (this._lastSpectrum[i] * 2 + this.spectrum[i]) / 3;
		}
		return this._lastSpectrum;
	}

	// Token: 0x04000001 RID: 1
	private int _bands = 32;

	// Token: 0x04000002 RID: 2
	private int _maxSpectrumValue = 255;

	// Token: 0x04000003 RID: 3
	private Complex[] fftBuffer;

	// Token: 0x04000004 RID: 4
	private int[] spectrum;

	// Token: 0x04000005 RID: 5
	private int _fftLength;

	// Token: 0x04000006 RID: 6
	private int _fftPos;

	// Token: 0x04000007 RID: 7
	private readonly int _sampleRate = 44100;

	// Token: 0x04000008 RID: 8
	private readonly int _channels = 2;

	// Token: 0x04000009 RID: 9
	private int[] _lastSpectrum;
}
