using System;
using UnityEngine;

/// <summary>
/// Controls the movement of a player
/// </summary>
public class PlayerJumper : MonoBehaviour
{
    Transform _platform;
    MarshmallowScript _platformScript;
    PlayerAnimation _animator;
    Rigidbody2D _rigidBody;

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
        if(_platform != null)
        {
            transform.position = new Vector3(_platform.position.x + _platformScript.OffsetX, transform.position.y, transform.position.z);
        }
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
}
