using Android.OS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	[InstanceStatics]
	public static class Rumble
	{
		internal static float rumbleStrength;

		internal static float rumbleTimerMax;

		internal static float rumbleTimerCurrent;

		internal static float rumbleDuringFade;

		internal static float maxRumbleDuringFade;

		internal static bool isRumbling;

		internal static bool fade;

		private static bool RumbleEnabled => Game1.options.vibrations;

		public static void update(float milliseconds)
		{
			float num = 0f;
			if (isRumbling)
			{
				num = rumbleStrength;
				rumbleTimerCurrent += milliseconds;
				if (rumbleTimerCurrent > rumbleTimerMax)
				{
					num = 0f;
				}
				else if (fade)
				{
					if (rumbleTimerCurrent > rumbleTimerMax - 1000f)
					{
						rumbleDuringFade = Utility.Lerp(maxRumbleDuringFade, 0f, (rumbleTimerCurrent - (rumbleTimerMax - 1000f)) / 1000f);
					}
					num = rumbleDuringFade;
				}
			}
			if (num <= 0f)
			{
				num = 0f;
				isRumbling = false;
			}
			if ((double)num > 1.0)
			{
				num = 1f;
			}
			if (!Game1.options.gamepadControls || !Game1.options.rumble)
			{
				num = 0f;
			}
			if (Game1.playerOneIndex != (PlayerIndex)(-1))
			{
				GamePad.SetVibration(Game1.playerOneIndex, num, num);
			}
		}

		public static void stopRumbling()
		{
			if (isRumbling && RumbleEnabled)
			{
				int num = 0;
				while (!SetVibration(Game1.playerOneIndex, 0f, 0f) && num < 5)
				{
					num++;
				}
				rumbleStrength = 0f;
				isRumbling = false;
			}
		}

		public static void rumble(float leftPower, float rightPower, float milliseconds)
		{
			rumble(leftPower, milliseconds);
		}

		public static void rumble(float power, float milliseconds)
		{
			if (!isRumbling && RumbleEnabled)
			{
				fade = false;
				rumbleTimerCurrent = 0f;
				rumbleTimerMax = milliseconds;
				isRumbling = true;
				rumbleStrength = power;
				SetVibration(Game1.playerOneIndex, power, power);
			}
		}

		public static void rumbleAndFade(float power, float milliseconds)
		{
			if (!isRumbling && RumbleEnabled)
			{
				rumbleTimerCurrent = 0f;
				rumbleTimerMax = milliseconds;
				isRumbling = true;
				SetVibration(Game1.playerOneIndex, power, power);
				fade = true;
				rumbleDuringFade = power;
				maxRumbleDuringFade = power;
				rumbleStrength = power;
			}
		}

		private static bool SetVibration(PlayerIndex playerIndex, float leftMotorPower, float rightMotorPower)
		{
			try
			{
				Vibrator vibrator = (Vibrator)MainActivity.instance.ApplicationContext.GetSystemService("vibrator");
				vibrator.Vibrate(VibrationEffect.CreateOneShot((long)rumbleTimerMax, -1));
				isRumbling = false;
				return true;
			}
			catch
			{
				Log.It("Rumble.SetVibration ERROR");
				return false;
			}
		}
	}
}
