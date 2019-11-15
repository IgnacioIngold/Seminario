using UnityEngine;
using Utility.Timers.RunTime;

public class TestTimer : MonoBehaviour
{
    [SerializeField] RT_ConditionalMultiStepTimer HitWindows = new RT_ConditionalMultiStepTimer();

    // Start is called before the first frame update
    void Start()
    {
        HitWindows.OnTimeStart +=() => { print("CountDown Started"); };
        HitWindows.OnTimesUp += () => { print("TimeIsFinished"); };
    }

    // Update is called once per frame
    void Update()
    {
        if (HitWindows.isReady)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                HitWindows.Start();
        }

        if (!HitWindows.isReady)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                HitWindows.NextStep();

            if ((Input.GetKeyDown(KeyCode.S)))
            {
                if (HitWindows.isPaused)
                    HitWindows.Continue();
                else
                    HitWindows.Pause();
            }

            if (Input.GetKeyDown(KeyCode.D))
                HitWindows.Restart();
        }

        HitWindows.Update();
    }
}
