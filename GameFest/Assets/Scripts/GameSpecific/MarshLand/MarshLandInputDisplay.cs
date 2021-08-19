using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MarshLandInputDisplay : MonoBehaviour
{
    public Image ColourPanel;
    public Image ActionInput;

    public Sprite[] ControllerImages;
    public Sprite[] KeyboardImages;

    /// <summary>
    /// Sets the colour of the display
    /// </summary>
    /// <param name="playerIndex">Index of the player</param>
    public void SetColour(int playerIndex)
    {
        ColourPanel.color = ColourFetcher.GetColour(playerIndex);
    }

    /// <summary>
    /// Displays the next action
    /// </summary>
    /// <param name="action">The action to display</param>
    /// <param name="device">The device the player is using</param>
    public void SetAction(MarshLandInputAction action, InputDevice device)
    {
        ActionInput.color = new Color(1, 1, 1, 1);

        // display the relevant image
        if ((int)action < ControllerImages.Length)
            ActionInput.sprite = device is Gamepad ? ControllerImages[(int)action] : KeyboardImages[(int)action];
        else
            // null if not enough images
            ActionInput.sprite = null;
    }

    /// <summary>
    /// Sets the display icon to null
    /// </summary>
    public void ClearAction()
    {
        // null if not enough images
        ActionInput.color = new Color(0,0,0,0);
    }
}
