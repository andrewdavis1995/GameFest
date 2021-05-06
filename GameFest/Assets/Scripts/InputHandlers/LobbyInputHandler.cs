using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyInputHandler : GenericInputHandler
{
    public override void OnMove(InputAction.CallbackContext ctx)
    {
        // only handle it once
        //if (!ctx.performed) return;

        // TODO: only move if X value is 1 or -1

        Debug.Log(ctx.ReadValue<Vector2>());
    }

    public override void OnCross()
    {
        Debug.Log("On Cross");
    }

    public override void OnTouchpad()
    {
        Debug.Log("On Touchpad");
    }
}
