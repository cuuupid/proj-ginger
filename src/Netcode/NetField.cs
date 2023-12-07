using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Netcode
{
	public abstract class NetField<T, TSelf> : NetFieldBase<T, TSelf>, IEnumerable<T>, IEnumerable where TSelf : NetField<T, TSelf>
	{
		public NetField()
		{
		}

		public NetField(T value)
			: base(value)
		{
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Enumerable.Repeat(Get(), 1).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(T value)
		{
			cleanSet(value);
		}

		public void Add(object value)
		{
			Add((T)value);
		}
	}
}
