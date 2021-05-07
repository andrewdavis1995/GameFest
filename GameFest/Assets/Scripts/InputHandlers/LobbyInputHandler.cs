using Assets.Scripts;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyInputHandler : GenericInputHandler
{
    // alphabet logic
    private string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private int _currentIndex = 0;

    // character selection logic
    public Sprite[] CharacterSprites;
    private int _characterIndex = 0;

    // what step is the player at
    LobbyState _state = new LobbyState();

    // link to display and PlayerInput
    LobbyDisplayScript _display = null;
    Action<string, int> _detailsCompleteCallback;

    // is this player the host (player 1)
    bool _isHost = false;

    #region Override functions
    /// <summary>
    /// When the movement event is triggered - change letter/character
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public override void OnMove(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        // only use buttons - joystick moves too much
        if (ctx.control.layout.ToLower() == "stick") return;

        var movement = ctx.ReadValue<Vector2>();

        // if right, move right
        if (movement.x > 0.99f)
            MoveRight_();

        // if left, move left
        if (movement.x < -0.99f)
            MoveLeft_();
    }

    /// <summary>
    /// When the cross is triggered - select letter
    /// </summary>
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

    /// <summary>
    /// When the circle is triggered - back
    /// </summary>
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

    /// <summary>
    /// When the touchpad event is triggered - continue
    /// </summary>
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
            case PlayerStateEnum.Ready:
                StartGame_();
                break;
        }
    }

    /// <summary>
    /// When the L1 event is triggered - backspace
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

    #endregion

    /// <summary>
    /// Assigns a UI display to the handler
    /// </summary>
    /// <param name="display">The UI element to update</param>
    /// <param name="nameCallback">The callback function to call when the name is updated</param>
    /// <param name="isHost">Is this player the host (player 1)</param>
    public void SetDisplay(LobbyDisplayScript display, Action<string, int> detailsCallback, bool isHost)
    {
        _display = display;
        _detailsCompleteCallback = detailsCallback;
        _isHost = isHost;
    }

    /// <summary>
    /// Move to the next letter/character to the left
    /// </summary>
    void MoveLeft_()
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

    /// <summary>
    /// Move to the next letter/character to the right
    /// </summary>
    void MoveRight_()
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

    /// <summary>
    /// Starts the game (if all players are ready)
    /// </summary>
    void StartGame_()
    {
        // non-hosts cannot start the game
        if (!_isHost) return;

        // find all players
        var allPlayers = GameObject.FindObjectsOfType<LobbyInputHandler>();

        // check if all players are ready
        var allReady = allPlayers.All(p => p.Ready());

        // if all ready...
        if(allReady)
        {
            // ...store the player list in the manager
            PlayerManagerScript.Instance.SetPlayers(allPlayers.Select(p => p.GetComponent<PlayerControls>()).ToList());
            // move to the game central scene
            PlayerManagerScript.Instance.NextScene(1);
        }
        else
        {
            // ...otherwise, stop
            // TODO: display message in UI
            Debug.Log("Not all players are ready");
        }
    }

    /// <summary>
    /// Is the player ready to player
    /// </summary>
    /// <returns>Whether the player is ready</returns>
    public bool Ready()
    {
        return _state.GetState() == PlayerStateEnum.Ready;
    }

    /// <summary>
    /// Moves the letter to the next/previous letter
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
    /// When the name entry is complete
    /// </summary>
    void NameComplete_()
    {
        // check the name is valid
        if (_display.GetPlayerName().Length >= 3)
        {
            // move to the next stage
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
    void CharacterSelected_()
    {
        // tell the player object what the name is
        _detailsCompleteCallback(_display.GetPlayerName(), _characterIndex);

        _state.SetState(PlayerStateEnum.Ready);
        _display.ShowReadyPanel(true);
    }

    #region Fetch index of the left/right elements to display
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
    #endregion
}
