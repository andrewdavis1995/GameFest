using System;
using System.Collections;
using UnityEngine;

public class PlayerClimber : MonoBehaviour
{
    // status
    bool _onSlope;
    bool _onGround;
    float _slopeDownAngle;
    Vector2 _slopeNormalPerp;
    float _movementX = 0;
    bool _active = true;
    Vector2 _newVelocity = new Vector2();

    // Unity config
    [SerializeField]
    float MOVE_SPEED = 7;
    [SerializeField]
    float JUMP_FORCE = 200;
    [SerializeField]
    float _slopeCheckDistance = 0.25f;

    // Unity links to other objects
    [SerializeField]
    PhysicsMaterial2D StaticMaterial;
    public LayerMask WhatIsGround;

    // links to other objects
    CapsuleCollider2D _collider;
    SpriteRenderer _renderer;
    Rigidbody2D _rigidbody;
    PlayerAnimation _animation;

    // stored information
    Vector2 _colliderSize;

    // Start is called before the first frame update
    void Start()
    {
        // gets references to components
        _rigidbody = GetComponent<Rigidbody2D>();
        _animation = GetComponent<PlayerAnimation>();
        _collider = GetComponent<CapsuleCollider2D>();
        _renderer = GetComponent<SpriteRenderer>();

        // gets the size of the collider - need to get it relative to the overall size of the player
        _colliderSize = _collider.size * transform.localScale;
    }

    /// <summary>
    /// Checks if the player is on a slope
    /// </summary>
    void SlopeCheck_()
    {
        // check from players feet
        Vector2 checkPos = transform.position - new Vector3(0, _colliderSize.y / 2);

        // check both x and y directions
        SlopeCheckHorizontal_(checkPos);
        SlopeCheckVertical_(checkPos);
    }

    /// <summary>
    /// Temporarily the player so no movement is possible
    /// </summary>
    /// <param name="duration">How long to disable the player for</param>
    public void Disable(float duration)
    {
        // only do this if currently active - we don't want multiple co-routines going at once
        if (_active)
            StartCoroutine(HandleDisable_(duration));
    }

    /// <summary>
    /// Disables the player, waits for a period, then re-enables the player
    /// </summary>
    /// <param name="duration">How long to disable the player for</param>
    IEnumerator HandleDisable_(float duration)
    {
        // disable
        // TODO: set lie down image
        _active = false;
        _animation.enabled = false;

        // wait
        yield return new WaitForSeconds(duration);

        // enable
        _active = true;
        _animation.enabled = true;
    }

    /// <summary>
    /// Checks if the player is interacting with a slope horizontally
    /// </summary>
    /// <param name="checkPos">The position to check from</param>
    void SlopeCheckHorizontal_(Vector2 checkPos)
    {
        // look for a collision
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right, _slopeCheckDistance, WhatIsGround);
        RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right, _slopeCheckDistance, WhatIsGround);

        // if the front or back hits, we are on a slope
        if (slopeHitFront || slopeHitBack)
        {
            _onSlope = true;
        }
        else
        {
            _onSlope = false;
        }
    }

    /// <summary>
    /// Check downwards for a collision
    /// </summary>
    /// <param name="checkPos">Where to check from</param>
    void SlopeCheckVertical_(Vector2 checkPos)
    {
        // look for a collision
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, _slopeCheckDistance, WhatIsGround);

        // if there was a collision
        if (hit)
        {
            // get the angle of the collision
            _slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            _slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            // the player is on a slope
            _onSlope = true;
        }

        // if the player is not moving, and is on the ground, set the material to one with high friction, to avoid sliding
        if (_onGround && _movementX == 0f)
            _rigidbody.sharedMaterial = StaticMaterial;
        else
            // otherwise, move freely
            _rigidbody.sharedMaterial = null;
    }

    /// <summary>
    /// Causes the player to jump
    /// </summary>
    void Jump()
    {
        // can only jump if on ground
        if (_onGround)
        {
            _onGround = false;
            _rigidbody.AddForce(new Vector2(0, JUMP_FORCE));
            _animation.SetAnimation("Jump");
        }
    }

    /// <summary>
    /// Is the player active, or are they disabled?
    /// </summary>
    /// <returns>Whether the player is active</returns>
    public bool IsActive()
    {
        return _active;
    }

    // Update is called once per frame
    void Update()
    {
        // if not active, we can't do anything
        if (!_active) return;

        bool moving = false;

        // check if we are on a slope
        SlopeCheck_();

        // TODO: Replace this with an input handler ############
        if (Input.GetKey(KeyCode.RightArrow))
        {
            _movementX = MOVE_SPEED;
            moving = true;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _movementX = -MOVE_SPEED;
            moving = true;
        }

        if (!moving)
            _movementX = 0;

        if (Input.GetKey(KeyCode.Space) && _onGround)
        {
            Jump();
        }
        // #####################################################

        // move the player
        Move();
    }

    /// <summary>
    /// When the player collides with an object
    /// </summary>
    /// <param name="collision">The object that the player collided with</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if the object is ground, the player is now on the ground
        if (collision.gameObject.tag == "Ground") _onGround = true;
    }

    /// <summary>
    /// When the player stops colliding with an object
    /// </summary>
    /// <param name="collision">The object that the player stopped colliding with</param>
    private void OnCollisionExit2D(Collision2D collision)
    {
        // if the object was ground, the player is no longer on the ground
        if (collision.gameObject.tag == "Ground") _onGround = false;
    }

    /// <summary>
    /// Moves the player
    /// </summary>
    private void Move()
    {
        // if on the ground, but not a slope, walk normally
        if (_onGround && !_onSlope)
        {
            _newVelocity.Set(_movementX, 0);
        }
        // if on a slope, do some maths to work out the force to add
        else if (_onGround && _onSlope)
        {
            _newVelocity.Set(-_movementX * _slopeNormalPerp.x, -_movementX * _slopeNormalPerp.y);
        }
        // otherwise (i.e. mid-air), move freely
        else if (!_onGround)
        {
            _newVelocity.Set(_movementX, _rigidbody.velocity.y);
        }

        // set the velocity of the player
        _rigidbody.velocity = _newVelocity;

        // set the sprite renderer direction
        if (_movementX > 0)
        {
            _renderer.flipX = false;
        }
        else if (_movementX < 0)
        {
            _renderer.flipX = true;
        }

        // set the correct animation
        if (_onGround)
        {
            // walking
            if (Math.Abs(_rigidbody.velocity.x) > 0.15f)
            {
                _animation.SetAnimation("Walk");
            }
            // still
            else
            {
                _animation.SetAnimation("Idle");
            }
        }
    }
}
