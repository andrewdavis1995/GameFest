using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class DrinkSlideInputHandler : GenericInputHandler
{
    List<Vector2> _joystickReadings = new List<Vector2>();
    int NUM_READINGS = 5;
    bool _isActive = false;
    bool _canFire = false;
    float _spin = 0f;

    private void Update()
    {
        var x = 0;
        var y = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) x = -1;
        else if (Input.GetKey(KeyCode.RightArrow)) x = 1;
        if (Input.GetKey(KeyCode.DownArrow)) y = -1;
        else if (Input.GetKey(KeyCode.UpArrow)) y = 1;

        JoystickMoved(new Vector2(x, y));
    }

    public void IsActive(bool state)
    {
        _isActive = state;
        _canFire = state;
    }

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

        // calculate angle - flip by -1 to point opposite direction
        float angle = 1 * (Mathf.Atan2(_joystickReadings[0].y, _joystickReadings[0].x) * Mathf.Rad2Deg + 90f);

        if (_isActive)
            DrinkSlideController.Instance.UpdatePointer(GetPlayerIndex(), angle);

        if (value.y >= -0.05f && _joystickReadings.Count == NUM_READINGS && _joystickReadings[0].y < -0.05f)
        {
            if (_canFire)
            {
                _canFire = false;
                _isActive = false;

                // calculate power
                float powerMultiplier = Math.Abs(_joystickReadings[0].x) + Math.Abs(_joystickReadings[0].y);

                // pass to controller (with power and angle)
                DrinkSlideController.Instance.Fire(GetPlayerIndex(), angle, powerMultiplier);

                _joystickReadings.Clear();
            }
        }
    }

    public override void OnMoveRight(InputAction.CallbackContext ctx)
    {
        _spin = ctx.ReadValue<Vector2>().x;
    }
}
