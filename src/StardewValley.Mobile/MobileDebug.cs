using System;

namespace StardewValley.Mobile
{
	internal class MobileDebug
	{
		public static void LoadCommand(Game1 game = null)
		{
			if (!(Environment.MachineName == "MACDADDY2"))
			{
				_ = Environment.MachineName == "MAC_DADDY";
			}
		}

		public static void TestFunc(Game1 game = null)
		{
			if (!(Environment.MachineName == "MACDADDY2"))
			{
				_ = Environment.MachineName == "MAC_DADDY";
			}
		}
	}
}
