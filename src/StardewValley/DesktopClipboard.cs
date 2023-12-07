namespace StardewValley
{
	public class DesktopClipboard
	{
		public static bool _availabilityChecked;

		public static bool _isAvailable;

		public static bool IsAvailable
		{
			get
			{
				if (!_availabilityChecked)
				{
					_availabilityChecked = true;
					_isAvailable = false;
				}
				return _isAvailable;
			}
		}

		public static bool GetText(ref string output)
		{
			return false;
		}

		public static bool SetText(string text)
		{
			return false;
		}
	}
}
