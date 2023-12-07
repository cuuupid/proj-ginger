using System;

namespace TinyTween
{
	public class Tween<T> : ITween<T>, ITween where T : struct
	{
		private readonly LerpFunc<T> lerpFunc;

		private float currentTime;

		private float duration;

		private ScaleFunc scaleFunc;

		private TweenState state;

		private T start;

		private T end;

		private T value;

		public float CurrentTime => currentTime;

		public float Duration => duration;

		public TweenState State => state;

		public T StartValue => start;

		public T EndValue => end;

		public T CurrentValue => value;

		public Tween(LerpFunc<T> lerpFunc)
		{
			this.lerpFunc = lerpFunc;
			state = TweenState.Stopped;
		}

		public void Start(T start, T end, float duration, ScaleFunc scaleFunc)
		{
			if (duration <= 0f)
			{
				throw new ArgumentException("duration must be greater than 0");
			}
			if (scaleFunc == null)
			{
				throw new ArgumentNullException("scaleFunc");
			}
			currentTime = 0f;
			this.duration = duration;
			this.scaleFunc = scaleFunc;
			state = TweenState.Running;
			this.start = start;
			this.end = end;
			UpdateValue();
		}

		public void Pause()
		{
			if (state == TweenState.Running)
			{
				state = TweenState.Paused;
			}
		}

		public void Resume()
		{
			if (state == TweenState.Paused)
			{
				state = TweenState.Running;
			}
		}

		public void Stop(StopBehavior stopBehavior)
		{
			state = TweenState.Stopped;
			if (stopBehavior == StopBehavior.ForceComplete)
			{
				currentTime = duration;
				UpdateValue();
			}
		}

		public void Update(float elapsedTime)
		{
			if (state == TweenState.Running)
			{
				currentTime += elapsedTime;
				if (currentTime >= duration)
				{
					currentTime = duration;
					state = TweenState.Stopped;
				}
				UpdateValue();
			}
		}

		private void UpdateValue()
		{
			value = lerpFunc(start, end, scaleFunc(currentTime / duration));
		}
	}
}
