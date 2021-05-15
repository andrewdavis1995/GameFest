using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PunchlineBlingInputHandler : GenericInputHandler
{
    PlayerMovement _movement;
    CardScript _currentCard;

    bool _isActivePlayer = false;

    List<Joke> _jokes = new List<Joke>();

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
        // move the player
        _movement.Move(ctx.ReadValue<Vector2>());
    }

    /// <summary>
    /// When the cross event is triggered
    /// </summary>
    public override void OnCross()
    {
        _movement.Jump();
    }

    /// <summary>
    /// When the triangle event is triggered
    /// </summary>
    public override void OnTriangle()
    {
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
}
