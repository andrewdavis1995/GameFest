using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComparisonGraphScript : ComparisonGraphScriptBase
{
    /// <summary>
    /// Sets the values for each segment
    /// </summary>
    /// <param name="values">The values to display</param>
    /// <param name="display">What to display in the centre of the circle</param>
    public override void SetValues(float[] values, string display)
    {
        StopAllCoroutines();

        List<float> _values = new List<float>();
        foreach (var v in values)
            _values.Add(v);

        var totalAmount = values.Sum() > 0 ? values.Sum() : 1;

        var runningTotal = 0f;
        var index = 0;

        // score needs to include all previous scores
        foreach (var v in values)
        {
            runningTotal += v;
            _values[index] = runningTotal / totalAmount;

            index++;
        }

        StartCoroutine(DisplayData(_values.ToArray(), display));
    }
}