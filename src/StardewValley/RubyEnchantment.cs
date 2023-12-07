using System;
using System.Collections.Generic;
using StardewValley.Tools;

namespace StardewValley
{
	public class RubyEnchantment : BaseWeaponEnchantment
	{
		protected override void _ApplyTo(Item item)
		{
			base._ApplyTo(item);
			if (item is MeleeWeapon meleeWeapon)
			{
				Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\weapons");
				string[] array = dictionary[meleeWeapon.InitialParentTileIndex].Split('/');
				int num = Convert.ToInt32(array[2]);
				int num2 = Convert.ToInt32(array[3]);
				meleeWeapon.minDamage.Value += Math.Max(1, (int)((float)num * 0.1f)) * GetLevel();
				meleeWeapon.maxDamage.Value += Math.Max(1, (int)((float)num2 * 0.1f)) * GetLevel();
			}
		}

		protected override void _UnapplyTo(Item item)
		{
			base._UnapplyTo(item);
			if (item is MeleeWeapon meleeWeapon)
			{
				Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\weapons");
				string[] array = dictionary[meleeWeapon.InitialParentTileIndex].Split('/');
				int num = Convert.ToInt32(array[2]);
				int num2 = Convert.ToInt32(array[3]);
				meleeWeapon.minDamage.Value -= Math.Max(1, (int)((float)num * 0.1f)) * GetLevel();
				meleeWeapon.maxDamage.Value -= Math.Max(1, (int)((float)num2 * 0.1f)) * GetLevel();
			}
		}

		public override bool ShouldBeDisplayed()
		{
			return false;
		}

		public override bool IsForge()
		{
			return true;
		}
	}
}
