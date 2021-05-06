using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;

/// <summary>
/// Controls the display of the lobby UI
/// </summary>
public class LobbyDisplayScript : MonoBehaviour
{
    // controls
    public Transform NoPlayerPanel;
    public Transform PlayerStartedPanel;
    public Transform PnlCharacter;
    public Transform PnlReady;
    public Transform[] InstructionsController;
    public Transform[] InstructionsKeyboard;
    public Image ImgController;

    // Texts to display the letters
    public Text LetterTextLeft;
    public Text LetterTextMiddle;
    public Text LetterTextRight;
    public Text NameDisplay;

    // Images to display the character
    public Image ImageCharacterLeft;
    public Image ImageCharacterMiddle;
    public Image ImageCharacterRight;

    // images to use
    public Sprite[] ControllerImages;

    // private variables
    InputDevice _device;
    int _playerIndex;

    /// <summary>
    /// Called when the player joins the game - updates the lobby display
    /// </summary>
    /// <param name="colour">The colour assigned to the player</param>
    /// <param name="device">The input device the player is using</param>
    /// <param name="playerIndex">The index of the player</param>
    public void PlayerStarted(Color colour, InputDevice device, int index)
    {
        _device = device;
        _playerIndex = index;

        // change the backgroud colour
        GetComponent<Image>().color = colour;
        // set the image to show which device the player is using
        SetControllerIcon_();

        // show the input selections
        NoPlayerPanel.gameObject.SetActive(false);
        PlayerStartedPanel.gameObject.SetActive(true);

        UpdateInstructions_(0, false);
    }

    /// <summary>
    /// Displays the currently selected characters
    /// </summary>
    internal void SetLetterDisplay(string current, string left, string right)
    {
        LetterTextLeft.text = left;
        LetterTextMiddle.text = current;
        LetterTextRight.text = right;
    }

    /// <summary>
    /// Displays the currently selected characters
    /// </summary>
    internal void SetCharacterDisplay(Sprite current, Sprite left, Sprite right)
    {
        ImageCharacterLeft.sprite = left;
        ImageCharacterMiddle.sprite = current;
        ImageCharacterRight.sprite = right;
    }

    /// <summary>
    /// Adds a letter to the player name
    /// </summary>
    internal void AddToPlayerName()
    {
        if (NameDisplay.text.Length < 10)
            NameDisplay.text += LetterTextMiddle.text;
    }

    /// <summary>
    /// Adds a letter to the player name
    /// </summary>
    internal void BackspacePlayerName()
    {
        if (NameDisplay.text.Length > 0)
            NameDisplay.text = NameDisplay.text.Substring(0, NameDisplay.text.Length - 1);
    }

    /// <summary>
    /// Returns the player name
    /// </summary>
    /// <returns>The player name</returns>
    internal string GetPlayerName()
    {
        return NameDisplay.text;
    }

    /// <summary>
    /// Shows the character selection controls
    /// </summary>
    internal void ShowCharacterSelectionPanel(bool state)
    {
        PnlCharacter.gameObject.SetActive(state);
        UpdateInstructions_(1, false);
    }

    /// <summary>
    /// Shows the appropriate instructions
    /// </summary>
    /// <param name="index">The instruction index to display</param>
    /// <param name="hostOnly">Whether these instructions are only valid for the host</param>
    private void UpdateInstructions_(int index, bool hostOnly)
    {
        if (_device is Gamepad)
        {
            for (int i = 0; i < InstructionsController.Length; i++)
            {
                var state = (i == index) && (!hostOnly || _playerIndex == 0);
                InstructionsController[i].gameObject.SetActive(state);
            }
        }
        else if (_device is Keyboard)
        {
            for (int i = 0; i < InstructionsKeyboard.Length; i++)
            {
                var state = (i == index) && (!hostOnly || _playerIndex == 0);
                InstructionsKeyboard[i].gameObject.SetActive(state);
            }
        }
    }

    /// <summary>
    /// Shows the "READY" message
    /// </summary>
    internal void ShowReadyPanel(bool state)
    {
        UpdateInstructions_(state ? 2 : 1, state == true);  // only player 1 only if being set to true
        PnlReady.gameObject.SetActive(state);
    }

    /// <summary>
    /// Sets the image to show which device the player is using
    /// </summary>
    private void SetControllerIcon_()
    {
        int index = -1;

        // get the image index, based on the controller type
        if (_device is DualShockGamepad)
            index = 0;
        else if (_device is Keyboard)
            index = 1;

        // if there is a suitable image, display it
        if (index >= 0)
        {
            ImgController.sprite = ControllerImages[index];
        }
        else
        {
            // should never get here
            ImgController.gameObject.SetActive(false);
        }
    }
}
