using System.Linq;
using UnityEngine.UI;

public class PieChart : ComparisonGraphScriptBase
{
    /// <summary>
    /// Sets the values for each segment
    /// </summary>
    /// <param name="values">The values to display</param>
    /// <param name="display">What to display in the centre of the circle</param>
    public override void SetValues(float[] values, string display)
    {
        StopAllCoroutines();
        StartCoroutine(DisplayData(values.ToArray(), display));
    }
}