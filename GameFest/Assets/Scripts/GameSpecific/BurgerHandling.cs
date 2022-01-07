using System.Collections.Generic;
using System.Diagnostics;

// types of each item
enum BurgerVegType { Lettuce, Tomato, Pickle };
enum BunType { Brioche, Sesame };
enum SauceType { Ketchup, BBQ, Mustard };
enum BurgerType { Beef, Chicken, Veggie };

/// <summary>
/// Logic for handling the patty part of a burger
/// </summary>
class BurgerPatty
{
    // thresholds for when a burger is cooked
    // MIN is higher than MAX because the r, g, b values go from 1 to 0
    public static float MIN_COOKED_LEVEL_BEEF = 0.3f;
    public static float MAX_COOKED_LEVEL_BEEF = 0.5f;
    public static float MIN_COOKED_LEVEL_VEGGIE = 0.6f;
    public static float MAX_COOKED_LEVEL_VEGGIE = 0.8f;
    public static float MIN_COOKED_LEVEL_CHICKEN = 0.5f;
    public static float MAX_COOKED_LEVEL_CHICKEN = 0.7f;

    // fields
    BurgerType _type;
    float[] _cookedLevel;
    float _heat;

    // accessors
    public BurgerType GetBurgerType() { return _type; }
    public float[] CookedLevel() { return _cookedLevel; }
    public float Heat() { return _heat; }

    /// <summary>
    /// Get the minimum value for perfectly cooked for the specified burger
    /// </summary>
    /// <param name="type">The type of burger to check</param>
    /// <returns>The minimum value that needs to be hit for the burger to be perfectly cooked</returns>
    public static float MinCookedLevel(BurgerType type)
    {
        float value = 1f;

        switch (type)
        {
            case BurgerType.Beef: value = MIN_COOKED_LEVEL_BEEF; break;
            case BurgerType.Chicken: value = MIN_COOKED_LEVEL_CHICKEN; break;
            case BurgerType.Veggie: value = MIN_COOKED_LEVEL_VEGGIE; break;
            default: Debug.Assert(false, "Invalid burger type specified"); break;
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

        switch (type)
        {
            case BurgerType.Beef: value = MAX_COOKED_LEVEL_BEEF; break;
            case BurgerType.Chicken: value = MAX_COOKED_LEVEL_CHICKEN; break;
            case BurgerType.Veggie: value = MAX_COOKED_LEVEL_VEGGIE; break;
            default: Debug.Assert(false, "Invalid burger type specified"); break;
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
}

/// <summary>
/// Logic for handling the sauce part of a burger
/// </summary>
class BurgerSauce
{
    // fields
    SauceType _type;

    // accessors
    public SauceType GetSauceType() { return _type; }
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

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="pointsLost">How many points were lost due to this complaint</param>
    /// <param name="error">Message describing the complaint</param>
    public BurgerComplaint(int pointsLost, string complaint)
    {
        _pointsLost = pointsLost;
        _complaint = complaint;
    }

    // accessors
    public int PointsLost() { return _pointsLost; }
    public string ErrorMessage() { return _complaint; }
}

  /// <summary>
  /// Class to store a customers order
  /// </summary>
  public class CustomerOrder
  {
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
      /// <param name="burger">The burger that was received</param>
      public void BurgerReceived(BurgerConstruction burger)
      {
          _given = burger;
      }

      // Accessors
      public BurgerConstruction GetRequest()
      {
          return _request;
      }

      public BurgerConstruction GetActual()
      {
          return _request;
      }
}
