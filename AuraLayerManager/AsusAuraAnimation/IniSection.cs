using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AsusAuraAnimation
{
	// Token: 0x02000012 RID: 18
	public class IniSection : IEnumerable<KeyValuePair<string, IniValue>>, IEnumerable, IDictionary<string, IniValue>, ICollection<KeyValuePair<string, IniValue>>
	{
		// Token: 0x06000188 RID: 392 RVA: 0x00013A74 File Offset: 0x00011C74
		public int IndexOf(string key)
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call IndexOf(string) on IniSection: section was not ordered.");
			}
			return this.IndexOf(key, 0, this.orderedKeys.Count);
		}

		// Token: 0x06000189 RID: 393 RVA: 0x00013A9C File Offset: 0x00011C9C
		public int IndexOf(string key, int index)
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call IndexOf(string, int) on IniSection: section was not ordered.");
			}
			return this.IndexOf(key, index, this.orderedKeys.Count - index);
		}

		// Token: 0x0600018A RID: 394 RVA: 0x00013AC8 File Offset: 0x00011CC8
		public int IndexOf(string key, int index, int count)
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call IndexOf(string, int, int) on IniSection: section was not ordered.");
			}
			if (index < 0 || index > this.orderedKeys.Count)
			{
				throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
			}
			if (count < 0)
			{
				throw new IndexOutOfRangeException("Count cannot be less than zero." + Environment.NewLine + "Parameter name: count");
			}
			if (index + count > this.orderedKeys.Count)
			{
				throw new ArgumentException("Index and count were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
			}
			int num = index + count;
			for (int i = index; i < num; i++)
			{
				if (this.Comparer.Equals(this.orderedKeys[i], key))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x0600018B RID: 395 RVA: 0x00013B7A File Offset: 0x00011D7A
		public int LastIndexOf(string key)
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call LastIndexOf(string) on IniSection: section was not ordered.");
			}
			return this.LastIndexOf(key, 0, this.orderedKeys.Count);
		}

		// Token: 0x0600018C RID: 396 RVA: 0x00013BA2 File Offset: 0x00011DA2
		public int LastIndexOf(string key, int index)
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call LastIndexOf(string, int) on IniSection: section was not ordered.");
			}
			return this.LastIndexOf(key, index, this.orderedKeys.Count - index);
		}

		// Token: 0x0600018D RID: 397 RVA: 0x00013BCC File Offset: 0x00011DCC
		public int LastIndexOf(string key, int index, int count)
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call LastIndexOf(string, int, int) on IniSection: section was not ordered.");
			}
			if (index < 0 || index > this.orderedKeys.Count)
			{
				throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
			}
			if (count < 0)
			{
				throw new IndexOutOfRangeException("Count cannot be less than zero." + Environment.NewLine + "Parameter name: count");
			}
			if (index + count > this.orderedKeys.Count)
			{
				throw new ArgumentException("Index and count were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
			}
			for (int i = index + count - 1; i >= index; i--)
			{
				if (this.Comparer.Equals(this.orderedKeys[i], key))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x0600018E RID: 398 RVA: 0x00013C80 File Offset: 0x00011E80
		public void Insert(int index, string key, IniValue value)
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call Insert(int, string, IniValue) on IniSection: section was not ordered.");
			}
			if (index < 0 || index > this.orderedKeys.Count)
			{
				throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
			}
			this.values.Add(key, value);
			this.orderedKeys.Insert(index, key);
		}

		// Token: 0x0600018F RID: 399 RVA: 0x00013CE8 File Offset: 0x00011EE8
		public void InsertRange(int index, IEnumerable<KeyValuePair<string, IniValue>> collection)
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call InsertRange(int, IEnumerable<KeyValuePair<string, IniValue>>) on IniSection: section was not ordered.");
			}
			if (collection == null)
			{
				throw new ArgumentNullException("Value cannot be null." + Environment.NewLine + "Parameter name: collection");
			}
			if (index < 0 || index > this.orderedKeys.Count)
			{
				throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
			}
			foreach (KeyValuePair<string, IniValue> keyValuePair in collection)
			{
				this.Insert(index, keyValuePair.Key, keyValuePair.Value);
				index++;
			}
		}

		// Token: 0x06000190 RID: 400 RVA: 0x00013DA0 File Offset: 0x00011FA0
		public void RemoveAt(int index)
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call RemoveAt(int) on IniSection: section was not ordered.");
			}
			if (index < 0 || index > this.orderedKeys.Count)
			{
				throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
			}
			string key = this.orderedKeys[index];
			this.orderedKeys.RemoveAt(index);
			this.values.Remove(key);
		}

		// Token: 0x06000191 RID: 401 RVA: 0x00013E14 File Offset: 0x00012014
		public void RemoveRange(int index, int count)
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call RemoveRange(int, int) on IniSection: section was not ordered.");
			}
			if (index < 0 || index > this.orderedKeys.Count)
			{
				throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
			}
			if (count < 0)
			{
				throw new IndexOutOfRangeException("Count cannot be less than zero." + Environment.NewLine + "Parameter name: count");
			}
			if (index + count > this.orderedKeys.Count)
			{
				throw new ArgumentException("Index and count were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
			}
			for (int i = 0; i < count; i++)
			{
				this.RemoveAt(index);
			}
		}

		// Token: 0x06000192 RID: 402 RVA: 0x00013EAC File Offset: 0x000120AC
		public void Reverse()
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call Reverse() on IniSection: section was not ordered.");
			}
			this.orderedKeys.Reverse();
		}

		// Token: 0x06000193 RID: 403 RVA: 0x00013ECC File Offset: 0x000120CC
		public void Reverse(int index, int count)
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call Reverse(int, int) on IniSection: section was not ordered.");
			}
			if (index < 0 || index > this.orderedKeys.Count)
			{
				throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
			}
			if (count < 0)
			{
				throw new IndexOutOfRangeException("Count cannot be less than zero." + Environment.NewLine + "Parameter name: count");
			}
			if (index + count > this.orderedKeys.Count)
			{
				throw new ArgumentException("Index and count were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
			}
			this.orderedKeys.Reverse(index, count);
		}

		// Token: 0x06000194 RID: 404 RVA: 0x00013F60 File Offset: 0x00012160
		public ICollection<IniValue> GetOrderedValues()
		{
			if (!this.Ordered)
			{
				throw new InvalidOperationException("Cannot call GetOrderedValues() on IniSection: section was not ordered.");
			}
			List<IniValue> list = new List<IniValue>();
			for (int i = 0; i < this.orderedKeys.Count; i++)
			{
				list.Add(this.values[this.orderedKeys[i]]);
			}
			return list;
		}

		// Token: 0x1700003A RID: 58
		public IniValue this[int index]
		{
			get
			{
				if (!this.Ordered)
				{
					throw new InvalidOperationException("Cannot index IniSection using integer key: section was not ordered.");
				}
				if (index < 0 || index >= this.orderedKeys.Count)
				{
					throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
				}
				return this.values[this.orderedKeys[index]];
			}
			set
			{
				if (!this.Ordered)
				{
					throw new InvalidOperationException("Cannot index IniSection using integer key: section was not ordered.");
				}
				if (index < 0 || index >= this.orderedKeys.Count)
				{
					throw new IndexOutOfRangeException("Index must be within the bounds." + Environment.NewLine + "Parameter name: index");
				}
				string key = this.orderedKeys[index];
				this.values[key] = value;
			}
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x06000197 RID: 407 RVA: 0x00014086 File Offset: 0x00012286
		// (set) Token: 0x06000198 RID: 408 RVA: 0x00014091 File Offset: 0x00012291
		public bool Ordered
		{
			get
			{
				return this.orderedKeys != null;
			}
			set
			{
				if (this.Ordered != value)
				{
					this.orderedKeys = (value ? new List<string>(this.values.Keys) : null);
				}
			}
		}

		// Token: 0x06000199 RID: 409 RVA: 0x000140B8 File Offset: 0x000122B8
		public IniSection() : this(IniFile.DefaultComparer)
		{
		}

		// Token: 0x0600019A RID: 410 RVA: 0x000140C5 File Offset: 0x000122C5
		public IniSection(IEqualityComparer<string> stringComparer)
		{
			this.values = new Dictionary<string, IniValue>(stringComparer);
		}

		// Token: 0x0600019B RID: 411 RVA: 0x000140D9 File Offset: 0x000122D9
		public IniSection(Dictionary<string, IniValue> values) : this(values, IniFile.DefaultComparer)
		{
		}

		// Token: 0x0600019C RID: 412 RVA: 0x000140E7 File Offset: 0x000122E7
		public IniSection(Dictionary<string, IniValue> values, IEqualityComparer<string> stringComparer)
		{
			this.values = new Dictionary<string, IniValue>(values, stringComparer);
		}

		// Token: 0x0600019D RID: 413 RVA: 0x000140FC File Offset: 0x000122FC
		public IniSection(IniSection values) : this(values, IniFile.DefaultComparer)
		{
		}

		// Token: 0x0600019E RID: 414 RVA: 0x0001410A File Offset: 0x0001230A
		public IniSection(IniSection values, IEqualityComparer<string> stringComparer)
		{
			this.values = new Dictionary<string, IniValue>(values.values, stringComparer);
		}

		// Token: 0x0600019F RID: 415 RVA: 0x00014124 File Offset: 0x00012324
		public void Add(string key, IniValue value)
		{
			this.values.Add(key, value);
			if (this.Ordered)
			{
				this.orderedKeys.Add(key);
			}
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x00014147 File Offset: 0x00012347
		public bool ContainsKey(string key)
		{
			return this.values.ContainsKey(key);
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060001A1 RID: 417 RVA: 0x00014158 File Offset: 0x00012358
		public ICollection<string> Keys
		{
			get
			{
				if (!this.Ordered)
				{
					return this.values.Keys;
				}
				return this.orderedKeys;
			}
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x00014184 File Offset: 0x00012384
		public bool Remove(string key)
		{
			bool flag = this.values.Remove(key);
			if (this.Ordered && flag)
			{
				for (int i = 0; i < this.orderedKeys.Count; i++)
				{
					if (this.Comparer.Equals(this.orderedKeys[i], key))
					{
						this.orderedKeys.RemoveAt(i);
						break;
					}
				}
			}
			return flag;
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x000141E7 File Offset: 0x000123E7
		public bool TryGetValue(string key, out IniValue value)
		{
			return this.values.TryGetValue(key, out value);
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060001A4 RID: 420 RVA: 0x000141F6 File Offset: 0x000123F6
		public ICollection<IniValue> Values
		{
			get
			{
				return this.values.Values;
			}
		}

		// Token: 0x060001A5 RID: 421 RVA: 0x00014203 File Offset: 0x00012403
		void ICollection<KeyValuePair<string, IniValue>>.Add(KeyValuePair<string, IniValue> item)
		{
			((ICollection<KeyValuePair<string, IniValue>>)this.values).Add(item);
			if (this.Ordered)
			{
				this.orderedKeys.Add(item.Key);
			}
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x0001422B File Offset: 0x0001242B
		public void Clear()
		{
			this.values.Clear();
			if (this.Ordered)
			{
				this.orderedKeys.Clear();
			}
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x0001424B File Offset: 0x0001244B
		bool ICollection<KeyValuePair<string, IniValue>>.Contains(KeyValuePair<string, IniValue> item)
		{
			return ((ICollection<KeyValuePair<string, IniValue>>)this.values).Contains(item);
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x00014259 File Offset: 0x00012459
		void ICollection<KeyValuePair<string, IniValue>>.CopyTo(KeyValuePair<string, IniValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, IniValue>>)this.values).CopyTo(array, arrayIndex);
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060001A9 RID: 425 RVA: 0x00014268 File Offset: 0x00012468
		public int Count
		{
			get
			{
				return this.values.Count;
			}
		}

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060001AA RID: 426 RVA: 0x00014275 File Offset: 0x00012475
		bool ICollection<KeyValuePair<string, IniValue>>.IsReadOnly
		{
			get
			{
				return ((ICollection<KeyValuePair<string, IniValue>>)this.values).IsReadOnly;
			}
		}

		// Token: 0x060001AB RID: 427 RVA: 0x00014284 File Offset: 0x00012484
		bool ICollection<KeyValuePair<string, IniValue>>.Remove(KeyValuePair<string, IniValue> item)
		{
			bool flag = ((ICollection<KeyValuePair<string, IniValue>>)this.values).Remove(item);
			if (this.Ordered && flag)
			{
				for (int i = 0; i < this.orderedKeys.Count; i++)
				{
					if (this.Comparer.Equals(this.orderedKeys[i], item.Key))
					{
						this.orderedKeys.RemoveAt(i);
						break;
					}
				}
			}
			return flag;
		}

		// Token: 0x060001AC RID: 428 RVA: 0x000142ED File Offset: 0x000124ED
		public IEnumerator<KeyValuePair<string, IniValue>> GetEnumerator()
		{
			if (this.Ordered)
			{
				return this.GetOrderedEnumerator();
			}
			return this.values.GetEnumerator();
		}

		// Token: 0x060001AD RID: 429 RVA: 0x0001430E File Offset: 0x0001250E
		private IEnumerator<KeyValuePair<string, IniValue>> GetOrderedEnumerator()
		{
			IniSection.<GetOrderedEnumerator>d__45 <GetOrderedEnumerator>d__ = new IniSection.<GetOrderedEnumerator>d__45(0);
			<GetOrderedEnumerator>d__.<>4__this = this;
			return <GetOrderedEnumerator>d__;
		}

		// Token: 0x060001AE RID: 430 RVA: 0x0001431D File Offset: 0x0001251D
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060001AF RID: 431 RVA: 0x00014325 File Offset: 0x00012525
		public IEqualityComparer<string> Comparer
		{
			get
			{
				return this.values.Comparer;
			}
		}

		// Token: 0x17000041 RID: 65
		public IniValue this[string name]
		{
			get
			{
				IniValue result;
				if (this.values.TryGetValue(name, out result))
				{
					return result;
				}
				return IniValue.Default;
			}
			set
			{
				if (this.Ordered && !this.orderedKeys.Contains(name, this.Comparer))
				{
					this.orderedKeys.Add(name);
				}
				this.values[name] = value;
			}
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x0001438F File Offset: 0x0001258F
		public static implicit operator IniSection(Dictionary<string, IniValue> dict)
		{
			return new IniSection(dict);
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x00014397 File Offset: 0x00012597
		public static explicit operator Dictionary<string, IniValue>(IniSection section)
		{
			return section.values;
		}

		// Token: 0x040000D5 RID: 213
		private Dictionary<string, IniValue> values;

		// Token: 0x040000D6 RID: 214
		private List<string> orderedKeys;
	}
}
