using UnityEngine;

public enum SelectionType { BreadBin, BriocheBun, BriocheBunTop, SesameBun, SesameBunTop, BrownBun, BrownBunTop, Sauce, Lettuce, Tomato, GrillZone, Plate, Pickle, None }

/// <summary>
/// Object that can be selected by a chef
/// </summary>
public class CookingSelectionObject : MonoBehaviour
{
    // components
    public SpriteRenderer Renderer;
    public SpriteRenderer RendererGlow;

    // configuration
    public int Index;

    // configuration
    public SelectionType ObjectType;

    /// <summary>
    /// When the item is selected
    /// </summary>
    public void Selected()
    {
        RendererGlow.gameObject.SetActive(true);
    }

    /// <summary>
    /// When the item is no longer selected
    /// </summary>
    public void Unselected()
    {
        RendererGlow.gameObject.SetActive(false);
    }
}
