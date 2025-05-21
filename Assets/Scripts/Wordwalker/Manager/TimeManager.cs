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

    private static Dictionary<string, float> namedTimer;
    private static float totalTimeElapsed = 0;

    // The timer is now on or off
    public static event Action<bool> activationChange;

    // The second has changed - update displays
    public static event Action<int> secondChanged;

    // The timer has run out - do whatever you initially sought out to do
    public static event Action<int> timerExpired;


    // Start is called before the first frame update
    void Start()
    {
        namedTimer = new Dictionary<string, float>();

        activationChange += (_) => { };
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

        totalTimeElapsed += Time.deltaTime;
    }

    /// INTERVAL TIMER
    public void startIntervalTimer()
    {
        timeRemaining = timeInterval;
        interval = -1;
        lastFullSecond = (int) timeInterval;
        timerEnabled = true;
        activationChange.Invoke(true);
        timerExpired.Invoke(-1);
    }

    public void stopIntervalTimer()
    {
        timerEnabled = false;
        activationChange.Invoke(false);
    }

    /// INTERVAL TIMER
    public static void startNamedTimer(string n)
    {
        if(namedTimer.ContainsKey(n))
        {
            Debug.LogWarning("Cannot start timer of name " + n + ": already exists");
        } else
        {
            Debug.Log("Set key " + n);
            namedTimer[n] = totalTimeElapsed;
        }
    }

    public static float stopNamedTimer(string n)
    {
        Debug.Log("Search for key " + n);
        float timeStarted = namedTimer[n];
        namedTimer.Remove(n);
        return totalTimeElapsed - timeStarted;
    }
}
