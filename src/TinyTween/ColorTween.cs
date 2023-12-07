using Microsoft.Xna.Framework;

namespace TinyTween
{
	public class ColorTween : Tween<Color>
	{
		private static readonly LerpFunc<Color> LerpFunc = Color.Lerp;

		public ColorTween()
			: base(LerpFunc)
		{
		}
	}
}
