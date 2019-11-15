using System;
using UnityEngine;

namespace Utility.Timers.RunTime
{
    [Serializable]
    public class RT_ConditionalMultiStepTimer
    {
        public Action OnTimeStart = delegate { };
        public Action OnTimesUp = delegate { };

        [HideInInspector] public bool isReady = true;
        [HideInInspector] public bool LastStep = false;
        [HideInInspector] public bool isPaused = false;

        public float[] steps = new float[0];
        public int Step
        {
            get => _currentStep;
            set
            {
                int maxValue = steps.Length > 1 ? steps.Length - 1 : 0;
                _currentStep = Mathf.Clamp(value, 0, maxValue);

                if (_currentStep == maxValue)
                    LastStep = true;
            }
        }
        public float normalizedTime
        {
            get => (_remainingTime / steps[_currentStep]);
        }

        int _currentStep;
        float _remainingTime = 0;

        public void Start()
        {
            if (steps.Length == 0 || !isReady) return;

            OnTimeStart();
            isReady = false;
            _remainingTime = steps[Step];
            if (_remainingTime == 0)
            {
                Restart();
                OnTimesUp();
            }
        }
        public void Update()
        {
            if (steps.Length == 0) return;

            if (!isReady && !isPaused && _remainingTime > 0)
            {
                _remainingTime -= Time.deltaTime;

                if (_remainingTime <= 0)
                {
                    OnTimesUp();
                    Restart();
                }
            }
        }

        public void NextStep()
        {
            if (steps.Length == 0) return;

            Step++;
            if (!LastStep)
            {
                _remainingTime = steps[Step];
                OnTimeStart();
            }
        }

        public void Pause()
        {
            isPaused = true;
        }
        public void Continue()
        {
            isPaused = false;
        }
        public void Restart()
        {
            Step = 0;
            isReady = true;
            isPaused = false;
            LastStep = false;

            if (steps.Length > 0) _remainingTime = steps[0];
        }
    }
}
