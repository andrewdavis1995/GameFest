using Assets.Scripts;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyInputHandler : GenericInputHandler
{
    // alphabet logic
    private string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private int _currentIndex = 0;

    public Sprite[] CharacterSprites;
    private int _characterIndex = 0;

    LobbyState _state = new LobbyState();

    LobbyDisplayScript _display = null;
    Action<string> _setNameCallback;

    public override void OnMove(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        var movement = ctx.ReadValue<Vector2>();
        if (movement.x > 0.95f)
            MoveRight_();

        if (movement.x < 0.95f)
            MoveLeft_();
    }

    public void SetDisplay(LobbyDisplayScript display, Action<string> nameCallback)
    {
        _display = display;
        _setNameCallback = nameCallback;
    }

    private void MoveLeft_()
    {
        switch (_state.GetState())
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

    private void MoveRight_()
    {
        switch (_state.GetState())
        {
            // name entry, move the letter to the right
            case PlayerStateEnum.NameEntry:
                UpdateIndex_(1);
                break;
            // name entry, move the character to the right
            case PlayerStateEnum.CharacterSelection:
                UpdateCharacters_(1);
                break;
        }
    }

    public override void OnCross()
    {
        switch (_state.GetState())
        {
            // name entry, move the letter to the left
            case PlayerStateEnum.NameEntry:
                _display.AddToPlayerName();
                break;
        }
    }

    public override void OnCircle()
    {
        switch (_state.GetState())
        {
            // name entry, move the letter to the right
            case PlayerStateEnum.CharacterSelection:
                _state.SetState(PlayerStateEnum.NameEntry);
                _display.ShowCharacterSelectionPanel(false);
                break;
            case PlayerStateEnum.Ready:
                _state.SetState(PlayerStateEnum.CharacterSelection);
                _display.ShowReadyPanel(false);
                break;
        }
    }

    public override void OnTouchpad()
    {
        switch (_state.GetState())
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
        _display.SetLetterDisplay(centre, left, right);
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
        _display.SetCharacterDisplay(centre, left, right);
    }

    /// <summary>
    /// Backspace
    /// </summary>
    public override void OnL1()
    {
        switch (_state.GetState())
        {
            // name entry, move the letter to the left
            case PlayerStateEnum.NameEntry:
                _display.BackspacePlayerName();
                break;
        }
    }

    /// <summary>
    /// When the name entry is complete
    /// </summary>
    private void NameComplete_()
    {
        // check the name is valid
        if (_display.GetPlayerName().Length >= 3)
        {
            _setNameCallback(_display.GetPlayerName());
            _state.SetState(PlayerStateEnum.CharacterSelection);
            _display.ShowCharacterSelectionPanel(true);
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
        _state.SetState(PlayerStateEnum.Ready);
        _display.ShowReadyPanel(true);
    }

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
}
