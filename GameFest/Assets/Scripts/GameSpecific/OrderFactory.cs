using System;
using System.Collections.Generic;

/// <summary>
/// Class for dealing with the creation of orders
/// </summary>
public static class OrderFactory
{
    /// <summary>
    /// Generates a burger with random components
    /// </summary>
    /// <returns>The constructed burger</returns>
    public static BurgerConstruction GetOrder()
    {
        BurgerConstruction burger = new BurgerConstruction();

        // bun
        var bun = GetValue_<BunType>();
        burger.AddItem(new BurgerBun(bun));    // top of bun

        // veg - can be 0, 1, or 2 items
        var numVeg = SpawnItemDistributionFetcher<int>.GetRandomEnumValue(GetDistributionVeg_());
        for(int i = 0; i < numVeg; i++)
            burger.AddItem(new BurgerVeg(GetValue_<BurgerVegType>()));

        // burger - can be 1 or 2 pattys
        var numPattys = SpawnItemDistributionFetcher<int>.GetRandomEnumValue(GetDistributionBurgers_());
        var burgerType = GetValue_<BurgerType>();
        for (int i = 0; i < numPattys; i++)
            burger.AddItem(new BurgerPatty(burgerType));

        // sauce - can be 0 or 1 sauces
        var numSauce = SpawnItemDistributionFetcher<int>.GetRandomEnumValue(GetDistributionSauce_());
        for (int i = 0; i < numSauce; i++)
            burger.AddItem(new BurgerSauce(GetValue_<SauceType>(), 0));

        burger.AddItem(new BurgerBun(bun));    // bottom of bun

        return burger;
    }

    /// <summary>
    /// Templated class to generate a random value of the required type
    /// </summary>
    /// <typeparam name="T">THe type of item to generate</typeparam>
    /// <returns>The randomly generated value</returns>
    private static T GetValue_<T>()
    {
        var numValues = Enum.GetValues(typeof(T)).Length;
        var random = UnityEngine.Random.Range(0, numValues);

        // convert randomly generated value to the specified type, and return
        return (T)Enum.Parse(typeof(T), random.ToString());
    }


    /// <summary>
    /// Gets the distribution of how many veg items to include
    /// </summary>
    /// <returns>A list of enum values and their % likelihood to be spawned</returns>
    static List<Tuple<int, int>> GetDistributionVeg_()
    {
        List<Tuple<int, int>> distribution = new List<Tuple<int, int>>();
        distribution.Add(new Tuple<int, int>(0, 20));
        distribution.Add(new Tuple<int, int>(1, 60));
        distribution.Add(new Tuple<int, int>(2, 20));

        return distribution;
    }

    /// <summary>
    /// Gets the distribution of how many burgers to include
    /// </summary>
    /// <returns>A list of enum values and their % likelihood to be spawned</returns>
    static List<Tuple<int, int>> GetDistributionBurgers_()
    {
        List<Tuple<int, int>> distribution = new List<Tuple<int, int>>();
        distribution.Add(new Tuple<int, int>(1, 90));
        distribution.Add(new Tuple<int, int>(2, 10));

        return distribution;
    }
    
    /// <summary>
    /// Gets the distribution of how many sauces to include
    /// </summary>
    /// <returns>A list of enum values and their % likelihood to be spawned</returns>
    static List<Tuple<int, int>> GetDistributionSauce_()
    {
        List<Tuple<int, int>> distribution = new List<Tuple<int, int>>();
        distribution.Add(new Tuple<int, int>(0, 20));
        distribution.Add(new Tuple<int, int>(1, 80));

        return distribution;
    }
}
