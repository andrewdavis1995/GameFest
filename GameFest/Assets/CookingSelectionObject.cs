using UnityEngine;

public enum SelectionType { BreadBin, BriocheBun, BriocheBunTop, SesameBun, SesameBunTop, Toast, ToastTop, Sauce, Lettuce, Tomato, GrillZone, None }

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
