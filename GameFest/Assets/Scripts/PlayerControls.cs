using Assets.Scripts;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using System;

public class PlayerControls : MonoBehaviour
{
    // components set in UI
    public PlayerInput PlayerInput;
    public Transform PlayerPrefab;

    GenericInputHandler _activeHandler = null;

    // player information
    InputDevice _device;
    Color _colour;
    PlayerState _state = new PlayerState();
    string _playerName = "";

    // the movement script for this player
    PlayerMovement _movement;

    // UI displays
    LobbyDisplayScript _lobbyDisplay;

    // paddles - TODO: move to another script
    Transform[] _paddles;
    bool _paddleState = false;


    /// <summary>
    /// Called when the object is created
    /// </summary>
    private void Start()
    {
        // get he necessary components
        _device = PlayerInput.devices.FirstOrDefault();
        _paddles = GameObject.FindGameObjectsWithTag("Paddle").Select(e => e.transform).ToArray();

        // work out which colour to assign to this player
        GetColour_();

        // display the player on UI
        UpdateMenu();

        // sets the colour on dualshock controls
        SetLightbarColour_();

        // create character
        var tr = Instantiate(PlayerPrefab);
        _movement = tr.GetComponentInChildren<PlayerMovement>();

        _activeHandler = GetComponent<LobbyInputHandler>();
    }

    /// <summary>
    /// Adds a letter to the player name
    /// </summary>
    /// <param name="character">The character to add to the name</param>
    internal void AddToPlayerName()
    {
        _lobbyDisplay.AddToPlayerName();
        _playerName = _lobbyDisplay.GetPlayerName();
    }

    /// <summary>
    /// Adds a letter to the player name
    /// </summary>
    /// <param name="character">The character to add to the name</param>
    internal void BackspacePlayerName()
    {
        _lobbyDisplay.BackspacePlayerName();
        _playerName = _lobbyDisplay.GetPlayerName();
    }

    /// <summary>
    /// Returns the name of the player
    /// </summary>
    /// <returns>The player name</returns>
    public string GetPlayerName()
    {
        return _playerName;
    }

    /// <summary>
    /// Sets the colour of the appropriate lobby panel to match the colour of this player
    /// </summary>
    private void UpdateMenu()
    {
        // find all panels
        var panels = GameObject.FindGameObjectsWithTag("PlayerColourDisplay");
        _lobbyDisplay = panels[PlayerInput.playerIndex].GetComponentInChildren<LobbyDisplayScript>();

        // set the panel that corresponds to this player with the correct colour and device
        _lobbyDisplay.PlayerStarted(_colour, _device, PlayerInput.playerIndex);

        // we will now ask for name
        _state.SetState(PlayerStateEnum.NameEntry);
    }

    /// <summary>
    /// Gets the colour for this player
    /// </summary>
    private void GetColour_()
    {
        float r = 0, g = 0, b = 0;

        // based on the player index, set the colour of the player
        switch (PlayerInput.playerIndex)
        {
            case 0:
                r = .8f;        // red
                break;
            case 1:
                b = .8f;        // blue
                break;
            case 2:
                g = .8f;        // green
                break;
            case 3:
                r = 1;          // yellow
                g = .8f;
                break;
        }

        // set the colour
        _colour = new Color(r, g, b);
    }

    /// <summary>
    /// Displays the currently selected characters
    /// </summary>
    internal void SetLetterDisplay(string current, string left, string right)
    {
        _lobbyDisplay.SetLetterDisplay(current, left, right);
    }

    /// <summary>
    /// Displays the currently selected characters
    /// </summary>
    internal void SetCharacterDisplay(Sprite current, Sprite left, Sprite right)
    {
        _lobbyDisplay.SetCharacterDisplay(current, left, right);
    }

    /// <summary>
    /// Sets the colour of the lightbar on the dualshock
    /// </summary>
    private void SetLightbarColour_()
    {
        var gamepad = _device as DualShockGamepad;

        // if the device is a gamepad,
        if (gamepad != null)
        {
            // set the lightbar colour
            gamepad.SetLightBarColor(_colour);
        }
    }

    /// <summary>
    /// Returns the current state the player is in
    /// </summary>
    /// <returns>The players state - what action they are doing</returns>
    public PlayerStateEnum GetPlayerState()
    {
        return _state.GetState();
    }

    /// <summary>
    /// Sets the current state the player is in
    /// </summary>
    /// <param name="state"></param>
    public void SetPlayerState(PlayerStateEnum state)
    {
        _state.SetState(state);
    }

    /// <summary>
    /// Sets the visibility of the character selection panel
    /// </summary>
    /// <param name="state"></param>
    public void ShowCharacterSelection(bool state)
    {
        _lobbyDisplay.ShowCharacterSelectionPanel(state);
    }

    /// <summary>
    /// When the movement input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (_activeHandler != null)
            _activeHandler.OnMove(ctx);

        //    if (_state.GetState() == PlayerStateEnum.Ready)
        //        // move the player
        //        _movement.Move(ctx.ReadValue<Vector2>());
    }

    /// <summary>
    /// When the Touchpad input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnTouchpad(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        if (_activeHandler != null)
            _activeHandler.OnTouchpad();
    }

    /// <summary>
    /// When the Cross input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnCross(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        if (_activeHandler != null)
            _activeHandler.OnCross();
    }

    /// <summary>
    /// When the Square input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnSquare(InputAction.CallbackContext ctx)
    {
        //if (_state.GetState() == PlayerStateEnum.Playing)
        //    _movement.Jump();
    }

    /// <summary>
    /// When the Triangle input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnTriangle(InputAction.CallbackContext ctx)
    {
        //if (_state.GetState() == PlayerStateEnum.Playing)
        //    _movement.Jump();
    }

    /// <summary>
    /// When the OnCircle input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnCircle(InputAction.CallbackContext ctx)
    {
        //if (_state.GetState() == PlayerStateEnum.Playing)
        //    _movement.Jump();
    }

    /// <summary>
    /// When the right paddle trigger is sent
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void PaddleRight(InputAction.CallbackContext ctx)
    {
        //_paddleState = false;
        //SetPaddles_();
    }

    /// <summary>
    /// When the left paddle trigger is sent
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void PaddleLeft(InputAction.CallbackContext ctx)
    {
        //_paddleState = true;
        //SetPaddles_();
    }

    /// <summary>
    /// When the player is ready
    /// <param name="ready">Whether the player is ready</param>
    /// </summary>
    public void Ready(bool ready)
    {
        _lobbyDisplay.ShowReadyPanel(ready);
    }

    /// <summary>
    /// Updates the angle of the paddles
    /// </summary>
    void SetPaddles_()
    {
        var paddleAngle = _paddleState ? 45 : -45;

        // loop through all the paddles
        foreach (var paddle in _paddles)
        {
            // change the angle
            paddle.eulerAngles = new Vector3(0, 0, paddleAngle);
        }
    }
}
