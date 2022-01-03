using UnityEngine;

/// <summary>
/// Controller for the LicenseToGrill game
/// </summary>
public class LicenseToGrillController : GenericController
{
    // static instance
    public static LicenseToGrillController Instance;

    // components
    public ChefScript[] Chefs;
    public Transform FoodPlateItemPrefab;

    // sprites
    public Sprite[] BreadBottoms;
    public Sprite[] BreadTop;
    public Sprite[] Burgers;
    public Sprite LettuceSlice;
    public Sprite TomatoSlices;
    public Sprite PickleSlices;

    /// <summary>
    /// Called once on startup
    /// </summary>
    private void Start()
    {
        Instance = this;
    }
}
