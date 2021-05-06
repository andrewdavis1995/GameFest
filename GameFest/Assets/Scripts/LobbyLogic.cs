using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts;

public class LobbyLogic : MonoBehaviour
{
    // link to the player input
    public PlayerControls PlayerController;

    // alphabet logic
    private string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private int _currentIndex = 0;

    public Sprite[] CharacterSprites;
    private int _characterIndex = 0;

    /// <summary>
    /// Gets the index of the letter to show on the right
    /// </summary>
    public int RightLetterIndex_()
    {
        if (_currentIndex == _alphabet.Length - 1) return 0;
        else return _currentIndex + 1;
    }

    /// <summary>
    /// Gets the index of the letter to show on the left
    /// </summary>
    public int LeftLetterIndex_()
    {
        if (_currentIndex == 0) return _alphabet.Length - 1;
        else return _currentIndex - 1;
    }

    /// <summary>
    /// Gets the index of the character to show on the right
    /// </summary>
    public int RightLetterIndexCharacter_()
    {
        if (_characterIndex == CharacterSprites.Length - 1) return 0;
        else return _characterIndex + 1;
    }

    /// <summary>
    /// Gets the index of the character to show on the left
    /// </summary>
    public int LeftLetterIndexCharacter_()
    {
        if (_characterIndex == 0) return CharacterSprites.Length - 1;
        else return _characterIndex - 1;
    }

    /// <summary>
    /// When the Left input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnLeft(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        switch (PlayerController.GetPlayerState())
        {
            // name entry, move the letter to the left
            case PlayerStateEnum.NameEntry:
                UpdateIndex_(-1);
                break;
            // name entry, move the character to the left
            case PlayerStateEnum.CharacterSelection:
                UpdateCharacters_(-1);
                break;
        }
    }

    /// <summary>
    /// When the Enter input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnEnter(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        switch (PlayerController.GetPlayerState())
        {
            // name entry, move the letter to the left
            case PlayerStateEnum.NameEntry:
                PlayerController.AddToPlayerName();
                break;
        }
    }

    /// <summary>
    /// When the Backspace input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnBackspace(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        switch (PlayerController.GetPlayerState())
        {
            // name entry, move the letter to the left
            case PlayerStateEnum.NameEntry:
                PlayerController.BackspacePlayerName();
                break;
        }
    }

    /// <summary>
    /// When the name entry is complete
    /// </summary>
    private void NameComplete_()
    {
        // check the name is valid
        if (PlayerController.GetPlayerName().Length >= 3)
        {
            PlayerController.SetPlayerState(PlayerStateEnum.CharacterSelection);
            PlayerController.ShowCharacterSelection(true);
        }
        else
        {
            // TODO: Display on screen
            Debug.Log("Name is not long enough");
        }
    }

    /// <summary>
    /// When character has been selected
    /// </summary>
    private void CharacterSelected_()
    {
        PlayerController.SetPlayerState(PlayerStateEnum.Ready);
        PlayerController.Ready(true);
    }

    /// <summary>
    /// When the Right input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnRight(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        switch (PlayerController.GetPlayerState())
        {
            // name entry, move the letter to the right
            case PlayerStateEnum.NameEntry:
                UpdateIndex_(1);
                break;
            // name entry, move the character to the left
            case PlayerStateEnum.CharacterSelection:
                UpdateCharacters_(1);
                break;
        }
    }

    /// <summary>
    /// When the Touchpad input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnTouchpadPress(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        switch (PlayerController.GetPlayerState())
        {
            // name entry, move the letter to the right
            case PlayerStateEnum.NameEntry:
                NameComplete_();
                break;
            // name entry, move the letter to the right
            case PlayerStateEnum.CharacterSelection:
                CharacterSelected_();
                break;
        }
    }

    /// <summary>
    /// When the cancel input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnCancelPress(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        switch (PlayerController.GetPlayerState())
        {
            // name entry, move the letter to the right
            case PlayerStateEnum.CharacterSelection:
                PlayerController.SetPlayerState(PlayerStateEnum.NameEntry);
                PlayerController.ShowCharacterSelection(false);
                break;
            case PlayerStateEnum.Ready:
                PlayerController.SetPlayerState(PlayerStateEnum.CharacterSelection);
                PlayerController.Ready(false);
                break;
        }
    }

    /// <summary>
    /// Moves the letters to the next/previous letter
    /// </summary>
    /// <param name="direction">How much to move by (positive or negative 1)</param>
    void UpdateIndex_(int direction)
    {
        // move to the next/previous letter
        _currentIndex += direction;

        // loop around
        if (_currentIndex < 0) _currentIndex = _alphabet.Length - 1;
        else if (_currentIndex > _alphabet.Length - 1) _currentIndex = 0;

        // get the values
        var left = _alphabet[LeftLetterIndex_()].ToString();
        var centre = _alphabet[_currentIndex].ToString();
        var right = _alphabet[RightLetterIndex_()].ToString();

        // display strings
        PlayerController.SetLetterDisplay(centre, left, right);
    }

    /// <summary>
    /// Moves the character to the next/previous character
    /// </summary>
    /// <param name="direction">How much to move by (positive or negative 1)</param>
    void UpdateCharacters_(int direction)
    {
        // move to the next/previous letter
        _characterIndex += direction;

        // loop around
        if (_characterIndex < 0) _characterIndex = CharacterSprites.Length - 1;
        else if (_characterIndex > CharacterSprites.Length - 1) _characterIndex = 0;

        // get the values
        var left = CharacterSprites[LeftLetterIndexCharacter_()];
        var centre = CharacterSprites[_characterIndex];
        var right = CharacterSprites[RightLetterIndexCharacter_()];

        // display strings
        PlayerController.SetCharacterDisplay(centre, left, right);
    }
}
