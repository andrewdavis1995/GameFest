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

    // what step is the player at
    LobbyState _state = new LobbyState();

    // link to display and PlayerInput
    LobbyDisplayScript _display = null;
    Action<string, int, Guid> _detailsCompleteCallback;

    // profile selection
    int _selectedProfileIndex = 0;
    int _selectedProfileTop = 0;
    int _selectedProfileBottom = 3;

    bool _done = false;
    bool _newProfile = false;
    Guid _newProfileGuid;

    const int GAMES_PER_ROW = 3;

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

        // if up, move up
        if (movement.y < -0.99f)
            MoveDown_();

        // if up, move down
        if (movement.y > 0.99f)
            MoveUp_();
    }

    /// <summary>
    /// When the cross is triggered - select letter
    /// </summary>
    public override void OnCross()
    {
        bool statusPanel = true;

        switch (_state.GetState())
        {
            // profile select, load profile
            case PlayerStateEnum.ProfileSelection:
                var profile = PlayerManagerScript.Instance.GetProfileList()[_selectedProfileIndex];
                if (profile == null)
                {
                    _state.SetState(PlayerStateEnum.NameEntry);
                    _display.PlayerStartedPanel.gameObject.SetActive(true);
                    _display.SelectingProfilePanel.gameObject.SetActive(false);
                    _newProfileGuid = new Guid();
                    _newProfile = true;
                }
                else
                {
                    _newProfileGuid = profile.GetGuid();

                    // check for other players using this profile
                    if (!ProfileInUse())
                    {
                        _newProfile = false;

                        // update details
                        _display.ShowCharacterSelectionPanel(true);

                        _display.PlayerStartedPanel.gameObject.SetActive(true);
                        _display.SelectingProfilePanel.gameObject.SetActive(false);

                        CharacterSelected_();
                    }
                    else
                    {
                        // show message
                        StartCoroutine(_display.ShowError("Profile is already in use"));
                        statusPanel = false;
                    }
                }
                break;
            // name entry, move the letter to the left
            case PlayerStateEnum.NameEntry:
                _display.AddToPlayerName();
                break;
        }
        _display.UpdateState(_state.GetState(), statusPanel);
    }

    /// <summary>
    /// Get the Guid of the selected profile
    /// </summary>
    /// <returns></returns>
    public Guid GetGuid()
    {
        return _newProfileGuid;
    }

    /// <summary>
    /// Check if the profile is already used elsewhere
    /// </summary>
    /// <returns>If the profile is in use</returns>
    public bool ProfileInUse()
    {
        var others = FindObjectsOfType<LobbyInputHandler>();
        var matched = others.Where(p => (p.Ready()) && (p.GetGuid() == _newProfileGuid) && p.GetPlayerIndex() != GetPlayerIndex());
        return matched.Count() > 0;
    }

    /// <summary>
    /// When the circle is triggered - back
    /// </summary>
    public override void OnCircle()
    {
        bool statusPanel = true;
        switch (_state.GetState())
        {
            // name entry, move the letter to the right
            case PlayerStateEnum.NameEntry:
                _state.SetState(PlayerStateEnum.ProfileSelection);
                _display.ShowCharacterSelectionPanel(false);
                _display.PlayerStartedPanel.gameObject.SetActive(false);
                _display.SelectingProfilePanel.gameObject.SetActive(true);
                statusPanel = false;
                break;
            // name entry, move the letter to the right
            case PlayerStateEnum.CharacterSelection:
                _state.SetState(PlayerStateEnum.NameEntry);
                _display.ShowCharacterSelectionPanel(false);
                break;
            case PlayerStateEnum.Ready:
                if (!PlayerManagerScript.Instance.LobbyComplete)
                {
                    var pl = PlayerManagerScript.Instance.GetPlayers().Where(p => p.GetGuid() == _newProfileGuid).FirstOrDefault();

                    if (pl != null) pl.NoLongerReady();

                    // if mode select, only host can go back
                    if (!PlayerManagerScript.Instance.ModeSelection.activeInHierarchy)
                    {
                        if (_newProfile)
                        {
                            _state.SetState(PlayerStateEnum.CharacterSelection);
                            _display.ShowReadyPanel(false);

                            // TODO: Delete saved profile
                            PlayerManagerScript.Instance.RemoveProfile(_newProfileGuid);
                        }
                        else
                        {
                            _state.SetState(PlayerStateEnum.ProfileSelection);
                            _display.ShowReadyPanel(false);
                            statusPanel = false;
                        }
                    }
                    else
                    {
                        // host can go back
                        if (IsHost())
                        {
                            PlayerManagerScript.Instance.NotComplete();
                            _done = false;
                        }
                    }
                }
                break;
        }

        _display.UpdateState(_state.GetState(), statusPanel);
    }

    /// <summary>
    /// When the touchpad event is triggered - continue
    /// </summary>
    public override void OnOptions()
    {
        bool statusPanel = true;
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
            {
                var allPlayers = FindObjectsOfType<LobbyInputHandler>();

                // show next page
                if (PlayerManagerScript.Instance.ModeSelection.activeInHierarchy)
                    PlayerManagerScript.Instance.Complete(allPlayers);
                else
                    StartGame_();
            }
            break;
            case PlayerStateEnum.ProfileSelection:
                statusPanel = false;
                break;
        }

        // update UI
        _display.UpdateState(_state.GetState(), statusPanel);
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
    /// <param name="detailsCallback">The callback function to call when the details are confirmed</param>
    /// <param name="playerIndex">The index of the player</param>
    public void SetDisplay(LobbyDisplayScript display, Action<string, int, Guid> detailsCallback, int playerIndex)
    {
        _display = display;
        _detailsCompleteCallback = detailsCallback;
        SetPlayerIndex(playerIndex);
    }

    /// <summary>
    /// Back to the start state
    /// </summary>
    public void ResetDisplay()
    {
        _display.ResetDisplay();
        SetCharacterIndex(0);
        UpdateCharacters_(0);
        _state.SetState(PlayerStateEnum.NameEntry);
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
            case PlayerStateEnum.Ready:
            {
                // update mode
                if (PlayerManagerScript.Instance.ModeSelection.activeInHierarchy && IsHost())
                {
                    PlayerManagerScript.Instance.UpdateMode(GameMode.QuickPlayMode);
                }
                break;
            }
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
            case PlayerStateEnum.Ready:
            {
                // update mode
                if (PlayerManagerScript.Instance.ModeSelection.activeInHierarchy && IsHost())
                {
                    PlayerManagerScript.Instance.UpdateMode(GameMode.HeroMode);
                }
                break;
            }
        }
    }

    /// <summary>
    /// Move to the next letter/character to the up
    /// </summary>
    void MoveUp_()
    {
        switch (_state.GetState())
        {
            case PlayerStateEnum.ProfileSelection:
            {
                // deselect item
                _display.ProfileSelectionControls[_selectedProfileIndex - _selectedProfileTop].Deselected();

                // move up until reached the top
                if (_selectedProfileIndex > 0)
                {
                    _selectedProfileIndex--;

                    // scroll up if we've reached the top
                    if (_selectedProfileIndex < _selectedProfileTop)
                    {
                        _selectedProfileBottom--;
                        _selectedProfileTop--;
                        _display.UpdateProfiles(_selectedProfileTop);
                    }
                }

                DisplayCurrentProfile_();
                break;
            }
        }
    }

    /// <summary>
    /// Displays the details of the currently selected profile
    /// </summary>
    private void DisplayCurrentProfile_()
    {
        // highlight the profile
        _display.ProfileSelectionControls[_selectedProfileIndex - _selectedProfileTop].Selected();

        var profile = PlayerManagerScript.Instance.GetProfileList()[_selectedProfileIndex];

        // if it is a profile
        if (profile != null)
        {
            // show details
            _display.NameDisplay.text = profile.GetProfileName();
            SetCharacterIndex(profile.GetCharacterIndex());
            UpdateCharacters_(0);

            // details to show
            _display.ShowCharacterSelectionPanel(true);
            _display.PlayerStartedPanel.gameObject.SetActive(true);
            _display.SelectingProfilePanel.gameObject.SetActive(false);
        }
        else
        {
            // show details
            _display.NameDisplay.text = "";
            SetCharacterIndex(0);
            UpdateCharacters_(0);

            // no details to show
            _display.ShowCharacterSelectionPanel(false);
            _display.PlayerStartedPanel.gameObject.SetActive(false);
            _display.SelectingProfilePanel.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Move to the next letter/character to the down
    /// </summary>
    void MoveDown_()
    {
        switch (_state.GetState())
        {
            case PlayerStateEnum.ProfileSelection:
            {
                _display.ProfileSelectionControls[_selectedProfileIndex - _selectedProfileTop].Deselected();

                if (_selectedProfileIndex < PlayerManagerScript.Instance.GetProfileList().Count() - 1)
                {
                    _selectedProfileIndex++;

                    // scroll down if we've reached the bottom
                    if (_selectedProfileIndex > _selectedProfileBottom)
                    {
                        _selectedProfileBottom++;
                        _selectedProfileTop++;
                        _display.UpdateProfiles(_selectedProfileTop);
                    }
                }

                DisplayCurrentProfile_();
                break;
            }
        }
    }

    /// <summary>
    /// Starts the game (if all players are ready)
    /// </summary>
    void StartGame_()
    {
        // non-hosts cannot start the game
        if (!IsHost()) return;

        // don't continue if already started
        if (PlayerManagerScript.Instance.LobbyComplete) return;

        // find all players
        var allPlayers = FindObjectsOfType<LobbyInputHandler>();

        // check if all players are ready
        var allReady = allPlayers.All(p => p.Ready());

        // if all ready...
        if (allReady && !_done)
        {
            PlayerManagerScript.Instance.ShowModeSelection();
        }
        else
        {
            // ...otherwise, stop
            StartCoroutine(_display.ShowError("Not all players are ready"));
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
        SetCharacterIndex(GetCharacterIndex() + direction);

        // loop around
        if (GetCharacterIndex() < 0) SetCharacterIndex(CharacterSprites.Length - 1);
        else if (GetCharacterIndex() > CharacterSprites.Length - 1) SetCharacterIndex(0);

        // get the values
        var left = CharacterSprites[LeftLetterIndexCharacter_()];
        var centre = CharacterSprites[GetCharacterIndex()];
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
            StartCoroutine(_display.ShowError("Name is not long enough"));
        }
    }

    /// <summary>
    /// When character has been selected
    /// </summary>
    void CharacterSelected_()
    {
        // tell the player object what the name is
        _detailsCompleteCallback(_display.GetPlayerName(), GetCharacterIndex(), _newProfileGuid);

        _state.SetState(PlayerStateEnum.Ready);
        _display.ShowReadyPanel(true);

        // save new profile
        if (_newProfile)
        {
            var profile = new PlayerProfile();
            _newProfileGuid = profile.GetGuid();
            profile.UpdateDetails(_display.NameDisplay.text, GetCharacterIndex());
            PlayerManagerScript.Instance.AddProfile(profile);
        }
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
        if (GetCharacterIndex() == CharacterSprites.Length - 1) return 0;
        else return GetCharacterIndex() + 1;
    }

    /// <summary>
    /// Gets the index of the character to show on the left
    /// </summary>
    public int LeftLetterIndexCharacter_()
    {
        if (GetCharacterIndex() == 0) return CharacterSprites.Length - 1;
        else return GetCharacterIndex() - 1;
    }
    #endregion
}
