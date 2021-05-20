using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controls the movement of a player
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // constant values
    const float JUMP_FORCE = 800f;

    // public configurable values
    public float Speed = 5f;

    // components/objects
    Rigidbody2D _rigidBody;
    Animator _animator;
    SpriteRenderer _renderer;

    // public objects
    public TextMesh TxtPlayerName;
    public GameObject ActivePlayerIcon;

    // player state
    Vector2 _movementInput;
    bool _onGround = false;

    // callback functions
    Action<Collider2D> _triggerEnterCallback;
    Action<Collider2D> _triggerExitCallback;

    public PlayerAnimation PlayerAnimator;

    void Start()
    {
        // find necessary components
        _renderer = GetComponent<SpriteRenderer>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Sets the functions to call when the player collides with a trigger
    /// </summary>
    /// <param name="triggerEnter">Function to call when player enters the trigger</param>
    /// <param name="triggerExit">Function to call when player leaves the trigger</param>
    internal void AddTriggerCallbacks(Action<Collider2D> triggerEnter, Action<Collider2D> triggerExit)
    {
        _triggerEnterCallback = triggerEnter;
        _triggerExitCallback = triggerExit;
    }

    /// <summary>
    /// Sets the visibilty of the active icon
    /// </summary>
    /// <param name="state">The state to set</param>
    public void SetActiveIcon(bool state)
    {
        ActivePlayerIcon.SetActive(state);
    }

    /// <summary>
    /// Sets the state of the player name display
    /// </summary>
    /// <param name="state">Visibility of the name</param>
    /// <param name="name">The name of the player</param>
    public void SetPlayerName(bool state, string name = "")
    {
        TxtPlayerName.gameObject.SetActive(state);
        TxtPlayerName.text = name;
    }

    /// <summary>
    /// Called once a frame
    /// </summary>
    void Update()
    {
        transform.eulerAngles = new Vector3(0, 0, 0);

        // only move if the value of the input is big enough to be noticeable
        var xMove = Math.Abs(_movementInput.x) > 0.25f ? _movementInput.x : 0;
        transform.Translate(new Vector2(xMove, 0) * Speed * Time.deltaTime);

        // if we are on the ground, i.e. walking or idle, update the animation
        if (_onGround)
            PlayerAnimator.SetAnimation(xMove == 0 ? "Idle" : "Walk");

        UpdateOrientation_(xMove);
    }

    /// <summary>
    /// Sets the FlipX property of the player image
    /// </summary>
    /// <param name="xMove">How much the player is moving</param>
    void UpdateOrientation_(float xMove)
    {
        // if moving right, set flip to false
        if (xMove > 0)
        {
            _renderer.flipX = false;
        }
        // if moving left, set flip to true
        else if (xMove < 0)
        {
            _renderer.flipX = true;
        }

        // note: if not moving, flip will remain the same as it was before
    }

    /// <summary>
    /// Stops movement from the player
    /// </summary>
    internal void DisableMovement()
    {
        _movementInput = new Vector2(0, 0);
        PlayerAnimator.SetAnimation("Idle");
    }

    /// <summary>
    /// Sets how much to move - comes from the Input
    /// </summary>
    /// <param name="movement">How much to move by</param>
    public void Move(Vector2 movement)
    {
        _movementInput = movement;
    }

    /// <summary>
    /// Makes the player "jump"
    /// </summary>
    public void Jump()
    {
        // cannot jump if not on the ground
        if (!_onGround)
            return;

        // set the animation to the jumping animation
        PlayerAnimator.SetAnimation("Jump");

        // might as well JUMP!
        _rigidBody.AddForce(new Vector2(0, JUMP_FORCE));
        _onGround = false;
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
            // we are now on the ground
            _onGround = true;
        }
    }

    /// <summary>
    /// Occurs when this object stops colliding with another
    /// </summary>
    /// <param name="collision">The collision event - including the object that the player stopped colliding with</param>
    void OnCollisionExit2D(Collision2D collision)
    {
        // if the player left the ground, they are falling
        if (collision.gameObject.tag == "Ground")
        {
            // they are no longer on the ground
            _onGround = false;
            PlayerAnimator.SetAnimation("Jump");
        }
    }

    /// <summary>
    /// When the player starts contact with a trigger
    /// </summary>
    /// <param name="collision">The trigger item</param>
    public void OnTriggerEnter2D(Collider2D collision)
    {
        _triggerEnterCallback?.Invoke(collision);
    }

    /// <summary>
    /// When the player leaves contact with a trigger
    /// </summary>
    /// <param name="collision">The trigger item</param>
    public void OnTriggerExit2D(Collider2D collision)
    {
        _triggerExitCallback?.Invoke(collision);
    }
}
