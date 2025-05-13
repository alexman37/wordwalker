using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Keeps track of periodic time intervals if we wish. 
/// Currently its only use is in the "Timer" challenge.
/// </summary>

public class TimeManager : MonoBehaviour
{
    private bool timerEnabled;
    private float timeRemaining;
    public static float timeInterval = 7f;  // How many seconds in each interval
    private int lastFullSecond = (int) timeInterval;
    private int interval = -1;

    // The second has changed - update displays
    public static event Action<int> secondChanged;

    // The timer has run out - do whatever you initially sought out to do
    public static event Action<int> timerExpired;


    // Start is called before the first frame update
    void Start()
    {
        secondChanged += (_) => { };
        timerExpired += (_) => { };
    }

    // Update is called once per frame
    void Update()
    {
        if(timerEnabled)
        {
            // If we run out of time reset the timer and increase the interval
            timeRemaining -= Time.deltaTime;
            if(Mathf.Ceil(timeRemaining) != lastFullSecond)
            {
                lastFullSecond = (int) Mathf.Ceil(timeRemaining);
                secondChanged.Invoke(lastFullSecond);
            }
            if(timeRemaining <= 0)
            {
                timeRemaining = timeInterval;
                ++interval;
                timerExpired.Invoke(interval);
            }
        }
    }

    public void startIntervalTimer()
    {
        timeRemaining = timeInterval;
        interval = -1;
        lastFullSecond = (int) timeInterval;
        timerEnabled = true;
    }

    public void stopIntervalTimer()
    {
        timerEnabled = false;
    }
}
