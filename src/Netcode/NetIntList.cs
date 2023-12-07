using System.Collections.Generic;

namespace Netcode
{
	public sealed class NetIntList : NetList<int, NetInt>
	{
		public NetIntList()
		{
		}

		public NetIntList(IEnumerable<int> values)
			: base(values)
		{
		}

		public NetIntList(int capacity)
			: base(capacity)
		{
		}

		public override bool Contains(int item)
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					if (current == item)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override int IndexOf(int item)
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
