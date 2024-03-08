using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class TimerHelper : MonoBehaviour
{
    public static TimerHelper instance { get; private set; }

    private List<Timer> timers = new List<Timer>();
    bool hasInit;
    public void Init()
    {
        if (instance == null)
            instance = this;
        hasInit = true;
    }
    public 
    // Update is called once per frame
    void Update()
    {
        if (!hasInit) return;
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            Timer timer = timers[i];
            if (timer.isRunning)
            {
                timer.timeLeft -= Time.deltaTime;
                if (timer.timeLeft <= 0)
                {
                    timer.isRunning = false;
                    timer.onTimerComplete?.Invoke();
                    timers.RemoveAt(i);
                }
            }
        }
    }
    public Action StartTimer(float duration, Action onTimerComplete = null)
    {
        Timer newTimer = new Timer
        {
            duration = duration,
            timeLeft = duration,
            isRunning = true,
            onTimerComplete = onTimerComplete
        };
        timers.Add(newTimer);

        // Tr? v? m?t hàm ?? d? dàng d?ng timer
        Action stopTimerAction = () =>
        {
            newTimer.isRunning = false;
            timers.Remove(newTimer);
        };

        return stopTimerAction;
    }
}
[System.Serializable]
public class Timer
{
    public float duration;
    public float timeLeft;
    public bool isRunning;
    public Action onTimerComplete;
}
