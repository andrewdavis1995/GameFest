using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PunchlineBlingInputHandler : GenericInputHandler
{
    // links to other scripts
    PlayerMovement _movement;
    CardScript _currentCard;

    // status members
    bool _canMove = true;
    bool _isActivePlayer = false;
    bool _walkingOn = false;
    bool _walkingOff = false;

    // jokes and points
    List<Joke> _jokes = new List<Joke>();
    int _points = 0;

    // callback functions
    Action _walkOnCallBack;
    Action _walkOffCallBack;

    /// <summary>
    /// Called once a frame
    /// </summary>
    private void Update()
    {
        // if walking on
        if(_walkingOn)
        {
            // move to the right
            _movement.Move(new Vector2(1, 0));

            // if reached the end point, stop
            if(_movement.transform.position.x > PunchlineBlingController.Instance.ResultPlayerReadingPosition)
            {
                // tell the controller they are done
                _walkingOn = false;
                _walkOnCallBack?.Invoke();
                _movement.Move(new Vector2(0, 0));
            }
        }
        else if (_walkingOff)
        {
            // move to the left
            _movement.Move(new Vector2(-1, 0));

            // if reached the end point, stop
            if (_movement.transform.position.x < PunchlineBlingController.Instance.ResultPlayerPosition.x)
            {
                // tell the controller they are done
                _walkingOff = false;
                _walkOffCallBack?.Invoke();
                _movement.Move(new Vector2(0, 0));
            }
        }
    }

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    public override void Spawn(Transform prefab, Vector2 position, int characterIndex, string playerName)
    {
        // create the player display
        var spawned = Instantiate(prefab, position, Quaternion.identity);

        // get the movement script attached to the visual player
        _movement = spawned.GetComponent<PlayerMovement>();
        // assign callbacks for when the item interacts with triggers
        _movement.AddTriggerCallbacks(TriggerEnter, TriggerExit);

        // set the height of the object
        SetHeight(spawned, characterIndex);

        // use the correct animation controller
        SetAnimation(spawned, characterIndex);

        _movement.SetPlayerName(true, playerName);
    }

    /// <summary>
    /// Sets whether the player is "active" or not - i.e. their turn to answer
    /// </summary>
    /// <param name="active">The state to set</param>
    internal void ActivePlayer(bool active)
    {
        _isActivePlayer = active;

        // show the card selection border
        if (_currentCard != null && active)
        {
            _currentCard.InZone(true);
        }

        // show/hide the active icon
        _movement.SetActiveIcon(active);
    }

    /// <summary>
    /// Gets whether the player is "active" or not - i.e. their turn to answer
    /// </summary>
    /// <returns>Whether the player is active</returns>
    internal bool ActivePlayer()
    {
        return _isActivePlayer;
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
        if (!_canMove) return;

        _movement.Jump();
    }

    /// <summary>
    /// When the triangle event is triggered
    /// </summary>
    public override void OnTriangle()
    {
        if (!_canMove) return;

        // flip the selected card
        if (_currentCard != null && _isActivePlayer)
            _currentCard.Flip();
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
            // if it was a card, this becomes the selected card
            case "Card":
                _currentCard = collision.GetComponent<CardScript>();
                if (_isActivePlayer)
                    _currentCard.InZone(true);
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
            // if it was a card, forget that this was the current card
            case "Card":
                if (_currentCard != null)
                {
                    _currentCard.InZone(false);
                    _currentCard = null;
                }
                break;
        }
    }

    /// <summary>
    /// When the player has successfully matched a joke
    /// </summary>
    /// <param name="joke">The joke that was earned</param>
    public void JokeEarned(Joke joke)
    {
        _jokes.Add(joke);
    }

    /// <summary>
    /// Get the list of jokes that the player has won
    /// </summary>
    /// <returns>The list of jokes matched by the player</returns>
    public List<Joke> GetJokes()
    {
        return _jokes;
    }

    /// <summary>
    /// Gets the player ready for the results reveal
    /// </summary>
    /// <param name="resultPlayerPosition">The position at which to place the player</param>
    internal void MoveToEnd(Vector2 resultPlayerPosition)
    {
        _movement.transform.position = resultPlayerPosition;

        // disable movement
        _canMove = false;

        // disable movement
        _movement.Move(new Vector2(0, 0));
        _movement.SetAnimation("Idle");
    }

    /// <summary>
    /// Walk on until the player reaches the specified point
    /// </summary>
    /// <param name="callback">The function to call when walked on</param>
    internal void WalkOn(Action callback)
    {
        _walkingOn = true;
        _walkOnCallBack = callback;
    }

    /// <summary>
    /// Walk off until the player reaches the specified point
    /// </summary>
    /// <param name="callback">The function to call when walked off</param>
    internal void WalkOff(Action callback)
    {
        _walkingOff = true;
        _walkOffCallBack = callback;
    }

    /// <summary>
    /// Add points for a joke
    /// </summary>
    internal void AddPoints()
    {
        // generate a random value for the points and add it to the list
        var points = UnityEngine.Random.Range(200, 215);
        _points += points;
    }

    /// <summary>
    /// Returns the number of points won by this player
    /// </summary>
    /// <returns>Points earned</returns>
    public int GetPoints()
    {
        return _points;
    }
}
