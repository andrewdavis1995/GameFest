using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Class to handle the comparison of two burgers
/// </summary>
public static class BurgerValidation
{
    // values for heat
    const float MIN_HEAT_THRESHOLD = 90f;

    // values for how many points to deduct for each mistake
    const int DEDUCTION_WRONG_BUN = 10;
    const int DEDUCTION_WRONG_PATTY = 25;
    const int DEDUCTION_WRONG_VEG = 10;
    const int DEDUCTION_WRONG_SAUCE = 10;

    const int DEDUCTION_MISSING_BUN = 20;
    const int DEDUCTION_MISSING_PATTY = 30;
    const int DEDUCTION_MISSING_VEG = 10;
    const int DEDUCTION_MISSING_SAUCE = 5;

    const int DEDUCTION_EXTRA_PATTY = 15;
    const int DEDUCTION_EXTRA_VEG = 5;
    const int DEDUCTION_EXTRA_SAUCE = 5;

    const int DEDUCTION_COLD = 10;
    const int DEDUCTION_UNCOOKED = 15;
    const int DEDUCTION_OVERCOOKED = 10;
    const int DEDUCTION_BURNED = 15;

    // strings for reporting mistakes
    const string REASON_WRONG_BUN = "I got the wrong bun";
    const string REASON_WRONG_PATTY = "I got the wrong burger";
    const string REASON_WRONG_VEG = "I got the wrong veg";
    const string REASON_WRONG_SAUCE = "I got the wrong sauce";

    const string REASON_MISSING_BUN = "I only got half a bun";
    const string REASON_MISSING_PATTY = "I didn't get a burger";
    const string REASON_MISSING_VEG_PREFIX = "I asked for ";
    const string REASON_MISSING_SAUCE_PREFIX = "I asked for ";

    const string REASON_EXTRA_PATTY = "I got an extra burger";
    const string REASON_EXTRA_VEG_PREFIX = "I got extra ";
    const string REASON_EXTRA_SAUCE_PREFIX = "I got extra ";

    const string REASON_COLD = "The burger was cold";
    const string REASON_UNCOOKED_PREFIX = "The burger was not cooked on ";
    const string REASON_OVERCOOKED_PREFIX = "The burger was over cooked on ";
    const string REASON_BURNED_PREFIX = "The burger was burnt to a crisp on";

    /// <summary>
    /// Checks how similar to the requested burger the created burger was
    /// </summary>
    /// <param name="created">The burger that was created</param>
    /// <param name="target">The burger that was requested</param>
    /// <returns>A list of complaints/differences between the created and requested burgers</returns>
    public static List<BurgerComplaint> ValidateBurger(CustomerOrder order)
    {
        List<BurgerComplaint> complaints = new List<BurgerComplaint>();

        // get editable list of items that were added to the created burger
        List<object> itemsServed = order.GetActual().GetItems().ToList();
        List<object> itemsRequested = order.GetRequest().GetItems().ToList();

        // check each item is present
        for (int i = 0; i < itemsRequested.Count; i++)
        {
            bool found = false;
            var item = itemsRequested[i];

            // buns
            if (item is BurgerBun)
            {
                found = CompareBuns_(item as BurgerBun, ref itemsServed, ref complaints);
            }
            // salad
            if (item is BurgerVeg)
            {
                found = CompareVeg_(item as BurgerVeg, ref itemsServed, ref complaints);
            }
            // burger
            if (item is BurgerPatty)
            {
                found = ComparePattys_(item as BurgerPatty, ref itemsServed, ref complaints);
            }
            // sauce
            if (item is BurgerSauce)
            {
                found = CompareSauces_(item as BurgerSauce, ref itemsServed, ref complaints);
            }

            if (found)
            {
                itemsRequested.RemoveAt(i);
                i--;
            }

            // nothing left to compare against
            if (itemsServed.Count == 0) break;
        }

        // check missing items
        AssessMissingItems_(itemsRequested, ref complaints);

        // check additional items
        AssessAdditionalItems_(itemsServed, ref complaints);

        // check burger temperature and cooked level
        AssessBurgerCooked_(order.GetActual(), ref complaints);

        return complaints;
    }

