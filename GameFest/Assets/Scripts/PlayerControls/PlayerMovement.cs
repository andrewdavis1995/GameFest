using System;
using System.Collections.Generic;
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
    Animator[] _animators;
    SpriteRenderer _renderer;

    // public objects
    public TextMesh TxtPlayerName;
    public SpriteRenderer ActivePlayerIcon;
    public SpriteRenderer Shadow;
    public List<SpriteRenderer> _blingRenderers;
    public Transform BlingHolder;
    public ItemFlash Flash;

    // player state
    Vector2 _movementInput;
    bool _onGround = false;
    bool _flipX = false;

    // callback functions
    Action<Collider2D> _triggerEnterCallback;
    Action<Collider2D> _triggerExitCallback;

    public PlayerAnimation PlayerAnimator;
    public PlayerAnimation ShadowAnimator;

    void Start()
    {
        // find necessary components
        _renderer = GetComponent<SpriteRenderer>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _animators = GetComponentsInChildren<Animator>();
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
    /// <param name="imgIndex">The image index to set it as</param>
    public void SetActiveIcon(bool state, int imgIndex)
    {
        ActivePlayerIcon.gameObject.SetActive(state);
        ActivePlayerIcon.sprite = PunchlineBlingController.Instance.ActiveIcons[imgIndex];
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
            Animate(xMove == 0 ? "Idle" : "Walk");

        UpdateOrientation_(xMove);
    }

    void Animate(string animation)
    {
        PlayerAnimator.SetAnimation(animation);
        ShadowAnimator.SetAnimation(animation);
    }

    /// <summary>
    /// Sets the FlipX property of the player image
    /// </summary>
    /// <param name="xMove">How much the player is moving</param>
    void UpdateOrientation_(float xMove)
    {
        if (PauseGameHandler.Instance != null && PauseGameHandler.Instance.IsPaused()) return;

        var flipped = _flipX;

        // if moving right, set flip to false
        if (xMove > 0)
        {
            _flipX = false;
        }
        // if moving left, set flip to true
        else if (xMove < 0)
        {
            _flipX = true;
        }

        // only change direction when necessary
        if (flipped != _flipX)
        {
            _renderer.flipX = _flipX;
            Shadow.flipX = _flipX;

            foreach (var rend in _blingRenderers)
            {
                rend.flipX = _flipX;
            }
        }

        // note: if not moving, flip will remain the same as it was before
    }

    /// <summary>
    /// Stops movement from the player
    /// </summary>
    internal void DisableMovement()
    {
        _movementInput = new Vector2(0, 0);
        Animate("Idle");
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
        Animate("Jump");

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
            Animate("Jump");
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

    /// <summary>
    /// Whether the image(s) are flipped
    /// </summary>
    /// <returns></returns>
    internal bool Flipped()
    {
        return _flipX;
    }

    /// <summary>
    /// Enables/disables the animator of this player
    /// </summary>
    /// <param name="state">The state to set the animator to be in</param>
    internal void SetAnimatorState(bool state)
    {
        foreach (var anim in _animators)
            anim.enabled = state;
    }

    internal void AddBling(int count)
    {
        Flash.Go(CreateBling_, null, count);
    }

    void CreateBling_(int index)
    {
        // create bling
        var created = Instantiate(PunchlineBlingController.Instance.BlingPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        // add bling
        created.SetParent(BlingHolder);

        created.transform.localScale = new Vector3(1, 1, 1);
        created.transform.localPosition = new Vector3(0, -0.01f, 0.001f - (0.001f * index));

        var renderer = created.GetComponent<SpriteRenderer>();
        renderer.sprite = PunchlineBlingController.Instance.BlingSprites[index];
        renderer.flipX = Flipped();

        _blingRenderers.Add(renderer);
    }
}
