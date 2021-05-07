using UnityEngine;
using UnityEngine.InputSystem;

public abstract class GenericInputHandler : MonoBehaviour
{
    public virtual void Spawn(Transform prefab, Vector2 position, int characterIndex) { }

    public virtual void OnMove(InputAction.CallbackContext ctx) { }
    public virtual void OnCross() { }
    public virtual void OnCircle() { }
    public virtual void OnTriangle() { }
    public virtual void OnSquare() { }
    public virtual void OnTouchpad() { }
    public virtual void OnL1() { }
    public virtual void OnR1() { }

    /// <summary>
    /// Sets the animation to that of the correct character
    /// </summary>
    /// <param name="spawned">The instantiated object with an animator attached</param>
    /// <param name="characterIndex">The index of the selected character</param>
    public void SetAnimation(Transform spawned, int characterIndex)
    {
        // update the animation controller
        spawned.GetComponent<Animator>().runtimeAnimatorController = PlayerManagerScript.Instance.CharacterControllers[characterIndex];
    }
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
 */
