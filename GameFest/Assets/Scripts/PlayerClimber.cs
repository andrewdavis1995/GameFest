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
    bool _isComplete = false;
    float _slopeDownAngle;
    Vector2 _slopeNormalPerp;
    [SerializeField]
    float _movementX = 0;
    bool _active = true;
    Vector2 _newVelocity = new Vector2();
    bool _inSludge = false;
    [SerializeField]
    bool _inWater = false;
    [SerializeField]
    int _recoveryPressesRemaining = 0;

    // Unity config
    [SerializeField]
    float MOVE_SPEED = 7;
    [SerializeField]
    float JUMP_FORCE = 200;
    public TextMesh PlayerNameText;

    [SerializeField]
    float _slopeCheckDistance = 0.25f;

    // Unity links to other objects
    [SerializeField]
    PhysicsMaterial2D StaticMaterial;
    public LayerMask WhatIsGround;
    public GameObject WaterCollider;

    // links to other objects
    CapsuleCollider2D _collider;
    SpriteRenderer _renderer;
    Rigidbody2D _rigidbody;
    PlayerAnimation _animation;

    // stored information
    Vector2 _colliderSize;
    int _playerIndex = 0;

    // callbacks
    Action _clearPowerupCallback;
    Action _increasePowerupCallback;
    Action _decreasePowerupCallback;

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
    /// Sets the speed at which the player is moving
    /// </summary>
    /// <param name="movementX">How much they are moving</param>
    internal void SetMovementVector(float movementX)
    {
        _movementX = movementX;
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
        if ((slopeHitFront && !slopeHitFront.collider.isTrigger)|| (slopeHitBack && !slopeHitBack.collider.isTrigger))
        {
            Debug.Log("SETTING ONGROUND");
            if (slopeHitFront)
                Debug.Log("FRONT: " + slopeHitFront.collider.gameObject.name);
            if (slopeHitBack)
                Debug.Log("BACK: " + slopeHitBack.collider.gameObject.name);
            _onSlope = true;
            _onGround = true;
        }
        else
        {
            _onSlope = false;
        }
    }

    /// <summary>
    /// Accessor for if the player is in water
    /// </summary>
    /// <returns>Whether the player is in wayer</returns>
    internal bool IsInWater()
    {
        return _inWater;
    }

    /// <summary>
    /// Checks if the player has reached the top
    /// </summary>
    /// <returns>Whether the player is complete</returns>
    internal bool IsComplete()
    {
        return _isComplete;
    }

    /// <summary>
    /// Sets up the callbacks and other info for the player
    /// </summary>
    /// <param name="playerIndex">The index of the player</param>
    /// <param name="playerName">The name of the player</param>
    /// <param name="increasePowerup">Callback function for increasing the power up</param>
    /// <param name="clearPowerup">Callback function for clearing the power up</param>
    /// <param name="decreasePowerup">Callback function for decreasing the power up</param>
    public void Initialise(int playerIndex, string playerName, Action increasePowerup, Action clearPowerup, Action decreasePowerup)
    {
        _playerIndex = playerIndex;
        _increasePowerupCallback = increasePowerup;
        _clearPowerupCallback = clearPowerup;
        _decreasePowerupCallback = decreasePowerup;

        // set player text
        PlayerNameText.text = playerName;
    }

    /// <summary>
    /// Gets the player index associated with the player
    /// </summary>
    /// <returns>The player index</returns>
    internal int GetPlayerIndex()
    {
        return _playerIndex;
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
        if (hit && _onGround)
        {
            // get the angle of the collision
            _slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            _slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            // the player is on a slope
            _onSlope = true;
            _onGround = true;
        }

        // if the player is not moving, and is on the ground, set the material to one with high friction, to avoid sliding
        if (_onGround && _movementX == 0f && _onSlope)
            _rigidbody.sharedMaterial = StaticMaterial;
        else
            // otherwise, move freely
            _rigidbody.sharedMaterial = null;
    }

    /// <summary>
    /// Decreases the power up level of the player
    /// </summary>
    internal void DecreasePowerUpLevel()
    {
        _decreasePowerupCallback();
    }

    /// <summary>
    /// Causes the player to jump
    /// </summary>
    public void Jump()
    {
        // can only jump if on ground
        if (_onGround && !_inSludge && _active)
        {
            Debug.Log("CLEARING ONGROUND");
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

        // check if we are on a slope
        SlopeCheck_();

        // move the player
        Move();
    }

    /// <summary>
    /// Recovery from falling into the water
    /// </summary>
    public void RecoveryKeyPressed()
    {
        // if the player is in the water, and recovery still to do
        if (_inWater && _recoveryPressesRemaining > 0)
        {
            _recoveryPressesRemaining--;

            // try to recover
            if (_recoveryPressesRemaining <= 0)
            {
                // recover when we reach 0
                StartCoroutine(WaterRecovery_());
                _recoveryPressesRemaining = 0;
            }
        }
    }

    /// <summary>
    /// When the player collides with an object
    /// </summary>
    /// <param name="collision">The object that the player collided with</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("COLLISION ONGROUND");
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
            _isComplete = true;
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
            _increasePowerupCallback();

            // remove from the active list
            LandslideController.Instance.RemoveBoost(collision.transform);
        }
        // the player has picked up a power up
        else if (collision.gameObject.tag == "Checkpoint")
        {
            // store the order in which the player reached the checkpoint
            bool newCheckpoint = collision.GetComponent<CheckpointScript>().AddPlayer(_playerIndex);
            if (newCheckpoint)
                _increasePowerupCallback();
        }
        // the player has picked up a power up
        else if (collision.gameObject.tag == "Water" && !_inWater)
        {
            Debug.Log("Fell into water");
            _recoveryPressesRemaining = 20;
            WaterCollider.SetActive(true);
            _rigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            StartCoroutine(FreezeYPosition());
        }
    }

    /// <summary>
    /// Waits two seconds after falling to the water, the freezes the player
    /// </summary>
    /// <returns></returns>
    private IEnumerator FreezeYPosition()
    {
        yield return new WaitForSeconds(1.6f);
        _inWater = true;
        yield return new WaitForSeconds(2.4f);
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        WaterCollider.SetActive(false);
        _collider.enabled = false;
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
    /// Removes player from the water and re-enables them
    /// </summary>
    IEnumerator WaterRecovery_()
    {
        // jump
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rigidbody.AddForce(new Vector2(-200, JUMP_FORCE));

        // wait
        yield return new WaitForSeconds(.5f);

        // re-enable collider
        _collider.enabled = true;
        _inWater = false;
    }

    /// <summary>
    /// Moves the player
    /// </summary>
    private void Move()
    {
        if (_active)
        {
            var moveSpeed = _movementX * MOVE_SPEED;
            // slow down if in sludge
            if (_inSludge) moveSpeed *= 0.4f;

            if (!_inWater)
            {
                // if on the ground, but not a slope, walk normally
                if (_onGround && !_onSlope)
                {
                    _newVelocity.Set(moveSpeed, 0);
                }
                // if on a slope, do some maths to work out the force to add
                else if (_onGround && _onSlope)
                {
                    _newVelocity.Set(-moveSpeed * _slopeNormalPerp.x, -moveSpeed * _slopeNormalPerp.y);
                }
                // otherwise (i.e. mid-air), move freely
                else if (!_onGround)
                {
                    _newVelocity.Set(moveSpeed, _rigidbody.velocity.y);
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
            }
        }

        // set the correct animation
        if (_onGround)
        {
            // walking
            if (Math.Abs(_movementX) > 0.15f)
            {
                _animation.SetAnimation("Walk");
            }
            // still
            else
            {
                _animation.SetAnimation("Idle");
            }
        }
        else if (_inWater)
        {
            // TODO: Another animation for water
            _animation.SetAnimation("Jump");
        }
    }
}
