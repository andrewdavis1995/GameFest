using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Gets the % chance of each item to be spawned
/// </summary>
/// <typeparam name="T">The enum to get a random </typeparam>
public static class SpawnItemDistributionFetcher<T>
{
    /// <summary>
    /// Get a list of 100 items, with 1 item for each % of likelihood that the item will be generated.
    /// Will assert if number of items is not 100
    /// </summary>
    /// <param name="distribution">The list of values and their percentages</param>
    /// <returns>A list of 100 items, in varying quantities</returns>
    static List<T> GetDistribution_(List<Tuple<T, int>> distribution)
    {
        List<T> values = new List<T>();

        // loop through every item provided
        foreach (var item in distribution)
        {
            // add the item X times, where X is the % of the distribution it was given
            for (int i = 0; i < item.Item2; i++)
                values.Add(item.Item1);
        }

        // assert that there are 100 items in the list (only shows in debug)
        Debug.Assert(values.Count == 100);

        return values;
    }

    /// <summary>
    /// Gets a random value from the given enum, using the distribution list for % likelihood
    /// </summary>
    /// <param name="list">The list of values and the % of the distribution they have</param>
    /// <returns>A random value from the enum</returns>
    public static T GetRandomEnumValue(List<Tuple<T, int>> list)
    {
        // get a list of 100 items, distributed by their % likelihood
        var items = GetDistribution_(list);

        // get random value
        int randomIndex = UnityEngine.Random.Range(0, items.Count);
        var value = items[randomIndex];

        return value;
    }

}
