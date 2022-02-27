using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToneDeathInputHandler : GenericInputHandler
{
    // components
    public PlayerMovement Movement;
    ToneDeathMovement _toneDeathMovement;
    Rigidbody2D _rigidBody;
    Animator _animator;
    SpriteRenderer[] _movementRenderers;
    float _zPosition;
    Instrument _instrument;

    // status variables
    ElevatorScript _elevatorZone;
    bool _enteredElevator = false;
    float _pointerAngle = 0f;
    bool _firing = false;
    InstrumentSelection _instrumentSelection;

    // Update is called once per frame
    void Update()
    {
        // TEMP
        if (!_enteredElevator && !Movement.AutoPilot())
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
            OnCross();
        if (Input.GetKey(KeyCode.Escape))
            OnCircle();
        if (Input.GetKey(KeyCode.T))
            OnTriangle();
        if (Input.GetKeyDown(KeyCode.P))
            OnR1();
        if (Input.GetKeyDown(KeyCode.O))
            HandleR2(1);
        if (Input.GetKeyUp(KeyCode.O))
            HandleR2(0);

        {
            int x = 0, y = 0;
            if (Input.GetKey(KeyCode.A)) x = -1;
            else if (Input.GetKey(KeyCode.D)) x = 1;
            if (Input.GetKey(KeyCode.S)) y = -1;
            else if (Input.GetKey(KeyCode.W)) y = 1;
            MovePointer_(new Vector2(x, y));
        }
    }

    /// <summary>
    /// The player has lost health
    /// </summary>
    /// <param name="damage">The amount of damage that was done</param>
    public void DamageDone(float damage)
    {
        _toneDeathMovement.DamageDone(damage);
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
        return _toneDeathMovement.Health() <= 0;
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

        // initialise game specific movement
        _toneDeathMovement = Movement.GetComponentInChildren<ToneDeathMovement>();
        _toneDeathMovement.Setup(GetPlayerIndex(), Die_);

        // set colour of particles
        var main = _toneDeathMovement.Particles.main;
        var col = ColourFetcher.GetColour(GetPlayerIndex());
        col.a = main.startColor.color.a;
        main.startColor = col;

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
                DamageDone(Crushendo.CRUSHENDO_DAMAGE);
                if (_toneDeathMovement.Health() > 0)
                    StartCoroutine(Movement.Disable(6f, ToneDeathController.Instance.DisabledImages[GetCharacterIndex()]));
            }
        }
    }

    internal void SetInstrument(InstrumentSelection instrumentSelection)
    {
        _instrumentSelection = instrumentSelection;
        _instrumentSelection.PlayerEntered(GetPlayerIndex());
        _instrument = _instrumentSelection.Instrument;
        _toneDeathMovement.ParticlesInstrument.material = _instrumentSelection.Material;

        OnCross();
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
    /// Returns the instrument associated with this player
    /// </summary>
    /// <returns></returns>
    internal Instrument GetInstrument()
    {
        return _instrument;
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
        // elevator zone
        if (collider.tag == "Checkpoint")
            _elevatorZone = collider.GetComponentInParent<ElevatorScript>();

        // bullet collection trigger
        if (collider.tag == "AreaTrigger")
            _toneDeathMovement.ResetBulletCount();

        // instrument selection zone
        if (collider.tag == "PlayerColourDisplay" && !Movement.AutoPilot())
        {
            _instrumentSelection = collider.GetComponent<InstrumentSelection>();
            _instrumentSelection.PlayerEntered(GetPlayerIndex());
            _instrument = _instrumentSelection.Instrument;
            _toneDeathMovement.ParticlesInstrument.material = _instrumentSelection.Material;
        }

        // speaker
        if (collider.tag == "Speaker")
        {
            collider.GetComponent<SpeakerScript>().StartClaim(GetPlayerIndex());
        }

        // enemy
        if (collider.tag == "Enemy")
        {
            var enemy = collider.GetComponent<EnemyControl>();
            if (enemy != null && !enemy.Claimed())
                enemy.StartClaim(GetPlayerIndex());
        }

        // blast
        if (collider.tag == "Blast")
        {
            var enemy = collider.GetComponentInParent<BassDropScript>();
            var sizeFactor = (enemy.MAX_SIZE - enemy.transform.localScale.x) / enemy.MAX_SIZE;
            DamageDone(enemy.DAMAGE * sizeFactor);
        }
    }

    /// <summary>
    /// The player has left a trigger
    /// </summary>
    /// <param name="collider">The trigger</param>
    private void TriggerLeft_(Collider2D collider)
    {
        // elevator zone
        if (collider.tag == "Checkpoint")
            _elevatorZone = null;

        // instrument selection zone
        if (collider.tag == "PlayerColourDisplay" && !Movement.AutoPilot())
        {
            _instrumentSelection = null;
            _instrument = Instrument.None;
            collider.GetComponent<InstrumentSelection>().PlayerExited(GetPlayerIndex());
        }

        // speaker
        if (collider.tag == "Speaker")
        {
            collider.GetComponent<SpeakerScript>().StopClaim(GetPlayerIndex());
        }

        // enemy
        if (collider.tag == "Enemy")
        {
            var enemy = collider.GetComponent<EnemyControl>();
            if (enemy != null)
                enemy.StopClaim(GetPlayerIndex());
        }
    }

    /// <summary>
    /// Makes the player go into the elevator (if they are beside one)
    /// </summary>
    public void EnterElevator()
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

            if (!ToneDeathController.Instance.InstrumentRunOff)
            {
                // check if there are any remaining players
                ToneDeathController.Instance.CheckAllPlayersComplete();
            }
        }
    }

    /// <summary>
    /// Rotates the pointer (direction in which shots will be fired)
    /// </summary>
    /// <param name="movement">Where to move to</param>
    void MovePointer_(Vector2 movement)
    {
        // if in elevator, do nothing
        if (_enteredElevator || Movement.Disabled() || ToneDeathController.Instance.InstrumentSelect()) return;

        // only update if a direction is selected
        if (Math.Abs(movement.x) > 0.1f || Math.Abs(movement.y) > 0.1f)
        {
            _toneDeathMovement.Pointer.gameObject.SetActive(true);

            // work out angle to position pointer at
            _pointerAngle = Mathf.Atan2(movement.y, movement.x) * 180 / Mathf.PI - 90;
            _toneDeathMovement.Pointer.eulerAngles = new Vector3(0, 0, _pointerAngle);
        }
        else
        {
            _toneDeathMovement.Pointer.gameObject.SetActive(false);
        }
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

    /// <summary>
    /// Temporarily disable firing to prevent rapid fire
    /// </summary>
    /// <returns></returns>
    private IEnumerator DisableFiring_()
    {
        _firing = true;
        yield return new WaitForSeconds(0.1f);
        _firing = false;
    }

    /// <summary>
    /// R2 has been pressed or released
    /// </summary>
    /// <param name="v">How pressed the R2 paddle is</param>
    private void HandleR2(float v)
    {
        if (_enteredElevator) return;

        // turn particles on/off
        _toneDeathMovement.ToggleParticles(v > 0.1f);
    }

    #region Player Controls
    public override void OnTriangle()
    {
        // go into elevator if on ground
        if (Movement.OnGround() && !ToneDeathController.Instance.InstrumentSelect())
            EnterElevator();
    }

    public override void OnMove(InputAction.CallbackContext ctx, InputDevice device)
    {
        // if in elevator, do nothing
        if (_enteredElevator) return;

        if (Movement.AutoPilot()) return;

        // move
        Movement.Move(ctx.ReadValue<Vector2>());
    }

    public override void OnCircle()
    {
        if (ToneDeathController.Instance.InstrumentSelect() && _instrumentSelection.Set())
        {
            _instrumentSelection.Set(false, GetPlayerIndex());
            Movement.Reenable();
        }
    }

    public override void OnCross()
    {
        // if in elevator, do nothing
        if (_enteredElevator) return;

        if (ToneDeathController.Instance.InstrumentSelect())
        {
            if (_instrumentSelection != null && !_instrumentSelection.Set() && !ToneDeathController.Instance.InstrumentRunOff)
            {
                _instrumentSelection.Set(true, GetPlayerIndex());
                Movement.DisableMovement();
                ToneDeathController.Instance.CheckAllInstrumentsSelected();
            }
        }
        else
        {
            // jump
            Movement.Jump();
        }
    }

    public override void OnMoveRight(InputAction.CallbackContext ctx)
    {
        if (_enteredElevator) return;

        MovePointer_(ctx.ReadValue<Vector2>());
    }

    public override void OnR1()
    {
        if (_enteredElevator) return;

        // shoot bullet
        if (!_firing && (PauseGameHandler.Instance == null || !PauseGameHandler.Instance.IsPaused()))
        {
            // don't shoot if not pointing
            if (_toneDeathMovement.Pointer.gameObject.activeInHierarchy)
            {
                _toneDeathMovement.Shoot();
                StartCoroutine(DisableFiring_());
            }
        }

        base.OnR1();
    }

    public override void OnR2(InputAction.CallbackContext ctx)
    {
        HandleR2(ctx.ReadValue<float>());
    }
    #endregion
}
