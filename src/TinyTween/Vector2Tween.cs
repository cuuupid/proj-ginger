using Microsoft.Xna.Framework;

namespace TinyTween
{
	public class Vector2Tween : Tween<Vector2>
	{
		private static readonly LerpFunc<Vector2> LerpFunc = Vector2.Lerp;

		public Vector2Tween()
			: base(LerpFunc)
		{
		}
	}
}
