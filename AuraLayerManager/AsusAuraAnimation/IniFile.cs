using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AsusAuraAnimation
{
	// Token: 0x02000011 RID: 17
	public class IniFile : IEnumerable<KeyValuePair<string, IniSection>>, IEnumerable, IDictionary<string, IniSection>, ICollection<KeyValuePair<string, IniSection>>
	{
		// Token: 0x06000167 RID: 359 RVA: 0x0001353F File Offset: 0x0001173F
		public IniFile() : this(IniFile.DefaultComparer)
		{
		}

		// Token: 0x06000168 RID: 360 RVA: 0x0001354C File Offset: 0x0001174C
		public IniFile(IEqualityComparer<string> stringComparer)
		{
			this.StringComparer = stringComparer;
			this.sections = new Dictionary<string, IniSection>(this.StringComparer);
		}

		// Token: 0x06000169 RID: 361 RVA: 0x0001356C File Offset: 0x0001176C
		public void Save(string path, FileMode mode = FileMode.Create)
		{
			using (FileStream fileStream = new FileStream(path, mode, FileAccess.Write))
			{
				this.Save(fileStream);
			}
		}

		// Token: 0x0600016A RID: 362 RVA: 0x000135A8 File Offset: 0x000117A8
		public void Save(Stream stream)
		{
			using (StreamWriter streamWriter = new StreamWriter(stream))
			{
				this.Save(streamWriter);
			}
		}

		// Token: 0x0600016B RID: 363 RVA: 0x000135E0 File Offset: 0x000117E0
		public void Save(StreamWriter writer)
		{
			foreach (KeyValuePair<string, IniSection> keyValuePair in this.sections)
			{
				if (keyValuePair.Value.Count > 0 || this.SaveEmptySections)
				{
					writer.WriteLine(string.Format("[{0}]", keyValuePair.Key.Trim()));
					foreach (KeyValuePair<string, IniValue> keyValuePair2 in keyValuePair.Value)
					{
						writer.WriteLine(string.Format("{0}={1}", keyValuePair2.Key, keyValuePair2.Value));
					}
					writer.WriteLine("");
				}
			}
		}

		// Token: 0x0600016C RID: 364 RVA: 0x000136CC File Offset: 0x000118CC
		public void Load(string path, bool ordered = false)
		{
			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				this.Load(fileStream, ordered);
			}
		}

		// Token: 0x0600016D RID: 365 RVA: 0x00013708 File Offset: 0x00011908
		public void Load(Stream stream, bool ordered = false)
		{
			using (StreamReader streamReader = new StreamReader(stream))
			{
				this.Load(streamReader, ordered);
			}
		}

		// Token: 0x0600016E RID: 366 RVA: 0x00013740 File Offset: 0x00011940
		public void Load(StreamReader reader, bool ordered = false)
		{
			IniSection iniSection = null;
			while (!reader.EndOfStream)
			{
				string text = reader.ReadLine();
				if (text != null)
				{
					string text2 = text.TrimStart(Array.Empty<char>());
					if (text2.Length > 0)
					{
						string name;
						IniValue value;
						if (text2[0] == '[')
						{
							int num = text2.IndexOf(']');
							if (num > 0)
							{
								string key = text2.Substring(1, num - 1).Trim();
								iniSection = new IniSection(this.StringComparer)
								{
									Ordered = ordered
								};
								this.sections[key] = iniSection;
							}
						}
						else if (iniSection != null && text2[0] != ';' && this.LoadValue(text, out name, out value))
						{
							iniSection[name] = value;
						}
					}
				}
			}
		}

		// Token: 0x0600016F RID: 367 RVA: 0x000137F4 File Offset: 0x000119F4
		private bool LoadValue(string line, out string key, out IniValue val)
		{
			int num = line.IndexOf('=');
			if (num <= 0)
			{
				key = null;
				val = null;
				return false;
			}
			key = line.Substring(0, num).Trim();
			string value = line.Substring(num + 1);
			val = new IniValue(value);
			return true;
		}

		// Token: 0x06000170 RID: 368 RVA: 0x00013845 File Offset: 0x00011A45
		public bool ContainsSection(string section)
		{
			return this.sections.ContainsKey(section);
		}

		// Token: 0x06000171 RID: 369 RVA: 0x00013853 File Offset: 0x00011A53
		public bool TryGetSection(string section, out IniSection result)
		{
			return this.sections.TryGetValue(section, out result);
		}

		// Token: 0x06000172 RID: 370 RVA: 0x00013862 File Offset: 0x00011A62
		bool IDictionary<string, IniSection>.TryGetValue(string key, out IniSection value)
		{
			return this.TryGetSection(key, out value);
		}

		// Token: 0x06000173 RID: 371 RVA: 0x0001386C File Offset: 0x00011A6C
		public bool Remove(string section)
		{
			return this.sections.Remove(section);
		}

		// Token: 0x06000174 RID: 372 RVA: 0x0001387A File Offset: 0x00011A7A
		public IniSection Add(string section, Dictionary<string, IniValue> values, bool ordered = false)
		{
			return this.Add(section, new IniSection(values, this.StringComparer)
			{
				Ordered = ordered
			});
		}

		// Token: 0x06000175 RID: 373 RVA: 0x00013896 File Offset: 0x00011A96
		public IniSection Add(string section, IniSection value)
		{
			if (value.Comparer != this.StringComparer)
			{
				value = new IniSection(value, this.StringComparer);
			}
			this.sections.Add(section, value);
			return value;
		}

		// Token: 0x06000176 RID: 374 RVA: 0x000138C4 File Offset: 0x00011AC4
		public IniSection Add(string section, bool ordered = false)
		{
			IniSection iniSection = new IniSection(this.StringComparer)
			{
				Ordered = ordered
			};
			this.sections.Add(section, iniSection);
			return iniSection;
		}

		// Token: 0x06000177 RID: 375 RVA: 0x000138F2 File Offset: 0x00011AF2
		void IDictionary<string, IniSection>.Add(string key, IniSection value)
		{
			this.Add(key, value);
		}

		// Token: 0x06000178 RID: 376 RVA: 0x000138FD File Offset: 0x00011AFD
		bool IDictionary<string, IniSection>.ContainsKey(string key)
		{
			return this.ContainsSection(key);
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000179 RID: 377 RVA: 0x00013906 File Offset: 0x00011B06
		public ICollection<string> Keys
		{
			get
			{
				return this.sections.Keys;
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x0600017A RID: 378 RVA: 0x00013913 File Offset: 0x00011B13
		public ICollection<IniSection> Values
		{
			get
			{
				return this.sections.Values;
			}
		}

		// Token: 0x0600017B RID: 379 RVA: 0x00013920 File Offset: 0x00011B20
		void ICollection<KeyValuePair<string, IniSection>>.Add(KeyValuePair<string, IniSection> item)
		{
			((ICollection<KeyValuePair<string, IniSection>>)this.sections).Add(item);
		}

		// Token: 0x0600017C RID: 380 RVA: 0x0001392E File Offset: 0x00011B2E
		public void Clear()
		{
			this.sections.Clear();
		}

		// Token: 0x0600017D RID: 381 RVA: 0x0001393B File Offset: 0x00011B3B
		bool ICollection<KeyValuePair<string, IniSection>>.Contains(KeyValuePair<string, IniSection> item)
		{
			return ((ICollection<KeyValuePair<string, IniSection>>)this.sections).Contains(item);
		}

		// Token: 0x0600017E RID: 382 RVA: 0x00013949 File Offset: 0x00011B49
		void ICollection<KeyValuePair<string, IniSection>>.CopyTo(KeyValuePair<string, IniSection>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, IniSection>>)this.sections).CopyTo(array, arrayIndex);
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x0600017F RID: 383 RVA: 0x00013958 File Offset: 0x00011B58
		public int Count
		{
			get
			{
				return this.sections.Count;
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000180 RID: 384 RVA: 0x00013965 File Offset: 0x00011B65
		bool ICollection<KeyValuePair<string, IniSection>>.IsReadOnly
		{
			get
			{
				return ((ICollection<KeyValuePair<string, IniSection>>)this.sections).IsReadOnly;
			}
		}

		// Token: 0x06000181 RID: 385 RVA: 0x00013972 File Offset: 0x00011B72
		bool ICollection<KeyValuePair<string, IniSection>>.Remove(KeyValuePair<string, IniSection> item)
		{
			return ((ICollection<KeyValuePair<string, IniSection>>)this.sections).Remove(item);
		}

		// Token: 0x06000182 RID: 386 RVA: 0x00013980 File Offset: 0x00011B80
		public IEnumerator<KeyValuePair<string, IniSection>> GetEnumerator()
		{
			return this.sections.GetEnumerator();
		}

		// Token: 0x06000183 RID: 387 RVA: 0x00013992 File Offset: 0x00011B92
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x17000039 RID: 57
		public IniSection this[string section]
		{
			get
			{
				IniSection iniSection;
				if (this.sections.TryGetValue(section, out iniSection))
				{
					return iniSection;
				}
				iniSection = new IniSection(this.StringComparer);
				this.sections[section] = iniSection;
				return iniSection;
			}
			set
			{
				IniSection iniSection = value;
				if (iniSection.Comparer != this.StringComparer)
				{
					iniSection = new IniSection(iniSection, this.StringComparer);
				}
				this.sections[section] = iniSection;
			}
		}

		// Token: 0x06000186 RID: 390 RVA: 0x00013A10 File Offset: 0x00011C10
		public string GetContents()
		{
			string result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				this.Save(memoryStream);
				memoryStream.Flush();
				result = new StringBuilder(Encoding.UTF8.GetString(memoryStream.ToArray())).ToString();
			}
			return result;
		}

		// Token: 0x040000D1 RID: 209
		private Dictionary<string, IniSection> sections;

		// Token: 0x040000D2 RID: 210
		public IEqualityComparer<string> StringComparer;

		// Token: 0x040000D3 RID: 211
		public bool SaveEmptySections;

		// Token: 0x040000D4 RID: 212
		public static IEqualityComparer<string> DefaultComparer = new IniFile.CaseInsensitiveStringComparer();

		// Token: 0x02000052 RID: 82
		private class CaseInsensitiveStringComparer : IEqualityComparer<string>
		{
			// Token: 0x0600035E RID: 862 RVA: 0x00019AD8 File Offset: 0x00017CD8
			public bool Equals(string x, string y)
			{
				return string.Compare(x, y, true) == 0;
			}

			// Token: 0x0600035F RID: 863 RVA: 0x00019AE5 File Offset: 0x00017CE5
			public int GetHashCode(string obj)
			{
				return obj.ToLowerInvariant().GetHashCode();
			}
		}
	}
}
