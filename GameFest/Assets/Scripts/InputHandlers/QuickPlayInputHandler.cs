using UnityEngine;
using UnityEngine.InputSystem;

public class QuickPlayInputHandler : GenericInputHandler
{
    /// <summary>
    /// When the movement event is triggered - change letter/character
    /// </summary>
    /// <param name="ctx">The context of the input</param>
    public override void OnMove(InputAction.CallbackContext ctx)
    {
        // only handle it once
        if (!ctx.performed) return;

        // only use buttons - joystick moves too much
        if (ctx.control.layout.ToLower() == "stick") return;

        if (!IsHost()) return;

        var movement = ctx.ReadValue<Vector2>();

        // if up, move up
        if (movement.y < -0.99f)
            MoveDown_();

        // if up, move down
        if (movement.y > 0.99f)
            MoveUp_();
    }

    /// <summary>
    /// When the X Button is pressed
    /// </summary>
    public override void OnCross()
    {
        QuickPlayManager.Instance.LoadGame();
    }

    /// <summary>
    /// Move up a game
    /// </summary>
    void MoveUp_()
    {
        QuickPlayManager.Instance.MoveGameUp();
    }

    /// <summary>
    /// Move down a game
    /// </summary>
    void MoveDown_()
    {
        QuickPlayManager.Instance.MoveGameDown();
    }
}