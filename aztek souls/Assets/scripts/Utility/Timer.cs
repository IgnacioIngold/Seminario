using System;
using UnityEngine;

namespace Utility.Timers
{
	public abstract class Timer
	{
		public Action OnTimesUp = delegate { };

		[SerializeField]
		protected bool isReady = true;
		[SerializeField]
		protected float Time;
		[SerializeField]
		protected float Presition;
		
		public bool Ready { get { return isReady; } private set { isReady = value; } }
		public float CurrentTime { get { return Time; } set { Time = value; } }

		public virtual void StartCount() { }
		public virtual void StartCount(float From) { }
		public virtual void Reset() { }
		public virtual void Pause() { }
		public virtual void Continue() { }
	}
}
