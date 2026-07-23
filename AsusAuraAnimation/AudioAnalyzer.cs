using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace AsusAuraAnimation
{
	// Token: 0x02000008 RID: 8
	[Guid("1A9482E3-2C71-44DF-9012-A969577325B6")]
	[ClassInterface(ClassInterfaceType.None)]
	public class AudioAnalyzer : IAudioAnalyzer
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000058 RID: 88 RVA: 0x00004855 File Offset: 0x00002A55
		// (set) Token: 0x06000059 RID: 89 RVA: 0x0000485D File Offset: 0x00002A5D
		public int BassCompressRatio
		{
			get
			{
				return this._BassCompressRatio;
			}
			set
			{
				this._BassCompressRatio = value;
				if (value > 50)
				{
					this._BassCompressRatio = 50;
				}
				if (value < 0)
				{
					this._BassCompressRatio = 0;
				}
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600005A RID: 90 RVA: 0x0000487E File Offset: 0x00002A7E
		// (set) Token: 0x0600005B RID: 91 RVA: 0x00004886 File Offset: 0x00002A86
		public int TrebleCompressRatio
		{
			get
			{
				return this._TrebleCompressRatio;
			}
			set
			{
				this._TrebleCompressRatio = value;
				if (value > 50)
				{
					this._TrebleCompressRatio = 50;
				}
				if (value < 0)
				{
					this._TrebleCompressRatio = 0;
				}
			}
		}

		// Token: 0x0600005C RID: 92 RVA: 0x000048A7 File Offset: 0x00002AA7
		public AudioAnalyzer()
		{
			LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer()", Array.Empty<object>());
		}

		// Token: 0x0600005D RID: 93 RVA: 0x000048E4 File Offset: 0x00002AE4
		~AudioAnalyzer()
		{
			LOGGER.DEBUG("[AudioAnalyzer] ~AudioAnalyzer()+", Array.Empty<object>());
			if (this._enable)
			{
				this.Enable(false, this.devindex);
			}
			this.Free();
			LOGGER.DEBUG("[AudioAnalyzer] ~AudioAnalyzer()-", Array.Empty<object>());
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00004944 File Offset: 0x00002B44
		public void Enable(bool isEnable, int index)
		{
			object locker = AudioAnalyzer.Locker;
			lock (locker)
			{
				LOGGER.DEBUG(string.Concat(new string[]
				{
					"[AudioAnalyzer] AudioAnalyzer::Enable(",
					isEnable.ToString(),
					",",
					index.ToString(),
					")+"
				}), Array.Empty<object>());
				if (index != 0)
				{
					this._enable = isEnable;
					if (isEnable)
					{
						this.devindex = index;
						foreach (int num in AudioAnalyzer.devindexList)
						{
						}
						if (AudioAnalyzer.devindexList.IndexOf(index) == -1)
						{
							BASS_WASAPI_DEVICEINFO bass_WASAPI_DEVICEINFO = BassWasapi.BASS_WASAPI_GetDeviceInfo(this.devindex);
							if (bass_WASAPI_DEVICEINFO == null)
							{
								LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer::Enable device info is null for index {0}", new object[]
								{
									this.devindex
								});
								return;
							}
							bool flag2 = BassWasapi.BASS_WASAPI_Init(this.devindex, 0, bass_WASAPI_DEVICEINFO.mixchans, 6, 2f, 0.05f, AudioAnalyzer._process, IntPtr.Zero);
							LOGGER.DEBUG("[AudioAnalyzer] BASS_WASAPI_Init({0})", new object[]
							{
								this.devindex
							});
							if (!flag2)
							{
								LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer::Enable init fail! " + Bass.BASS_ErrorGetCode().ToString(), Array.Empty<object>());
								return;
							}
							BassWasapi.BASS_WASAPI_Start();
							LOGGER.DEBUG("[AudioAnalyzer] BASS_WASAPI_Start()", Array.Empty<object>());
						}
						AudioAnalyzer.devindexList.Add(this.devindex);
					}
					else
					{
						AudioAnalyzer.devindexList.Remove(index);
						if (AudioAnalyzer.devindexList.IndexOf(index) == -1)
						{
							BassWasapi.BASS_WASAPI_SetDevice(index);
							BassWasapi.BASS_WASAPI_Stop(true);
							LOGGER.DEBUG("[AudioAnalyzer] BASS_WASAPI_Stop()", Array.Empty<object>());
							BassWasapi.BASS_WASAPI_Free();
							LOGGER.DEBUG("[AudioAnalyzer] BASS_WASAPI_Free({0})", new object[]
							{
								index
							});
						}
					}
					LOGGER.DEBUG(string.Concat(new string[]
					{
						"[AudioAnalyzer] AudioAnalyzer::Enable(",
						isEnable.ToString(),
						",",
						index.ToString(),
						")-"
					}), Array.Empty<object>());
				}
			}
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00004BA8 File Offset: 0x00002DA8
		public void Enable2(bool isEnable, string devName)
		{
			int index = this.QueryAudioDeviceIDFromNameFromDictionary(devName);
			this.lastDevName = devName;
			this.Enable(isEnable, index);
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00004BCC File Offset: 0x00002DCC
		private string ParsingAudioDeviceName(string input)
		{
			return Regex.Match(input, "\\(([^)]*)\\)").Groups[1].Value;
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00004BEC File Offset: 0x00002DEC
		private int QueryAudioDeviceIDFromNameFromDictionary(string devName)
		{
			LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromNameFromDictionary({0})+", devName), Array.Empty<object>());
			if (!AudioAnalyzer.AudioDeviceDict.ContainsKey(devName))
			{
				return this.QueryAudioDeviceIDFromName(devName);
			}
			int num = AudioAnalyzer.AudioDeviceDict[devName];
			if (BassWasapi.BASS_WASAPI_GetDeviceInfo(num).name.Equals(devName))
			{
				LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromNameFromDictionary({0}) == {1}", devName, num), Array.Empty<object>());
				return num;
			}
			LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromNameFromDictionary({0}) not found", devName), Array.Empty<object>());
			return this.QueryAudioDeviceIDFromName(devName);
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00004C7C File Offset: 0x00002E7C
		private int QueryAudioDeviceIDFromName(string devName)
		{
			if (devName == null)
			{
				LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromName(null)", Array.Empty<object>()), Array.Empty<object>());
				return 0;
			}
			LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromName({0})+", devName), Array.Empty<object>());
			int num = 0;
			int num2 = BassWasapi.BASS_WASAPI_GetDeviceCount();
			for (int i = 0; i < num2; i++)
			{
				LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromName({0}) BASS_WASAPI_GetDeviceInfo+", devName), Array.Empty<object>());
				BASS_WASAPI_DEVICEINFO bass_WASAPI_DEVICEINFO = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
				LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromName({0}) BASS_WASAPI_GetDeviceInfo- : {1}", devName, bass_WASAPI_DEVICEINFO.name), Array.Empty<object>());
				if (bass_WASAPI_DEVICEINFO.IsEnabled && bass_WASAPI_DEVICEINFO.IsLoopback)
				{
					if (num == 0)
					{
						num = i;
					}
					LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromName({0}) compare to {1}:{2}:Channel count={3}, Freq={4}", new object[]
					{
						devName,
						i,
						bass_WASAPI_DEVICEINFO.ToString(),
						bass_WASAPI_DEVICEINFO.mixchans,
						bass_WASAPI_DEVICEINFO.mixfreq
					}), Array.Empty<object>());
					if (!AudioAnalyzer.AudioDeviceDict.ContainsKey(bass_WASAPI_DEVICEINFO.name))
					{
						AudioAnalyzer.AudioDeviceDict.Add(bass_WASAPI_DEVICEINFO.name, i);
					}
					else
					{
						AudioAnalyzer.AudioDeviceDict[bass_WASAPI_DEVICEINFO.name] = i;
					}
					if (bass_WASAPI_DEVICEINFO.name.Equals(devName))
					{
						LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromName({0}) = {1}", devName, i), Array.Empty<object>());
						return i;
					}
				}
			}
			LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromName({0}) not found, try to compare device name only", devName), Array.Empty<object>());
			for (int j = 0; j < num2; j++)
			{
				LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromName({0}) BASS_WASAPI_GetDeviceInfo+", devName), Array.Empty<object>());
				BASS_WASAPI_DEVICEINFO bass_WASAPI_DEVICEINFO2 = BassWasapi.BASS_WASAPI_GetDeviceInfo(j);
				LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromName({0}) BASS_WASAPI_GetDeviceInfo-", devName), Array.Empty<object>());
				if (bass_WASAPI_DEVICEINFO2.IsEnabled && bass_WASAPI_DEVICEINFO2.IsLoopback)
				{
					LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromName:dev({0}) ParsingAudioDeviceName compare to {1}:({2})", this.ParsingAudioDeviceName(devName), j, this.ParsingAudioDeviceName(bass_WASAPI_DEVICEINFO2.ToString())), Array.Empty<object>());
					if (this.ParsingAudioDeviceName(bass_WASAPI_DEVICEINFO2.name).Equals(this.ParsingAudioDeviceName(devName)))
					{
						LOGGER.DEBUG(string.Format("[AudioAnalyzer] AudioAnalyzer::QueryAudioDeviceIDFromName:dev({0}) = {1}", devName, j), Array.Empty<object>());
						return j;
					}
				}
			}
			return -3;
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00004EB0 File Offset: 0x000030B0
		public string GetAudioDeviceList()
		{
			LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer::GetAudioDeviceList()", Array.Empty<object>());
			string text = string.Format("", Array.Empty<object>());
			int num = BassWasapi.BASS_WASAPI_GetDeviceCount();
			for (int i = 0; i < num; i++)
			{
				BASS_WASAPI_DEVICEINFO bass_WASAPI_DEVICEINFO = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
				if (bass_WASAPI_DEVICEINFO.IsEnabled && bass_WASAPI_DEVICEINFO.IsLoopback)
				{
					text += string.Format("{0},{1},", i, bass_WASAPI_DEVICEINFO.name);
				}
			}
			return text;
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00004F23 File Offset: 0x00003123
		public int GetAudioDeviceCount()
		{
			LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer::GetAudioDeviceCount()", Array.Empty<object>());
			return BassWasapi.BASS_WASAPI_GetDeviceCount();
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00004F3C File Offset: 0x0000313C
		public void Init()
		{
			object locker = AudioAnalyzer.Locker;
			lock (locker)
			{
				LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer::Init()+", Array.Empty<object>());
				this._fft = new float[8192];
				this._fftPtr = Marshal.AllocCoTaskMem(32768);
				this._lastlevel = 0;
				this._hanctr = 0;
				this._spectrumdata = new List<byte>();
				Bass.BASS_SetConfig(24, 4);
				if (AudioAnalyzer.refcount == 0)
				{
					AudioAnalyzer._process = new WASAPIPROC(AudioAnalyzer.Process);
					LOGGER.DEBUG("[AudioAnalyzer] Bass.BASS_Init()", Array.Empty<object>());
					if (!Bass.BASS_Init(0, 44100, 0, IntPtr.Zero))
					{
						LOGGER.DEBUG("[AudioAnalyzer] Analyzer:Init() Bass.BASS_Init fail! " + Bass.BASS_ErrorGetCode().ToString(), Array.Empty<object>());
					}
				}
				AudioAnalyzer.refcount++;
				LOGGER.DEBUG("[AudioAnalyzer] Analyzer:Init()- refcount=" + AudioAnalyzer.refcount.ToString(), Array.Empty<object>());
			}
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00005054 File Offset: 0x00003254
		public void SetLines(int lines)
		{
			this._lines = lines;
			if (lines < 1)
			{
				this._lines = 1;
			}
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00005068 File Offset: 0x00003268
		public void UpdateAudioData(int Strength)
		{
			object locker = AudioAnalyzer.Locker;
			lock (locker)
			{
				if (this.devindex == 0)
				{
					this.Invaildcount = (this.Invaildcount + 1) % 20;
					if (this.Invaildcount == 0)
					{
						int num = this.QueryAudioDeviceIDFromName(this.lastDevName);
						if (num != 0)
						{
							this.devindex = num;
							if (AudioAnalyzer.devindexList.IndexOf(num) == -1)
							{
								bool flag2 = BassWasapi.BASS_WASAPI_Init(this.devindex, 0, 2, 6, 1f, 0.05f, AudioAnalyzer._process, IntPtr.Zero);
								LOGGER.DEBUG("[AudioAnalyzer] BASS_WASAPI_Init()", Array.Empty<object>());
								if (!flag2)
								{
									LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer::Enable init fail! " + Bass.BASS_ErrorGetCode().ToString(), Array.Empty<object>());
									return;
								}
								BassWasapi.BASS_WASAPI_Start();
								LOGGER.DEBUG("[AudioAnalyzer] BASS_WASAPI_Start()", Array.Empty<object>());
							}
							AudioAnalyzer.devindexList.Add(this.devindex);
						}
					}
					this._spectrumdata.Clear();
					for (int i = 0; i < this._lines; i++)
					{
						this._spectrumdata.Add(0);
					}
				}
				else if (!BassWasapi.BASS_WASAPI_SetDevice(this.devindex))
				{
					this._spectrumdata.Clear();
					for (int j = 0; j < this._lines; j++)
					{
						this._spectrumdata.Add(0);
					}
					BassWasapi.BASS_WASAPI_Init(this.devindex, 0, 2, 6, 1f, 0.05f, AudioAnalyzer._process, IntPtr.Zero);
					BassWasapi.BASS_WASAPI_SetDevice(this.devindex);
					BassWasapi.BASS_WASAPI_Start();
				}
				else
				{
					int k = 0;
					this._spectrumdata.Clear();
					int num2 = BassWasapi.BASS_WASAPI_GetLevel();
					if (BassWasapi.BASS_WASAPI_GetDeviceLevel(this.devindex, -1) < 1E-05f)
					{
						num2 = 0;
					}
					if (num2 == -1)
					{
						LOGGER.DEBUG("[AudioAnalyzer] BassWasapi.BASS_WASAPI_GetLevel({0}) {1}", new object[]
						{
							this.devindex,
							Bass.BASS_ErrorGetCode().ToString()
						});
					}
					this.leftLevel = Utils.LowWord32(num2);
					this.rightLevel = Utils.HighWord32(num2);
					if (num2 == 0)
					{
						for (int l = 0; l < this._lines; l++)
						{
							this._spectrumdata.Add(0);
						}
					}
					else
					{
						if (this._lines == 1)
						{
							this._spectrumdata.Add(0);
							int num3 = this.leftLevel * (Strength + 100) / 100;
							if (num3 >> 7 > 255)
							{
								this._spectrumdata[0] = byte.MaxValue;
							}
							else
							{
								this._spectrumdata[0] = (byte)(num3 >> 7);
							}
						}
						else if (this._lines == 2)
						{
							this._spectrumdata.Add(0);
							this._spectrumdata.Add(0);
							int num4 = this.leftLevel * (Strength + 100) / 100;
							int num5 = this.rightLevel * (Strength + 100) / 100;
							if (num4 >> 7 > 255)
							{
								this._spectrumdata[0] = byte.MaxValue;
							}
							else
							{
								this._spectrumdata[0] = (byte)(num4 >> 7);
							}
							if (num5 >> 7 > 255)
							{
								this._spectrumdata[1] = byte.MaxValue;
							}
							else
							{
								this._spectrumdata[1] = (byte)(num5 >> 7);
							}
						}
						else
						{
							try
							{
								if (BassWasapi.BASS_WASAPI_GetData(this._fftPtr, -2147483645) < -1)
								{
									LOGGER.DEBUG("[AudioAnalyzer] [{1}]UpdateAudioData()- BASS_WASAPI_GetData({0}) fail! {2]", new object[]
									{
										this.devindex,
										Thread.CurrentThread.ManagedThreadId,
										Bass.BASS_ErrorGetCode().ToString()
									});
									return;
								}
								Marshal.Copy(this._fftPtr, this._fft, 0, this._fft.Length);
							}
							catch (Exception ex)
							{
								LOGGER.DEBUG("[AudioAnalyzer] [{1}]UpdateAudioData()- BASS_WASAPI_GetData({0}) ex = {2}", new object[]
								{
									this.devindex,
									Thread.CurrentThread.ManagedThreadId,
									ex.ToString()
								});
								return;
							}
							int num6 = this._lines * this._BassCompressRatio / 100;
							int num7 = this._lines * this._TrebleCompressRatio / 100;
							int num8 = this._lines + num6 + num7;
							List<int> list = new List<int>();
							for (int m = 0; m < num8; m++)
							{
								float num9 = 0f;
								int num10 = (int)Math.Pow(2.0, (double)m * 10.0 / (double)(num8 - 1));
								if (num10 > 1023)
								{
									num10 = 1023;
								}
								if (num10 <= k)
								{
									num10 = k + 1;
								}
								while (k < num10)
								{
									if (num9 < this._fft[1 + k])
									{
										num9 = this._fft[1 + k];
									}
									k++;
								}
								int num11 = (int)(Math.Sqrt((double)num9) * 3.0 * 255.0 - 4.0);
								num11 = num11 * (Strength + 100) / 100;
								if (num11 > 255)
								{
									num11 = 255;
								}
								if (num11 < 0)
								{
									num11 = 0;
								}
								list.Add(num11);
							}
							int num12 = 0;
							for (int n = 0; n < num6 + 1; n++)
							{
								num12 = Math.Max(num12, list[n]);
							}
							this._spectrumdata.Add((byte)num12);
							for (int m = 1; m < this._lines; m++)
							{
								this._spectrumdata.Add((byte)list[num6 + m]);
							}
						}
						if (num2 == this._lastlevel && num2 != 0)
						{
							this._hanctr++;
						}
						this._lastlevel = num2;
						if (this._hanctr > 3)
						{
							LOGGER.DEBUG("[AudioAnalyzer] {1}:[{0}]Analyze ==> _hanctr > 3, lastlevel={2} devindexList.Count() = {3}", new object[]
							{
								this.devindex,
								Thread.CurrentThread.ManagedThreadId,
								this._lastlevel,
								AudioAnalyzer.devindexList.Count<int>()
							});
							this._hanctr = 0;
							this.leftLevel = 0;
							this.rightLevel = 0;
							for (int num13 = 0; num13 < AudioAnalyzer.devindexList.Count<int>(); num13++)
							{
								BassWasapi.BASS_WASAPI_SetDevice(AudioAnalyzer.devindexList[num13]);
								BassWasapi.BASS_WASAPI_Stop(true);
								BassWasapi.BASS_WASAPI_Free();
							}
							LOGGER.DEBUG("[AudioAnalyzer] Bass.BASS_Free()", Array.Empty<object>());
							Bass.BASS_Free();
							LOGGER.DEBUG("[AudioAnalyzer] Bass.BASS_Init()", Array.Empty<object>());
							Bass.BASS_Init(0, 44100, 0, IntPtr.Zero);
							for (int num14 = 0; num14 < AudioAnalyzer.devindexList.Count<int>(); num14++)
							{
								if (!BassWasapi.BASS_WASAPI_Init(AudioAnalyzer.devindexList[num14], 0, 2, 6, 1f, 0.05f, AudioAnalyzer._process, IntPtr.Zero))
								{
									Bass.BASS_ErrorGetCode();
								}
								BassWasapi.BASS_WASAPI_SetDevice(AudioAnalyzer.devindexList[num14]);
								BassWasapi.BASS_WASAPI_Start();
							}
						}
					}
				}
			}
		}

		// Token: 0x06000068 RID: 104 RVA: 0x0000577C File Offset: 0x0000397C
		public string UpdateAudioDataString(int lines, int Strength)
		{
			this._lines = lines;
			this.UpdateAudioData(Strength);
			string text = string.Format("", Array.Empty<object>());
			for (int i = 0; i < this._spectrumdata.Count<byte>(); i++)
			{
				text += string.Format("{0},", this._spectrumdata[i]);
			}
			return text;
		}

		// Token: 0x06000069 RID: 105 RVA: 0x000057E0 File Offset: 0x000039E0
		private static int Process(IntPtr buffer, int length, IntPtr user)
		{
			return length;
		}

		// Token: 0x0600006A RID: 106 RVA: 0x000057E4 File Offset: 0x000039E4
		public void Free()
		{
			object locker = AudioAnalyzer.Locker;
			lock (locker)
			{
				LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer::Free()+", Array.Empty<object>());
				AudioAnalyzer.refcount--;
				if (AudioAnalyzer.refcount == 0)
				{
					LOGGER.DEBUG("[AudioAnalyzer] Bass.BASS_Free()", Array.Empty<object>());
					Bass.BASS_Free();
				}
				Marshal.FreeCoTaskMem(this._fftPtr);
				LOGGER.DEBUG("[AudioAnalyzer] AudioAnalyzer::Free()-, refcount = " + AudioAnalyzer.refcount.ToString(), Array.Empty<object>());
			}
		}

		// Token: 0x0400003B RID: 59
		private bool _enable;

		// Token: 0x0400003C RID: 60
		private float[] _fft;

		// Token: 0x0400003D RID: 61
		private IntPtr _fftPtr;

		// Token: 0x0400003E RID: 62
		private static WASAPIPROC _process;

		// Token: 0x0400003F RID: 63
		private int _lastlevel;

		// Token: 0x04000040 RID: 64
		private int _hanctr;

		// Token: 0x04000041 RID: 65
		private int devindex;

		// Token: 0x04000042 RID: 66
		private string lastDevName = "";

		// Token: 0x04000043 RID: 67
		private int _lines = 68;

		// Token: 0x04000044 RID: 68
		private static readonly object Locker = new object();

		// Token: 0x04000045 RID: 69
		private static List<int> devindexList = new List<int>(100);

		// Token: 0x04000046 RID: 70
		private static int refcount = 0;

		// Token: 0x04000047 RID: 71
		private int Invaildcount;

		// Token: 0x04000048 RID: 72
		private int _BassCompressRatio = 15;

		// Token: 0x04000049 RID: 73
		private int _TrebleCompressRatio = 15;

		// Token: 0x0400004A RID: 74
		public List<byte> _spectrumdata;

		// Token: 0x0400004B RID: 75
		public int leftLevel;

		// Token: 0x0400004C RID: 76
		public int rightLevel;

		// Token: 0x0400004D RID: 77
		private static Dictionary<string, int> AudioDeviceDict = new Dictionary<string, int>();
	}
}
