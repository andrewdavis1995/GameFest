using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MarshLandInputDisplay : MonoBehaviour
{
    public Image ColourPanel;
    public Image ActionInput;

    public Sprite[] ControllerImages;
    public Sprite[] KeyboardImages;
    public Sprite MashImage;
    public Sprite UnknownIconImage;
    public Text TxtName;

    Color _normalColour;
    bool _inWater;
    Sprite _currentImage;

    /// <summary>
    /// Sets the colour of the display
    /// </summary>
    /// <param name="playerIndex">Index of the player</param>
    /// <param name="plName">Name of the player</param>
    public void SetColour(int playerIndex, string plName)
    {
        _normalColour  = ColourFetcher.GetColour(playerIndex);
        ColourPanel.color = _normalColour;
        TxtName.text = plName;
    }

    /// <summary>
    /// When the player falls into the water
    /// </summary>
    public void FallInWater()
    {
        _inWater = true;
        StartCoroutine(FallInWater_());
    }

    /// <summary>
    /// When the player gets out of the water
    /// </summary>
    public void Recover()
    {
        StopCoroutine(FallInWater_());
        _inWater = false;
        ColourPanel.color = _normalColour;
    }

    /// <summary>
    /// Coroutine for when the player falls into the water
    /// </summary>
    IEnumerator FallInWater_()
    {
        ActionInput.sprite = MashImage;

        while(_inWater)
        {
            ColourPanel.color = _normalColour;
            yield return new WaitForSeconds(0.1f);
            ColourPanel.color = new Color(_normalColour.r*1.2f, _normalColour.g * 1.2f, _normalColour.b * 1.2f);
            yield return new WaitForSeconds(0.1f);
        }
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
            ActionInput.sprite = UnknownIconImage;

        _currentImage = ActionInput.sprite;
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
