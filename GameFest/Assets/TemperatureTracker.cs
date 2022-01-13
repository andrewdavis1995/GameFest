using System.Collections;
using UnityEngine;

/// <summary>
/// Tracks the temperature of a burger
/// </summary>
public class TemperatureTracker : MonoBehaviour
{
    int _temperature = 100;

    // Start is called before the first frame update
    void OnEnable()
    {
        StopAllCoroutines();
        _temperature = 100;
        StartCoroutine(Countdown_());
    }

    /// <summary>
    /// Decrease temperature over time
    /// <returns></returns>
    private IEnumerator Countdown_()
    {
        while (_temperature > 0)
        {
            _temperature--;
            yield return new WaitForSeconds(0.8f);
        }
    }

    /// <summary>
    /// Get the current temperature
    /// </summary>
    /// <returns>The temperature of the burger</returns>
    public int Temperature()
    {
        return _temperature;
    }
}
