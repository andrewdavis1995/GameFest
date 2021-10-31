using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ComparisonGraphScriptBase : MonoBehaviour
{
    List<float> _currentValues = new List<float>();
    public Image[] DisplayImages;
    public Text TxtDisplayValue;

    /// <summary>
    /// Sets the values for each segment
    /// </summary>
    /// <param name="values">The values to display</param>
    /// <param name="display">What to display in the centre of the circle</param>
    public virtual void SetValues(float[] values, string display)
    {
        StopAllCoroutines();

        ResetDisplay_();

        List<float> displayValues = new List<float>();
        foreach (var v in values)
            displayValues.Add(v);

        var max = values.Max();

        var index = 0;
        foreach (var v in values)
        {
            displayValues[index] = v / max;
            index++;
        }

        StartCoroutine(DisplayData(displayValues.ToArray(), display));
    }

    /// <summary>
    /// Increases the size of each display until complete
    /// </summary>
    /// <param name="values">The values to display</param>
    /// <param name="display">What to display in the centre of the circle</param>
    /// <returns>Data to show</returns>
    public IEnumerator DisplayData(float[] values, string display)
    {
        TxtDisplayValue.text = display;

        _currentValues = new List<float>();
        foreach (var f in values)
            _currentValues.Add(0);

        bool running = false;
        do
        {
            running = false;

            // loop through each control
            for (int i = 0; i < _currentValues.Count; i++)
            {
                // go until target value is met
                if (_currentValues[i] < values[i])
                {
                    // increase size
                    running = true;
                    _currentValues[i] += 0.025f;
                    DisplayImages[i].fillAmount = _currentValues[i];
                }
                else
                {
                    Debug.Log(i);

                    // done
                    DisplayImages[i].fillAmount = values[i];
                }
            }
            yield return new WaitForSeconds(0.001f);

        } while (running);
    }

    /// <summary>
    /// Resets the values to zero
    /// </summary>
    void ResetDisplay_()
    {
        StopAllCoroutines();

        // loop through each control
        for (int i = 0; i < DisplayImages.Length; i++)
        {
            DisplayImages[i].fillAmount = 0;
        }
    }
}