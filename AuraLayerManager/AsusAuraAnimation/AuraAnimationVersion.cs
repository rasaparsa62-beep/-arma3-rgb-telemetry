using System;
using System.Runtime.InteropServices;

namespace AsusAuraAnimation
{
	// Token: 0x02000014 RID: 20
	[Guid("0647D986-BD6B-48C9-B496-91E73A06F3BD")]
	[ClassInterface(ClassInterfaceType.None)]
	public class AuraAnimationVersion : IAuraAnimationVersion
	{
		// Token: 0x060001B5 RID: 437 RVA: 0x0001439F File Offset: 0x0001259F
		public int VERSION()
		{
			LOGGER.DEBUG("[AuraAnimationVersion] Version = 6", Array.Empty<object>());
			return 6;
		}
	}
}
