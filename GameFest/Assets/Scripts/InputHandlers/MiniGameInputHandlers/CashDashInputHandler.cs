using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CashDashInputHandler : GenericInputHandler
{
    PlayerMovement _movement;
    bool _canMove = true;

    bool _hasBvKey = false;

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
        _movement.SetMasking(SpriteMaskInteraction.VisibleOutsideMask);

        // set the height of the object
        SetHeight(spawned, characterIndex);

        // use the correct animation controller
        SetAnimation(spawned, characterIndex);

        // sets the player name
        _movement.SetPlayerName(true, playerName);

        return spawned;
    }

    /// <summary>
    /// Callback for when the player collides with an object
    /// </summary>
    /// <param name="collider">The item that was collided with</param>
    void PlatformLanded(Collision2D collider)
    {
        if (collider.gameObject.tag == "Ground")
            _movement.transform.SetParent(collider.transform);
        else if (collider.gameObject.tag == "KickBack")
            // bounce back a bit
            _movement.BounceBack(collider);
    }

    /// <summary>
    /// Callback for when the player stops colliding with an object
    /// </summary>
    /// <param name="collider">The item that was left</param>
    void PlatformLeft(Collision2D collider)
    {
        if (collider.gameObject.tag == "Ground")
            _movement.transform.SetParent(null);
    }

    /// <summary>
    /// Callback for when the player enters a trigger
    /// </summary>
    /// <param name="collider">The trigger that was triggered</param>
    void TriggerEnter(Collider2D collider)
    {
        // is it a coin?
        if (collider.gameObject.tag == "PowerUp")
        {
            var coin = collider.GetComponent<CoinScript>();
            coin.Disable();
            AddPoints(coin.Points);
        }
        // is it the BV key?
        else if (collider.gameObject.tag == "Card")
        {
            // only destroy if matches the player
            if (collider.gameObject.name == GetPlayerIndex().ToString())
                collider.gameObject.SetActive(false);

            _movement.ActivePlayerIcon.gameObject.SetActive(true);
            _hasBvKey = true;
            _movement.SetIcon(CashDashController.Instance.KeyIcon);
            _movement.IgnoreCollisions(CashDashController.Instance.BvColliders);
        }
        // BV gate causes player to be blocked
        else if (collider.gameObject.tag == "KickBack")
        {
            var bvGate = collider.GetComponentInParent<BvGateScript>();
            if(bvGate != null)
            {
                // check if the player has a key
                if (_hasBvKey)
                {
                    // they may pass
                    _movement.ActivePlayerIcon.gameObject.SetActive(false);
                    bvGate.DisplayMessage("Welcome " + GetPlayerName(), GetPlayerIndex());
                }
                else
                {
                    // blocked
                    StartCoroutine(_movement.Disable(2, CashDashController.Instance.DisabledImages[GetCharacterIndex()]));
                    bvGate.DisplayMessage("FOREIGN OBJECT", GetPlayerIndex());
                }
            }
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
