namespace StardewValley
{
	public class LocalMultiplayer
	{
		public static bool IsLocalMultiplayer(bool is_local_only = false)
		{
			if (is_local_only)
			{
				return Game1.hasLocalClientsOnly;
			}
			return GameRunner.instance.gameInstances.Count > 1;
		}

		public static void Initialize()
		{
		}

		public static void SaveOptions()
		{
		}
	}
}
