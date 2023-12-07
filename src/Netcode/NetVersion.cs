using System;
using System.IO;

namespace Netcode
{
	public struct NetVersion : IEquatable<NetVersion>
	{
		private int count;

		private uint peer1;

		private uint peer2;

		private uint peer3;

		private uint peer4;

		public uint this[int peerId]
		{
			get
			{
				if (peerId >= count)
				{
					return 0u;
				}
				return peerId switch
				{
					0 => peer1, 
					1 => peer2, 
					2 => peer3, 
					3 => peer4, 
					_ => 0u, 
				};
			}
			set
			{
				if (peerId > 3)
				{
					throw new NotSupportedException();
				}
				while (count <= peerId)
				{
					count++;
					switch (count)
					{
					case 1:
						peer1 = 0u;
						break;
					case 2:
						peer2 = 0u;
						break;
					case 3:
						peer3 = 0u;
						break;
					case 4:
						peer4 = 0u;
						break;
					}
				}
				switch (peerId)
				{
				case 0:
					peer1 = value;
					break;
				case 1:
					peer2 = value;
					break;
				case 2:
					peer3 = value;
					break;
				case 3:
					peer4 = value;
					break;
				}
			}
		}

		public NetVersion(int count = 0, uint peer1 = 0u, uint peer2 = 0u, uint peer3 = 0u, uint peer4 = 0u)
		{
			this.count = count;
			this.peer1 = peer1;
			this.peer2 = peer2;
			this.peer3 = peer3;
			this.peer4 = peer4;
		}

		public NetVersion(NetVersion other)
		{
			count = other.count;
			peer1 = other.peer1;
			peer2 = other.peer2;
			peer3 = other.peer3;
			peer4 = other.peer4;
			Set(other);
		}

		public NetTimestamp GetTimestamp(int peerId)
		{
			NetTimestamp result = default(NetTimestamp);
			result.PeerId = peerId;
			result.Tick = this[peerId];
			return result;
		}

		public int Size()
		{
			return count;
		}

		public void Set(NetVersion other)
		{
			for (int i = 0; i < Math.Max(Size(), other.Size()); i++)
			{
				this[i] = other[i];
			}
		}

		public void Merge(NetVersion other)
		{
			for (int i = 0; i < Math.Max(Size(), other.Size()); i++)
			{
				this[i] = Math.Max(this[i], other[i]);
			}
		}

		public bool IsPriorityOver(NetVersion other)
		{
			for (int i = 0; i < Math.Max(Size(), other.Size()); i++)
			{
				if (this[i] > other[i])
				{
					return true;
				}
				if (this[i] < other[i])
				{
					return false;
				}
			}
			return true;
		}

		public bool IsSimultaneousWith(NetVersion other)
		{
			return isOrdered(other, (uint a, uint b) => a == b);
		}

		public bool IsPrecededBy(NetVersion other)
		{
			return isOrdered(other, (uint a, uint b) => a >= b);
		}

		public bool IsFollowedBy(NetVersion other)
		{
			return isOrdered(other, (uint a, uint b) => a < b);
		}

		public bool IsIndependent(NetVersion other)
		{
			if (!IsSimultaneousWith(other) && !IsPrecededBy(other))
			{
				return !IsFollowedBy(other);
			}
			return false;
		}

		private bool isOrdered(NetVersion other, Func<uint, uint, bool> comparison)
		{
			for (int i = 0; i < Math.Max(Size(), other.Size()); i++)
			{
				if (!comparison(this[i], other[i]))
				{
					return false;
				}
			}
			return true;
		}

		public override string ToString()
		{
			if (Size() == 0)
			{
				return "v0";
			}
			if (count == 1)
			{
				return "v" + peer1;
			}
			if (count == 2)
			{
				return "v" + peer1 + "," + peer2;
			}
			if (count == 3)
			{
				return "v" + peer1 + "," + peer2 + "," + peer3;
			}
			return "v" + peer1 + "," + peer2 + "," + peer3 + "," + peer4;
		}

		public bool Equals(NetVersion other)
		{
			for (int i = 0; i < Math.Max(Size(), other.Size()); i++)
			{
				if (this[i] != other[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			int num = 0;
			num = (int)((count > 0) ? ((num * 397) ^ peer1) : num);
			num = (int)((count > 1) ? ((num * 397) ^ peer2) : num);
			num = (int)((count > 2) ? ((num * 397) ^ peer3) : num);
			return (int)((count > 3) ? ((num * 397) ^ peer4) : num);
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)Size());
			for (int i = 0; i < Size(); i++)
			{
				writer.Write(this[i]);
			}
		}

		public void Read(BinaryReader reader)
		{
			int num = (count = reader.ReadByte());
			for (int i = 0; i < num; i++)
			{
				this[i] = reader.ReadUInt32();
			}
			for (int j = num; j < Size(); j++)
			{
				this[j] = 0u;
			}
		}

		public void Clear()
		{
			for (int i = 0; i < Size(); i++)
			{
				this[i] = 0u;
			}
		}
	}
}
