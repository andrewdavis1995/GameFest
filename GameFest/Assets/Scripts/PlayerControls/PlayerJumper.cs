using System;
using UnityEngine;

/// <summary>
/// Controls the movement of a player
/// </summary>
public class PlayerJumper : MonoBehaviour
{
    // links to objects
    Transform _platform;
    MarshmallowScript _platformScript;
    PlayerAnimation _animator;
    Rigidbody2D _rigidBody;

    // callback functions
    Action<Collision2D> _onCollisionCallback;

    // unity configuration
    public BoxCollider2D ColliderA;
    public BoxCollider2D ColliderB;

    // Runs once when player is created
    void Start()
    {
        // find components
        _rigidBody = gameObject.GetComponent<Rigidbody2D>();
        _animator = gameObject.GetComponent<PlayerAnimation>();
    }

    // Update is called once per frame
    private void Update()
    {
        // ensure the player stays upright
        transform.eulerAngles = new Vector3(0, 0, 0);

        // if the player is on a platform, set the player's position to the platform position
        if(_platform != null && _platformScript != null)
        {
            transform.position = new Vector3(_platform.position.x + _platformScript.OffsetX, transform.position.y, transform.position.z);
        }
    }

    /// <summary>
    /// Sets whether to lock the X position of the player
    /// </summary>
    /// <param name="state">Whether to lock the X position of the player</param>
    public void SetPositionLock_(bool state)
    {
        _rigidBody.constraints = state ? RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.FreezeRotation;
    }

    /// <summary>
    /// Sets the state of the first collider
    /// </summary>
    /// <param name="state">Whether the collider is enabled</param>
    public void SetColliderA_(bool state)
    {
        ColliderA.enabled = state;
    }

    /// <summary>
    /// Sets the state of the first collider
    /// </summary>
    /// <param name="state">Whether the collider is enabled</param>
    public void SetColliderB_(bool state)
    {
        ColliderB.enabled = state;
    }

    /// <summary>
    /// Sets the animation on the linked animator
    /// </summary>
    /// <param name="animation">The animation to be set</param>
    public void SetAnimation(string animation)
    {
        _animator.SetAnimation(animation);
    }

    /// <summary>
    /// Sets the function to call when the player collides with another object
    /// </summary>
    /// <param name="action">The function to call</param>
    public void SetCollisionCallback(Action<Collision2D> action)
    {
        _onCollisionCallback = action;
    }

    /// <summary>
    /// CHecks that the player is on the ground and ready to jump
    /// </summary>
    /// <returns>Whether the player can jump</returns>
    public bool OnGround()
    {
        return _platform != null;
    }

    /// <summary>
    /// Makes the player jump forwards
    /// </summary>
    public void Jump()
    {
        _rigidBody.AddForce(new Vector2(120, 150));
        _animator.SetAnimation("Jump");
    }

    /// <summary>
    /// Destroys the link between the player and the platform they were on
    /// </summary>
    public void Detach_()
    {
        _platform = null;
        transform.SetParent(null);
    }

    /// <summary>
    /// Links the player to the platform they are n=on
    /// </summary>
    /// <param name="platform"></param>
    public void Attach_(Transform platform)
    {
        _platform = platform;

        // remove momentum
        _rigidBody.velocity = Vector2.zero;
        transform.SetParent(platform);

        _platformScript = _platform.GetComponent<MarshmallowScript>();
    }

    /// <summary>
    /// When the player stops colliding with another object
    /// </summary>
    /// <param name="collision">The object that the player stopped colliding with</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if the object is the ground, attach to the platform
        if (collision.gameObject.tag == "Ground")
        {
            Attach_(collision.transform);
            _onCollisionCallback?.Invoke(collision);
        }
    }

    /// <summary>
    /// When the player collides with another object
    /// </summary>
    /// <param name="collision">The object that the player collided with</param>
    private void OnCollisionExit2D(Collision2D collision)
    {
        // if the object is the ground, detach from the platform
        if (collision.gameObject.tag == "Ground")
        {
            Detach_();
        }
    }

    /// <summary>
    /// Pops the player up - after recovering from falling in
    /// </summary>
    internal void Recover()
    {
        _rigidBody.AddForce(new Vector2(0, 215f));
    }

    /// <summary>
    /// Sets the variables after falling in/recovering
    /// </summary>
    /// <param name="state"></param>
    internal void RecoveryComplete(bool state)
    {
        SetColliderA_(state);
        SetColliderB_(!state);
        SetPositionLock_(!state);
    }
}
