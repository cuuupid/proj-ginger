using System.Collections;
using System.Collections.Generic;
using Netcode;
using StardewValley.Buildings;

namespace StardewValley.Network
{
	public class NetBuildingRef : INetObject<NetFields>, IEnumerable<Building>, IEnumerable
	{
		private readonly NetString nameOfIndoors = new NetString();

		public NetFields NetFields { get; } = new NetFields();


		public Building Value
		{
			get
			{
				string text = nameOfIndoors.Get();
				if (text == null)
				{
					return null;
				}
				return Game1.getFarm().getBuildingByName(text);
			}
			set
			{
				if (value == null)
				{
					nameOfIndoors.Value = null;
				}
				else
				{
					nameOfIndoors.Value = value.nameOfIndoors;
				}
			}
		}

		public NetBuildingRef()
		{
			NetFields.AddFields(nameOfIndoors);
		}

		public IEnumerator<Building> GetEnumerator()
		{
			yield return Value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public static implicit operator Building(NetBuildingRef buildingRef)
		{
			return buildingRef.Value;
		}
	}
}
