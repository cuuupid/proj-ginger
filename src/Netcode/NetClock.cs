using System.Collections.Generic;

namespace Netcode
{
	public class NetClock
	{
		public NetVersion netVersion;

		public int LocalId;

		public int InterpolationTicks;

		public List<bool> blanks = new List<bool>();

		public NetClock()
		{
			netVersion = default(NetVersion);
			LocalId = AddNewPeer();
		}

		public int AddNewPeer()
		{
			int num = blanks.IndexOf(item: true);
			if (num != -1)
			{
				blanks[num] = false;
			}
			else
			{
				num = netVersion.Size();
				while (blanks.Count < netVersion.Size())
				{
					blanks.Add(item: false);
				}
				netVersion[num] = 0u;
			}
			return num;
		}

		public void RemovePeer(int id)
		{
			while (blanks.Count <= id)
			{
				blanks.Add(item: false);
			}
			blanks[id] = true;
		}

		public uint GetLocalTick()
		{
			return netVersion[LocalId];
		}

		public NetTimestamp GetLocalTimestamp()
		{
			return netVersion.GetTimestamp(LocalId);
		}

		public void Tick()
		{
			netVersion[LocalId]++;
		}

		public void Clear()
		{
			netVersion.Clear();
			LocalId = 0;
		}

		public override string ToString()
		{
			return base.ToString() + ";LocalId=" + LocalId;
		}
	}
}
