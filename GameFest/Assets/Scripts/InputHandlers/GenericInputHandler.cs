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
