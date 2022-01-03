using UnityEngine;

public enum SelectionType { BreadBin, Sauce, Lettuce, Tomato }

/// <summary>
/// Object that can be selected by a chef
/// </summary>
public class CookingSelectionObject : MonoBehaviour
{
    // sprites
    public Sprite SelectedImage;
    public Sprite NotSelectedImage;

    // components
    public SpriteRenderer Renderer;

    // configuration
    public SelectionType ObjectType;

    /// <summary>
    /// When the item is selected
    /// </summary>
    public void Selected()
    {
        Renderer.sprite = SelectedImage;
    }

    /// <summary>
    /// When the item is no longer selected
    /// </summary>
    public void Unselected()
    {
        Renderer.sprite = NotSelectedImage;
    }
}
