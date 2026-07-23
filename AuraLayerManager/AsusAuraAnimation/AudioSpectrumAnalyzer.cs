using System;
using System.Timers;

namespace AsusAuraAnimation
{
	// Token: 0x0200000C RID: 12
	public class AudioSpectrumAnalyzer : IDisposable
	{
		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000095 RID: 149 RVA: 0x000062B8 File Offset: 0x000044B8
		public float[] LatestSpectrum
		{
			get
			{
				return this.latestSpectrum;
			}
		}

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000096 RID: 150 RVA: 0x000062C0 File Offset: 0x000044C0
		// (remove) Token: 0x06000097 RID: 151 RVA: 0x000062F8 File Offset: 0x000044F8
		public event EventHandler<float[]> SpectrumUpdated;

		// Token: 0x06000098 RID: 152 RVA: 0x00006330 File Offset: 0x00004530
		public AudioSpectrumAnalyzer(AudioSourceType type, int fftLength = 1024)
		{
			if (type != AudioSourceType.System)
			{
				if (type != AudioSourceType.Microphone)
				{
					throw new ArgumentException("不支援的音訊來源");
				}
				this.audioSource = AudioSourceCapture.CreateMicAudioCapture(44100, 1);
			}
			else
			{
				this.audioSource = AudioSourceCapture.CreateSystemAudioCapture(fftLength);
				this._devWatcher = new AudioDeviceWatcher(0, delegate()
				{
					this.audioSource.RecreateLoopbackFromDefault();
					EventHandler<float[]> spectrumUpdated = this.SpectrumUpdated;
					if (spectrumUpdated == null)
					{
						return;
					}
					spectrumUpdated(this, new float[0]);
				});
			}
			this.audioSource.SpectrumDataAvailable += delegate(object s, float[] data)
			{
				this.latestSpectrum = ((data == null || data.Length == 0) ? new float[0] : data);
				this.lastDataAt = DateTime.UtcNow;
				EventHandler<float[]> spectrumUpdated = this.SpectrumUpdated;
				if (spectrumUpdated == null)
				{
					return;
				}
				spectrumUpdated(this, data);
			};
			this.idleTimer = new Timer(100.0);
			this.idleTimer.AutoReset = true;
			this.idleTimer.Elapsed += delegate(object s, ElapsedEventArgs e)
			{
				if (this.lastDataAt == DateTime.MinValue)
				{
					return;
				}
				if ((DateTime.UtcNow - this.lastDataAt).TotalMilliseconds > 200.0)
				{
					this.latestSpectrum = Array.Empty<float>();
					EventHandler<float[]> spectrumUpdated = this.SpectrumUpdated;
					if (spectrumUpdated != null)
					{
						spectrumUpdated(this, this.latestSpectrum);
					}
					this.lastDataAt = DateTime.MinValue;
				}
			};
			this.idleTimer.Start();
		}

		// Token: 0x06000099 RID: 153 RVA: 0x000063F4 File Offset: 0x000045F4
		public void Start()
		{
			this.audioSource.Start();
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00006401 File Offset: 0x00004601
		public void Stop()
		{
			this.audioSource.Stop();
			this.latestSpectrum = Array.Empty<float>();
			EventHandler<float[]> spectrumUpdated = this.SpectrumUpdated;
			if (spectrumUpdated != null)
			{
				spectrumUpdated(this, this.latestSpectrum);
			}
			this.lastDataAt = DateTime.MinValue;
		}

		// Token: 0x0600009B RID: 155 RVA: 0x0000643C File Offset: 0x0000463C
		public void Dispose()
		{
			try
			{
				AudioDeviceWatcher devWatcher = this._devWatcher;
				if (devWatcher != null)
				{
					devWatcher.Dispose();
				}
			}
			catch
			{
			}
			this.audioSource.Stop();
		}

		// Token: 0x0600009C RID: 156 RVA: 0x0000647C File Offset: 0x0000467C
		public byte[] GetSpectrum(int targetLength)
		{
			if (this.latestSpectrum == null || this.latestSpectrum.Length == 0)
			{
				return new byte[targetLength];
			}
			byte[] array = new byte[targetLength];
			if (targetLength == this.latestSpectrum.Length)
			{
				for (int i = 0; i < targetLength; i++)
				{
					array[i] = (byte)Math.Min(255f, Math.Max(0f, this.latestSpectrum[i] * 255f));
				}
				return array;
			}
			for (int j = 0; j < targetLength; j++)
			{
				int num = (int)((float)j / (float)targetLength * (float)this.latestSpectrum.Length);
				if (num >= this.latestSpectrum.Length)
				{
					num = this.latestSpectrum.Length - 1;
				}
				float num2 = this.latestSpectrum[num];
				array[j] = (byte)Math.Min(255f, Math.Max(0f, num2 * 255f));
			}
			return array;
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00006548 File Offset: 0x00004748
		public byte[] GetNormalizedSpectrum(int targetLength)
		{
			if (this.latestSpectrum == null || this.latestSpectrum.Length == 0)
			{
				return new byte[targetLength];
			}
			byte[] array = new byte[targetLength];
			int num = this.latestSpectrum.Length;
			float num2 = 0.0001f;
			foreach (float num3 in this.latestSpectrum)
			{
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
			for (int j = 0; j < targetLength; j++)
			{
				int num4 = j * num / targetLength;
				int num5 = (j + 1) * num / targetLength;
				num5 = Math.Min(num, num5);
				float num6 = 0f;
				for (int k = num4; k < num5; k++)
				{
					if (this.latestSpectrum[k] > num6)
					{
						num6 = this.latestSpectrum[k];
					}
				}
				float num7 = num6 / num2;
				array[j] = (byte)Math.Min(255f, num7 * 255f);
			}
			return array;
		}

		// Token: 0x04000060 RID: 96
		private readonly AudioSourceCapture audioSource;

		// Token: 0x04000061 RID: 97
		private float[] latestSpectrum;

		// Token: 0x04000062 RID: 98
		private Timer idleTimer;

		// Token: 0x04000063 RID: 99
		private DateTime lastDataAt = DateTime.MinValue;

		// Token: 0x04000064 RID: 100
		private AudioDeviceWatcher _devWatcher;
	}
}
