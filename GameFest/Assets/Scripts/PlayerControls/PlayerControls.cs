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
    int _characterIndex = 0;
    private int _points = 0;

    /// <summary>
    /// Called when the object is created
    /// </summary>
    void Start()
    {
        DontDestroyOnLoad(this);

        // get the necessary components
        _device = PlayerInput.devices.FirstOrDefault();

        _activeHandler = GetComponent<LobbyInputHandler>();

        // work out which colour to assign to this player
        _colour = ColourFetcher.GetColour(PlayerInput.playerIndex);

        // display the player on UI
        UpdateMenu();

        // sets the colour on dualshock controls
        SetLightbarColour_();
    }

    /// <summary>
    /// Returns the device used by the player
    /// </summary>
    /// <returns>The type of device in use</returns>
    public InputDevice GetDevice()
    {
        return _device;
    }

    /// <summary>
    /// Gets the total points the player has earned
    /// </summary>
    /// <returns>Points earned by the player</returns>
    internal int GetPoints()
    {
        return _points;
    }

    /// <summary>
    /// Removes the current input handler and adds the specified type
    /// </summary>
    /// <param name="type">The type of handler to add</param>
    internal void SetActiveScript(Type type)
    {
        Destroy(gameObject.GetComponent<GenericInputHandler>());
        _activeHandler = (GenericInputHandler)gameObject.AddComponent(type);
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
    /// Adds the points from the current script
    /// </summary>
    public void UpdatePoints()
    {
        // add points from the current input handler
        if (gameObject.GetComponent<GenericInputHandler>() != null)
            _points += gameObject.GetComponent<GenericInputHandler>().GetPoints();
    }

    /// <summary>
    /// Calls the spawn function on the active input handler - creating the necessary prefab
    /// </summary>
    /// <param name="playerPrefab">The prefab to spawn</param>
    /// <param name="position">Where to spawn</param>
    /// <returns>The spawned item</returns>
    internal Transform Spawn(Transform playerPrefab, Vector3 position)
    {
        return _activeHandler.Spawn(playerPrefab, position, _characterIndex, _playerName, PlayerInput.playerIndex);
    }

    /// <summary>
    /// Sets the colour of the appropriate lobby panel to match the colour of this player
    /// </summary>
    void UpdateMenu()
    {
        var lobbyDisplay = PlayerManagerScript.Instance.PlayerDisplays[PlayerInput.playerIndex];

        // set the panel that corresponds to this player with the correct colour and device
        lobbyDisplay.PlayerStarted(_colour, _device, PlayerInput.playerIndex);

        // connects the input handler to the display
        (_activeHandler as LobbyInputHandler).SetDisplay(lobbyDisplay, (x, y) => { _playerName = x; _characterIndex = y; }, PlayerInput.playerIndex);
    }

    /// <summary>
    /// Sets the colour of the lightbar on the dualshock
    /// </summary>
    void SetLightbarColour_()
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

    /// <summary>
    /// When the L1 input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnL1(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        if (_activeHandler != null)
            _activeHandler.OnL1();
    }


    /// <summary>
    /// When the R1 input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnR1(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        if (_activeHandler != null)
            _activeHandler.OnR1();
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
        // only handle it once
        if (!ctx.performed) return;

        if (_activeHandler != null)
            _activeHandler.OnSquare();
    }

    /// <summary>
    /// When the Triangle input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnTriangle(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        if (_activeHandler != null)
            _activeHandler.OnTriangle();
    }

    /// <summary>
    /// When the Circle input is triggered
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
    /// When the L2 input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnL2(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        if (_activeHandler != null)
            _activeHandler.OnL2();
    }

    /// <summary>
    /// When the R2 input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnR2(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        if (_activeHandler != null)
            _activeHandler.OnR2();
    }

    /// <summary>
    /// When the Options input is triggered
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public void OnOptions(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        if (_activeHandler != null)
            _activeHandler.OnOptions();
    }
}
