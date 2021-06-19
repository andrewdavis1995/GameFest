using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class XTinguishInputHandler : GenericInputHandler
{
    // links to other scripts
    ZeroGravityMovement _zeroGravityScript;

    // status values
    int _playerIndex;
    int _characterIndex;
    bool _active = false;
    string _playerName;

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    /// <param name="playerIndex">The index of the player</param>
    /// <returns>The transform that was created</returns>
    public override Transform Spawn(Transform prefab, Vector3 position, int characterIndex, string playerName, int playerIndex)
    {
        _playerIndex = playerIndex;
        _playerName = playerName;
        _characterIndex = characterIndex;

        // create the player display
        var player = Instantiate(prefab, position, Quaternion.identity);

        // set the height of the object
        SetHeight(player, characterIndex);

        // get the movement script - disable it to stop the animations getting in each others way
        _zeroGravityScript = player.GetComponent<ZeroGravityMovement>();
        _zeroGravityScript.SetPlayerColour(_playerIndex);

        return player;
    }

    /// <summary>
    /// Checks if the current player can move
    /// </summary>
    /// <returns>Whether the player can move</returns>
    public bool Active()
    {
        return _active;
    }

    /// <summary>
    /// Sets if the current player can move
    /// </summary>
    /// <param name="active">If the player is active</param>
    public void Active(bool active)
    {
        _active = active;
    }

    /// <summary>
    /// Set the height of the player based on the selected character
    /// </summary>
    /// <param name="player">The transform the the player display</param>
    /// <param name="characterIndex">The index of the chosen character</param>
    public override void SetHeight(Transform player, int characterIndex)
    {
        float size = 0.2f;

        switch (characterIndex)
        {
            // Andrew
            case 0:
                size += 1f;
                break;
            // Rachel
            case 1:
                size += .65f;
                break;
            // Naomi
            case 2:
                size += .77f;
                break;
            // Heather
            case 3:
                size += 0.75f;
                break;
            // Mum & Dad
            case 4:
            case 5:
                size += 0.89f;
                break;
            // John & Fraser
            case 6:
            case 7:
                size += 0.83f;
                break;
            // Matthew
            case 8:
                size += 0.93f;
                break;
        }

        // set height
        player.localScale = new Vector3(size, size, 1);
    }

    /// <summary>
    /// Checks if the player is complete - i.e. no longer playing
    /// </summary>
    /// <returns>Whether the player is complete</returns>
    public bool IsComplete()
    {
        return _zeroGravityScript.IsComplete();
    }

    #region Input Handlers
    public override void OnMove(InputAction.CallbackContext ctx)
    {
        // get the value from the input
        var movement = ctx.ReadValue<Vector2>();

        // cause the player to move left to right
        _zeroGravityScript.X_Movement(movement.x);

        // set value to be used for propulsion
        _zeroGravityScript.Y_Movement(movement.y);
    }

    public override void OnTriangle()
    {
        // if the player is in the door and chooses to bail, make them exit
        _zeroGravityScript.Escape();
    }

    /// <summary>
    /// End the player on timeout
    /// </summary>
    internal void Timeout()
    {
        _zeroGravityScript.Timeout();
    }

    internal int GetCharacterIndex()
    {
        return _characterIndex;
    }

    /// <summary>
    /// Returns the name of the player
    /// </summary>
    /// <returns>The players name</returns>
    internal string GetPlayerName()
    {
        return _playerName;
    }

    /// <summary>
    /// Gets the index of the player
    /// </summary>
    /// <returns>The index of the player</returns>
    internal int GetPlayerIndex()
    {
        return _playerIndex;
    }

    /// <summary>
    /// Fetches the points collected by the player
    /// </summary>
    /// <returns>List of battery values collected</returns>
    internal List<int> GetBatteryList()
    {
        return _zeroGravityScript.GetBatteryList();
    }
    #endregion
}