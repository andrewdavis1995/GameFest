using System.Collections.Generic;
using UnityEngine;

// types of each item
public enum BurgerVegType { Lettuce, Tomato, Pickle };
public enum BunType { Brioche, Sesame, Brown };
public enum SauceType { Ketchup, BBQ, Mustard };
public enum BurgerType { Beef, Chicken, Veggie };

/// <summary>
/// Logic for handling the patty part of a burger
/// </summary>
class BurgerPatty
{
    // thresholds for when a burger is cooked
    // MIN is higher than MAX because the r, g, b values go from 1 to 0
    public static float MIN_COOKED_LEVEL_BEEF = 0.2f;
    public static float MAX_COOKED_LEVEL_BEEF = 0.4f;
    public static float MIN_COOKED_LEVEL_VEGGIE = 0.5f;
    public static float MAX_COOKED_LEVEL_VEGGIE = 0.7f;
    public static float MIN_COOKED_LEVEL_CHICKEN = 0.4f;
    public static float MAX_COOKED_LEVEL_CHICKEN = 0.6f;

    // fields
    BurgerType _type;
    float[] _cookedLevel;
    float _heat;
    Color _colour;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="burger">Type of burger</param>
    /// <param name="cookedLevels">How cooked the burger is</param>
    /// <param name="heat">Heat of the burger</param>
    /// <param name="colour">Colour of the burger</param>
    public BurgerPatty(BurgerType burger, float[] cookedLevels, float heat, Color colour)
    {
        _type = burger;
        _cookedLevel = cookedLevels;
        _heat = heat;
        _colour = colour;
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="burger">Type of burger</param>
    public BurgerPatty(BurgerType burger)
    {
        _type = burger;
    }

    // mutators
    public void SetHeat(int heat)
    {
        _heat = heat;
    }

    // accessors
    public BurgerType GetBurgerType() { return _type; }
    public float[] CookedLevel() { return _cookedLevel; }
    public float Heat() { return _heat; }
    public Color Colour() { return _colour; }

    /// <summary>
    /// Get the minimum value for perfectly cooked for the specified burger
    /// </summary>
    /// <param name="type">The type of burger to check</param>
    /// <returns>The minimum value that needs to be hit for the burger to be perfectly cooked</returns>
    public static float MinCookedLevel(BurgerType type)
    {
        float value = 1f;

        // get the correct threshold based on the type
        switch (type)
        {
            case BurgerType.Beef: value = MIN_COOKED_LEVEL_BEEF; break;
            case BurgerType.Chicken: value = MIN_COOKED_LEVEL_CHICKEN; break;
            case BurgerType.Veggie: value = MIN_COOKED_LEVEL_VEGGIE; break;
            default: System.Diagnostics.Debug.Assert(false, "Invalid burger type specified"); break;
        }

        return value;
    }

    /// <summary>
    /// Get the maximum value for perfectly cooked for the specified burger
    /// </summary>
    /// <param name="type">The type of burger to check</param>
    /// <returns>The maximum value that needs to be hit for the burger to be perfectly cooked</returns>
    public static float MaxCookedLevel(BurgerType type)
    {
        float value = 1f;

        // get the correct threshold based on the type
        switch (type)
        {
            case BurgerType.Beef: value = MAX_COOKED_LEVEL_BEEF; break;
            case BurgerType.Chicken: value = MAX_COOKED_LEVEL_CHICKEN; break;
            case BurgerType.Veggie: value = MAX_COOKED_LEVEL_VEGGIE; break;
            default: System.Diagnostics.Debug.Assert(false, "Invalid burger type specified"); break;
        }

        return value;
    }
}

/// <summary>
/// Logic for handling the bun part of a burger
/// </summary>
class BurgerBun
{
    // fields
    BunType _type;

    // accessors
    public BunType GetBunType() { return _type; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bunType">The type to set</param>
    public BurgerBun(BunType bunType)
    {
        _type = bunType;
    }
}

/// <summary>
/// Logic for handling the sauce part of a burger
/// </summary>
class BurgerSauce
{
    // fields
    SauceType _type;
    float _size;

    // accessors
    public SauceType GetSauceType() { return _type; }
    public float GetSauceSize() { return _size; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="sauceType">The type to set</param>
    public BurgerSauce(SauceType sauceType, float size)
    {
        _type = sauceType;
        _size = size;
    }
}

/// <summary>
/// Logic for handling the veg part of a burger
/// </summary>
class BurgerVeg
{
    // fields
    BurgerVegType _type;

    // accessors
    public BurgerVegType GetVegType() { return _type; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="vegType">The type to set</param>
    public BurgerVeg(BurgerVegType vegType)
    {
        _type = vegType;
    }
}

/// <summary>
/// Logic for handling the construction of a burger
/// </summary>
public class BurgerConstruction
{
    // fields
    List<object> _items = new List<object>();

    // accessors
    public List<object> GetItems() { return _items; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="items">List of items</param>
    public BurgerConstruction(List<object> items)
    {
        _items.AddRange(items);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public BurgerConstruction()
    {

    }

    /// <summary>
    /// Adds an item to the burger
    /// </summary>
    /// <param name="item">The item to add</param>
    public void AddItem(object item)
    {
        _items.Add(item);
    }
}

/// <summary>
/// Handles a complaint about a burger component
/// </summary>
public class BurgerComplaint
{
    // fields
    int _pointsLost;
    string _complaint;
    Sprite _sprite;
    Color _spriteColour;
    int _spriteIndex;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="pointsLost">How many points were lost due to this complaint</param>
    /// <param name="error">Message describing the complaint</param>
    /// <param name="sprite">Sprite to show in the left burger</param>
    /// <param name="colour">Colour to set on the sprite to show in the left burger</param>
    /// <param name="spriteIndex">Index of the sprite</param>
    public BurgerComplaint(int pointsLost, string complaint, Sprite sprite, Color colour, int spriteIndex = -1)
    {
        _pointsLost = pointsLost;
        _complaint = complaint;
        _sprite = sprite;
        _spriteColour = colour;
        _spriteIndex = spriteIndex;
    }

    // accessors
    public int PointsLost() { return _pointsLost; }
    public string ErrorMessage() { return _complaint; }
    public Sprite GetSprite() { return _sprite; }
    public Color GetColour() { return _spriteColour; }
    public int GetSpriteIndex() { return _spriteIndex; }
}

/// <summary>
/// Class to store a customers order
/// </summary>
public class CustomerOrder
{
    // fields
    BurgerConstruction _request;
    BurgerConstruction _given;
    string _customerName;

    /// <summary>
    /// Constructor
    /// </summary>
    public CustomerOrder(string name, BurgerConstruction request)
    {
        _customerName = name;
        _request = OrderFactory.GetOrder();
    }

    /// <summary>
    /// A burger has been sent to the customer
    /// </summary>
    /// <param name="items">The burger items that were received</param>
    public void BurgerReceived(List<object> items)
    {
        _given = new BurgerConstruction(items);
    }

    // Accessors
    public BurgerConstruction GetRequest()
    {
        return _request;
    }

    public BurgerConstruction GetActual()
    {
        return _given;
    }

    public string GetCustomerName()
    {
        return _customerName;
    }
}
