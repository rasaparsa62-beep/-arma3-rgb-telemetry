using System;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace AsusAuraAnimation
{
	// Token: 0x0200000A RID: 10
	public sealed class AudioDeviceWatcher : IDisposable, IMMNotificationClient
	{
		// Token: 0x0600007F RID: 127 RVA: 0x00005E7C File Offset: 0x0000407C
		public AudioDeviceWatcher(DataFlow flow, Action onChange)
		{
			this._flow = flow;
			Action onChange2 = onChange;
			if (onChange == null && (onChange2 = AudioDeviceWatcher.<>c.<>9__4_0) == null)
			{
				onChange2 = (AudioDeviceWatcher.<>c.<>9__4_0 = delegate()
				{
				});
			}
			this._onChange = onChange2;
			this._enumerator.RegisterEndpointNotificationCallback(this);
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00005ED8 File Offset: 0x000040D8
		public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
		{
			if (flow == this._flow)
			{
				this.SafeNotify();
			}
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00005EE9 File Offset: 0x000040E9
		public void OnDeviceAdded(string deviceId)
		{
			this.SafeNotify();
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00005EF1 File Offset: 0x000040F1
		public void OnDeviceRemoved(string deviceId)
		{
			this.SafeNotify();
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00005EF9 File Offset: 0x000040F9
		public void OnDeviceStateChanged(string deviceId, DeviceState newState)
		{
			this.SafeNotify();
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00005F01 File Offset: 0x00004101
		public void OnPropertyValueChanged(string deviceId, PropertyKey key)
		{
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00005F04 File Offset: 0x00004104
		private void SafeNotify()
		{
			try
			{
				this._onChange();
			}
			catch
			{
			}
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00005F34 File Offset: 0x00004134
		public void Dispose()
		{
			if (this._disposed)
			{
				return;
			}
			try
			{
				try
				{
					this._enumerator.UnregisterEndpointNotificationCallback(this);
				}
				catch
				{
				}
				this._enumerator.Dispose();
			}
			finally
			{
				this._disposed = true;
			}
		}

		// Token: 0x04000057 RID: 87
		private readonly MMDeviceEnumerator _enumerator = new MMDeviceEnumerator();

		// Token: 0x04000058 RID: 88
		private readonly DataFlow _flow;

		// Token: 0x04000059 RID: 89
		private readonly Action _onChange;

		// Token: 0x0400005A RID: 90
		private bool _disposed;
	}
}
