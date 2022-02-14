using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToneDeathInputHandler : GenericInputHandler
{
    public PlayerMovement Movement;

    ElevatorScript _elevatorZone;
    bool _enteredElevator = false;
    Rigidbody2D _rigidBody;
    Animator _animator;
    SpriteRenderer[] _movementRenderers;
    float _zPosition;

    // Update is called once per frame
    void Update()
    {
        // TEMP
        if (Movement == null)
            return;

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

    public void InitialisePlayer(PlayerMovement movement)
    {
        Movement = movement;
        Movement.SetJumpModifier(1.5f);
        Movement.AddTriggerCallbacks(TriggerEntered_, TriggerLeft_);

        _rigidBody = Movement.GetComponent<Rigidbody2D>();
        _animator = movement.GetComponent<Animator>();
        _movementRenderers = Movement.GetComponentsInChildren<SpriteRenderer>();
        _zPosition = Movement.transform.localPosition.z;
    }

    internal void Hide()
    {
        foreach (var r in _movementRenderers)
        {
            r.color = new Color(r.color.r, r.color.g, r.color.b, 0);
        }
    }

    internal void Show()
    {
        foreach (var r in _movementRenderers)
        {
            r.color = new Color(r.color.r, r.color.g, r.color.b, 1);
        }
    }

    internal void FadeBackIn()
    {
        _rigidBody.isKinematic = false;
        _enteredElevator = false;
        Movement.transform.SetParent(null);
        Movement.transform.localPosition = new Vector3(Movement.transform.localPosition.x, Movement.transform.localPosition.y, _zPosition);
        Movement.SetExitDisable(false);
    }

    private void TriggerEntered_(Collider2D collider)
    {
        if (collider.tag == "Checkpoint")
            _elevatorZone = collider.GetComponentInParent<ElevatorScript>();
    }

    private void TriggerLeft_(Collider2D collider)
    {
        if (collider.tag == "Checkpoint")
            _elevatorZone = null;
    }

    public override void OnTriangle()
    {
        ElevatorCheck_();
    }

    private void ElevatorCheck_()
    {
        if (_elevatorZone != null && !_enteredElevator)
        {
            Movement.SetExitDisable(true);
            _animator.SetTrigger("Idle");
            _rigidBody.isKinematic = true;
            Movement.Move(new Vector2(0, 0));
            _enteredElevator = true;
            Movement.transform.Translate(new Vector3(0, 0.25f, 0f));
            Movement.transform.SetParent(_elevatorZone.Platform);
            Movement.transform.localPosition = new Vector3(Movement.transform.localPosition.x, Movement.transform.localPosition.y, .5f);
            ToneDeathController.Instance.CheckAllPlayersComplete();
        }
    }

    public bool FloorComplete()
    {
        return _enteredElevator;
    }

    public override void OnMove(InputAction.CallbackContext ctx, InputDevice device)
    {
        if (_enteredElevator) return;
        Movement.Move(ctx.ReadValue<Vector2>());
    }

    public override void OnCross()
    {
        if (_enteredElevator) return;
        Movement.Jump();
    }
}
