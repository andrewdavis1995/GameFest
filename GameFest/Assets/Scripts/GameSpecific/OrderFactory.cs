using System;

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
        burger.AddItem(bun);    // top of bun

        // veg - can be 0, 1, or 2 items
        var numVeg = 1; // TODO: Use distribution class
        for(int i = 0; i < numVeg; i++)
            burger.AddItem(GetValue_<BurgerVegType>());

        // burger - can be 1 or 2 pattys
        var numPattys = 1; // TODO: Use distribution class
        for (int i = 0; i < numPattys; i++)
            burger.AddItem(GetValue_<BurgerType>());

        // sauce - can be 0 or 1 sauces
        var numSauce = 1;   // TODO: Use distribution class
        for (int i = 0; i < numSauce; i++)
            burger.AddItem(GetValue_<SauceType>());

        burger.AddItem(bun);    // bottom of bun

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
}
