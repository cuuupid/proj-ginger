using System;

namespace TinyTween
{
	public static class ScaleFuncs
	{
		public static readonly ScaleFunc Linear = LinearImpl;

		public static readonly ScaleFunc QuadraticEaseIn = QuadraticEaseInImpl;

		public static readonly ScaleFunc QuadraticEaseOut = QuadraticEaseOutImpl;

		public static readonly ScaleFunc QuadraticEaseInOut = QuadraticEaseInOutImpl;

		public static readonly ScaleFunc CubicEaseIn = CubicEaseInImpl;

		public static readonly ScaleFunc CubicEaseOut = CubicEaseOutImpl;

		public static readonly ScaleFunc CubicEaseInOut = CubicEaseInOutImpl;

		public static readonly ScaleFunc QuarticEaseIn = QuarticEaseInImpl;

		public static readonly ScaleFunc QuarticEaseOut = QuarticEaseOutImpl;

		public static readonly ScaleFunc QuarticEaseInOut = QuarticEaseInOutImpl;

		public static readonly ScaleFunc QuinticEaseIn = QuinticEaseInImpl;

		public static readonly ScaleFunc QuinticEaseOut = QuinticEaseOutImpl;

		public static readonly ScaleFunc QuinticEaseInOut = QuinticEaseInOutImpl;

		public static readonly ScaleFunc SineEaseIn = SineEaseInImpl;

		public static readonly ScaleFunc SineEaseOut = SineEaseOutImpl;

		public static readonly ScaleFunc SineEaseInOut = SineEaseInOutImpl;

		private const float Pi = (float)Math.PI;

		private const float HalfPi = (float)Math.PI / 2f;

		private static float LinearImpl(float progress)
		{
			return progress;
		}

		private static float QuadraticEaseInImpl(float progress)
		{
			return EaseInPower(progress, 2);
		}

		private static float QuadraticEaseOutImpl(float progress)
		{
			return EaseOutPower(progress, 2);
		}

		private static float QuadraticEaseInOutImpl(float progress)
		{
			return EaseInOutPower(progress, 2);
		}

		private static float CubicEaseInImpl(float progress)
		{
			return EaseInPower(progress, 3);
		}

		private static float CubicEaseOutImpl(float progress)
		{
			return EaseOutPower(progress, 3);
		}

		private static float CubicEaseInOutImpl(float progress)
		{
			return EaseInOutPower(progress, 3);
		}

		private static float QuarticEaseInImpl(float progress)
		{
			return EaseInPower(progress, 4);
		}

		private static float QuarticEaseOutImpl(float progress)
		{
			return EaseOutPower(progress, 4);
		}

		private static float QuarticEaseInOutImpl(float progress)
		{
			return EaseInOutPower(progress, 4);
		}

		private static float QuinticEaseInImpl(float progress)
		{
			return EaseInPower(progress, 5);
		}

		private static float QuinticEaseOutImpl(float progress)
		{
			return EaseOutPower(progress, 5);
		}

		private static float QuinticEaseInOutImpl(float progress)
		{
			return EaseInOutPower(progress, 5);
		}

		private static float EaseInPower(float progress, int power)
		{
			return (float)Math.Pow(progress, power);
		}

		private static float EaseOutPower(float progress, int power)
		{
			int num = ((power % 2 != 0) ? 1 : (-1));
			return (float)((double)num * (Math.Pow(progress - 1f, power) + (double)num));
		}

		private static float EaseInOutPower(float progress, int power)
		{
			progress *= 2f;
			if (progress < 1f)
			{
				return (float)Math.Pow(progress, power) / 2f;
			}
			int num = ((power % 2 != 0) ? 1 : (-1));
			return (float)((double)num / 2.0 * (Math.Pow(progress - 2f, power) + (double)(num * 2)));
		}

		private static float SineEaseInImpl(float progress)
		{
			return (float)Math.Sin(progress * ((float)Math.PI / 2f) - (float)Math.PI / 2f) + 1f;
		}

		private static float SineEaseOutImpl(float progress)
		{
			return (float)Math.Sin(progress * ((float)Math.PI / 2f));
		}

		private static float SineEaseInOutImpl(float progress)
		{
			return (float)(Math.Sin(progress * (float)Math.PI - (float)Math.PI / 2f) + 1.0) / 2f;
		}
	}
}
