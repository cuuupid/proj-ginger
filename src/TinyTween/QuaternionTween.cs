using Microsoft.Xna.Framework;

namespace TinyTween
{
	public class QuaternionTween : Tween<Quaternion>
	{
		private static readonly LerpFunc<Quaternion> LerpFunc = Quaternion.Lerp;

		public QuaternionTween()
			: base(LerpFunc)
		{
		}
	}
}
