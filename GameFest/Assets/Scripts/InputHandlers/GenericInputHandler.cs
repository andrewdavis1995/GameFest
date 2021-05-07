using UnityEngine;
using UnityEngine.InputSystem;

public abstract class GenericInputHandler : MonoBehaviour
{
    public virtual void OnMove(InputAction.CallbackContext ctx) { }
    public virtual void OnCross() { }
    public virtual void OnCircle() { }
    public virtual void OnTriangle() { }
    public virtual void OnSquare() { }
    public virtual void OnTouchpad() { }
    public virtual void OnL1() { }
    public virtual void OnR1() { }
}

/* CODE DUMP
 * 
 * Paddle code
 * 
 * 
 *         // paddles - TODO: move to another script
        Transform[] _paddles;
        bool _paddleState = false;

        _paddles = GameObject.FindGameObjectsWithTag("Paddle").Select(e => e.transform).ToArray();

 * 

    /// <summary>
    /// Updates the angle of the paddles
    /// </summary>
    void SetPaddles_()
    {
        var paddleAngle = _paddleState ? 45 : -45;

        // loop through all the paddles
        foreach (var paddle in _paddles)
        {
            // change the angle
            paddle.eulerAngles = new Vector3(0, 0, paddleAngle);
        }
    }



 * 
 * MOVEMENT
 * 
        //if (_state.GetState() == PlayerStateEnum.Playing)
        //    _movement.Jump();
 * 
 * 
        //    if (_state.GetState() == PlayerStateEnum.Ready)
        //        // move the player
        //        _movement.Move(ctx.ReadValue<Vector2>());
 * 
 */
