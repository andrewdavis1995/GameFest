using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class DrinkSlideInputHandler : GenericInputHandler
{
    const float ANGLE_CORRECTION = 90f;
    const float THROW_POWER = 1800f;

    List<Vector2> _joystickReadings = new List<Vector2>();
    int NUM_READINGS = 5;
    bool _canFire = false;
    float _spin = 0f;
    float _angle = 0f;

    DrinkObjectScript _activeShot;
    LineRenderer _activeShotLine;

    Vector3 _direction = Vector3.zero;

    public void Initialise(InputDevice device)
    {
        NUM_READINGS = device is DualShockGamepad ? 5 : 2;
    }

    public override void OnMove(InputAction.CallbackContext ctx, InputDevice device)
    {
        JoystickMoved(ctx.ReadValue<Vector2>());
    }

    void JoystickMoved(Vector2 value)
    {
        _joystickReadings.Add(value);

        if (_joystickReadings.Count > NUM_READINGS)
            _joystickReadings.RemoveAt(0);

        // calculate angle
        _angle = 1 * (Mathf.Atan2(_joystickReadings[0].y, _joystickReadings[0].x) * Mathf.Rad2Deg + 90f);

        _direction = _joystickReadings[0];

        if (value.y >= -0.05f && _joystickReadings.Count == NUM_READINGS && _joystickReadings[0].y < -0.2f)
        {
            if (_canFire)
            {
                // calculate power
                float powerMultiplier = Math.Abs(_joystickReadings[0].x) + Math.Abs(_joystickReadings[0].y);

                // pass to controller (with power and angle)
                Throw_(THROW_POWER * powerMultiplier);

                _joystickReadings.Clear();

                _angle = 0;

                _direction = Vector3.zero;
            }
        }

        // if zero, clear list
        if (Math.Abs(value.x) < 0.05f && Math.Abs(value.y) < 0.05f)
        {
            _joystickReadings.Clear();
        }
    }

    public override void OnMoveRight(InputAction.CallbackContext ctx)
    {
        _spin = ctx.ReadValue<Vector2>().x;
    }

    private void Update()
    {
        if (_canFire)
        {
            // TODO: Update direction
            // TODO: Update curve
            if (_activeShotLine.enabled && _activeShot != null && _activeShot.gameObject.activeInHierarchy)
            {
                _activeShotLine.positionCount = 2;
                var position = _activeShot.transform.position - (5 * _direction);
                //var midPos = ((position + _nextShot.transform.position) / 2) + new Vector3(_direction.y * _rightCurve, 0, 0);

                _activeShotLine.SetPositions(new Vector3[] { _activeShot.transform.position, position });

                _activeShot.transform.eulerAngles = new Vector3(0, 0, _angle);
            }
        }
    }

    public void Throw_(float force)
    {
        _canFire = false;
        var fireAngle = _angle + ANGLE_CORRECTION;

        _activeShotLine.enabled = false;
        _activeShotLine.SetPositions(new Vector3[] { });

        float xcomponent = Mathf.Cos(fireAngle * Mathf.PI / 180) * force;
        float ycomponent = Mathf.Sin(fireAngle * Mathf.PI / 180) * force;

        _activeShot.GetRigidBody().AddForce(new Vector2(xcomponent, ycomponent));

        var drink = _activeShot.GetComponent<DrinkObjectScript>();
        drink.UpdateSpin(_spin);
        StartCoroutine(drink.SpinMovement());
        StartCoroutine(drink.DrinkTimeout());
        StartCoroutine(CheckForShotEnd_());
    }

    private IEnumerator CheckForShotEnd_()
    {
        yield return new WaitForSeconds(1f);

        while (_activeShot != null && _activeShot.gameObject.activeInHierarchy && _activeShot.GetRigidBody().velocity.y > 0.05f)
        {
            yield return new WaitForSeconds(0.1f);
        }

        CreateDrink_();
    }

    public void Enable()
    {
        CreateDrink_();
    }

    private void CreateDrink_()
    {
        var item = Instantiate(DrinkSlideController.Instance.DrinkPrefab, DrinkSlideController.Instance.StartPositions[GetPlayerIndex()], Quaternion.identity);
        _activeShot = item.GetComponent<DrinkObjectScript>();
        _activeShotLine = item.GetComponent<LineRenderer>();
        _activeShot.Initialise(GetPlayerIndex());
        _activeShot.GetComponent<SpriteRenderer>().sortingOrder = 1;

        _activeShotLine.enabled = true;

        _canFire = true;
    }
}
