using System;
using System.Globalization;

namespace AsusAuraAnimation
{
	// Token: 0x02000010 RID: 16
	public struct IniValue
	{
		// Token: 0x0600014D RID: 333 RVA: 0x00013280 File Offset: 0x00011480
		private static bool TryParseInt(string text, out int value)
		{
			int num;
			if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
			{
				value = num;
				return true;
			}
			value = 0;
			return false;
		}

		// Token: 0x0600014E RID: 334 RVA: 0x000132A8 File Offset: 0x000114A8
		private static bool TryParseDouble(string text, out double value)
		{
			double num;
			if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out num))
			{
				value = num;
				return true;
			}
			value = double.NaN;
			return false;
		}

		// Token: 0x0600014F RID: 335 RVA: 0x000132DC File Offset: 0x000114DC
		public IniValue(object value)
		{
			IFormattable formattable = value as IFormattable;
			if (formattable != null)
			{
				this.Value = formattable.ToString(null, CultureInfo.InvariantCulture);
				return;
			}
			this.Value = ((value != null) ? value.ToString() : null);
		}

		// Token: 0x06000150 RID: 336 RVA: 0x00013318 File Offset: 0x00011518
		public IniValue(string value)
		{
			this.Value = value;
		}

		// Token: 0x06000151 RID: 337 RVA: 0x00013324 File Offset: 0x00011524
		public bool ToBool(bool valueIfInvalid = false)
		{
			bool result;
			if (this.TryConvertBool(out result))
			{
				return result;
			}
			return valueIfInvalid;
		}

		// Token: 0x06000152 RID: 338 RVA: 0x00013340 File Offset: 0x00011540
		public bool TryConvertBool(out bool result)
		{
			if (this.Value == null)
			{
				result = false;
				return false;
			}
			string a = this.Value.Trim().ToLowerInvariant();
			if (a == "true")
			{
				result = true;
				return true;
			}
			if (a == "false")
			{
				result = false;
				return true;
			}
			result = false;
			return false;
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00013394 File Offset: 0x00011594
		public int ToInt(int valueIfInvalid = 0)
		{
			int result;
			if (this.TryConvertInt(out result))
			{
				return result;
			}
			return valueIfInvalid;
		}

		// Token: 0x06000154 RID: 340 RVA: 0x000133AE File Offset: 0x000115AE
		public bool TryConvertInt(out int result)
		{
			if (this.Value == null)
			{
				result = 0;
				return false;
			}
			return IniValue.TryParseInt(this.Value.Trim(), out result);
		}

		// Token: 0x06000155 RID: 341 RVA: 0x000133D4 File Offset: 0x000115D4
		public double ToDouble(double valueIfInvalid = 0.0)
		{
			double result;
			if (this.TryConvertDouble(out result))
			{
				return result;
			}
			return valueIfInvalid;
		}

		// Token: 0x06000156 RID: 342 RVA: 0x000133EE File Offset: 0x000115EE
		public bool TryConvertDouble(out double result)
		{
			if (this.Value == null)
			{
				result = 0.0;
				return false;
			}
			return IniValue.TryParseDouble(this.Value.Trim(), out result);
		}

		// Token: 0x06000157 RID: 343 RVA: 0x0001341B File Offset: 0x0001161B
		public string GetString()
		{
			return this.GetString(true, false);
		}

		// Token: 0x06000158 RID: 344 RVA: 0x00013425 File Offset: 0x00011625
		public string GetString(bool preserveWhitespace)
		{
			return this.GetString(true, preserveWhitespace);
		}

		// Token: 0x06000159 RID: 345 RVA: 0x00013430 File Offset: 0x00011630
		public string GetString(bool allowOuterQuotes, bool preserveWhitespace)
		{
			if (this.Value == null)
			{
				return "";
			}
			string text = this.Value.Trim();
			if (allowOuterQuotes && text.Length >= 2 && text[0] == '"' && text[text.Length - 1] == '"')
			{
				string text2 = text.Substring(1, text.Length - 2);
				if (!preserveWhitespace)
				{
					return text2.Trim();
				}
				return text2;
			}
			else
			{
				if (!preserveWhitespace)
				{
					return this.Value.Trim();
				}
				return this.Value;
			}
		}

		// Token: 0x0600015A RID: 346 RVA: 0x000134B1 File Offset: 0x000116B1
		public override string ToString()
		{
			return this.Value;
		}

		// Token: 0x0600015B RID: 347 RVA: 0x000134B9 File Offset: 0x000116B9
		public static implicit operator IniValue(byte o)
		{
			return new IniValue(o);
		}

		// Token: 0x0600015C RID: 348 RVA: 0x000134C6 File Offset: 0x000116C6
		public static implicit operator IniValue(short o)
		{
			return new IniValue(o);
		}

		// Token: 0x0600015D RID: 349 RVA: 0x000134D3 File Offset: 0x000116D3
		public static implicit operator IniValue(int o)
		{
			return new IniValue(o);
		}

		// Token: 0x0600015E RID: 350 RVA: 0x000134E0 File Offset: 0x000116E0
		public static implicit operator IniValue(sbyte o)
		{
			return new IniValue(o);
		}

		// Token: 0x0600015F RID: 351 RVA: 0x000134ED File Offset: 0x000116ED
		public static implicit operator IniValue(ushort o)
		{
			return new IniValue(o);
		}

		// Token: 0x06000160 RID: 352 RVA: 0x000134FA File Offset: 0x000116FA
		public static implicit operator IniValue(uint o)
		{
			return new IniValue(o);
		}

		// Token: 0x06000161 RID: 353 RVA: 0x00013507 File Offset: 0x00011707
		public static implicit operator IniValue(float o)
		{
			return new IniValue(o);
		}

		// Token: 0x06000162 RID: 354 RVA: 0x00013514 File Offset: 0x00011714
		public static implicit operator IniValue(double o)
		{
			return new IniValue(o);
		}

		// Token: 0x06000163 RID: 355 RVA: 0x00013521 File Offset: 0x00011721
		public static implicit operator IniValue(bool o)
		{
			return new IniValue(o);
		}

		// Token: 0x06000164 RID: 356 RVA: 0x0001352E File Offset: 0x0001172E
		public static implicit operator IniValue(string o)
		{
			return new IniValue(o);
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000165 RID: 357 RVA: 0x00013536 File Offset: 0x00011736
		public static IniValue Default
		{
			get
			{
				return IniValue._default;
			}
		}

		// Token: 0x040000CF RID: 207
		public string Value;

		// Token: 0x040000D0 RID: 208
		private static readonly IniValue _default;
	}
}
