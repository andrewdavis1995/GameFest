using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BeachBowlesInputHandler : GenericInputHandler
{
    float _movementX;

    private void Update()
    {
        if (_movementX < -0.1f) BeachBowlesController.Instance.MoveLeft(GetPlayerIndex());
        if (_movementX > 0.1f) BeachBowlesController.Instance.MoveRight(GetPlayerIndex());
    }

    /// <summary>
    /// Creates the specified object for the player attached to this object
    /// </summary>
    /// <param name="prefab">The prefab to instantiate</param>
    /// <param name="position">The location at which to spawn the item</param>
    /// <param name="characterIndex">The index of the selected character</param>
    /// <param name="playerIndex">The index of the player</param>
    /// <param name="id">ID of the profile</param>
    /// <returns>The transform that was created</returns>
    public override Transform Spawn(Transform prefab, Vector3 position, int characterIndex, string playerName, int playerIndex, Guid id)
    {
        base.Spawn(prefab, position, characterIndex, playerName, playerIndex, id);
        return null;
    }

    /// <summary>
    /// When cross is pressed
    /// </summary>
    public override void OnCross()
    {
        base.OnCross();
        if (PauseGameHandler.Instance.IsPaused()) return;

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
    public override void OnMove(InputAction.CallbackContext ctx, InputDevice device)
    {
        // the vector of the movement input from the user
        var movement = ctx.ReadValue<Vector2>();
        _movementX = movement.x;
    }

    public override void OnR1()
    {
        if (!PauseGameHandler.Instance.IsPaused())
            BeachBowlesController.Instance.CameraPreview(GetPlayerIndex());

        base.OnR1();
    }
}