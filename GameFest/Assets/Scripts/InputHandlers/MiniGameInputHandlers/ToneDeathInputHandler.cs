using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToneDeathInputHandler : GenericInputHandler
{
    // components
    public Transform Pointer;
    public PlayerMovement Movement;
    Rigidbody2D _rigidBody;
    Animator _animator;
    SpriteRenderer[] _movementRenderers;
    float _zPosition;

    // status variables
    ElevatorScript _elevatorZone;
    bool _enteredElevator = false;
    float _health = 100f;

    // Update is called once per frame
    void Update()
    {
        // TEMP
        if (!_enteredElevator)
        {
            var x = 0f;
            var y = 0f;
            if (Input.GetKey(KeyCode.LeftArrow)) x = -1f;
            else if (Input.GetKey(KeyCode.RightArrow)) x = 1f;
            if (Input.GetKey(KeyCode.UpArrow)) y = 1f;
            else if (Input.GetKey(KeyCode.DownArrow)) y = -1f;
            Movement.Move(new Vector2(x, y));
        }
        if (Input.GetKey(KeyCode.KeypadEnter))
        {
            OnCross();
        }
        if (Input.GetKey(KeyCode.T))
        {
            OnTriangle();
        }
    }

    /// <summary>
    /// The player has lost health
    /// </summary>
    /// <param name="damage">The amount of damage that was done</param>
    public void DamageDone_(float damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            Die_();
        }
    }

    /// <summary>
    /// The player has died and is disabled
    /// </summary>
    private void Die_()
    {
        _enteredElevator = true;
        Movement.Disable(ToneDeathController.Instance.DisabledImages[GetCharacterIndex()]);
        ToneDeathController.Instance.CheckAllPlayersComplete();
    }

    /// <summary>
    /// Checks if the player died
    /// </summary>
    /// <returns>If the player died</returns>
    internal bool Died()
    {
        return _health <= 0;
    }

    /// <summary>
    /// Initialises the player and movement script
    /// </summary>
    /// <param name="movement">Movement script associated with this player</param>
    public void InitialisePlayer(PlayerMovement movement)
    {
        // configure movement
        Movement = movement;
        Movement.SetJumpModifier(1.5f);
        Movement.AddTriggerCallbacks(TriggerEntered_, TriggerLeft_);
        Movement.AddMovementCallbacks(CollisionEntered_, null);

        // gather components
        _rigidBody = Movement.GetComponent<Rigidbody2D>();
        _animator = movement.GetComponent<Animator>();
        _movementRenderers = Movement.GetComponentsInChildren<SpriteRenderer>();
        _zPosition = Movement.transform.localPosition.z;
    }

    /// <summary>
    /// Called when the player collides with another object
    /// </summary>
    /// <param name="obj">The object that was collided with</param>
    private void CollisionEntered_(Collision2D obj)
    {
        if (obj.gameObject.tag == "KickBack")
        {
            // Crushendo - do damage and briefly disable
            if (obj.gameObject.name.Contains("Dropper") && !Movement.Disabled())
            {
                DamageDone_(Crushendo.CRUSHENDO_DAMAGE);
                if (_health > 0)
                    StartCoroutine(Movement.Disable(6f, ToneDeathController.Instance.DisabledImages[GetCharacterIndex()]));
            }
        }
    }

    /// <summary>
    /// Hide the player
    /// </summary>
    internal void Hide()
    {
        // hide all renderers
        foreach (var r in _movementRenderers)
            r.enabled = false;
    }

    /// <summary>
    /// Show the player
    /// </summary>
    internal void Show()
    {
        // show all renderers
        foreach (var r in _movementRenderers)
            r.enabled = true;
    }

    /// <summary>
    /// Player is leaving the elevator
    /// </summary>
    internal void PlayerExitElevator()
    {
        _enteredElevator = false;

        // move back to the normal position
        _rigidBody.isKinematic = false;
        Movement.transform.SetParent(null);
        Movement.transform.localPosition = new Vector3(Movement.transform.localPosition.x, Movement.transform.localPosition.y, _zPosition);

        // leaving ground should now trigger jump animation
        Movement.SetExitDisable(false);
    }

    /// <summary>
    /// The player has entered a trigger
    /// </summary>
    /// <param name="collider">The trigger</param>
    private void TriggerEntered_(Collider2D collider)
    {
        if (collider.tag == "Checkpoint")
            _elevatorZone = collider.GetComponentInParent<ElevatorScript>();
    }

    /// <summary>
    /// The player has left a trigger
    /// </summary>
    /// <param name="collider">The trigger</param>
    private void TriggerLeft_(Collider2D collider)
    {
        if (collider.tag == "Checkpoint")
            _elevatorZone = null;
    }

    /// <summary>
    /// Makes the player go into the elevator (if they are beside one)
    /// </summary>
    private void EnterElevator_()
    {
        // if no elevator (and not already in it)
        if (_elevatorZone != null && !_enteredElevator)
        {
            _enteredElevator = true;

            // jump trigger is not set when player leaves ground
            Movement.SetExitDisable(true);
            _animator.SetTrigger("Idle");

            // disable movement
            Movement.Move(new Vector2(0, 0));

            // move into elevator
            _rigidBody.isKinematic = true;
            Movement.transform.Translate(new Vector3(0, 0.25f, 0f));
            Movement.transform.SetParent(_elevatorZone.Platform);
            Movement.transform.localPosition = new Vector3(Movement.transform.localPosition.x, Movement.transform.localPosition.y, .5f);

            // check if there are any remaining players
            ToneDeathController.Instance.CheckAllPlayersComplete();
        }
    }
    
    /// <summary>
    /// Rotates the pointer (direction in which shots will be fired)
    /// </summary>
    /// <param name="movement">Where to move to</param>
    void MovePointer_(Vector2 movement)
    {
        // work out angle to position pointer at
        var angle = Vector2.Angle(Vector2.zero, movement);
        Pointer.eulerAngles = new Vector3(0, 0, angle);
    }

    /// <summary>
    /// Checks if the player is complete for this level
    /// </summary>
    /// <returns>If the player is complete or dead</returns>
    public bool FloorComplete()
    {
        // TODO: add check for dead
        return _enteredElevator;
    }

    #region Player Controls
    public override void OnTriangle()
    {
        // go into elevator if on ground
        if (Movement.OnGround())
            EnterElevator_();
    }

    public override void OnMove(InputAction.CallbackContext ctx, InputDevice device)
    {
        // if in elevator, do nothing
        if (_enteredElevator) return;

        // move
        Movement.Move(ctx.ReadValue<Vector2>());
    }

    public override void OnCross()
    {
        // if in elevator, do nothing
        if (_enteredElevator) return;

        // jump
        Movement.Jump();
    }    

    /// <summary>
    /// When the right joystick is moved
    /// </summary>
    /// <param name="ctx">Context of the input</param>
    public override void OnMoveRight(InputAction.CallbackContext ctx)
    {
        MovePointer_(ctx.ReadValue<Vector2>());
    }
    #endregion
}
