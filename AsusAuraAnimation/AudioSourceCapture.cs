using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace AsusAuraAnimation
{
	// Token: 0x0200000B RID: 11
	public class AudioSourceCapture : IAudioSource
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000087 RID: 135 RVA: 0x00005F90 File Offset: 0x00004190
		// (remove) Token: 0x06000088 RID: 136 RVA: 0x00005FC8 File Offset: 0x000041C8
		public event EventHandler<float[]> SpectrumDataAvailable;

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000089 RID: 137 RVA: 0x00005FFD File Offset: 0x000041FD
		public string CurrentDeviceId
		{
			get
			{
				MMDevice currentRenderDevice = this._currentRenderDevice;
				if (currentRenderDevice == null)
				{
					return null;
				}
				return currentRenderDevice.ID;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600008A RID: 138 RVA: 0x00006010 File Offset: 0x00004210
		// (set) Token: 0x0600008B RID: 139 RVA: 0x00006018 File Offset: 0x00004218
		public bool IsRunning { get; private set; }

		// Token: 0x0600008C RID: 140 RVA: 0x00006024 File Offset: 0x00004224
		public AudioSourceCapture(IWaveIn waveIn, int fftLength = 1024)
		{
			this.waveIn = waveIn;
			this.spectrumProcessor = new SpectrumProcessor(fftLength, 256);
			this.waveIn.DataAvailable += this.OnDataAvailable;
			this.waveIn.RecordingStopped += delegate(object s, StoppedEventArgs e)
			{
				try
				{
					EventHandler<float[]> spectrumDataAvailable = this.SpectrumDataAvailable;
					if (spectrumDataAvailable != null)
					{
						spectrumDataAvailable(this, Array.Empty<float>());
					}
				}
				catch
				{
				}
				finally
				{
					this.IsRunning = false;
				}
			};
		}

		// Token: 0x0600008D RID: 141 RVA: 0x0000607D File Offset: 0x0000427D
		private void OnDataAvailable(object sender, WaveInEventArgs e)
		{
			this.spectrumProcessor.Process(e, this.waveIn, delegate(float[] spectrum)
			{
				EventHandler<float[]> spectrumDataAvailable = this.SpectrumDataAvailable;
				if (spectrumDataAvailable == null)
				{
					return;
				}
				spectrumDataAvailable(this, spectrum);
			});
		}

		// Token: 0x0600008E RID: 142 RVA: 0x0000609D File Offset: 0x0000429D
		public void Start()
		{
			if (!this.IsRunning)
			{
				this.waveIn.StartRecording();
				this.IsRunning = true;
			}
		}

		// Token: 0x0600008F RID: 143 RVA: 0x000060B9 File Offset: 0x000042B9
		public void Stop()
		{
			if (this.IsRunning)
			{
				this.waveIn.StopRecording();
				this.IsRunning = false;
			}
		}

		// Token: 0x06000090 RID: 144 RVA: 0x000060D8 File Offset: 0x000042D8
		public static AudioSourceCapture CreateSystemAudioCapture(int fftLength = 1024)
		{
			MMDevice defaultAudioEndpoint = new MMDeviceEnumerator().GetDefaultAudioEndpoint(0, 1);
			return new AudioSourceCapture(new WasapiLoopbackCapture(defaultAudioEndpoint), fftLength)
			{
				_currentRenderDevice = defaultAudioEndpoint
			};
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00006108 File Offset: 0x00004308
		public void RecreateLoopbackFromDefault()
		{
			if (!(this.waveIn is WasapiLoopbackCapture))
			{
				return;
			}
			bool isRunning = this.IsRunning;
			try
			{
				this.Stop();
			}
			catch
			{
			}
			try
			{
				try
				{
					this.waveIn.DataAvailable -= this.OnDataAvailable;
				}
				catch
				{
				}
				try
				{
					this.waveIn.Dispose();
				}
				catch
				{
				}
				MMDeviceEnumerator mmdeviceEnumerator = new MMDeviceEnumerator();
				MMDevice mmdevice = null;
				try
				{
					mmdevice = mmdeviceEnumerator.GetDefaultAudioEndpoint(0, 0);
				}
				catch
				{
				}
				if (mmdevice == null)
				{
					mmdevice = mmdeviceEnumerator.GetDefaultAudioEndpoint(0, 1);
				}
				this._currentRenderDevice = mmdevice;
				WasapiLoopbackCapture wasapiLoopbackCapture = new WasapiLoopbackCapture(mmdevice);
				this.waveIn = wasapiLoopbackCapture;
				this.waveIn.DataAvailable += this.OnDataAvailable;
				if (isRunning)
				{
					this.Start();
				}
			}
			catch
			{
				try
				{
					EventHandler<float[]> spectrumDataAvailable = this.SpectrumDataAvailable;
					if (spectrumDataAvailable != null)
					{
						spectrumDataAvailable(this, Array.Empty<float>());
					}
				}
				catch
				{
				}
				this.IsRunning = false;
			}
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00006230 File Offset: 0x00004430
		public static AudioSourceCapture CreateMicAudioCapture(int sampleRate = 44100, int channels = 1)
		{
			return new AudioSourceCapture(new WaveInEvent
			{
				WaveFormat = new WaveFormat(sampleRate, channels),
				DeviceNumber = 0
			}, sampleRate / 50);
		}

		// Token: 0x0400005B RID: 91
		private IWaveIn waveIn;

		// Token: 0x0400005C RID: 92
		private readonly SpectrumProcessor spectrumProcessor;

		// Token: 0x0400005E RID: 94
		private MMDevice _currentRenderDevice;
	}
}
