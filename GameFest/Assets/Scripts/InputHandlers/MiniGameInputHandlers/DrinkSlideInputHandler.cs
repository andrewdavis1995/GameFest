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
        _joystickReadings.Add(ctx.ReadValue<Vector2>());

        if (_joystickReadings.Count > NUM_READINGS)
            _joystickReadings.RemoveAt(0);

        if (ctx.ReadValue<Vector2>().y >= -0.05f && _joystickReadings.Count == NUM_READINGS)
        {
            if(_canFire)
            {   
                _canFire = false;
                _isActive = false;
                Debug.Log("IT WAS: " + _joystickReadings[0].y);
                // TODO: calculate angle
                float angle = 0f;
                // TODO: calculate power
                float powerMultiplier = Math.Abs(_joystickReadings[0].x) + Math.Abs(_joystickReadings[0].y);
                // pass to controller (with power and angle)
                DrinkSlideController.Instance.Fire(GetPlayerIndex(), angle, powerMultiplier);
            }
        }
    }

    public override void OnMoveRight(InputAction.CallbackContext ctx, InputDevice device)
    {        
        _spin = ctx.ReadValue<Vector2>();
    }
}
