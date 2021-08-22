using UnityEngine;
using UnityEngine.InputSystem;

public class BeachBowlesInputHandler : GenericInputHandler
{
    float _movementX;

    private void Update()
    {
        if (_movementX < -0.1f) BeachBowlesController.Instance.MoveLeft();
        if (_movementX > 0.1f) BeachBowlesController.Instance.MoveRight();
    }

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
        base.Spawn(prefab, position, characterIndex, playerName, playerIndex);

        return null;
    }

    /// <summary>
    /// When cross is pressed
    /// </summary>
    public override void OnCross()
    {
        BeachBowlesController.Instance.ConfirmPressed(GetPlayerIndex());
    }

    /// <summary>
    /// When triangle is pressed
    /// </summary>
    public override void OnTriangle()
    {
        BeachBowlesController.Instance.TrianglePressed(GetPlayerIndex());
    }

    /// <summary>
    /// When the move event is triggered
    /// </summary>
    /// <param name="ctx">The context of the movement</param>
    public override void OnMove(InputAction.CallbackContext ctx)
    {
        // the vector of the movement input from the user
        var movement = ctx.ReadValue<Vector2>();
        _movementX = movement.x;
    }
}