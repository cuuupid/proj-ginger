namespace TinyTween
{
	public class FloatTween : Tween<float>
	{
		private static readonly LerpFunc<float> LerpFunc = LerpFloat;

		private static float LerpFloat(float start, float end, float progress)
		{
			return start + (end - start) * progress;
		}

		public FloatTween()
			: base(LerpFunc)
		{
		}
	}
}
