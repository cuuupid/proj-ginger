using Microsoft.Xna.Framework;

namespace TinyTween
{
	public class Vector3Tween : Tween<Vector3>
	{
		private static readonly LerpFunc<Vector3> LerpFunc = Vector3.Lerp;

		public Vector3Tween()
			: base(LerpFunc)
		{
		}
	}
}