    /// <summary>
    /// Assesses if a burger is cooked correctly
    /// </summary>
    /// <param name="created">The burger that was created</param>
    /// <param name="complaints">Reference to the list of complaints/differences between the created and requested burgers</param>
    private static void AssessBurgerCooked_(BurgerConstruction created, ref List<BurgerComplaint> complaints)
    {
        // loop through all items in the created burger
        foreach (var item in created.GetItems())
        {
            // find all pattys
            if (item is BurgerPatty)
            {
                var patty = item as BurgerPatty;

                // if colder than allowed, add a complaint
                if (patty.Heat() < MIN_HEAT_THRESHOLD)
                {
                    // lose points based on how cold it is
                    var percentage = (MIN_HEAT_THRESHOLD - patty.Heat()) / MIN_HEAT_THRESHOLD;
                    complaints.Add(new BurgerComplaint((int)(DEDUCTION_COLD * percentage), REASON_COLD));
                }

                // find the range of perfectly cooked
                var min = BurgerPatty.MinCookedLevel(patty.GetBurgerType());
                var max = BurgerPatty.MaxCookedLevel(patty.GetBurgerType());

                int uncooked = 0;
                int overcooked = 0;
                int burnt = 0;

                // check each side of the patty and look for sides that are under/over cooked, or completely burnt
                foreach (var side in patty.CookedLevel())
                {
                    if (side < min && side > burnt)
                        uncooked++;
                    else if (side < burnt)
                        burnt++;
                    else if (side > max)
                        uncooked++;
                    // otherwise, good.
                }

                // if any sides uncooked, add complaint
                if (uncooked > 0)
                {
                    var msg = uncooked == 2 ? "Both sides" : "One side";
                    complaints.Add(new BurgerComplaint(DEDUCTION_UNCOOKED * uncooked, REASON_UNCOOKED_PREFIX + msg));
                }
                // if any sides burnt, add complaint
                if (burnt > 0)
                {
                    var msg = burnt == 2 ? "Both sides" : "One side";
                    complaints.Add(new BurgerComplaint(DEDUCTION_BURNED * burnt, REASON_BURNED_PREFIX + msg));
                }
                // if any sides overcooked, add complaint
                if (overcooked > 0)
                {
                    var msg = overcooked == 2 ? "Both sides" : "One side";
                    complaints.Add(new BurgerComplaint(DEDUCTION_OVERCOOKED * overcooked, REASON_OVERCOOKED_PREFIX + msg));
                }
            }
        }
    }

    /// <summary>
    /// Looks for a bun matching the specified bun
    /// </summary>
    /// <param name="item">The item to search for</param>
    /// <param name="itemsServed">List of items that were included in the burger</param>
    /// <param name="complaints">Reference to the list of complaints/differences between the created and requested burgers</param>
    /// <returns>Whether a match was found</returns>
    static bool CompareBuns_(BurgerBun item, ref List<object> itemsServed, ref List<BurgerComplaint> complaints)
    {
        bool found = false;

        // loop through each item
        for (var i = 0; i < itemsServed.Count; i++)
        {
            // find a bun
            if (itemsServed[i] is BurgerBun)
            {
                found = true;

                // compare types of buns
                if ((itemsServed[i] as BurgerBun).GetBunType() != ((item as BurgerBun).GetBunType()))
                {
                    complaints.Add(new BurgerComplaint(DEDUCTION_WRONG_BUN, REASON_WRONG_BUN));
                }

                itemsServed.RemoveAt(i);
                break;
            }
        }

        return found;
    }

    /// <summary>
    /// Looks for a patty matching the specified patty
    /// </summary>
    /// <param name="item">The item to search for</param>
    /// <param name="itemsServed">List of items that were included in the burger</param>
    /// <param name="complaints">Reference to the list of complaints/differences between the created and requested burgers</param>
    /// <returns>Whether a match was found</returns>
    static bool ComparePattys_(BurgerPatty item, ref List<object> itemsServed, ref List<BurgerComplaint> complaints)
    {
        bool matchFound = false;
        bool oneExists = false;

        // loop through list of created items
        for (var i = 0; i < itemsServed.Count; i++)
        {
            // find a patty
            if (itemsServed[i] is BurgerPatty)
            {
                // we've found one, so we can complain if it does not match
                oneExists = true;

                // compare types
                if ((itemsServed[i] as BurgerPatty).GetBurgerType() == ((item as BurgerPatty).GetBurgerType()))
                {
                    // we have found a match
                    matchFound = true;
                    itemsServed.RemoveAt(i);
                    break;
                }
            }
        }

        // if there is no match, and one was found of the wrong type, add a complaint
        if (!matchFound && oneExists)
            complaints.Add(new BurgerComplaint(DEDUCTION_WRONG_PATTY, REASON_WRONG_PATTY));

        return oneExists;
    }

