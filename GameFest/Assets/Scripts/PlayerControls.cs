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

    // the handler which will deal with inputs
    GenericInputHandler _activeHandler = null;

    // player information
    InputDevice _device;
    Color _colour;
    string _playerName = "";

    /// <summary>
    /// Called when the object is created
    /// </summary>
    private void Start()
    {
        // get the necessary components
        _device = PlayerInput.devices.FirstOrDefault();

        _activeHandler = GetComponent<LobbyInputHandler>();

        // work out which colour to assign to this player
        GetColour_();

        // display the player on UI
        UpdateMenu();

        // sets the colour on dualshock controls
        SetLightbarColour_();
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
        var lobbyDisplay = panels[PlayerInput.playerIndex].GetComponentInChildren<LobbyDisplayScript>();

        // set the panel that corresponds to this player with the correct colour and device
        lobbyDisplay.PlayerStarted(_colour, _device, PlayerInput.playerIndex);

        (_activeHandler as LobbyInputHandler).SetDisplay(lobbyDisplay, (x) => _playerName = x);
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
    /// Sets the colour of the lightbar on the dualshock
    /// </summary>
    private void SetLightbarColour_()
    {
        var gamepad = _device as DualShockGamepad;

        // if the device is a gamepad,
        if (gamepad != null)
        {
            // set the lightbar colour on dualshock controllers
            gamepad.SetLightBarColor(_colour);
        }
    }

    /// <summary>
    /// When the movement input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (_activeHandler != null)
            _activeHandler.OnMove(ctx);
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

    public void OnL1(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        if (_activeHandler != null)
            _activeHandler.OnL1();
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
    }

    /// <summary>
    /// When the Triangle input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnTriangle(InputAction.CallbackContext ctx)
    {
    }

    /// <summary>
    /// When the OnCircle input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnCircle(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        if (_activeHandler != null)
            _activeHandler.OnCircle();
    }

    /// <summary>
    /// When the right paddle trigger is sent
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void PaddleRight(InputAction.CallbackContext ctx)
    {
    }

    /// <summary>
    /// When the left paddle trigger is sent
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void PaddleLeft(InputAction.CallbackContext ctx)
    {
    }
}
