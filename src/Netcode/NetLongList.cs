using System.Collections.Generic;

namespace Netcode
{
	public sealed class NetLongList : NetList<long, NetLong>
	{
		public NetLongList()
		{
		}

		public NetLongList(IEnumerable<long> values)
			: base(values)
		{
		}

		public NetLongList(int capacity)
			: base(capacity)
		{
		}

		public override bool Contains(long item)
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					long current = enumerator.Current;
					if (current == item)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override int IndexOf(long item)
		{
			NetInt netInt = count;
			for (int i = 0; i < (int)netInt; i++)
			{
				if (array.Value[i] == item)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
