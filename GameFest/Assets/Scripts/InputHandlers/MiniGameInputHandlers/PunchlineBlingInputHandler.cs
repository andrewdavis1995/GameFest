using UnityEngine;
using UnityEngine.InputSystem;

public class PunchlineBlingInputHandler : GenericInputHandler
{
    PlayerMovement _movement;
    CardScript _currentCard;

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    public override void Spawn(Transform prefab, Vector2 position, int characterIndex)
    {
        var spawned = Instantiate(prefab, position, Quaternion.identity);

        // get the movement script attached to the visual player
        _movement = spawned.GetComponent<PlayerMovement>();
        // assign callbacks for when the item interacts with triggers
        _movement.AddTriggerCallbacks(TriggerEnter, TriggerExit);

        // use the correct animation controller
        SetAnimation(spawned, characterIndex);
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
        // TODO: Only do this if the player is active
        if (_currentCard != null)
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
                // TODO: only do this if player is active
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
                // TODO: only do this if player is active
                    _currentCard.InZone(false);

                    _currentCard = null;
                }
                break;
        }
    }
}
