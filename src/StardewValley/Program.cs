using StardewValley.SDKs;

namespace StardewValley
{
	public static class Program
	{
		public static Game1 gamePtr;

		public static bool GameTesterMode = false;

		public const int build_steam = 0;

		public const int build_gog = 1;

		public const int build_rail = 2;

		public const int buildType = 0;

		public static bool releaseBuild = true;

		private static SDKHelper _sdk;

		public static SDKHelper sdk
		{
			get
			{
				if (_sdk == null)
				{
					_sdk = new NullSDKHelper();
				}
				return _sdk;
			}
		}
	}
}
