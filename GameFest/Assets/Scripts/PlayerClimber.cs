using System;
using System.Collections;
using UnityEngine;

public class PlayerClimber : MonoBehaviour
{
    // status
    [SerializeField]
    bool _onSlope;
    [SerializeField]
    bool _onGround;
    float _slopeDownAngle;
    Vector2 _slopeNormalPerp;
    float _movementX = 0;
    bool _active = true;
    Vector2 _newVelocity = new Vector2();
    bool _inSludge = false;
    [SerializeField]
    int _powerUpLevel;

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

    int playerIndex = 0;    // TODO: replace with stuff from input handler

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
    /// Gets the player index associated with this player
    /// </summary>
    /// <returns>The index of this player</returns>
    internal int GetPlayerIndex()
    {
        return playerIndex;
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
        if (_onGround && _movementX == 0f && _onSlope)
            _rigidbody.sharedMaterial = StaticMaterial;
        else
            // otherwise, move freely
            _rigidbody.sharedMaterial = null;
    }

    /// <summary>
    /// Players power up goes to zero when hit by rock
    /// </summary>
    internal void DecreasePowerUpLevel()
    {
        if (_powerUpLevel > 0)
            _powerUpLevel--;
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

        float sludgeAffector = _inSludge ? 0.4f : 1f;

        // TODO: Replace this with an input handler ############
        if (Input.GetKey(KeyCode.RightArrow))
        {
            _movementX = MOVE_SPEED * sludgeAffector;
            moving = true;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _movementX = -MOVE_SPEED * sludgeAffector;
            moving = true;
        }

        if (!moving)
            _movementX = 0;

        if (Input.GetKey(KeyCode.Space) && _onGround && !_inSludge)
        {
            Jump();
        }

        if (Input.GetKey(KeyCode.KeypadEnter))
        {
            PerformPowerUpAction_(_powerUpLevel);
        }
        // #####################################################

        // move the player
        Move();
    }

    /// <summary>
    /// Performs the action based on the power up level
    /// </summary>
    /// <param name="powerUpLevel">The power up level the player has reached</param>
    private void PerformPowerUpAction_(int powerUpLevel)
    {
        switch (powerUpLevel)
        {
            case 0:
                // do nothing
                break;
            case 1:
                // spawn a few small rocks
                LandslideController.Instance.RockBarageSmall(playerIndex);
                break;
            case 2:
                // spawn a mixture of small and bigger rocks
                LandslideController.Instance.RockBarage(playerIndex);
                break;
            default:
                // spawn a giant rock
                LandslideController.Instance.SpawnGiantRock(playerIndex);
                break;
        }

        _powerUpLevel = 0;
    }

    /// <summary>
    /// When the player collides with an object
    /// </summary>
    /// <param name="collision">The object that the player collided with</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if the object is ground, the player is now on the ground
        if (collision.gameObject.tag == "Ground" && collision.relativeVelocity.y > 0) _onGround = true;
    }

    /// <summary>
    /// When the player stops colliding with an object
    /// </summary>
    /// <param name="collision">The object that the player stopped colliding with</param>
    private void OnCollisionExit2D(Collision2D collision)
    {
        // if the object was ground, the player is no longer on the ground
        if (collision.gameObject.tag == "Ground")
        {
            _onGround = false;
            _rigidbody.sharedMaterial = null;
        }
    }

    /// <summary>
    /// When the player hits a trigger
    /// </summary>
    /// <param name="collision">The trigger the player collided with</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if it was the end point
        if (collision.gameObject.name == "End Point")
        {
            // the player is complete
            _active = false;
            _animation.SetAnimation("Celebrate");

            // check if all players are complete
            LandslideController.Instance.CheckForFinish();
        }
        // the player is in sludge
        else if (collision.gameObject.name.Contains("Sludge"))
        {
            _inSludge = true;
        }
        // the player has picked up a power up
        else if (collision.gameObject.tag == "PowerUp")
        {
            // destroy the object, and increase power up
            Destroy(collision.gameObject);
            _powerUpLevel++;

            // remove from the active list
            LandslideController.Instance.RemoveBoost(collision.transform);
        }
        // the player has picked up a power up
        else if (collision.gameObject.tag == "Checkpoint")
        {
            // store the order in which the player reached the checkpoint
            bool newCheckpoint = collision.GetComponent<CheckpointScript>().AddPlayer(playerIndex);
            if (newCheckpoint) _powerUpLevel++;
        }
    }

    /// <summary>
    /// When a trigger is exited
    /// </summary>
    /// <param name="collision">The object that the player stop colliding with</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        // check if the player has left the sludge
        if (collision.gameObject.name.Contains("Sludge"))
        {
            _inSludge = false;
        }
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
