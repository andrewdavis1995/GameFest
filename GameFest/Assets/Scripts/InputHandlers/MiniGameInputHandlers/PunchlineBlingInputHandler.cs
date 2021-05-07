using UnityEngine;
using UnityEngine.InputSystem;

public class PunchlineBlingInputHandler : GenericInputHandler
{
    PlayerMovement _movement;

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    public override void Spawn(Transform prefab, Vector2 position, int characterIndex)
    {
        var spawned = Instantiate(prefab, position, Quaternion.identity);
        _movement = spawned.GetComponent<PlayerMovement>();
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

    public override void OnCross()
    {
        _movement.Jump();
    }
}
