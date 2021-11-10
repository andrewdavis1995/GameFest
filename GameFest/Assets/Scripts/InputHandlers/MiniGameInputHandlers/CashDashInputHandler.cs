using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CashDashInputHandler : GenericInputHandler
{
    PlayerMovement _movement;
    bool _canMove = true;

    bool _hasBvKey = false;
    bool _passedBv = false;

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    /// <param name="playerName">The index of the selected character</param>
    /// <param name="id">ID of the profile</param>
    public override Transform Spawn(Transform prefab, Vector3 position, int characterIndex, string playerName, int playerIndex, Guid id)
    {
        base.Spawn(prefab, position, characterIndex, playerName, playerIndex, id);

        // create the player display
        var spawned = Instantiate(prefab, position, Quaternion.identity);

        // get the movement script attached to the visual player
        _movement = spawned.GetComponent<PlayerMovement>();

        _movement.SetJumpModifier(0.82f);
        _movement.AddMovementCallbacks(PlatformLanded, PlatformLeft);
        _movement.AddTriggerCallbacks(TriggerEnter, null);

        // set the height of the object
        SetHeight(spawned, characterIndex);

        // use the correct animation controller
        SetAnimation(spawned, characterIndex);

        // sets the player name
        _movement.SetPlayerName(true, playerName);

        return spawned;
    }

    void PlatformLanded(Collision2D collider)
    {
        if (collider.gameObject.tag == "Ground")
            _movement.transform.SetParent(collider.transform);
    }

    void PlatformLeft(Collision2D collider)
    {
        if (collider.gameObject.tag == "Ground")
            _movement.transform.SetParent(null);
    }

    void TriggerEnter(Collider2D collider)
    {
        if (collider.gameObject.tag == "PowerUp")
        {
            var coin = collider.GetComponent<CoinScript>();
            coin.Disable();
            AddPoints(coin.Points);
        }
        else if (collider.gameObject.tag == "Card")
        {
            // only destroy if matches the player
            if (collider.gameObject.name == GetPlayerIndex().ToString())
                collider.gameObject.SetActive(false);

            _hasBvKey = true;
            _movement.IgnoreCollisions(CashDashController.Instance.BvColliders);
        }
    }

    /// <summary>
    /// Set the height of the player based on the selected character
    /// </summary>
    /// <param name="player">The transform the the player display</param>
    /// <param name="characterIndex">The index of the chosen character</param>
    public override void SetHeight(Transform player, int characterIndex)
    {
        float size = 0.12f;

        switch (characterIndex)
        {
            // Rachel
            case 1:
                size -= .03f;
                break;
            // Naomi
            case 2:
                size -= .015f;
                break;
            // Heather
            case 3:
                size -= 0.2f;
                break;
            // Mum & Dad
            case 4:
            case 5:
                size -= 0.01f;
                break;
            // John & Fraser
            case 6:
            case 7:
                size -= 0.007f;
                break;
            // Matthew
            case 8:
                size += 0.003f;
                break;
        }

        // set height
        player.localScale = new Vector3(size, size, 1);
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

}
