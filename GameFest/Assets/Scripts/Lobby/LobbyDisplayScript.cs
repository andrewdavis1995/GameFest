using System;
using System.Collections;
using Assets.Scripts;
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
    public Transform PnlLetters;
    public Transform PnlReady;
    public Transform PnlShadow;
    public Text TxtCurrentAction;
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
    bool _errorMessageShowing = false;

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
        PnlReady.GetComponent<Image>().color = colour;
        // set the image to show which device the player is using
        SetControllerIcon_();

        // show the input selections
        NoPlayerPanel.gameObject.SetActive(false);
        PlayerStartedPanel.gameObject.SetActive(true);
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
    /// Sets the message for what state the player is in
    /// </summary>
    /// <param name="playerStateEnum">The state the player is in</param>
    internal void UpdateState(PlayerStateEnum playerStateEnum)
    {
        var msg = "";
        switch(playerStateEnum)
        {
            case PlayerStateEnum.CharacterSelection: msg = "Selecting character"; break;
            case PlayerStateEnum.ChoosingGames: msg = "Choosing Games"; break;
            case PlayerStateEnum.NameEntry: msg = "Entering name"; break;
            case PlayerStateEnum.Ready: msg = "Ready"; break;
        }

        TxtCurrentAction.text = msg;
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
        PnlLetters.gameObject.SetActive(!state);
        PnlCharacter.gameObject.SetActive(state);
    }

    /// <summary>
    /// Shows the "READY" message
    /// </summary>
    internal void ShowReadyPanel(bool state)
    {
        PnlReady.gameObject.SetActive(state);
        PnlShadow.gameObject.SetActive(state);
        PlayerManagerScript.Instance.SetGameSelectionState(state);
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

    /// <summary>
    /// Displays the pause request message for this player
    /// </summary>
    public IEnumerator ShowError(string msg)
    {
        var txt = PlayerManagerScript.Instance.PausePopups[_playerIndex].GetComponentInChildren<Text>();

        // can display if the message is not showing, or a different message is to be shown
        if (!_errorMessageShowing || msg != txt.text)
        {
            _errorMessageShowing = true;
            txt.text = msg;
            PlayerManagerScript.Instance.PausePopups[_playerIndex].SetActive(true);
            yield return new WaitForSeconds(4);
            PlayerManagerScript.Instance.PausePopups[_playerIndex].SetActive(false);
            _errorMessageShowing = false;
        }
    }

    /// <summary>
    /// Back to the start state
    /// </summary>
    public void ResetDisplay()
    {
        NoPlayerPanel.gameObject.SetActive(true);
        PlayerStartedPanel.gameObject.SetActive(true);

        _playerIndex = 0;
        NameDisplay.text = "";
    }
}
