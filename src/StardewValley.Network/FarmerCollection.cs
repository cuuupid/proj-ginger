using System;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Network
{
	public class FarmerCollection : IEnumerable<Farmer>, IEnumerable
	{
		public struct Enumerator : IEnumerator<Farmer>, IDisposable, IEnumerator
		{
			private GameLocation _locationFilter;

			private Dictionary<long, Netcode.NetRoot<Farmer>>.Enumerator _enumerator;

			private Farmer _player;

			private Farmer _current;

			private int _done;

			public Farmer Current => _current;

			object IEnumerator.Current
			{
				get
				{
					if (_done == 0)
					{
						throw new InvalidOperationException();
					}
					return _current;
				}
			}

			public Enumerator(GameLocation locationFilter)
			{
				_locationFilter = locationFilter;
				_player = Game1.player;
				_enumerator = Game1.otherFarmers.Roots.GetEnumerator();
				_current = null;
				_done = 2;
			}

			public bool MoveNext()
			{
				if (_done == 2)
				{
					_done = 1;
					if (_locationFilter == null || (_player.currentLocation != null && _locationFilter.Equals(_player.currentLocation)))
					{
						_current = _player;
						return true;
					}
				}
				while (_enumerator.MoveNext())
				{
					Farmer value = _enumerator.Current.Value.Value;
					if (value != _player && (_locationFilter == null || (value.currentLocation != null && _locationFilter.Equals(value.currentLocation))))
					{
						_current = value;
						return true;
					}
				}
				_done = 0;
				_current = null;
				return false;
			}

			public void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				_player = Game1.player;
				_enumerator = Game1.otherFarmers.Roots.GetEnumerator();
				_current = null;
				_done = 2;
			}
		}

		private GameLocation _locationFilter;

		public int Count
		{
			get
			{
				int num = 0;
				using Enumerator enumerator = GetEnumerator();
				while (enumerator.MoveNext())
				{
					Farmer current = enumerator.Current;
					num++;
				}
				return num;
			}
		}

		public FarmerCollection(GameLocation locationFilter = null)
		{
			_locationFilter = locationFilter;
		}

		public bool Any()
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Farmer current = enumerator.Current;
					return true;
				}
			}
			return false;
		}

		public bool Contains(Farmer farmer)
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Farmer current = enumerator.Current;
					if (current == farmer)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool ContainsID(long uniqueMultiplayerID)
		{
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Farmer current = enumerator.Current;
					if (current.UniqueMultiplayerID == uniqueMultiplayerID)
					{
						return true;
					}
				}
			}
			return false;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_locationFilter);
		}

		IEnumerator<Farmer> IEnumerable<Farmer>.GetEnumerator()
		{
			return new Enumerator(_locationFilter);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(_locationFilter);
		}
	}
}
