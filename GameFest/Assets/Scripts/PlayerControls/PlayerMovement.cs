using System;
using System.Collections;
using System.Collections.Generic;
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
    Animator[] _animators;
    SpriteRenderer _renderer;
    Collider2D _collider;

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
    bool _animationControl = true;
    bool _disabled = false;
    float _jumpForce = JUMP_FORCE;
    float _movementForce = 1;

    // callback functions
    Action<Collider2D> _triggerEnterCallback;
    Action<Collider2D> _triggerExitCallback;
    Action<Collision2D> _collisionCallback;
    Action<Collision2D> _collisionExitCallback;

    public PlayerAnimation PlayerAnimator;
    public PlayerAnimation ShadowAnimator;

    void Awake()
    {
        // find necessary components
        _renderer = GetComponent<SpriteRenderer>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _animators = GetComponentsInChildren<Animator>();
        _collider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Adds a callback for when the item collides with something
    /// </summary>
    /// <param name="collisionEnter">The callback action to carry out</param>
    /// <param name="collisionExit">The callback action to carry out</param>
    internal void AddMovementCallbacks(Action<Collision2D> collisionEnter, Action<Collision2D> collisionExit)
    {
        _collisionCallback = collisionEnter;
        _collisionExitCallback = collisionExit;
    }

    /// <summary>
    /// Bonces the player back from the item it collided with
    /// </summary>
    /// <param name="collider"></param>
    internal void BounceBack(Collision2D collider)
    {
        // work out which direction to apply force in
        int offset = transform.position.x < collider.transform.position.x ? -1 : 1;
        _rigidBody.AddForce(new Vector3(180f * offset, 150f, 0));
    }

    /// <summary>
    /// Sets whether the sprite renderer is affected by SpriteMasks
    /// </summary>
    public void SetMasking(SpriteMaskInteraction interaction)
    {
        _renderer.maskInteraction = interaction;
    }

    /// <summary>
    /// Changes the force used to make the player jump
    /// </summary>
    /// <param name="modifier">How much to affect the power by</param>
    public void SetJumpModifier(float modifier)
    {
        _jumpForce = JUMP_FORCE * modifier;
        _movementForce = modifier;
    }

    /// <summary>
    /// Causes thee player to ignore collisions with the specified colliders
    /// </summary>
    /// <param name="bvColliders"></param>
    internal void IgnoreCollisions(Collider2D[] bvColliders)
    {
        foreach (var col in bvColliders)
            Physics2D.IgnoreCollision(_collider, col);
    }

    internal void SetIcon(object keyIcon)
    {
        throw new NotImplementedException();
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
        SetIcon(PunchlineBlingController.Instance.ActiveIcons[imgIndex]);
    }

    /// <summary>
    /// Sets the icon that displays over head
    /// </summary>
    /// <param name="sprite"></param>
    public void SetIcon(Sprite sprite)
    {
        ActivePlayerIcon.sprite = sprite;
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
    /// Stop movement for a duration
    /// </summary>
    /// <param name="duration">How long to disable for</param>
    /// <param name="disabledImage">Image to use when disabled</param>
    public IEnumerator Disable(float duration, Sprite disabledImage)
    {
        _disabled = true;
        _renderer.sprite = disabledImage;

        // disable animations
        foreach (var anim in _animators)
            anim.enabled = false;

        yield return new WaitForSeconds(duration);
        _disabled = false;
        // enable animations
        foreach (var anim in _animators)
            anim.enabled = true;
    }

    /// <summary>
    /// Called once a frame
    /// </summary>
    void Update()
    {
        if (!_disabled)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);

            // only move if the value of the input is big enough to be noticeable
            var xMove = Math.Abs(_movementInput.x) > 0.25f ? _movementInput.x : 0;
            transform.Translate(new Vector2(xMove, 0) * Speed * Time.deltaTime);

            // if we are on the ground, i.e. walking or idle, update the animation
            if (_onGround && _animationControl)
                Animate(xMove == 0 ? "Idle" : "Walk");

            UpdateOrientation_(xMove);
        }
    }

    /// <summary>
    /// Sets whether this script has control over the animator
    /// </summary>
    /// <param name="state"Whether this script can control the animator</param>
    public void SetAnimationControl(bool state)
    {
        _animationControl = state;
    }

    /// <summary>
    /// Returns the renderer of the player
    /// </summary>
    /// <returns>The renderer</returns>
    internal SpriteRenderer GetRenderer()
    {
        return _renderer;
    }

    /// <summary>
    /// Sets the animation trigger of the player and shadow
    /// </summary>
    /// <param name="animation"></param>
    public void Animate(string animation)
    {
        PlayerAnimator.SetAnimation(animation);
        ShadowAnimator?.SetAnimation(animation);
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

    public void DisableAnimators()
    {
        foreach (var animator in _animators)
        {
            animator.enabled = false;
        }
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
        _movementInput = movement* _movementForce;
    }

    /// <summary>
    /// Makes the player "jump"
    /// </summary>
    public void Jump()
    {
        // cannot jump if not on the ground
        if (!_onGround)
            return;

        // cannot jump is disabled
        if (_disabled)
            return;

        // set the animation to the jumping animation
        Animate("Jump");

        // might as well JUMP!
        _rigidBody.AddForce(new Vector2(0, _jumpForce));
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

        // invoke the callback if set
        _collisionCallback?.Invoke(collision);
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

        // invoke the callback if set
        _collisionExitCallback?.Invoke(collision);
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
