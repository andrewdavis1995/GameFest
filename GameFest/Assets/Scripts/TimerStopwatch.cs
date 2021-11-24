using System;
using System.Collections;
using UnityEngine;

public class TimeStopwatch : MonoBehaviour
{
    // configuration
    Action<int> _onTick;
    float _interval;

    // status variables
    int _currentTime = 0;
    bool _running = false;

    /// <summary>
    /// Sets the parameters of the timer
    /// </summary>
    /// <param name="tickCallback">The function to call every second</param>
    /// <param name="interval">The time to wait (seconds)</param>
    public void Initialise(Action<int> tickCallback, float interval = 1f)
    {
        _onTick = tickCallback;
        _interval = interval;
    }

    /// <summary>
    /// Starts the countdown ticking
    /// </summary>
    public void StartTimer()
    {
        Restart();
        if (!_running)
            StartCoroutine(Process());
    }

    /// <summary>
    /// Return to the start time
    /// </summary>
    public void Restart()
    {
        _currentTime = 0;
    }

    /// <summary>
    /// Stop the timer
    /// </summary>
    public void Abort()
    {
        _running = false;
    }

    /// <summary>
    /// Runs the countdown
    /// </summary>
    public IEnumerator Process()
    {
        _running = true;

        // tick callback
        _onTick?.Invoke(_currentTime);

        // loop until stopped
        while (_running)
        {
            // tick callback every second
            yield return new WaitForSeconds(_interval);
            _onTick?.Invoke(_currentTime);
            _currentTime++;
        }
    }

    /// <summary>
    /// Return the value of the timer
    /// </summary>
    /// <returns>The time elapsed</returns>
    public int GetCurrentTime()
    {
        return _currentTime;
    }
}
