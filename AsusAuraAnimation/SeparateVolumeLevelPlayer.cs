using System;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

namespace AsusAuraAnimation
{
	// Token: 0x02000015 RID: 21
	public class SeparateVolumeLevelPlayer : IDisposable
	{
		// Token: 0x060001B7 RID: 439 RVA: 0x000143BC File Offset: 0x000125BC
		public SeparateVolumeLevelPlayer(string fileName, int numberOfSpeakers)
		{
			this.numberOfSpeakers = numberOfSpeakers;
			this.outputMixerStream = BassMix.BASS_Mixer_StreamCreate(44100, numberOfSpeakers, 65536);
			SeparateVolumeLevelPlayer.ThrowOnError();
			this.inputStream = Bass.BASS_StreamCreateFile(fileName, 0L, 0L, 2162690);
			SeparateVolumeLevelPlayer.ThrowOnError();
			BassMix.BASS_Mixer_StreamAddChannel(this.outputMixerStream, this.inputStream, 65536);
			SeparateVolumeLevelPlayer.ThrowOnError();
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x00014427 File Offset: 0x00012627
		public void Play()
		{
			Bass.BASS_ChannelPlay(this.outputMixerStream, false);
			SeparateVolumeLevelPlayer.ThrowOnError();
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x0001443C File Offset: 0x0001263C
		public void SetVolume(float[] volumeValues)
		{
			if (volumeValues == null)
			{
				throw new ArgumentNullException("volumeValues");
			}
			if (volumeValues.Length != this.numberOfSpeakers)
			{
				throw new ArgumentException(string.Format("You must pass a volume level for every speaker. You provided {0} values for {1} speakers", volumeValues.Length, this.numberOfSpeakers));
			}
			float[,] array = new float[this.numberOfSpeakers, 1];
			for (int i = 0; i < this.numberOfSpeakers; i++)
			{
				array[i, 0] = volumeValues[i];
			}
			BassMix.BASS_Mixer_ChannelSetMatrix(this.inputStream, array);
			SeparateVolumeLevelPlayer.ThrowOnError();
		}

		// Token: 0x060001BA RID: 442 RVA: 0x000144C0 File Offset: 0x000126C0
		private static void ThrowOnError()
		{
			BASSError basserror = Bass.BASS_ErrorGetCode();
			if (basserror != null)
			{
				throw new ApplicationException(string.Format("bass.dll reported {0}.", basserror));
			}
		}

		// Token: 0x060001BB RID: 443 RVA: 0x000144EC File Offset: 0x000126EC
		public void Dispose()
		{
			Bass.BASS_StreamFree(this.inputStream);
			Bass.BASS_StreamFree(this.outputMixerStream);
		}

		// Token: 0x040000D7 RID: 215
		private readonly int outputMixerStream;

		// Token: 0x040000D8 RID: 216
		private readonly int inputStream;

		// Token: 0x040000D9 RID: 217
		private readonly int numberOfSpeakers;
	}
}
