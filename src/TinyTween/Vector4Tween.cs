using Microsoft.Xna.Framework;

namespace TinyTween
{
	public class Vector4Tween : Tween<Vector4>
	{
		private static readonly LerpFunc<Vector4> LerpFunc = Vector4.Lerp;

		public Vector4Tween()
			: base(LerpFunc)
		{
		}
	}
}
