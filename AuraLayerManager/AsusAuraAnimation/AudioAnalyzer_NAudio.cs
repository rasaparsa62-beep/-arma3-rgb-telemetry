using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace AsusAuraAnimation
{
	// Token: 0x02000009 RID: 9
	[Guid("366B6AA9-29FB-4BC6-B8ED-AAA84F4FC6F0")]
	[ClassInterface(ClassInterfaceType.None)]
	public class AudioAnalyzer_NAudio : IAudioAnalyzer
	{
		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600006C RID: 108 RVA: 0x000058A8 File Offset: 0x00003AA8
		// (set) Token: 0x0600006D RID: 109 RVA: 0x000058B0 File Offset: 0x00003AB0
		public int BassCompressRatio
		{
			get
			{
				return this._bassCompressRatio;
			}
			set
			{
				this._bassCompressRatio = Math.Max(0, Math.Min(value, 50));
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600006E RID: 110 RVA: 0x000058C6 File Offset: 0x00003AC6
		// (set) Token: 0x0600006F RID: 111 RVA: 0x000058CE File Offset: 0x00003ACE
		public int TrebleCompressRatio
		{
			get
			{
				return this._trebleCompressRatio;
			}
			set
			{
				this._trebleCompressRatio = Math.Max(0, Math.Min(value, 50));
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x000058E4 File Offset: 0x00003AE4
		public AudioAnalyzer_NAudio()
		{
			LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer_NAudio::AudioAnalyzer_NAudio()+", Array.Empty<object>());
			this.systemAnalyzer = new AudioSpectrumAnalyzer(AudioSourceType.System, 1024);
			this.micAnalyzer = new AudioSpectrumAnalyzer(AudioSourceType.Microphone, 1024);
			LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer_NAudio::AudioAnalyzer_NAudio()-", Array.Empty<object>());
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00005964 File Offset: 0x00003B64
		protected override void Finalize()
		{
			try
			{
				AudioSpectrumAnalyzer audioSpectrumAnalyzer = this.systemAnalyzer;
				if (audioSpectrumAnalyzer != null)
				{
					audioSpectrumAnalyzer.Dispose();
				}
				AudioSpectrumAnalyzer audioSpectrumAnalyzer2 = this.micAnalyzer;
				if (audioSpectrumAnalyzer2 != null)
				{
					audioSpectrumAnalyzer2.Dispose();
				}
			}
			finally
			{
				base.Finalize();
			}
		}

		// Token: 0x06000072 RID: 114 RVA: 0x000059AC File Offset: 0x00003BAC
		public void Enable(bool isEnable, int index)
		{
			LOGGER.DEBUG(string.Concat(new string[]
			{
				"[AudioAnalyzer] AudioAnalyzer_NAudio::Enable(",
				isEnable.ToString(),
				",",
				index.ToString(),
				")+"
			}), Array.Empty<object>());
			object locker = this._locker;
			lock (locker)
			{
				if (this._enable == isEnable)
				{
					return;
				}
				this._enable = isEnable;
				if (this._enable)
				{
					if (this._useSystemAudio)
					{
						this.systemAnalyzer.Start();
					}
					if (this._useMicrophone)
					{
						this.micAnalyzer.Start();
					}
				}
				else
				{
					AudioSpectrumAnalyzer audioSpectrumAnalyzer = this.systemAnalyzer;
					if (audioSpectrumAnalyzer != null)
					{
						audioSpectrumAnalyzer.Stop();
					}
					AudioSpectrumAnalyzer audioSpectrumAnalyzer2 = this.micAnalyzer;
					if (audioSpectrumAnalyzer2 != null)
					{
						audioSpectrumAnalyzer2.Stop();
					}
				}
			}
			LOGGER.DEBUG(string.Concat(new string[]
			{
				"[AudioAnalyzer] AudioAnalyzer_NAudio::Enable(",
				isEnable.ToString(),
				",",
				index.ToString(),
				")-"
			}), Array.Empty<object>());
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00005AD0 File Offset: 0x00003CD0
		public void Enable2(bool isEnable, string devName)
		{
			this.Enable(isEnable, 0);
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00005ADA File Offset: 0x00003CDA
		public void Init()
		{
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00005ADC File Offset: 0x00003CDC
		public string GetAudioDeviceList()
		{
			LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer_NAudio::GetAudioDeviceList() return empty string", Array.Empty<object>());
			return "";
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00005AF2 File Offset: 0x00003CF2
		public int GetAudioDeviceCount()
		{
			LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer_NAudio::GetAudioDeviceCount() return 0", Array.Empty<object>());
			return 0;
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00005B04 File Offset: 0x00003D04
		public void SetAudioSource(bool useSystemAudio, bool useMicrophone)
		{
			LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer_NAudio::SetAudioSource()+ sys: " + useSystemAudio.ToString() + " mic: " + useMicrophone.ToString(), Array.Empty<object>());
			if (useSystemAudio == this._useSystemAudio && useMicrophone == this._useMicrophone)
			{
				return;
			}
			this._useSystemAudio = useSystemAudio;
			this._useMicrophone = useMicrophone;
			if (this._enable)
			{
				this.Enable(false, 0);
				Thread.Sleep(500);
				this.Enable(true, 0);
			}
			LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer_NAudio::SetAudioSource()-", Array.Empty<object>());
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00005B8C File Offset: 0x00003D8C
		public byte[] UpdateSpectrumData()
		{
			object locker = this._locker;
			byte[] result;
			lock (locker)
			{
				byte[] array = null;
				byte[] array2 = null;
				byte[] array3 = new byte[this._lines];
				if (this._useSystemAudio)
				{
					array = this.systemAnalyzer.GetNormalizedSpectrum(this._lines);
				}
				if (this._useMicrophone)
				{
					array2 = this.micAnalyzer.GetNormalizedSpectrum(this._lines);
				}
				for (int i = 0; i < this._lines; i++)
				{
					if (this._useSystemAudio && this._useMicrophone && array != null && array2 != null)
					{
						array3[i] = Math.Max(array[i], array2[i]);
					}
					else if (this._useSystemAudio && array != null)
					{
						array3[i] = array[i];
					}
					else if (this._useMicrophone && array2 != null)
					{
						array3[i] = array2[i];
					}
					else
					{
						array3[i] = 0;
					}
				}
				result = array3;
			}
			return result;
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00005C84 File Offset: 0x00003E84
		public ValueTuple<byte[], byte[]> UpdateSpectrumData2()
		{
			object locker = this._locker;
			ValueTuple<byte[], byte[]> result;
			lock (locker)
			{
				byte[] array = null;
				byte[] array2 = null;
				new byte[this._lines];
				if (this._useSystemAudio)
				{
					array = this.systemAnalyzer.GetNormalizedSpectrum(this._lines);
				}
				if (this._useMicrophone)
				{
					array2 = this.micAnalyzer.GetNormalizedSpectrum(this._lines);
				}
				LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer_NAudio::UpdateSpectrumData2()" + string.Join<byte>(",", array) + " --- " + string.Join<byte>(",", array2), Array.Empty<object>());
				result = new ValueTuple<byte[], byte[]>(array, array2);
			}
			return result;
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00005D3C File Offset: 0x00003F3C
		public byte GetSpectrumDataAt(int index)
		{
			byte[] array = this.UpdateSpectrumData();
			if (index < 0 || index >= array.Length)
			{
				throw new ArgumentOutOfRangeException("index", "Index is out of range.");
			}
			return array[index];
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00005D70 File Offset: 0x00003F70
		public double GetAverageSpectrumData()
		{
			byte[] array = this.UpdateSpectrumData();
			if (array.Length == 0)
			{
				return 0.0;
			}
			return array.Average((byte b) => (double)b);
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00005DB7 File Offset: 0x00003FB7
		public void SetLines(int lines)
		{
			this._lines = lines;
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00005DC0 File Offset: 0x00003FC0
		public string UpdateAudioDataString(int lines, int strength)
		{
			this.SetLines(lines);
			string result = string.Empty;
			try
			{
				if (!this._useMicrophone)
				{
					this.SetAudioSource(true, true);
				}
				ValueTuple<byte[], byte[]> valueTuple = this.UpdateSpectrumData2();
				byte[] item = valueTuple.Item1;
				byte[] item2 = valueTuple.Item2;
				result = string.Join<byte>(",", item ?? Array.Empty<byte>()) + "," + string.Join<byte>(",", item2 ?? Array.Empty<byte>());
			}
			catch (Exception ex)
			{
				LOGGER.DEBUG("[AudioANalyzer_NAudio] UpdateAudioDataString exception: " + ex.ToString(), Array.Empty<object>());
			}
			return result;
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00005E60 File Offset: 0x00004060
		public void Free()
		{
			LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer_NAudio:Free()", Array.Empty<object>());
			this.Enable(false, 0);
		}

		// Token: 0x0400004E RID: 78
		private bool _enable;

		// Token: 0x0400004F RID: 79
		private object _locker = new object();

		// Token: 0x04000050 RID: 80
		private int _lines = 68;

		// Token: 0x04000051 RID: 81
		private int _bassCompressRatio = 15;

		// Token: 0x04000052 RID: 82
		private int _trebleCompressRatio = 15;

		// Token: 0x04000053 RID: 83
		private bool _useSystemAudio = true;

		// Token: 0x04000054 RID: 84
		private bool _useMicrophone;

		// Token: 0x04000055 RID: 85
		private AudioSpectrumAnalyzer systemAnalyzer;

		// Token: 0x04000056 RID: 86
		private AudioSpectrumAnalyzer micAnalyzer;
	}
}
