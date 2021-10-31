using UnityEngine;
using UnityEngine.InputSystem;

public class StatsInputHandler : GenericInputHandler
{
    #region Override functions
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

        // only host can control stat viewing
        if (!IsHost()) return;

        var movement = ctx.ReadValue<Vector2>();

        // if right, move right
        if (movement.x > 0.99f)
            MoveRight_();

        // if left, move left
        if (movement.x < -0.99f)
            MoveLeft_();

        // if up, move up
        if (movement.y < -0.99f)
            MoveDown_();

        // if up, move down
        if (movement.y > 0.99f)
            MoveUp_();
    }

    /// <summary>
    /// When the circle is triggered - back
    /// </summary>
    public override void OnCircle()
    {
        // only host can control stat viewing
        if (!IsHost()) return;

        ComparisonController.Instance.ReturnToMenu();
    }

    #endregion

    /// <summary>
    /// Move left
    /// </summary>
    void MoveLeft_()
    {
        // only host can control stat viewing
        if (!IsHost()) return;

        ComparisonController.Instance.GameLeft();
    }

    /// <summary>
    /// Move right
    /// </summary>
    void MoveRight_()
    {
        // only host can control stat viewing
        if (!IsHost()) return;

        ComparisonController.Instance.GameRight();
    }

    /// <summary>
    /// Move up
    /// </summary>
    void MoveUp_()
    {
        // only host can control stat viewing
        if (!IsHost()) return;

        ComparisonController.Instance.PlayerUp();
    }

    /// <summary>
    /// Move down
    /// </summary>
    void MoveDown_()
    {
        // only host can control stat viewing
        if (!IsHost()) return;

        ComparisonController.Instance.PlayerDown();
    }
}
