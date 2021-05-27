using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MarshLandInputDisplay : MonoBehaviour
{
    public Image ColourPanel;
    public Image ActionInput;

    public Sprite[] ControllerImages;
    public Sprite[] KeyboardImages;

    public void SetColour(int playerIndex)
    {
        ColourPanel.color = ColourFetcher.GetColour(playerIndex);
    }

    public void SetAction(MarshLandInputAction action, InputDevice device)
    {
        if ((int)action < ControllerImages.Length)
            ActionInput.sprite = device is Gamepad ? ControllerImages[(int)action] : KeyboardImages[(int)action];
        else
            ActionInput.sprite = null;
    }
}
