using UnityEngine;

/// <summary>
/// Controls the movement of a player
/// </summary>
public class PlayerAnimation : MonoBehaviour
{
    Animator _animator;

    void Start()
    {
        // find necessary components
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Sets the animation of the player to the specified trigger
    /// </summary>
    /// <param name="animation">The trigger to set</param>
    public void SetAnimation(string animation)
    {
        // ensure the animator exists
        if (_animator != null)
        {
            // reset all triggers
            _animator.ResetTrigger("Jump");
            _animator.ResetTrigger("Walk");
            _animator.ResetTrigger("Idle");
            _animator.ResetTrigger("Celebrate");

            // set the specified trigger
            _animator.SetTrigger(animation);
        }
    }

    /// <summary>
    /// Occurs when this object collides with another
    /// </summary>
    /// <param name="collision">The collision event - including the object that the player collided with</param>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // if colliding with the ground, and moving downwards
        if (collision.gameObject.tag == "Ground" && collision.relativeVelocity.y > 0)
        {
            SetAnimation("Idle");
        }
    }
}
