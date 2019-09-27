using System;
using UnityEngine;

namespace Utility.Timers
{
	public abstract class Timer
	{
		public bool isReady = true;
		[SerializeField]
		protected float Time;
		[SerializeField]
		protected float Presition;

		public float CurrentTime { get { return Time; } set { Time = value; } }

		public virtual void StartCount() { }
		public virtual void StartCount(float From) { }
		public virtual void Reset() { }
		public virtual void Pause() { }
		public virtual void Continue() { }
	}
}
