using System;
using System.Collections;
using UnityEngine;

public class TimeLimit : MonoBehaviour
{
    // configuration
    int _timeLimit;
    Action<int> _onTick;
    Action _onTimeout;

    // status variables
    int _currentTime = 0;
    bool _aborted = false;
    bool _running = false;

    /// <summary>
    /// Sets the parameters of the timer
    /// </summary>
    /// <param name="limit">How many seconds the timeout is</param>
    /// <param name="tickCallback">The function to call every second</param>
    /// <param name="timeoutCallback">The function to call upon timeout</param>
    public void Initialise(int limit, Action<int> tickCallback, Action timeoutCallback)
    {
        _timeLimit = limit;
        _onTick = tickCallback;
        _onTimeout = timeoutCallback;
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
        _currentTime = _timeLimit;
        _aborted = false;
    }

    /// <summary>
    /// Stop the timer
    /// </summary>
    public void Abort()
    {
        _aborted = true;
    }

    /// <summary>
    /// Runs the countdown
    /// </summary>
    public IEnumerator Process()
    {
        _running = true;

        // tick callback
        _onTick?.Invoke(_currentTime);

        // loop until out of time of aborted
        while (_currentTime >= 0 && !_aborted)
        {
            // tick callback every second
            yield return new WaitForSeconds(1);
            _onTick?.Invoke(_currentTime);
            _currentTime--;
        }

        // run the timeout callback, unless the timer was aborted
        if (!_aborted)
            _onTimeout?.Invoke();

        _running = false;
    }
}
