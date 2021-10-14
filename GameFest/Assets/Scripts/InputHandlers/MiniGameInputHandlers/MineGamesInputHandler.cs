using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MineGamesInputHandler : GenericInputHandler
{
    PlayerMovement _movement;
    Collider2D _collider;
    bool _canMove = false;
    int _activeZone = -1;
    bool _walkingOn = false;
    bool _walkingOff = false;
    float _walkOffX;
    float _walkOnX;

    Action _walkOffCallback;
    Action _walkOnCallback;

    List<string> _results = new List<string>();
    int _roundPoints = 0;

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    /// <param name="playerIndex">The index of the player</param>
    /// <param name="id">ID of the profile</param>
    public override Transform Spawn(Transform prefab, Vector3 position, int characterIndex, string playerName, int playerIndex, Guid id)
    {
        base.Spawn(prefab, position, characterIndex, playerName, playerIndex, id);

        // create the player display
        var spawned = Instantiate(prefab, position, Quaternion.identity);

        // get the movement script attached to the visual player
        _movement = spawned.GetComponent<PlayerMovement>();
        // assign callbacks for when the item interacts with triggers
        _movement.AddTriggerCallbacks(TriggerEnter, TriggerExit);

        // get the collider for the player
        _collider = _movement.GetComponentInChildren<Collider2D>();

        // set the height of the object
        SetHeight(spawned, characterIndex);

        // use the correct animation controller
        SetAnimation(spawned, characterIndex);

        // sets the player name
        _movement.SetPlayerName(true, playerName);

        return spawned;
    }

    /// <summary>
    /// Called once per frame
    /// </summary>
    private void Update()
    {
        // check if we have reached the edge of the screen
        if (_walkingOff && (_movement.transform.localPosition.x >= _walkOffX))
        {
            _walkingOff = false;
            _walkOffX = 0;
            _walkOffCallback?.Invoke();
        }

        // check if we have reached the correct platform position
        if (_walkingOn && (_movement.transform.localPosition.x <= _walkOnX))
        {
            _walkingOn = false;
            _walkOnX = 0;
            _walkOnCallback?.Invoke();
            _movement.Move(Vector2.zero);
            Physics2D.IgnoreCollision(_collider, MineGamesController.Instance.RightWall, false);
        }
    }

    /// <summary>
    /// Makes the player run on to the platform
    /// </summary>
    /// <param name="platformPlayerPosition">The position of the player on the plaform</param>
    /// <param name="runOnCallback">Callback to call once the player reaches the position</param>
    internal void RunOn(Vector2 platformPlayerPosition, Action runOnCallback)
    {
        _walkOnX = platformPlayerPosition.x;
        _walkingOn = true;
        _walkOnCallback = runOnCallback;
        _movement.transform.localPosition = new Vector3(_movement.transform.position.x, platformPlayerPosition.y, _movement.transform.localPosition.z);
        _movement.Move(new Vector2(-1, 0));
    }

    /// <summary>
    /// Makes the player run off to the way to the platform
    /// </summary>
    /// <param name="runOffX">The X position of the player on the platform</param>
    /// <param name="runOnCallback">Callback to call once the player reaches the edge of the screen</param>
    internal void RunOff(float runOffX, Action runOffCallback)
    {
        _walkOffX = runOffX;
        _walkingOff = true;
        _walkOffCallback = runOffCallback;
        _movement.Move(new Vector2(1, 0));
        Physics2D.IgnoreCollision(_collider, MineGamesController.Instance.RightWall, true);
    }

    /// <summary>
    /// When the player interacts with a trigger object
    /// </summary>
    /// <param name="collision">The object that triggered this event</param>
    public void TriggerEnter(Collider2D collision)
    {
        // check the tag of the item that triggered the event
        switch (collision.gameObject.tag)
        {
            case "AreaTrigger":
                _activeZone = int.Parse(collision.gameObject.name);
                MineGamesController.Instance.SetActiveIcon(GetPlayerIndex(), _activeZone);
                break;
        }
    }

    /// <summary>
    /// When the player stops interacting with a trigger object
    /// </summary>
    /// <param name="collision">The object that triggered this event</param>
    public void TriggerExit(Collider2D collision)
    {
        // check the tag of the item that triggered the event
        switch (collision.gameObject.tag)
        {
            case "AreaTrigger":
                _activeZone = -1;
                MineGamesController.Instance.SetActiveIcon(GetPlayerIndex(), _activeZone);
                break;
        }
    }

    /// <summary>
    /// Sets whether the player can move (not disabled)
    /// </summary>
    /// <param name="state">State of the player</param>
    public void CanMove(bool state)
    {
        _canMove = state;

        // stop movement
        if (!_canMove)
            _movement.Move(Vector2.zero);
    }

    /// <summary>
    /// When the move event is triggered
    /// </summary>
    /// <param name="ctx">The context of the movement</param>
    public override void OnMove(InputAction.CallbackContext ctx)
    {
        if (!_canMove) return;

        // move the player
        _movement.Move(ctx.ReadValue<Vector2>());
    }

    /// <summary>
    /// When the cross event is triggered
    /// </summary>
    public override void OnCross()
    {
        MineGamesController.Instance.OptionSelected(GetPlayerIndex(), ButtonValues.Cross);
    }

    /// <summary>
    /// When the triangle event is triggered
    /// </summary>
    public override void OnTriangle()
    {
        MineGamesController.Instance.OptionSelected(GetPlayerIndex(), ButtonValues.Triangle);
    }

    /// <summary>
    /// When the circle event is triggered
    /// </summary>
    public override void OnCircle()
    {
        MineGamesController.Instance.OptionSelected(GetPlayerIndex(), ButtonValues.Circle);
    }

    /// <summary>
    /// When the square event is triggered
    /// </summary>
    public override void OnSquare()
    {
        MineGamesController.Instance.OptionSelected(GetPlayerIndex(), ButtonValues.Square);
    }

    /// <summary>
    /// Which zone is the player in
    /// </summary>
    /// <returns>The zone that the player is in</returns>
    public int ActiveZone()
    {
        return _activeZone;
    }

    /// <summary>
    /// Add a result and description
    /// </summary>
    /// <param name="result">The result to add</param>
    /// <param name="points">The points to add</param>
    public void AddResultString(string result, int points)
    {
        _results.Add(result);
        _roundPoints += points;
    }

    /// <summary>
    /// Gets results and descriptions
    /// </summary>
    /// <returns>The list of results</returns>
    public List<string> GetResultList()
    {
        return _results;
    }

    /// <summary>
    /// Gets the points for the current round
    /// </summary>
    /// <returns>The list of results</returns>
    public int GetRoundPoints()
    {
        return _roundPoints;
    }

    /// <summary>
    /// Clears results
    /// </summary>
    public void ClearResults()
    {
        _roundPoints = 0;
        _results.Clear();
    }
}