    /// <summary>
    /// Looks for a veg matching the specified veg
    /// </summary>
    /// <param name="item">The item to search for</param>
    /// <param name="itemsServed">List of items that were included in the burger</param>
    /// <param name="complaints">Reference to the list of complaints/differences between the created and requested burgers</param>
    /// <returns>Whether a match was found</returns>
    static bool CompareVeg_(BurgerVeg item, ref List<object> itemsServed, ref List<BurgerComplaint> complaints)
    {
        bool matchFound = false;
        bool oneExists = false;

        // loop through each item
        for (var i = 0; i < itemsServed.Count; i++)
        {
            // find some veg
            if (itemsServed[i] is BurgerVeg)
            {
                // we've found one, so we can complain if it does not match
                oneExists = true;

                // compare types
                if ((itemsServed[i] as BurgerVeg).GetVegType() == ((item as BurgerVeg).GetVegType()))
                {
                    // we have found a match
                    matchFound = true;
                    itemsServed.RemoveAt(i);
                    break;
                }
            }
        }

        // if there is no match, and one was found of the wrong type, add a complaint
        if (!matchFound && oneExists)
            complaints.Add(new BurgerComplaint(DEDUCTION_WRONG_VEG, REASON_WRONG_VEG));

        return oneExists;
    }

    /// <summary>
    /// Looks for a sauce matching the specified sauce
    /// </summary>
    /// <param name="item">The item to search for</param>
    /// <param name="itemsServed">List of items that were included in the burger</param>
    /// <param name="complaints">Reference to the list of complaints/differences between the created and requested burgers</param>
    /// <returns>Whether a match was found</returns>
    static bool CompareSauces_(BurgerSauce item, ref List<object> itemsServed, ref List<BurgerComplaint> complaints)
    {
        bool matchFound = false;
        bool oneExists = false;

        // loop through each item
        for (var i = 0; i < itemsServed.Count; i++)
        {
            // find a sauce
            if (itemsServed[i] is BurgerSauce)
            {
                // we've found one, so we can complain if it does not match
                oneExists = true;

                // compare types
                if ((itemsServed[i] as BurgerSauce).GetSauceType() == ((item as BurgerSauce).GetSauceType()))
                {
                    // we have found a match
                    matchFound = true;
                    itemsServed.RemoveAt(i);
                    break;
                }
            }
        }

        // if there is no match, and one was found of the wrong type, add a complaint
        if (!matchFound && oneExists)
            complaints.Add(new BurgerComplaint(DEDUCTION_WRONG_SAUCE, REASON_WRONG_SAUCE));

        return oneExists;
    }

    /// <summary>
    /// Check which (if any) items are missing
    /// </summary>
    /// <param name="target">The </param>
    /// <param name="complaints"></param>
    private static void AssessMissingItems_(List<object> requested, ref List<BurgerComplaint> complaints)
    {
        // add a complaint suitable to the item type
        foreach (var item in requested)
        {
            if (item is BurgerBun) complaints.Add(new BurgerComplaint(DEDUCTION_MISSING_BUN, REASON_MISSING_BUN));
            if (item is BurgerPatty) complaints.Add(new BurgerComplaint(DEDUCTION_MISSING_PATTY, REASON_MISSING_PATTY));
            if (item is BurgerVeg) complaints.Add(new BurgerComplaint(DEDUCTION_MISSING_VEG, REASON_MISSING_VEG_PREFIX
                 + Enum.GetName(typeof(BurgerVegType), (int)((item as BurgerVeg).GetVegType()))));
            if (item is BurgerSauce) complaints.Add(new BurgerComplaint(DEDUCTION_MISSING_SAUCE, REASON_MISSING_SAUCE_PREFIX
                 + Enum.GetName(typeof(SauceType), (int)((item as BurgerSauce).GetSauceType()))));
        }
    }

    /// <summary>
    /// Check which (if any) items were added but not requested
    /// </summary>
    /// <param name="extras">The list of items that were added and not requested</param>
    /// <param name="complaints">Reference to the list of complaints that have been raised</param>
    private static void AssessAdditionalItems_(List<object> extras, ref List<BurgerComplaint> complaints)
    {
        // add a complaint suitable to the item type
        foreach (var item in extras)
        {
            if (item is BurgerPatty) complaints.Add(new BurgerComplaint(DEDUCTION_EXTRA_PATTY, REASON_EXTRA_PATTY));
            if (item is BurgerVeg) complaints.Add(new BurgerComplaint(DEDUCTION_EXTRA_VEG, REASON_EXTRA_VEG_PREFIX
                 + Enum.GetName(typeof(BurgerVegType), (int)((item as BurgerVeg).GetVegType()))));
            if (item is BurgerSauce) complaints.Add(new BurgerComplaint(DEDUCTION_EXTRA_SAUCE, REASON_EXTRA_SAUCE_PREFIX
                 + Enum.GetName(typeof(SauceType), (int)((item as BurgerSauce).GetSauceType()))));
        }
    }
}
