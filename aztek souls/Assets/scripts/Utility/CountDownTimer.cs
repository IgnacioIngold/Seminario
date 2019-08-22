using System;
using System.Collections;
using UnityEngine;

namespace Utility.Timers
{
	[Serializable]
	public class CountDownTimer : Timer
	{
		public float CoolDown;
		MonoBehaviour _mono;

		//Constructores
		public CountDownTimer(MonoBehaviour MonoObject, float Presition = 0.01f)
		{
			this.Presition = Presition;
			_mono = MonoObject;
		}
		public CountDownTimer SetCoolDown(float CoolDown)
		{
			this.CoolDown = CoolDown;
			return this;
		}

		public override void StartCount()
		{
			if (isReady)
			{
				isReady = false;
				Time = CoolDown;
				_mono.StartCoroutine(CountDown());
			}
		}
		public override void StartCount(float From)
		{
			if (isReady)
			{
				isReady = false;
				Time = CoolDown;
				_mono.StartCoroutine(CountDown(From));
			}
		}

		public override void Reset()
		{
			isReady = true;
			Time = CoolDown;
			_mono.StopAllCoroutines();
		}
		public override void Pause()
		{
			_mono.StopAllCoroutines();
		}
		public override void Continue()
		{
			_mono.StartCoroutine(CountDown(Time));
		}

		IEnumerator CountDown()
		{
			while( Time > 0)
			{
				Time -= Presition;
				yield return new WaitForSeconds(Presition);
			}

			isReady = true;
			Time = CoolDown;
			OnTimesUp();
		}
		IEnumerator CountDown(float From)
		{
			Time = From;
			while ( Time > 0)
			{
				Time -= Presition;
				yield return new WaitForSeconds(Presition);
			}
			
			isReady = true;
			Time = CoolDown;
			OnTimesUp();
		}
	}

    public static class Timers
    {
        public static CountDownTimer CreateCountDownTimer(this MonoBehaviour MonoObject, float Presition = 0.01f)
        {
            return new CountDownTimer(MonoObject, Presition);
        }
    }
}
